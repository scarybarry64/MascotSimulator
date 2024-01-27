using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private const float SPEED = 8f;

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
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        _input = new PlayerInput();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _input.Enable();
        _input.Player.Move.performed += OnMovementButtonPressed;
        _input.Player.Move.canceled += OnMovementButtonCancelled;
        Kid.onKidAttacking += OnKidAttacking;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMovementButtonPressed;
        _input.Player.Move.canceled -= OnMovementButtonCancelled;
        _input.Disable();
        Kid.onKidAttacking -= OnKidAttacking;
    }

    private void Update()
    {
        _slider.value = Exhaustion;
    }


    private void OnKidAttacking(int damage)
    {
        Exhaustion += damage;
    }



    #region Input
    // Called whenever the player presses the movement buttons (WASD / arrows)
    private void OnMovementButtonPressed(InputAction.CallbackContext data)
    {
        _body.velocity = data.ReadValue<Vector2>() * SPEED;
    }

    // Called whenever the player STOPS pressing the movement buttons (WASD / arrows)
    private void OnMovementButtonCancelled(InputAction.CallbackContext data)
    {
        _body.velocity = Vector2.zero;
    }
    #endregion
}
