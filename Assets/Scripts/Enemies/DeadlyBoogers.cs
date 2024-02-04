using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyBoogers : MonoBehaviour
{
    [HideInInspector] public Vector2 Direction;

    private float startTime;

    private void OnEnable()
    {
        startTime = Time.time;
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)Direction * GrossKid.BOOGER_SPEED * Time.fixedDeltaTime;

        if (Time.time - startTime > GrossKid.BOOGER_DURATION)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(GameManager.PLAYER_TAG))
        {
            Events.OnPlayerTakingDamage.Invoke(KidType.GROSS, GrossKid.BOOGER_DAMAGE, 3);
        }

        Destroy(gameObject);
    }
}
