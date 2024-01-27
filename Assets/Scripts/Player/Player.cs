using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float _speed;
    
    private PlayerInput _input;
    private Rigidbody2D _body;

    private void Awake()
    {
        _input = new PlayerInput();
        _body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += OnMovementButtonPressed;
        _input.Player.Move.canceled += OnMovementButtonCancelled;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMovementButtonPressed;
        _input.Player.Move.canceled -= OnMovementButtonCancelled;
        _input.Disable();
    }

    // Called whenever the player presses the movement buttons (WASD / arrows)
    private void OnMovementButtonPressed(InputAction.CallbackContext data)
    {
        _body.velocity = data.ReadValue<Vector2>() * _speed;
    }

    // Called whenever the player STOPS pressing the movement buttons (WASD / arrows)
    private void OnMovementButtonCancelled(InputAction.CallbackContext data)
    {
        _body.velocity = Vector2.zero;
    }
}
