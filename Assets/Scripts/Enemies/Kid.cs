using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum KidType
{
    BASE,
    PRINCESS,
    WEIRD,
    GOTH,
    SUGAR,
    GROSS
}

/// <summary>
/// Idle: Passive, stationary
/// Wandering: Randomly moves around
/// Hunting: Spotted player, moving towards them
/// Searching: Lost sight of player while hunting, now moving to last known location
/// Attacking: Damaging player
/// Stunned: Temporarily immobilized after player escapes hug
/// </summary>
public enum KidAIState
{
    IDLE,
    WANDERING,
    HUNTING,
    SEARCHING,
    HUG_ATTACKING,
    STUNNED
}


public class Kid : MonoBehaviour
{
    // Settings
    [SerializeField] protected float RunSpeed = 3f;
    [SerializeField] protected float AnimationSpeed = 1f;
    protected int AttackDamage = BASE_HUG_DAMAGE;
    protected float AttackSpeed = BASE_ATTACK_SPEED;
    protected float IdleDuration = BASE_IDLE_DURATION;
    protected KidType type = KidType.BASE;


    public const int BASE_HUG_DAMAGE = 20;
    public const int BASE_ATTACK_STRENGTH = 5;
    public const float BASE_ATTACK_SPEED = 1f;
    public const float BASE_IDLE_DURATION = 5f;

    protected KidAIState _state;
    protected NavMeshAgent _agent;
    protected SpriteRenderer _renderer;
    protected Animator _animator;
    protected Vector2 _positionPlayerLastSeen;
    protected float _timeSinceLastAttack;
    protected SpriteRenderer _floor; // for wander behavior
    protected LayerMask _maskBlockable; // bockables prevent line of sight with player

    protected Coroutine _coroutineIdleAI;
    protected Coroutine _coroutineWanderAI;
    protected Coroutine _coroutineSearchAI;
    protected Coroutine _coroutineHuntingAI;
    protected Coroutine _coroutineHugAttackAI;
    protected Coroutine _coroutineStunnedAI;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<SpriteRenderer>(); // find floor here, might be bad for performance (should use singleton game manager instead)
        _maskBlockable = LayerMask.GetMask("Blockable");

        _agent.speed = RunSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _timeSinceLastAttack = 0f;
        _animator.speed = AnimationSpeed;

        Events.OnPlayerEscapingHug.Subscribe(OnPlayerEscapingHug);

