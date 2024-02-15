using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum KidState
{
    IDLE,
    WANDERING,
    SEARCHING,
    HUNTING,
    STUNNED
}

public enum KidType
{
    BASE,
    PRINCESS,
    WEIRD,
    GOTH,
    SUGAR,
    GROSS
}

public class Kid : MonoBehaviour
{
    // Settings
    [SerializeField] protected float RunSpeed = 3f;
    [SerializeField] protected float AnimationSpeed = 1f;
    protected int HugDamage = BASE_HUG_DAMAGE;
    protected float AttackSpeed = BASE_ATTACK_SPEED;
    protected float IdleDuration = BASE_IDLE_DURATION;
    protected KidType type = KidType.BASE;


    public const int BASE_HUG_DAMAGE = 25;
    public const int BASE_HUG_STRENGTH = 5;
    public const float BASE_ATTACK_SPEED = 1f;
    public const float STUN_DURATION = 2f;
    public const float BASE_IDLE_DURATION = 5f;

    protected KidState _state;
    protected NavMeshAgent _agent;
    protected SpriteRenderer _renderer;
    protected Animator _animator;
    protected Vector2 _positionPlayerLastSeen;
    protected float _timeSinceLastAttack;
    protected Renderer _floor; // for wander behavior
    protected LayerMask _maskBlockable; // bockables prevent line of sight with player

    protected Coroutine _coroutineIdle;
    protected Coroutine _coroutineWander;
    protected Coroutine _coroutineSearch;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Renderer>(); // find floor here, might be bad for performance (should use singleton game manager instead)
        _maskBlockable = LayerMask.GetMask("Blockable");

        _agent.speed = RunSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _timeSinceLastAttack = 0f;
        _animator.speed = AnimationSpeed;

        Player.OnPlayerEscapingHug += OnPlayerEscapingHug;

        SetState(KidState.IDLE);

        Debug.Log("Calling Start for: " + gameObject.name);
    }

    private void OnDisable()
    {
        Player.OnPlayerEscapingHug -= OnPlayerEscapingHug;
    }

    private void Update()
    {
        HandleSpriteFlipping();
    }

    protected void SetState(KidState state)
    {
        _state = state;

        //Debug.Log("Kid" + gameObject.name + " is: " + state.ToString());
        StopAICoroutines();
        switch (state)
        {
            case KidState.IDLE:

                _coroutineIdle = StartCoroutine(IdleCoroutine());
                return;

            case KidState.WANDERING:

                _coroutineWander = StartCoroutine(WanderCoroutine());
                return;

            case KidState.HUNTING:

                StopAICoroutines();
                _agent.SetDestination(_positionPlayerLastSeen);
                _agent.isStopped = false;
                _animator.SetBool("isMoving", true);
                return;

            case KidState.SEARCHING:

                _coroutineSearch = StartCoroutine(SearchCoroutine());
                return;

            case KidState.STUNNED:

                StartCoroutine(StunnedCoroutine());
                return;
        }
    }


    #region Collision

    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag(GameManager.PLAYER_TAG))
        {
            _positionPlayerLastSeen = collider.transform.position;

            if (_state != KidState.STUNNED)
            {
                if (IsClose(collider) && !HasAttackedRecently())
                {
                    MeleeAttack();
                }

                if (HasLineOfSight(collider))
                {
                    SetState(KidState.HUNTING);
                }
                else if (_state == KidState.HUNTING)
                {
                    SetState(KidState.SEARCHING);
                }
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag(GameManager.PLAYER_TAG))
        {
            SetState(KidState.IDLE);
        }
    }

    #endregion


    #region Actions

    protected virtual void MeleeAttack()
    {
        Events.OnPlayerTakingDamage.Invoke(type, HugDamage, UnityEngine.Random.Range(BASE_HUG_STRENGTH - 3, BASE_HUG_STRENGTH + 3));
        _timeSinceLastAttack = Time.time;
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


    protected bool IsClose(Collider2D collider)
    {
        return Vector2.Distance(transform.position, collider.transform.position) <= 1f;
    }

    protected bool HasAttackedRecently()
    {
        return Time.time - _timeSinceLastAttack < (1f / AttackSpeed);
    }

    #endregion


    #region Event Recievers

    private void OnPlayerEscapingHug()
    {
        SetState(KidState.STUNNED);
    }

    #endregion


    #region Coroutines

    private IEnumerator IdleCoroutine()
    {
        _agent.isStopped = true;
        _animator.SetBool("isMoving", false);

        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(IdleDuration - 3, IdleDuration + 3));

        SetState(KidState.WANDERING);
    }

    // this can be combned with search coroutine
    private IEnumerator WanderCoroutine()
    {
        Vector2 destination = CalculateRandomLocation();
        _agent.SetDestination(destination);
        _agent.isStopped = false;
        _animator.SetBool("isMoving", true);

        while (Vector2.Distance(transform.position, destination) > 1f)
        {
            yield return null;
        }

        SetState(KidState.IDLE);
    }

    // this can be combined with wander coroutine
    private IEnumerator SearchCoroutine()
    {
        _agent.SetDestination(_positionPlayerLastSeen);
        _agent.isStopped = false;
        _animator.SetBool("isMoving", true);

        // this needs adjustment
        while (Vector2.Distance(transform.position, _positionPlayerLastSeen) > 4f)
        {
            yield return null;
        }

        SetState(KidState.IDLE);
    }

    private IEnumerator StunnedCoroutine()
    {
        _agent.isStopped = true;

        Color colorDefault = _renderer.color;
        Color colorTransparent = _renderer.color;
        colorTransparent.a = 0.25f;
        _renderer.color = colorTransparent;
        _animator.SetBool("isMoving", false);

        yield return new WaitForSecondsRealtime(STUN_DURATION);

        _renderer.color = colorDefault;



        Debug.Log("Stun duration done");


        SetState(KidState.SEARCHING);
    }

    private void StopAICoroutines()
    {
        if (_coroutineIdle != null)
        {
            StopCoroutine(_coroutineIdle);
        }

        if (_coroutineWander != null)
        {
            StopCoroutine(_coroutineWander);
        }

        if (_coroutineSearch != null)
        {
            StopCoroutine(_coroutineSearch);
        }
    }

    #endregion


    #region Misc

    // Finds a random point on the floor of the level, for wandering behavior
    private Vector2 CalculateRandomLocation()
    {
        float x = UnityEngine.Random.Range(_floor.bounds.min.x, _floor.bounds.max.x);
        float y = UnityEngine.Random.Range(_floor.bounds.min.y, _floor.bounds.max.y);
        return new Vector2(x, y);
    }

    private void HandleSpriteFlipping()
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
