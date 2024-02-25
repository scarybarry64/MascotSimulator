using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityDoor : MonoBehaviour
{
    [SerializeField] private EmployeeSecurityLevel securityLevel;

    private BoxCollider2D _collider;
    private Animator _animator;

    private bool _isLocked;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        
        Events.OnEmployeeCardUpgraded.Subscribe(OnEmployeeCardUpgraded);

        SetLocked(true);
    }

    private void OnDisable()
    {
        Events.OnEmployeeCardUpgraded.Unsubscribe(OnEmployeeCardUpgraded);
    }


    private void SetLocked(bool isLocked)
    {
        _isLocked = isLocked;
        _collider.enabled = isLocked;

        if (!isLocked)
        {
            _animator.SetTrigger("isUnlocked");
        }
    }


    private void OnEmployeeCardUpgraded(EmployeeSecurityLevel cardLevel)
    {
        if (cardLevel >= securityLevel && _isLocked)
        {
            SetLocked(false);

            Debug.Log("Door unlocked");
        }
    }
}