        SetAIState(KidAIState.IDLE);
    }


    private void OnDisable()
    {
        Events.OnPlayerEscapingHug.Unsubscribe(OnPlayerEscapingHug);
    }



    private void Update()
    {
        HandleSpriteFlipping();
    }


    #region AI State Machine

    public KidAIState GetAIState()
    {
        return _state;
    }

    public bool IsAIState(KidAIState state)
    {
        return state == _state;
    }

    protected virtual void SetAIState(KidAIState state)
    {
        if (IsAIState(state) || !gameObject.activeSelf)
        {
            return;
        }

        _state = state;

        switch (state)
        {
            case KidAIState.IDLE:

                _coroutineIdleAI = StartCoroutine(IdleCoroutine());
                return;

            case KidAIState.WANDERING:

                _coroutineWanderAI = StartCoroutine(WanderCoroutine());
                return;

            case KidAIState.HUNTING:

                StopAllAICoroutines();
                _coroutineHuntingAI = StartCoroutine(HuntingAICoroutine());
                return;

            case KidAIState.HUG_ATTACKING:

                _coroutineHugAttackAI = StartCoroutine(HugAttackCoroutine());
                return;

            case KidAIState.SEARCHING:

                _coroutineSearchAI = StartCoroutine(SearchCoroutine());
                return;

            case KidAIState.STUNNED:

                _coroutineStunnedAI = StartCoroutine(StunnedCoroutine());
                return;
        }
    }

    #endregion


    #region Collision

    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        if (IsPlayerCollision(collider))
        {
            _positionPlayerLastSeen = collider.transform.position;

            if (_state != KidAIState.STUNNED)
            {
                if (HasLineOfSight(collider))
                {
                    if (InMeleeRangeToPlayer(collider.transform.position) && !IsAIState(KidAIState.HUG_ATTACKING))
                    {
                        SetAIState(KidAIState.HUG_ATTACKING);
                    }
                    else if (!InMeleeRangeToPlayer(collider.transform.position))
                    {
                        SetAIState(KidAIState.HUNTING);
                    }
                }
                else if (IsAIState(KidAIState.HUNTING))
                {
                    SetAIState(KidAIState.SEARCHING);
                }
            }
        }
    }


    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (IsPlayerCollision(collider))
        {
            SetAIState(KidAIState.IDLE);
        }
    }

    #endregion


    #region AI Coroutines

    protected virtual IEnumerator IdleCoroutine()
    {
        _agent.isStopped = true;
        _animator.SetBool("isMoving", false);

        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(IdleDuration - 3, IdleDuration + 3));

        _coroutineIdleAI = null;
        SetAIState(KidAIState.WANDERING);
    }

    // Code is similar to search coroutine, combine?
    protected virtual IEnumerator WanderCoroutine()
    {
        Vector2 destination = CalculateRandomLevelLocation();
        _agent.SetDestination(destination);
        _agent.isStopped = false;
        _animator.SetBool("isMoving", true);

        while (Vector2.Distance(transform.position, destination) > 1f)
        {
            yield return null;
        }

        _coroutineWanderAI = null;
        SetAIState(KidAIState.IDLE);
    }

    private IEnumerator HuntingAICoroutine()
    {
        _agent.isStopped = false;
        _animator.SetBool("isMoving", true);

        while (IsAIState(KidAIState.HUNTING))
        {
            _agent.SetDestination(_positionPlayerLastSeen);

            yield return null;
        }

        _coroutineHuntingAI = null;
    }

    private IEnumerator HugAttackCoroutine()
    {
        _agent.isStopped = true;
        _animator.SetBool("isMoving", false);

        while (IsAIState(KidAIState.HUG_ATTACKING))
        {
            HugAttack();

            yield return new WaitForSecondsRealtime(CalculateTimeBetweenAttacks());
        }

        _coroutineHugAttackAI = null;
    }

    protected virtual IEnumerator SearchCoroutine()
    {
        _agent.SetDestination(_positionPlayerLastSeen);
        _agent.isStopped = false;
        _animator.SetBool("isMoving", true);

        // this needs adjustment
        while (Vector2.Distance(transform.position, _positionPlayerLastSeen) > 4f)
        {
            yield return null;
        }

        _coroutineSearchAI = null;
        SetAIState(KidAIState.IDLE);
    }

    protected virtual IEnumerator StunnedCoroutine()
    {
        _agent.isStopped = true;
        Color colorDefault = _renderer.color;
        Color colorTransparent = _renderer.color;
        colorTransparent.a = 0.25f;
        _renderer.color = colorTransparent;
        _animator.SetBool("isMoving", false);

        yield return new WaitForSecondsRealtime(GameManager.KID_STUN_DURATION);

        _renderer.color = colorDefault;
        _coroutineStunnedAI = null;
        SetAIState(KidAIState.SEARCHING);
    }

    protected virtual void StopAllAICoroutines()
    {
        if (_coroutineIdleAI != null)
        {
            StopCoroutine(_coroutineIdleAI);
            _coroutineIdleAI = null;
        }

        if (_coroutineWanderAI != null)
        {
            StopCoroutine(_coroutineWanderAI);
            _coroutineWanderAI = null;
        }

        if (_coroutineHuntingAI != null)
        {
            StopCoroutine(_coroutineHuntingAI);
            _coroutineHuntingAI = null;
        }

        if (_coroutineHugAttackAI != null)
        {
            StopCoroutine(_coroutineHugAttackAI);
            _coroutineHugAttackAI = null;
        }

        if (_coroutineSearchAI != null)
        {
            StopCoroutine(_coroutineSearchAI);
            _coroutineSearchAI = null;
        }

        if (_coroutineStunnedAI != null)
        {
            StopCoroutine(_coroutineStunnedAI);
            _coroutineStunnedAI = null;
        }
    }

    #endregion


    #region Actions

    // Unnessecary abstraction for now, but perhaps will have more later (like animations and sound)
    protected virtual void HugAttack()
    {
        Events.OnKidAttacking.Invoke(type, AttackDamage, CalculateHugAttackStrength());
    }

    #endregion


    #region Bool Checks

    protected bool HasLineOfSight(Collider2D colliderPlayer)
    {
        //var test = Physics2D.CircleCast(transform.position, 0.5f, colliderPlayer.transform.position - transform.position,
        //    Vector2.Distance(transform.position, colliderPlayer.transform.position), BlockableLayerMask);

        //Debug.Log("LOS?: " + !test);

        //if (test)
        //{
        //    Debug.Log("Hit: " + test.collider.gameObject.name);
        //}

        return !Physics2D.CircleCast(transform.position, 0.5f, colliderPlayer.transform.position - transform.position,
            Vector2.Distance(transform.position, colliderPlayer.transform.position), _maskBlockable);
    }

    protected bool InMeleeRangeToPlayer(Vector2 positionPlayer)
    {
        return Vector2.Distance(transform.position, positionPlayer) <= 1f;
    }


    protected bool InCloseRangeToPlayer(Vector2 positionPlayer)
    {
        return Vector2.Distance(transform.position, positionPlayer) <= 4f;
    }


    // Shorthand for checking if player collision, and ensuring that the AI stuff doesn't trigger when the kid is disabled
    protected bool IsPlayerCollision(Collider2D collider)
    {
        return collider.CompareTag(GameManager.TAG_PLAYER);
    }



    #endregion


    #region Event Listeners

    protected virtual void OnPlayerEscapingHug()
    {

        if (IsAIState(KidAIState.HUG_ATTACKING) || (IsAIState(KidAIState.HUNTING) && InCloseRangeToPlayer(_positionPlayerLastSeen)))
        {
            SetAIState(KidAIState.STUNNED);
        }
        
        
        //Debug.Log("Kid reacting to escape hug 1");
        
        //SetState(KidState.STUNNED);

        //Debug.Log("Kid reacting to escape hug 2");


        //Events.OnPlayerEscapingHug.Unsubscribe(OnPlayerEscapingHug);
    }

    #endregion


    #region Misc

    // Finds a random point on the floor of the level, for wandering behavior
    protected Vector2 CalculateRandomLevelLocation()
    {
        float x = UnityEngine.Random.Range(_floor.bounds.min.x, _floor.bounds.max.x);
        float y = UnityEngine.Random.Range(_floor.bounds.min.y, _floor.bounds.max.y);
        return new Vector2(x, y);
    }

    protected int CalculateHugAttackStrength()
    {
        return UnityEngine.Random.Range(BASE_ATTACK_STRENGTH - 3, BASE_ATTACK_STRENGTH + 3);
    }

    protected float CalculateTimeBetweenAttacks()
    {
        return 1f / AttackSpeed;
    }


    protected void HandleSpriteFlipping()
    {
        if (_agent.velocity.x > 0)
        {
            _renderer.flipX = true;
        }
        else if (_agent.velocity.x < 0)
        {
            _renderer.flipX = false;
        }
    }

    #endregion
}
