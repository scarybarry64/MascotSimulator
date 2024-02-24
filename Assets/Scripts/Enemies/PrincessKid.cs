using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrincessKid : Kid
{
    protected override void Start()
    {
        base.Start();

        type = KidType.PRINCESS;
    }
}
