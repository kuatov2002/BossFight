using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private PlayerMovement playerMovement;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (isDashing)
        {
            PerformDashMovement();
        }

        UpdateTimers();
    }

    public void PerformDash(Vector3 inputDirection)
    {
        if (!CanDash()) return;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Определяем направление дэша
        if (inputDirection != Vector3.zero)
        {
            dashDirection = inputDirection;
        }
        else
        {
            // Если нет направления, используем направление камеры
            dashDirection = Camera.main.transform.forward;
        }

        dashDirection.y = 0;
        dashDirection.Normalize();

        // Применяем силу дэша
        Vector3 dashVelocity = dashDirection * dashForce;
        dashVelocity.y = 0;
        
        playerMovement.StartDash(dashVelocity);
    }

    void PerformDashMovement()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
        {
            isDashing = false;
            playerMovement.EndDash();
        }
    }

    void UpdateTimers()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
    }

    public bool IsDashing() => isDashing;
    public bool CanDash() => dashCooldownTimer <= 0 && !isDashing;
    public float GetDashCooldownRemaining() => Mathf.Max(0, dashCooldownTimer);
}