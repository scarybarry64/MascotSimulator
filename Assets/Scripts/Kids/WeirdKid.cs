using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeirdKid : Kid
{
    protected override void Start()
    {
        base.Start();

        type = KidType.WEIRD;


        AttackDamage = 100;
        RunSpeed = 1f;
        AnimationSpeed = 0.5f;


        _agent.speed = RunSpeed;
        _animator.speed = AnimationSpeed;

        Debug.Log("Weird Kid subclass Start is called");
    }


    // always follow player
    protected override void OnTriggerStay2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case CollisionTags.PLAYER:

                if (_state != KidAIState.STUNNED)
                {
                    _positionPlayerLastSeen = GameManager.Instance.Player.transform.position;

                    if (IsPlayerWithinMeleeRange() && !IsAIState(KidAIState.HUG_ATTACKING))
                    {
                        SetAIState(KidAIState.HUG_ATTACKING);
                    }
                    else if (!IsPlayerWithinMeleeRange())
                    {
                        SetAIState(KidAIState.HUNTING);
                    }
                }
                return;
        }
    }


}
