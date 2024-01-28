using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Kid : MonoBehaviour
{
    [SerializeField] private float RunSpeed = 3f;
    [SerializeField] private float AnimationSpeed = 1f;

    // Settings
    private const int BASE_HUG_DAMAGE = 25;
    private const int BASE_HUG_STRENGTH = 5;
    private const float TIME_BETWEEN_HUGS = 1f;
    private const float STUN_DURATION = 2f;
    private const float BASE_IDLE_DURATION = 5f;
    private const string PLAYER_TAG = "Player";

    public static event Action<int, int> OnKidAttacking;

    private enum KidState
    {
        IDLE,
        WANDERING,
        SEARCHING,
        HUNTING,
        STUNNED
    }

    private KidState _state;
    private NavMeshAgent _agent;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private Vector2 _positionPlayerLastSeen;
    private float _timeSinceLastAttack;
    private SpriteRenderer _floor; // for wander behavior
    private LayerMask _maskBlockable; // bockables prevent line of sight with player

    private Coroutine _coroutineIdle;
    private Coroutine _coroutineWander;
    private Coroutine _coroutineSearch;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _agent.speed = RunSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _timeSinceLastAttack = Time.time;
        _animator.speed = AnimationSpeed;

        // find floor here, might be bad for performance (should use singleton game manager instead)
        _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<SpriteRenderer>();


        _maskBlockable = LayerMask.GetMask("Blockable");


        SetState(KidState.IDLE);

        Player.OnPlayerEscapingHug += OnPlayerEscapingHug;
    }

    private void OnDisable()
    {
        Player.OnPlayerEscapingHug -= OnPlayerEscapingHug;
    }

    private void Update()
    {
        HandleSpriteFlipping();
    }

    private void SetState(KidState state)
    {
        _state = state;

        //Debug.Log("Kid is: " + state.ToString());

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

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag(PLAYER_TAG))
        {
            _positionPlayerLastSeen = collider.transform.position;

            if (_state != KidState.STUNNED)
            {
                if (IsClose(collider) && !HasAttackedRecently())
                {
                    Attack();
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
        if (collider.CompareTag(PLAYER_TAG))
        {
            SetState(KidState.IDLE);
        }
    }

    #endregion


    #region Actions

    protected virtual void Attack()
    {
        OnKidAttacking?.Invoke(BASE_HUG_DAMAGE, UnityEngine.Random.Range(BASE_HUG_STRENGTH - 3, BASE_HUG_STRENGTH + 3));
        _timeSinceLastAttack = Time.time;
    }

    #endregion


    #region Bool Checks

    private bool HasLineOfSight(Collider2D colliderPlayer)
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


    private bool IsClose(Collider2D collider)
    {
        return Vector2.Distance(transform.position, collider.transform.position) <= 1f;
    }

    private bool HasAttackedRecently()
    {
        return Time.time - _timeSinceLastAttack < TIME_BETWEEN_HUGS;
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

        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(BASE_IDLE_DURATION - 3, BASE_IDLE_DURATION + 3));

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
