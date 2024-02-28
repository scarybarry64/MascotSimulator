using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyBoogersProjectile : Projectile
{
    private void Awake()
    {
        Speed = GrossKid.BOOGER_SPEED;
        Lifetime = GrossKid.BOOGER_DURATION;
    }


    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag(CollisionTags.PLAYER))
        {
            Events.OnKidAttacking.Invoke(KidType.GROSS, GrossKid.BOOGER_DAMAGE, 3);
        }

        Destroy(gameObject);
    }
}
