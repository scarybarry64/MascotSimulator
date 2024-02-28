using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmployeeSecurityLevel
{
    NONE,
    BLUE,
    GREEN,
    RED
}

public class EmployeeCardUpgradeItem : Item
{
    [SerializeField] EmployeeSecurityLevel _cardLevel;
    
    
    public override void Use()
    {
        Events.OnEmployeeCardUpgraded.Invoke(_cardLevel);
        Destroy(gameObject);
    }

}
