using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TissuesProjectile : Projectile
{
    // spawned by TissuesItem



    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        // blocked by boggers, blockable


        //if (collider.CompareTag(CollisionTags.PROJECTILE_DEADLY_BOOGERS))
        //{
        //    // destroy itself
            
        //    //Destroy(collider.gameObject);
        //    //Destroy(gameObject);
        //}




    }


}
