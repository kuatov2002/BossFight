using System.Collections;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public int jumpCount = 1; // Максимальное количество прыжков
    public float gravity = 20f;
    
    [SerializeField] private Transform followTarget;
    [SerializeField] private Rope webRope;
    
    private CharacterController _controller;
    private Vector3 _moveDirection;
    private bool _isDashing = false;
    private Vector2 _look;
    private int _currentJumpCount; // Текущее количество доступных прыжков
    private bool _canShootWeb = true;
    private bool _isWebDashing = false;
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _currentJumpCount = jumpCount; // Инициализируем количество прыжков
    }

    private void Update()
    {
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.F))
        {
            ShootWeb();
        }
        
        // Пример вызова прыжка (можно убрать, если вызывается из другого скрипта)
        // if (Input.GetButtonDown("Jump"))
        //     Jump();
    }

    public void HandleMovement(Vector3 input)
    {
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

    private void HandleMouseLook()
    {
        _look.x = Input.GetAxis("Mouse X");
        _look.y = -Input.GetAxis("Mouse Y");

        followTarget.rotation *= Quaternion.AngleAxis(_look.x, Vector3.up);
        followTarget.rotation *= Quaternion.AngleAxis(_look.y, Vector3.right);

        var angles = followTarget.localEulerAngles;
        angles.z = 0;

        var angle = followTarget.localEulerAngles.x;
        if (angle is > 180 and < 300)
        {
            angles.x = 300;
        }
        else if (angle is < 180 and > 70)
        {
            angles.x = 70;
        }

        followTarget.localEulerAngles = angles;
        transform.rotation = Quaternion.Euler(0, followTarget.rotation.eulerAngles.y, 0);
        followTarget.localEulerAngles = new Vector3(angles.x, 0, 0);
    }

    public void Jump()
    {
        if (_currentJumpCount > 0)
        {
            _moveDirection.y = jumpForce;
            _currentJumpCount--;
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
            _currentJumpCount = jumpCount; // Восстанавливаем количество прыжков при касании земли
        }
    }

    public void StartDash(Vector3 dashVelocity)
    {
        _moveDirection = dashVelocity;
        _isDashing = true;
    }

    public void EndDash()
    {
        _isDashing = false;
    }

    public void EnableWebShooting(bool enable)
    {
        _canShootWeb = enable;
        if (!enable && _isWebDashing)
        {
            StopWebDash();
        }
        Debug.Log($"Web shooting: {(enable ? "Enabled" : "Disabled")}");
    }

    private void ShootWeb()
    {
        Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 100f))
        {
            Vector3 directionToHit = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);

            // Скорость постоянная, время зависит от расстояния
            float webSpeed = 30f;
            float travelTime = distance / webSpeed;
            travelTime = travelTime > 0.2f ? travelTime : 0.2f;
            Vector3 impulse = directionToHit * webSpeed;
            StartDash(impulse);

            // --- Создаём пустой объект в точке попадания ---
            GameObject emptyObject = new GameObject("WebTarget");
            emptyObject.transform.position = hit.point;

            // --- Назначаем этот объект как конечную точку верёвки ---
            webRope.SetEndPoint(emptyObject.transform);
            webRope.ropeLength = distance;
            _isWebDashing = true;

            Invoke(nameof(StopWebDash), travelTime);
        }
    }
    private void StopWebDash()
    {
        if (_isWebDashing)
        {
            // Плавное торможение вместо резкой остановки
            StartCoroutine(SmoothStopWebDash());
            CancelInvoke(nameof(StopWebDash));
        }
    }

    private IEnumerator SmoothStopWebDash()
    {
        Vector3 initialVelocity = _moveDirection;
        float smoothTime = 0.3f;
        float elapsedTime = 0f;
    
        while (elapsedTime < smoothTime)
        {
            _moveDirection = Vector3.Lerp(initialVelocity, Vector3.zero, 
                elapsedTime / smoothTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    
        EndDash();
        _isWebDashing = false;
        webRope.SetEndPoint(null);
    }

    public bool IsGrounded() => _controller.isGrounded;
    public Vector3 GetMoveDirection() => _moveDirection;
}