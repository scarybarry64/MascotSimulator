using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossKid : Kid
{
    public const int BOOGER_DAMAGE = BASE_HUG_DAMAGE * 4/3;
    public const float BOOGER_SPEED = 10f;
    public const float BOOGER_DURATION = 5f;

    private DeadlyBoogers _boogers;
    
    protected override void Start()
    {
        base.Start();

        type = KidType.GROSS;

        AttackSpeed = BASE_ATTACK_SPEED / 3f;

        _boogers = Resources.Load<DeadlyBoogers>("DeadlyBoogers");
    }

    protected override void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag(GameManager.PLAYER_TAG))
        {
            _positionPlayerLastSeen = collider.transform.position;

            if (_state != KidState.STUNNED)
            {
                if (!HasAttackedRecently())
                {
                    if (!IsClose(collider))
                    {
                        RangedAttack();
                    }
                    else
                    {
                        MeleeAttack();
                    }
                }
                else
                {
                    Debug.Log("Gross kid attacked recently");
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

    private void RangedAttack()
    {
        DeadlyBoogers boogers = Instantiate(_boogers, transform.position, Quaternion.identity);
        boogers.Direction = (_positionPlayerLastSeen - (Vector2)transform.position).normalized;
        _timeSinceLastAttack = Time.time;
    }
}
