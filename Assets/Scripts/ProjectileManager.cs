using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
    TISSUES,
    DEADLY_BOOGERS
}


public class ProjectileManager : MonoBehaviour
{

    private readonly Dictionary<ProjectileType, Projectile> tableProjectiles = new();

    private void Awake()
    {
        LoadProjectilePrefabs();
    }


    // Retrieves corresponding projectile prefab from table and instantiates it with given info
    public void SpawnProjectile(ProjectileType type, Transform parent, Vector2 position, float speed, Vector2 direction, float lifetime = 10f)
    {
        if (!tableProjectiles.ContainsKey(type))
        {
            Debug.LogError("ERROR: Projecitle type " + type + " does not exist.");
            return;
        }

        Projectile projectile = Instantiate(tableProjectiles[type], position, Quaternion.identity, parent);
        projectile.Speed = speed;
        projectile.Direction = direction;
        projectile.Lifetime = lifetime;
    }



    private void LoadProjectilePrefabs()
    {
        Projectile[] prefabs = Resources.LoadAll<Projectile>("Projectiles");

        foreach (Projectile prefab in prefabs)
        {

            if (!Enum.IsDefined(typeof(ProjectileType), prefab.name))
            {
                Debug.LogError("ERROR: Projecitle prefab " + prefab.name + " does not have a corresponding ProjectileType enum.");
                return;
            }

            tableProjectiles.Add((ProjectileType)Enum.Parse(typeof(ProjectileType), prefab.name), prefab);
        }
    }
}
