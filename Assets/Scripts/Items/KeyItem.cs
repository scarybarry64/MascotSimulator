using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : Item
{
    public override void Use()
    {
        Debug.Log("Using key");
    }

    // each key has enum that specifies which door?
}
