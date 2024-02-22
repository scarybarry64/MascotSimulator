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
}
