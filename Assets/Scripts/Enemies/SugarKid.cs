using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugarKid : Kid
{

    protected override void Start()
    {
        base.Start();


        type = KidType.SUGAR;

        RunSpeed = 20f;
        AnimationSpeed = 10f;

        IdleDuration = BASE_IDLE_DURATION * 0.5f;


        _agent.speed = RunSpeed;
        _animator.speed = AnimationSpeed;

        Debug.Log("Sugar Kid subclass Start is called");
    }


    //private void Update()
    //{
    //    Debug.Log("Sugar kid state: " + _state);
    //}
}
