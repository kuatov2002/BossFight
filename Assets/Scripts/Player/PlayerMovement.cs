using System.Collections;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public int jumpCount = 2;
    public float gravity = 20f;

    [SerializeField] private Transform followTarget;
    [SerializeField] private Animator animator;

    [Header("Spider Settings")]
    [SerializeField] private Rope webRope;
    public bool canShootWeb = true;

    private CharacterController _controller;
    private Vector3 _moveDirection;
    private bool _isDashing = false;
    private Vector2 _look;
    private int _currentJumpCount;
    private bool _isWebDashing = false;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        _currentJumpCount = jumpCount;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.F))
        {
            ShootWeb();
        }

        // Передача параметров в аниматор
        animator.SetFloat("Speed", new Vector3(_moveDirection.x, 0, _moveDirection.z).magnitude);
        animator.SetBool("IsGrounded", _controller.isGrounded);
        animator.SetFloat("VerticalVelocity", _moveDirection.y);
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

    public void Jump()
    {
        if (_currentJumpCount > 0)
        {
            _moveDirection.y = jumpForce;
            _currentJumpCount--;
            animator.SetTrigger("Jump"); // Запуск анимации прыжка
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
            _currentJumpCount = jumpCount;
        }
    }

    public void StartDash(Vector3 dashVelocity)
    {
        _moveDirection = dashVelocity;
        _isDashing = true;
        animator.SetBool("IsDashing", true);
    }

    public void EndDash()
    {
        _isDashing = false;
        animator.SetBool("IsDashing", false);
    }

    private void ShootWeb()
    {
        Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 100f))
        {
            Vector3 directionToHit = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);

            float webSpeed = 30f;
            float travelTime = distance / webSpeed;
            travelTime = travelTime > 0.2f ? travelTime : 0.2f;
            Vector3 impulse = directionToHit * webSpeed;
            StartDash(impulse);
            webRope.RecalculateRope();

            GameObject emptyObject = new GameObject("WebTarget");
            emptyObject.transform.position = hit.point;

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
            StartCoroutine(SmoothStopWebDash());
            CancelInvoke(nameof(StopWebDash));
        }
    }
    private void HandleMouseLook()
    {//
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Вращаем тело игрока по оси Y
        transform.Rotate(Vector3.up * mouseX);

        // Вращаем камеру (followTarget) по оси X
        followTarget.Rotate(Vector3.left * mouseY);

        // Ограничиваем угол наклона камеры (чтобы не переворачивалась)
        Vector3 currentRotation = followTarget.localEulerAngles;
        if (currentRotation.x > 180) currentRotation.x -= 360;
        currentRotation.x = Mathf.Clamp(currentRotation.x, -70f, 70f);
        followTarget.localEulerAngles = currentRotation;

        // Обнуляем Z-вращение (чтобы не кренилась)
        followTarget.localEulerAngles = new Vector3(followTarget.localEulerAngles.x, followTarget.localEulerAngles.y, 0);
    }
    private IEnumerator SmoothStopWebDash()
    {
        Vector3 initialVelocity = _moveDirection;
        float smoothTime = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < smoothTime)
        {
            _moveDirection = Vector3.Lerp(initialVelocity, Vector3.zero, elapsedTime / smoothTime);
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