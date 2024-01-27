using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Kid : MonoBehaviour
{
    public static event Action<int> onKidAttacking;
    
    protected enum State
    {
        IDLE,
        SEARCHING,
        HUNTING,
        ATTACKING
    }



    public LayerMask mask;




    private const float SPEED = 3f;
    private const int DAMAGE = 25;
    private const float TIME_BETWEEN_ATTACKS = 1f;

    private const string PLAYER_TAG = "Player";



    protected State _state;
    protected NavMeshAgent _agent;



    private Vector2 _positionPlayerLastSeen;
    private float timeSinceLastAttack;



    // event is enemy attacks player
    // OnKidAttacking

    // kid invokes this in their attack function
    // player subscribes their recieve function to this


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        _agent.speed = SPEED;


        timeSinceLastAttack = Time.time;


        SetState(State.IDLE);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag(PLAYER_TAG))
        {
            _positionPlayerLastSeen = collider.transform.position;

            if (IsClose(collider) && !HasAttackedRecently())
            {
                Attack();
            }
            
            if (HasLineOfSight(collider))
            {
                SetState(State.HUNTING);
            }
            else if (_state == State.HUNTING)
            {
                SetState(State.SEARCHING);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag(PLAYER_TAG))
        {
            SetState(State.IDLE);
        }
    }


    private void SetState(State state)
    {
        _state = state;

        Debug.Log("Kid is: " + state.ToString());

        switch (state)
        {
            case State.IDLE:

                _agent.isStopped = true;
                return;

            case State.HUNTING:
            case State.SEARCHING:

                _agent.SetDestination(_positionPlayerLastSeen);
                _agent.isStopped = false;
                return;
        }
    }

    protected virtual void Attack()
    {
        onKidAttacking?.Invoke(DAMAGE);
        timeSinceLastAttack = Time.time;
    }


    // patrol behavior



    private bool HasLineOfSight(Collider2D colliderPlayer)
    {
        return !Physics2D.CircleCast(transform.position, 0.5f, colliderPlayer.transform.position - transform.position, 100f, mask).collider;
    }


    private bool IsClose(Collider2D collider)
    {
        return Vector2.Distance(transform.position, collider.transform.position) <= 1f;
    }

    private bool HasAttackedRecently()
    {
        return Time.time - timeSinceLastAttack < TIME_BETWEEN_ATTACKS;
    }
}
