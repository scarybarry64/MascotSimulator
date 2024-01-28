using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossKid : Kid
{
    protected override void Start()
    {
        base.Start();

        type = KidType.GROSS;
    }
}
