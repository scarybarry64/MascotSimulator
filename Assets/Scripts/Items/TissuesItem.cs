using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TissuesItem : Item
{
    
    public const float TISSUES_SPEED = 1.5f;


    private void OnTriggerEnter2D(Collider2D collider)
    {
        //
    }

    // Spawns a TissuesProjectile on the player, heading towards the mouse cursor
    public override void Use()
    {
        // Determine direction of mouse pointer
        Vector3 positionMouseScreen = Mouse.current.position.ReadValue();
        positionMouseScreen.z = -Camera.main.transform.position.z; // camera is perspective, must calculate distance to it (same as taking the negative of its position)
        Vector2 direction = Camera.main.ScreenToWorldPoint(positionMouseScreen) - GameManager.Instance.Player.transform.position;

        GameManager.Instance.Projectiles.SpawnProjectile(ProjectileType.TISSUES, null, GameManager.Instance.Player.transform.position, TISSUES_SPEED, direction);

        Destroy(gameObject);
    }
}
