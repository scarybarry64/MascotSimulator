using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugarKid : Kid
{


    private Quaternion rotationDefault;


    private bool doIdleSpinning;


    public Vector3 test;


    protected override void Start()
    {
        base.Start();

        type = KidType.SUGAR;

        RunSpeed = 20f;
        AnimationSpeed = 10f;

        IdleDuration = BASE_IDLE_DURATION * 0.5f;


        _agent.speed = RunSpeed;
        _animator.speed = AnimationSpeed;


        rotationDefault = transform.rotation;


        Debug.Log("Sugar Kid subclass Start is called");
    }



    protected override void Update()
    {
        base.Update();


        if (doIdleSpinning)
        {
            transform.Rotate(100f * Time.deltaTime * test);
        }

    }


    // maybe she will occasioanlly dart directly at player?

    protected override void SetAIState(KidAIState state)
    {
        if (IsAIState(state) || !gameObject.activeSelf)
        {
            return;
        }


        Debug.Log("Setting sugar to: " + state);


        base.SetAIState(state);

        if (IsAIState(KidAIState.IDLE))
        {
            doIdleSpinning = true;
        }
        else
        {
            doIdleSpinning = false;
            transform.rotation = rotationDefault;
        }
    }

    // There's a 5% chance she will dart directly towards player
    protected override Vector2 CalculateRandomLevelLocation()
    {
        float x;
        float y;

        if (UnityEngine.Random.Range(0f, 1f) < 0.95f)
        {
            x = UnityEngine.Random.Range(_floor.bounds.min.x, _floor.bounds.max.x);
            y = UnityEngine.Random.Range(_floor.bounds.min.y, _floor.bounds.max.y);
        }
        else
        {
            Debug.Log("Goodbye player");
            
            x = GameManager.Instance.Player.transform.position.x;
            y = GameManager.Instance.Player.transform.position.y;
        }
        
        return new Vector2(x, y);
    }



    protected override float CalculateIdleDuration()
    {
        // 75% chance to pick a smaller value
        if (UnityEngine.Random.Range(0f, 1f) < 0.75f)
        {
            return UnityEngine.Random.Range(0.1f, 1f);
        }
        else
        {
            return UnityEngine.Random.Range(1f, 4f);
        }
    }
}
