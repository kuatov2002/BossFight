using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private PlayerMovement _playerMovement;
    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private Vector3 _dashDirection;

    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (_isDashing)
        {
            PerformDashMovement();
        }

        UpdateTimers();
    }

    public void PerformDash(Vector3 inputDirection)
    {
        if (!CanDash()) return;

        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;

        // Определяем направление дэша
        if (inputDirection != Vector3.zero)
        {
            _dashDirection = inputDirection;
        }
        else
        {
            // Если нет направления, используем направление камеры
            _dashDirection = Camera.main.transform.forward;
        }

        _dashDirection.y = 0;
        _dashDirection.Normalize();

        // Применяем силу дэша
        Vector3 dashVelocity = _dashDirection * dashForce;
        dashVelocity.y = 0;
        
        _playerMovement.StartDash(dashVelocity);
    }

    void PerformDashMovement()
    {
        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0)
        {
            _isDashing = false;
            _playerMovement.EndDash();
        }
    }

    void UpdateTimers()
    {
        if (_dashCooldownTimer > 0)
            _dashCooldownTimer -= Time.deltaTime;
    }

    public bool IsDashing() => _isDashing;
    public bool CanDash() => _dashCooldownTimer <= 0 && !_isDashing;
    public float GetDashCooldownRemaining() => Mathf.Max(0, _dashCooldownTimer);
}