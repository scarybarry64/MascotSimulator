using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GothKid : Kid
{
    public const int GOTH_KID_MAX_AGGRO = 3;
    public const float GOTH_KID_MIN_TELEPORT_DISTANCE = 15f;

    private int countAggro = 0;


    private bool doingTeleportSequence;


    protected override void Start()
    {
        base.Start();

        type = KidType.GOTH;



        AttackDamage = 100;
        RunSpeed = 20f;
        AnimationSpeed = 10f;


        _agent.speed = RunSpeed;
        _animator.speed = AnimationSpeed;



    }


    // only ever IDLE, HUNTING, or insta killing

    // if player enters detection and has LOS, she teleports away

    // when teleporting: disable her, find random spot and check if far away, if not then yield

    // on third time player bugs her, she lunges at player


    protected override void OnTriggerStay2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case CollisionTags.PLAYER:

                if (IsPlayerWithinLineOfSight() && !doingTeleportSequence && !IsAIState(KidAIState.HUNTING) && !IsAIState(KidAIState.HUG_ATTACKING))
                {
                    ++countAggro;


                    if (countAggro < GOTH_KID_MAX_AGGRO)
                    {
                        StartCoroutine(TeleportationCoroutine());
                    }
                    else
                    {
                        SetAIState(KidAIState.HUNTING);
                    }
                }
                return;
        }
    }


    protected override IEnumerator IdleCoroutine()
    {
        StopMovement();

        yield return null;

        _coroutineIdleAI = null;
    }


    protected override IEnumerator HuntingAICoroutine()
    {
        MoveToDestination(GameManager.Instance.Player.transform.position);

        while (!IsPlayerWithinMeleeRange())
        {
            yield return new WaitForFixedUpdate();

            _agent.SetDestination(GameManager.Instance.Player.transform.position);
        }

        _coroutineHuntingAI = null;
        SetAIState(KidAIState.HUG_ATTACKING);
    }




    private IEnumerator TeleportationCoroutine()
    {
        doingTeleportSequence = true;
        _collider.enabled = false;
        _colliderAIPlayerDetection.enabled = false;
        _renderer.enabled = false;
        Vector2 positionTeleport;

        while (true)
        {
            yield return null;

            positionTeleport = CalculateRandomLevelLocation();
            if (Vector2.Distance(positionTeleport, GameManager.Instance.Player.transform.position) >= GOTH_KID_MIN_TELEPORT_DISTANCE)
            {
                break;
            }
        }

        transform.position = positionTeleport;
        _collider.enabled = true;
        _colliderAIPlayerDetection.enabled = true;
        _renderer.enabled = true;
        doingTeleportSequence = false;
    }

}
