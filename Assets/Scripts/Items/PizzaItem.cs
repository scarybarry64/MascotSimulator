using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaItem : Item
{
    public const int PIZZA_HEALTH_AMOUNT = 50;


    public override void Use()
    {
        Debug.Log("Replenishing health");


        Events.OnHealthReplenished.Invoke(PIZZA_HEALTH_AMOUNT);
    }


}
