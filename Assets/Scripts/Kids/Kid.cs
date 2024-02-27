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
/// Null: Default for game setup, should never be used otherwise
/// Idle: Passive, stationary
/// Wandering: Randomly moves around
/// Hunting: Spotted player, moving towards them
/// Searching: Lost sight of player while hunting, now moving to last known location
/// Hug Attacking: Next to player and actively hurting them
/// Stunned: Temporarily immobilized after player escapes hug attack
/// </summary>
public enum KidAIState
{
    NULL,
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

    protected KidAIState _state = KidAIState.NULL;
    protected NavMeshAgent _agent;
    protected CapsuleCollider2D _collider;
    protected CircleCollider2D _colliderAIPlayerDetection;
    protected SpriteRenderer _renderer;
    protected Animator _animator;

    protected Vector2 _positionPlayerLastSeen;
    protected float _timeSinceLastAttack;
    protected Renderer _floor; // for wander behavior
    protected LayerMask _maskBlockable; // bockables prevent line of sight with player
    protected bool inPrincessCommandZone; // when inside, able to be commanded to attack player by princess
    protected bool isCommandedToHuntPlayer; // while active, hunting player regardless of distance

    protected Coroutine _coroutineIdleAI;
    protected Coroutine _coroutineWanderAI;
    protected Coroutine _coroutineSearchAI;
    protected Coroutine _coroutineHuntingAI;
    protected Coroutine _coroutineHugAttackAI;
    protected Coroutine _coroutineStunnedAI;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<CapsuleCollider2D>();
        _colliderAIPlayerDetection = transform.Find("AI Player Detection").GetComponent<CircleCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Renderer>(); // find floor here, might be bad for performance (should use singleton game manager instead)
        _maskBlockable = LayerMask.GetMask("Blockable");

        _agent.speed = RunSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _timeSinceLastAttack = 0f;
        _animator.speed = AnimationSpeed;



        Events.OnPlayerEscapingHug.Subscribe(OnPlayerEscapingHug);
        Events.OnPrincessCommanding.Subscribe(OnPrincessCommanding);

