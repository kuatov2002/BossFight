using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    private CharacterController _controller;
    private Vector3 _moveDirection;
    private bool _isDashing = false;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void HandleMovement(Vector3 input)
    {
        // Если не в процессе дэша, обрабатываем обычное движение
        if (!_isDashing)
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
            _moveDirection.x = move.x * moveSpeed;
            _moveDirection.z = move.z * moveSpeed;
        }

        ApplyGravity();
        _controller.Move(_moveDirection * Time.deltaTime);
    }

    public void Jump()
    {
        if (_controller.isGrounded)
        {
            _moveDirection.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (!_controller.isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (_moveDirection.y < 0)
        {
            _moveDirection.y = -0.1f;
        }
    }

    // Методы для управления дэшем
    public void StartDash(Vector3 dashVelocity)
    {
        _moveDirection = dashVelocity;
        _isDashing = true;
    }

    public void EndDash()
    {
        _isDashing = false;
    }

    public bool IsGrounded() => _controller.isGrounded;
    public Vector3 GetMoveDirection() => _moveDirection;
}