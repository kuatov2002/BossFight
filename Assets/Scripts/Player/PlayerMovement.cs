using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool isDashing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void HandleMovement(Vector3 input)
    {
        // Если не в процессе дэша, обрабатываем обычное движение
        if (!isDashing)
        {
            if (input.magnitude > 1f)
                input.Normalize();

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 move = (forward * input.z + right * input.x).normalized;
            moveDirection.x = move.x * moveSpeed;
            moveDirection.z = move.z * moveSpeed;
        }

        ApplyGravity();
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void Jump()
    {
        if (controller.isGrounded)
        {
            moveDirection.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (moveDirection.y < 0)
        {
            moveDirection.y = -0.1f;
        }
    }

    // Методы для управления дэшем
    public void StartDash(Vector3 dashVelocity)
    {
        moveDirection = dashVelocity;
        isDashing = true;
    }

    public void EndDash()
    {
        isDashing = false;
    }

    public bool IsGrounded() => controller.isGrounded;
    public Vector3 GetMoveDirection() => moveDirection;
}