        SetAIState(KidAIState.IDLE);
    }


    protected virtual void OnDisable()
    {
        Events.OnPlayerEscapingHug.Unsubscribe(OnPlayerEscapingHug);
        Events.OnPrincessCommanding.Unsubscribe(OnPrincessCommanding);
    }



    protected virtual void Update()
    {
        HandleSpriteFlipping();
    }


    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case CollisionTags.PLAYER:

                if (_state != KidAIState.STUNNED)
                {
                    if (IsPlayerWithinLineOfSight())
                    {
                        _positionPlayerLastSeen = GameManager.instance.Player.transform.position;

                        if (IsPlayerWithinMeleeRange() && !IsAIState(KidAIState.HUG_ATTACKING))
                        {
                            SetAIState(KidAIState.HUG_ATTACKING);
                        }
                        else if (!IsPlayerWithinMeleeRange())
                        {
                            SetAIState(KidAIState.HUNTING);
                        }
                    }
                    else if (IsAIState(KidAIState.HUNTING) && !isCommandedToHuntPlayer)
                    {
                        SetAIState(KidAIState.SEARCHING);
                    }
                }
                return;

            case CollisionTags.PRINCESS_COMMAND_ZONE:

                inPrincessCommandZone = true;
                return;
        }
    }


    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case CollisionTags.PLAYER:

                if (!isCommandedToHuntPlayer)
                {
                    SetAIState(KidAIState.IDLE);
                }
                return;
        }
    }




    #region Actions

    // Move to specified position
    protected void MoveToDestination(Vector2 destination)
    {
        _agent.speed = RunSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
        _animator.SetBool("isMoving", true);
    }

    // Picks a random point in the level, move towards it, returns it
    protected Vector2 MoveToRandomDestination()
    {
        Vector2 destination = CalculateRandomLevelLocation();
        _agent.speed = RunSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
        _animator.SetBool("isMoving", true);

        return destination;
    }

    protected void StopMovement()
    {
        _agent.speed = 0f;
        _agent.isStopped = true;
        _animator.SetBool("isMoving", false);
    }

    // Unnessecary abstraction for now, but perhaps will have more later (like animations and sound)
    protected virtual void HugAttack()
    {
        Events.OnKidAttacking.Invoke(type, AttackDamage, CalculateHugAttackStrength());
    }

    #endregion


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


    #region AI Coroutines

    protected virtual IEnumerator IdleCoroutine()
    {
        StopMovement();

        yield return new WaitForSecondsRealtime(CalculateIdleDuration());

        _coroutineIdleAI = null;
        SetAIState(KidAIState.WANDERING);
    }

    // Code is similar to search coroutine, combine?
    protected virtual IEnumerator WanderCoroutine()
    {
        Vector2 destination = MoveToRandomDestination();

        while (Vector2.Distance(transform.position, destination) > 1f)
        {
            yield return null;
        }

        _coroutineWanderAI = null;
        SetAIState(KidAIState.IDLE);
    }

    // Pursue player
    protected virtual IEnumerator HuntingAICoroutine()
    {
        MoveToDestination(GameManager.instance.Player.transform.position);

        while (IsAIState(KidAIState.HUNTING))
        {
            yield return new WaitForFixedUpdate();

            _agent.SetDestination(GameManager.instance.Player.transform.position);
        }

        _coroutineHuntingAI = null;
    }

    protected virtual IEnumerator HugAttackCoroutine()
    {
        StopMovement();

        while (IsAIState(KidAIState.HUG_ATTACKING))
        {
            HugAttack();

            yield return new WaitForSecondsRealtime(CalculateTimeBetweenAttacks());
        }

        _coroutineHugAttackAI = null;
    }

    protected virtual IEnumerator SearchCoroutine()
    {

        MoveToDestination(_positionPlayerLastSeen);

        // this needs adjustment
        while (Vector2.Distance(transform.position, _positionPlayerLastSeen) > 1f)
        {
            //Debug.Log("Searching for: " + _positionPlayerLastSeen);
            //Debug.Log(Vector2.Distance(transform.position, _positionPlayerLastSeen));

            yield return null;

            _agent.SetDestination(_positionPlayerLastSeen);
        }

        _coroutineSearchAI = null;
        SetAIState(KidAIState.IDLE);
    }

    protected virtual IEnumerator StunnedCoroutine()
    {
        StopMovement();
        Color colorDefault = _renderer.color;
        Color colorTransparent = _renderer.color;
        colorTransparent.a = 0.25f;
        _renderer.color = colorTransparent;

        yield return new WaitForSecondsRealtime(GameManager.KID_STUN_DURATION);

        _renderer.color = colorDefault; // needed to fix this, sometimes it doesn't reach here and kid stays transparent
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


    #region Bool Checks

    // Does a simple line cast to player, blocked by walls
    protected bool IsPlayerWithinLineOfSight()
    {
        Debug.DrawLine(transform.position, GameManager.instance.Player.transform.position, Color.magenta);

        return !Physics2D.Linecast(transform.position, GameManager.instance.Player.transform.position, _maskBlockable);
    }

    // Player is inside the bounds of the AI Player Detection circle
    private bool IsPlayerWithinDetectionRange()
    {
        return Vector2.Distance(transform.position, GameManager.instance.Player.transform.position) <= _colliderAIPlayerDetection.radius;
    }

    // combine this with InCloseRangeToPlayer? (get player distance level or something)
    protected bool IsPlayerWithinMeleeRange()
    {
        return Vector2.Distance(transform.position, GameManager.instance.Player.transform.position) <= 1f;
    }

    // idk, like between detection and melee distances
    protected bool IsPlayerWithinMediumRange()
    {
        return Vector2.Distance(transform.position, GameManager.instance.Player.transform.position) <= 4f;
    }

    #endregion


    #region Event Listeners

    protected virtual void OnPlayerEscapingHug()
    {
        if (IsAIState(KidAIState.HUG_ATTACKING) || (IsAIState(KidAIState.HUNTING) && IsPlayerWithinMediumRange()))
        {
            SetAIState(KidAIState.STUNNED);
        }
    }

    // If princess detects player, she commands all nearby kids to hunt them
    // Kids are considered nearby if inside Princess Command Zone circle
    // Commanded kids will hunt player indefinitely regardless of distance, until princess loses player
    // When princess loses player, kids that are hunting and still close to player will continue to hunt, otherwise they revert to idle
    protected virtual void OnPrincessCommanding(bool huntPlayer)
    {
        if (inPrincessCommandZone)
        {
            if (huntPlayer && !IsAIState(KidAIState.HUNTING) && !IsAIState(KidAIState.HUG_ATTACKING))
            {
                isCommandedToHuntPlayer = true;
                SetAIState(KidAIState.HUNTING);
            }
            else if (!huntPlayer)
            {
                isCommandedToHuntPlayer = false;
                inPrincessCommandZone = false;

                if (IsAIState(KidAIState.HUNTING) && !IsPlayerWithinDetectionRange())
                {
                    SetAIState(KidAIState.IDLE);
                }
            }
        }
    }

    #endregion


    #region Misc

    // Finds a random point on the floor of the level, for wandering behavior
    protected virtual Vector2 CalculateRandomLevelLocation()
    {
        float x = UnityEngine.Random.Range(_floor.bounds.min.x, _floor.bounds.max.x);
        float y = UnityEngine.Random.Range(_floor.bounds.min.y, _floor.bounds.max.y);
        return new Vector2(x, y);
    }

    protected virtual int CalculateHugAttackStrength()
    {
        return UnityEngine.Random.Range(BASE_ATTACK_STRENGTH - 3, BASE_ATTACK_STRENGTH + 3);
    }

    protected virtual float CalculateTimeBetweenAttacks()
    {
        return 1f / AttackSpeed;
    }

    protected virtual float CalculateIdleDuration()
    {
        return UnityEngine.Random.Range(IdleDuration - 3f, IdleDuration + 3f);
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
