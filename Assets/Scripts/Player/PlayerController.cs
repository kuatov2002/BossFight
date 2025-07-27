using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public PlayerMovement movement;
    public PlayerDash dash;
    public PlayerAttack attack;

    private PlayerInputActions playerControls;
    private Vector2 moveInput;

    void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();
        attack = GetComponent<PlayerAttack>();
        
        if (movement == null) Debug.LogError("PlayerMovement не найден!");
        if (dash == null) Debug.LogError("PlayerDash не найден!");
        if (attack == null) Debug.LogError("PlayerAttack не найден!");

        // Подписываемся на события ввода
        playerControls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerControls.Player.Jump.performed += ctx => movement.Jump();
        playerControls.Player.Attack.performed += ctx => attack.PerformAttack();
        playerControls.Player.Dash.performed += ctx => dash.PerformDash(new Vector3(moveInput.x, 0, moveInput.y));
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 moveVector = new Vector3(moveInput.x, 0, moveInput.y);
        movement.HandleMovement(moveVector);
    }

    void OnEnable()
    {
        playerControls.Player.Enable();
    }

    void OnDisable()
    {
        playerControls.Player.Disable();
    }
}