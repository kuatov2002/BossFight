using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public PlayerMovement movement;
    public PlayerDash dash;
    public PlayerAttack attack;

    private PlayerInputActions _playerControls;
    private Vector2 _moveInput;

    void Awake()
    {
        _playerControls = new PlayerInputActions();
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
        _playerControls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _playerControls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        _playerControls.Player.Jump.performed += ctx => movement.Jump();
        _playerControls.Player.Attack.performed += ctx => attack.PerformAttack();
        _playerControls.Player.Dash.performed += ctx => dash.PerformDash(new Vector3(_moveInput.x, 0, _moveInput.y));
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 moveVector = new Vector3(_moveInput.x, 0, _moveInput.y);
        movement.HandleMovement(moveVector);
    }

    void OnEnable()
    {
        _playerControls.Player.Enable();
    }

    void OnDisable()
    {
        _playerControls.Player.Disable();
    }
}