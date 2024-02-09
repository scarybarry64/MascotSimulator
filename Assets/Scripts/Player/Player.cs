using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static event Action OnPlayerEscapingHug;

    [SerializeField] private float MoveSpeed = 8f;
    [SerializeField] private float AnimationSpeed = 1f;

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

    private PlayerInput _input;
    private Rigidbody2D _body;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private Slider _sliderExhaustionBar;
    private GameObject _escapeButton;
    private bool _isBeingAttacked;
    private int _escapeValue;
    private int _kidAttackStrength;

    private bool inBoogers;
    private Color colorDefault;

    private void Start()
    {
        _input = new PlayerInput();
        _body = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _sliderExhaustionBar = GameObject.FindGameObjectWithTag("ExhaustionBar").GetComponent<Slider>();
        _escapeButton = GameObject.FindGameObjectWithTag("EscapeButton");
        _escapeButton.SetActive(false);

        _input.Enable();
        _animator.speed = AnimationSpeed;
        colorDefault = _renderer.color;

        _input.Player.Move.performed += OnMovementPressed;
        _input.Player.Move.canceled += OnMovementCancelled;
        _input.Player.Action.performed += OnActionPressed;
        Events.OnPlayerTakingDamage.Subscribe(OnPlayerTakingDamage);
    }

    private void OnDisable()
    {
        _input.Disable();
        _input.Player.Move.performed -= OnMovementPressed;
        _input.Player.Move.canceled -= OnMovementCancelled;
        _input.Player.Action.performed -= OnActionPressed;
        Events.OnPlayerTakingDamage.Unsubscribe(OnPlayerTakingDamage);
    }

    private void Update()
    {
        _sliderExhaustionBar.value = Exhaustion;
    }



    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("ExitDoor"))
        {
            GameManager.instance.WinGame();
        }
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

    private void OnActionPressed(InputAction.CallbackContext ignore)
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
        _body.velocity = axis * MoveSpeed;
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

        if (_escapeValue >= _kidAttackStrength)
        {
            OnPlayerEscapingHug?.Invoke();
            _isBeingAttacked = false;
            _escapeButton.SetActive(false);


            if (inBoogers)
            {
                inBoogers = false;
                _renderer.color = colorDefault;
            }

        }
    }

    private void Die(KidType type)
    {
        GameManager.instance.DoGameOver(type);
    }


    #endregion


    #region Primary Event Recievers

    private void OnPlayerTakingDamage(KidType type, int hugDamage, int hugStrength)
    {
        if (!_isBeingAttacked)
        {
            Stop();
            _isBeingAttacked = true;
            _kidAttackStrength = hugStrength;
            _escapeValue = 0;


            _escapeButton.SetActive(true);
        }


        Exhaustion += hugDamage;


        // do booger visuals here
        if (type == KidType.GROSS)
        {
            Debug.Log("Do boogers");
            
            inBoogers = true;
            Color colorGreenTint = colorDefault;
            colorGreenTint.b = 0.5f;
            colorGreenTint.r = 0.5f;
            _renderer.color = colorGreenTint;
        }


        if (Exhaustion >= 100)
        {
            Die(type);
        }



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
