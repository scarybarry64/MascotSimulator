using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Modified from: https://www.youtube.com/watch?v=RPhTEJw6KbI

#region Events
public static class Events
{
    
    public static readonly Event<KidType, int, int> OnKidAttacking = new();
    public static readonly Event OnPlayerEscapingHug = new();
    public static readonly Event<bool> OnPrincessCommanding = new();

}
#endregion

#region Event
public class Event
{
    private event Action Action = delegate { };

    public void Invoke()
    {
        Action?.Invoke();
    }

    public void Subscribe(Action listener)
    {
        Action -= listener;
        Action += listener;
    }

    public void Unsubscribe(Action listener)
    {
        Action -= listener;
    }
}

public class Event<T>
{
    private event Action<T> Action = delegate { };

    public void Invoke(T parameter)
    {
        Action?.Invoke(parameter);
    }

    public void Subscribe(Action<T> listener)
    {
        Action -= listener;
        Action += listener;
    }

    public void Unsubscribe(Action<T> listener)
    {
        Action -= listener;
    }
}

public class Event<T1, T2>
{
    private event Action<T1, T2> Action = delegate { };

    public void Invoke(T1 parameter1, T2 parameter2)
    {
        Action?.Invoke(parameter1, parameter2);
    }

    public void Subscribe(Action<T1, T2> listener)
    {
        Action -= listener;
        Action += listener;
    }

    public void Unsubscribe(Action<T1, T2> listener)
    {
        Action -= listener;
    }
}

public class Event<T1, T2, T3>
{
    private event Action<T1, T2, T3> Action = delegate { };

    public void Invoke(T1 parameter1, T2 parameter2, T3 parameter3)
    {
        Action?.Invoke(parameter1, parameter2, parameter3);
    }

    public void Subscribe(Action<T1, T2, T3> listener)
    {
        Action -= listener;
        Action += listener;
    }

    public void Unsubscribe(Action<T1, T2, T3> listener)
    {
        Action -= listener;
    }
}
#endregion