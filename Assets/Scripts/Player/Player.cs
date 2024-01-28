using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public static event Action OnPlayerEscapingHug;

    public const float SPEED = 15f;

    private int _exhaustion;
    public int Exhaustion
    {
        get
        {
            return _exhaustion;
        }

        set
        {
            _exhaustion = Mathf.Clamp(value, 0, 100);
        }
    }

    [SerializeField] private Slider _slider;

    private PlayerInput _input;
    private Rigidbody2D _body;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private bool _isBeingAttacked;
    private int _escapeValue;
    private int _kidHugStrength;

    private void Start()
    {
        _input = new PlayerInput();
        _body = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _animator.speed = 1.75f;
        _input.Enable();

        _input.Player.Move.performed += OnMovementPressed;
        _input.Player.Move.canceled += OnMovementCancelled;
        _input.Player.EscapeHug.performed += OnEscapeHugPressed;
        Kid.OnKidAttacking += OnKidAttacking;
    }

    private void OnDisable()
    {
        _input.Disable();
        _input.Player.Move.performed -= OnMovementPressed;
        _input.Player.Move.canceled -= OnMovementCancelled;
        _input.Player.EscapeHug.performed -= OnEscapeHugPressed;
        Kid.OnKidAttacking -= OnKidAttacking;
    }

    private void Update()
    {
        _slider.value = Exhaustion;
    }


    #region Input

    private void OnMovementPressed(InputAction.CallbackContext data)
    {
        if (!_isBeingAttacked)
        {
            Move(data.ReadValue<Vector2>());
        }
    }


    private void OnMovementCancelled(InputAction.CallbackContext ignore)
    {
        Stop();
    }

    private void OnEscapeHugPressed(InputAction.CallbackContext ignore)
    {
        if (_isBeingAttacked)
        {
            EscapeHug();
        }
    }

    #endregion


    #region Primary Actions

    private void Move(Vector2 axis)
    {
        _body.velocity = axis * SPEED;
        _animator.SetBool("isMoving", true);

        if (_body.velocity.x > 0)
        {
            _renderer.flipX = true;
        }
        else if (_body.velocity.x < 0)
        {
            _renderer.flipX = false;
        }
    }

    private void Stop()
    {
        _body.velocity = Vector2.zero;
        _animator.SetBool("isMoving", false);
    }


    // pick a random value, the "hug strength"
    // every time the player hits a button, increment a "escape value"
    // if escape value >= hug strength, break free
    private void EscapeHug()
    {
        ++_escapeValue;

        if (_escapeValue >= _kidHugStrength)
        {
            OnPlayerEscapingHug?.Invoke();
            _isBeingAttacked = false;
        }
    }




    #endregion


    #region Primary Event Recievers

    private void OnKidAttacking(int hugDamage, int hugStrength)
    {
        if (!_isBeingAttacked)
        {
            Stop();
            _isBeingAttacked = true;
            _kidHugStrength = hugStrength;
            _escapeValue = 0;
        }


        Exhaustion += hugDamage;






        // 





        // start thingy here

        // doesn't happen if player is already being attacked

        // player cant move

        // player takes damage every second

        // player must button mash to escape

        // when escaped, kid is stunned for 0.5 seconds and cannot attack

        // repeat if player doesn't run away in time

    }

    #endregion
}
