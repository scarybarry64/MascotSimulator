using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [HideInInspector] public float Speed = 0f;
    [HideInInspector] public Vector2 Direction = Vector2.zero;
    [HideInInspector] public float Lifetime = 0f;

    private float _timeStart;

    private void OnEnable()
    {
        _timeStart = Time.time;



        Debug.Log("Spawned: " + name);
        Debug.Log("At: " + transform.position);
    }

    protected virtual void FixedUpdate()
    {
        transform.position += Speed * Time.fixedDeltaTime * (Vector3)Direction;

        if (Time.time - _timeStart > Lifetime)
        {
            Destroy(gameObject);
        }
    }

    protected abstract void OnTriggerEnter2D(Collider2D collider);
}
