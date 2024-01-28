using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Kid : MonoBehaviour
{
    public static event Action<int, int> OnKidAttacking;
    
    private enum KidState
    {
        IDLE,
        SEARCHING,
        HUNTING,
        STUNNED
    }

    [SerializeField] private LayerMask _maskBlockable;

    private const float BASE_RUN_SPEED = 3f;
    private const int BASE_HUG_DAMAGE = 25;
    private const int BASE_HUG_STRENGTH = 5;
    private const float TIME_BETWEEN_HUGS = 1f;
    private const float STUN_DURATION = 2f;
    private const string PLAYER_TAG = "Player";

    private KidState _state;
    private NavMeshAgent _agent;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private Vector2 _positionPlayerLastSeen;
    private float _timeSinceLastAttack;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _agent.speed = BASE_RUN_SPEED;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _timeSinceLastAttack = Time.time;

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

                _agent.isStopped = true;
                _animator.SetBool("isMoving", false);
                return;

            case KidState.HUNTING:
            case KidState.SEARCHING:

                _agent.SetDestination(_positionPlayerLastSeen);
                _agent.isStopped = false;
                _animator.SetBool("isMoving", true);
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

    #endregion


    #region Misc

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
