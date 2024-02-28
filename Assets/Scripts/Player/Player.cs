using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    //public static event Action OnPlayerEscapingHug;

    [SerializeField] private float MoveSpeed = 6f;
    [SerializeField] private float AnimationSpeed = 1.5f;

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

            if (_exhaustion == 100)
            {
                Die();
            }
        }
    }

    private PlayerInput _input;
    private Rigidbody2D _body;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private Slider _sliderExhaustionBar;
    private GameObject _escapeButton;
    private DialogueManager _dialogue_manager;
    private bool _isBeingAttacked;
    private int _escapeValue;
    private int _kidAttackStrength;

    private bool inBoogers;
    private Color colorDefault;



    private KidType typeKidAttacking = KidType.BASE;


    private EmployeeSecurityLevel cardLevel = EmployeeSecurityLevel.NONE;


    public Item ItemHeld { get; private set; }




    private void Start()
    {
        _input = new PlayerInput();
        _body = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _sliderExhaustionBar = GameObject.FindGameObjectWithTag("ExhaustionBar").GetComponent<Slider>();
        _escapeButton = GameObject.FindGameObjectWithTag("EscapeButton");
        _escapeButton.SetActive(false);

        GameObject dialogue = GameObject.Find("DialogueManager");
        if (dialogue)
        {
            _dialogue_manager = dialogue.GetComponent<DialogueManager>();
        }

        _input.Enable();
        _animator.speed = AnimationSpeed;
        colorDefault = _renderer.color;

        _input.Player.Move.performed += OnMovementPressed;
        _input.Player.Move.canceled += OnMovementCancelled;
        _input.Player.Action.performed += OnActionPressed;

        ItemHeld = null;

        Events.OnKidAttacking.Subscribe(OnKidAttacking);
        Events.OnEmployeeCardUpgraded.Subscribe(OnEmployeeCardUpgraded);
        Events.OnHealthReplenished.Subscribe(OnHealthReplenished);
    }

    private void OnDisable()
    {
        _input.Disable();
        _input.Player.Move.performed -= OnMovementPressed;
        _input.Player.Move.canceled -= OnMovementCancelled;
        _input.Player.Action.performed -= OnActionPressed;


        Events.OnKidAttacking.Unsubscribe(OnKidAttacking);
        Events.OnEmployeeCardUpgraded.Subscribe(OnEmployeeCardUpgraded);
        Events.OnHealthReplenished.Unsubscribe(OnHealthReplenished);
    }


    private void Update()
    {
        _sliderExhaustionBar.value = Exhaustion;


        //Vector3 positionMouseScreen = Mouse.current.position.ReadValue();


        //positionMouseScreen.z = 1;

        //Vector3 positionMouseWorld = Camera.main.ScreenToWorldPoint(positionMouseScreen);


        //Debug.DrawRay(positionMouseWorld, Vector2.left * 0.1f, Color.red);

        //Debug.Log("Pos: " + positionMouseWorld);
        //Debug.Log("Player: " + transform.position);

    }



    private void OnTriggerEnter2D(Collider2D collider)
    {

        switch (collider.tag)
        {
            case CollisionTags.ITEM_EMPLOYEE_CARD_UPGRADE:
            case CollisionTags.ITEM_PIZZA:

                UseItemInstantly(collider);
                return;

            case CollisionTags.ITEM_SODA:
            case CollisionTags.ITEM_TISSUES:
            case CollisionTags.ITEM_CANDY:

                PickupItem(collider);
                return;

            case CollisionTags.DOOR_EXIT:

                GameManager.Instance.WinGame();
                return;
        }
    }

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

    public void StopMovement()
    {
        _body.velocity = Vector2.zero;
        _animator.SetBool("isMoving", false);
    }


    // pick a random value, the "hug strength"
    // every time the player hits a button, increment a "escape value"
    // if escape value >= hug strength, break free

    // multiple kids can attack player at same time
    // breaking free causes all attackers and nearby kids to be stunned
    private void EscapeHug()
    {
        ++_escapeValue;

        if (_escapeValue >= _kidAttackStrength)
        {
            _isBeingAttacked = false;
            _escapeButton.SetActive(false);
            Events.OnPlayerEscapingHug.Invoke();

            if (inBoogers)
            {
                SetBoogerVisuals(false);
            }
        }
    }



    private void PickupItem(Collider2D colliderItem)
    {





        ItemHeld = colliderItem.GetComponent<Item>();
        colliderItem.gameObject.SetActive(false);
        colliderItem.transform.SetParent(transform);
        colliderItem.transform.position = transform.position;


    }


    private void UseHeldItem()
    {
        if (ItemHeld != null)
        {
            ItemHeld.Use();
            ItemHeld = null;
        }
    }


    private void UseItemInstantly(Collider2D colliderItem)
    {
        // use and destory without inventory management

        colliderItem.GetComponent<Item>().Use();

        Destroy(colliderItem.gameObject);
    }



    // game over screen determined by type of kid that kills player
    private void Die()
    {
        GameManager.Instance.DoGameOver(typeKidAttacking);
    }

    #endregion


    #region Input Listeners

    private void OnMovementPressed(InputAction.CallbackContext data)
    {
        if (!_isBeingAttacked && _dialogue_manager.IsDialogueFinished())
        {
            Move(data.ReadValue<Vector2>());
        }
    }


    private void OnMovementCancelled(InputAction.CallbackContext ignore)
    {
        StopMovement();
    }

    // clean this up somehow
    private void OnActionPressed(InputAction.CallbackContext ignore)
    {
        if (_isBeingAttacked)
        {
            EscapeHug();
        }
        else if (!_dialogue_manager.IsDialogueOpen())
        {
            UseHeldItem();
        }
        else
        {
            _dialogue_manager.NextDialogue();
        }
    }

    #endregion


    #region Event Listeners

    private void OnKidAttacking(KidType type, int damage, int strength)
    {
        typeKidAttacking = type; // needs to happen before taking damage, game over screen dependent on this
        Exhaustion += damage;

        if (!_isBeingAttacked)
        {
            StopMovement();
            _isBeingAttacked = true;
            _kidAttackStrength = strength;

            _escapeValue = 0;
            _escapeButton.SetActive(true);
        }

        if (type == KidType.GROSS)
        {
            SetBoogerVisuals(true);
        }
    }


    private void OnHealthReplenished(int amount)
    {
        Exhaustion -= amount;
    }

    
    private void OnEmployeeCardUpgraded(EmployeeSecurityLevel level)
    {
        if (cardLevel < level)
        {
            cardLevel = level;

            Debug.Log("Card upgraded to: " + cardLevel);
        }
    }


    #endregion


    #region Misc

    private void SetBoogerVisuals(bool isEnabled)
    {
        if (isEnabled)
        {
            inBoogers = true;
            Color colorGreenTint = colorDefault;
            colorGreenTint.b = 0.5f;
            colorGreenTint.r = 0.5f;
            _renderer.color = colorGreenTint;
        }
        else
        {
            inBoogers = false;
            _renderer.color = colorDefault;
        }
    }



    #endregion

}
