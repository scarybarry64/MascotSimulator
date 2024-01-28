using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Kid : MonoBehaviour
{
    public static event Action<int, int> onKidAttacking;
    
    protected enum KidState
    {
        IDLE,
        SEARCHING,
        HUNTING,
        STUNNED
    }



    public LayerMask mask;




    private const float RUN_SPEED = 3f;
    private const int HUG_DAMAGE = 25;
    private const int BASE_HUG_STRENGTH = 5;
    private const float TIME_BETWEEN_HUGS = 1f;
    private const float STUN_DURATION = 2f;

    private const string PLAYER_TAG = "Player";



    private KidState _state;
    private NavMeshAgent _agent;
    private SpriteRenderer _spriteRenderer;


    private Vector2 _positionPlayerLastSeen;
    private float timeSinceLastAttack;


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();


        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        _agent.speed = RUN_SPEED;


        timeSinceLastAttack = Time.time;


        SetState(KidState.IDLE);
    }


    private void OnEnable()
    {
        Player.onPlayerEscapingHug += OnPlayerEscapingHug;
    }


    private void OnDisable()
    {
        Player.onPlayerEscapingHug -= OnPlayerEscapingHug;
    }


    private void SetState(KidState state)
    {
        _state = state;

        Debug.Log("Kid is: " + state.ToString());

        switch (state)
        {
            case KidState.IDLE:

                _agent.isStopped = true;
                return;

            case KidState.HUNTING:
            case KidState.SEARCHING:

                _agent.SetDestination(_positionPlayerLastSeen);
                _agent.isStopped = false;
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
        onKidAttacking?.Invoke(HUG_DAMAGE, UnityEngine.Random.Range(BASE_HUG_STRENGTH - 3, BASE_HUG_STRENGTH + 3));
        timeSinceLastAttack = Time.time;
    }

    #endregion


    #region Bool Checks

    private bool HasLineOfSight(Collider2D colliderPlayer)
    {
        return !Physics2D.CircleCast(transform.position, 0.5f, colliderPlayer.transform.position - transform.position,
            Vector2.Distance(transform.position, colliderPlayer.transform.position), mask);
    }


    private bool IsClose(Collider2D collider)
    {
        return Vector2.Distance(transform.position, collider.transform.position) <= 1f;
    }

    private bool HasAttackedRecently()
    {
        return Time.time - timeSinceLastAttack < TIME_BETWEEN_HUGS;
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

        Color colorDefault = _spriteRenderer.color;
        Color colorTransparent = _spriteRenderer.color;
        colorTransparent.a = 0.25f;
        _spriteRenderer.color = colorTransparent;


        yield return new WaitForSecondsRealtime(STUN_DURATION);

        _spriteRenderer.color = colorDefault;



        Debug.Log("Stun duration done");


        SetState(KidState.SEARCHING);
    }

    #endregion
}
