using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GothKid : Kid
{
    protected override void Start()
    {
        base.Start();

        type = KidType.GOTH;
    }
}
