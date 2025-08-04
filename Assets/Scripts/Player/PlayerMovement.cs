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
    [SerializeField] private Transform geometry;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Header("Spider Settings")]
    [SerializeField] private Rope webRope;
    public bool canShootWeb = true;

    // --- Новое поле для силы отбрасывания ---
    [Header("Knockback Settings")]
    public float knockbackForce = 10f; // Сила отбрасывания
    public float knockbackDuration = 0.5f; // Длительность отбрасывания (необязательно)

    private CharacterController _controller;
    private Vector3 _moveDirection;
    private bool _isDashing = false;
    private Vector2 _look;
    private int _currentJumpCount;
    private bool _isWebDashing = false;
    private Vector3 _lastMoveInput; // Сохраняем последний вектор движения

    // --- Новое поле для состояния отбрасывания ---
    private bool _isKnockedback = false;
    private Vector3 _knockbackDirection; // Направление отбрасывания

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
        ApplyGravity();
        // --- Блокируем управление во время отбрасывания ---
        if (!_isDashing && !_isKnockedback)
        {
            HandleMouseLook();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ShootWeb();
        }

        // Передача параметров в аниматор
        // --- Также блокируем анимацию движения во время отбрасывания ---
        if (!_isKnockedback)
        {
            animator.SetFloat("Speed", new Vector3(_moveDirection.x, 0, _moveDirection.z).magnitude);
        }
        animator.SetBool("IsGrounded", _controller.isGrounded);
        animator.SetFloat("VerticalVelocity", _moveDirection.y);

        // Поворачиваем геометрию игрока
        // --- Блокируем поворот во время отбрасывания ---
        if (!_isKnockedback)
        {
            RotatePlayerGeometry();
        }
    }

    // --- Обновленный HandleMovement с проверкой на отбрасывание ---
    public void HandleMovement(Vector3 input)
    {
        // --- Не двигаем игрока, если он отбрасывается ---
        if (_isKnockedback) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Hit1") || stateInfo.IsName("Hit2") || stateInfo.IsName("Hit3"))
        {
            return;
        }

        // Сохраняем вектор ввода для поворота
        _lastMoveInput = input;

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
        
        _controller.Move(_moveDirection * Time.deltaTime);
    }


    private void RotatePlayerGeometry()
    {
        if (geometry == null || _isDashing) return;
        // Если нет движения - не поворачиваем
        if (_lastMoveInput.magnitude < 0.1f) return;
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        // Вычисляем направление движения
        Vector3 moveDirection = (forward * _lastMoveInput.z + right * _lastMoveInput.x).normalized;
        // Если движемся назад - смотрим на камеру
        if (_lastMoveInput.z < -0.1f && Mathf.Abs(_lastMoveInput.x) < 0.5f)
        {
            // Поворачиваем к камере (обратное направление камеры)
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            // Инвертируем направление, чтобы смотреть на камеру
            moveDirection = -cameraForward;
        }
        // Поворот только по оси Y
        if (moveDirection != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float currentAngle = geometry.eulerAngles.y;
            // Плавный поворот только по Y
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            geometry.eulerAngles = new Vector3(geometry.eulerAngles.x, newAngle, geometry.eulerAngles.z);
        }
    }

    public void Jump()
    {
         // --- Не позволяем прыгать во время отбрасывания ---
        if (_isKnockedback) return;

        if (_currentJumpCount > 0)
        {
            _moveDirection.y = jumpForce;
            _currentJumpCount--;
            animator.SetTrigger("Jump"); // Запуск анимации прыжка
        }
    }

    void ApplyGravity()
    {
        // --- Применяем гравитацию даже во время отбрасывания ---
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
        // --- Не позволяем начать дэш, если игрок отбрасывается ---
        if (_isKnockedback) return;

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
         // --- Не позволяем стрелять паутиной во время отбрасывания ---
        if (_isKnockedback) return;

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
    {
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

    // --- Обновлённый метод Knockback ---
public void Knockback(Vector3 knockDirection)
{
    // Нормализуем направление и устанавливаем его
    _knockbackDirection = knockDirection.normalized;
    _isKnockedback = true;
    StartCoroutine(KnockbackCoroutine());
}

// --- Обновлённая корутина KnockbackCoroutine ---
private IEnumerator KnockbackCoroutine()
{
    float elapsedTime = 0f;
    Vector3 knockbackVelocity = _knockbackDirection * knockbackForce; // Вычисляем вектор скорости отбрасывания

    // --- Добавляем силу отбрасывания к текущему движению, а не заменяем его ---
    _moveDirection += knockbackVelocity;

    while (elapsedTime < knockbackDuration)
    {
        // --- Применяем движение отбрасывания (гравитация всё ещё действует отдельно) ---
        // _moveDirection.y обрабатывается ApplyGravity, поэтому мы можем двигать только X и Z,
        // или доверить CharacterController обработку вертикального перемещения на этом этапе.
        // Для простоты, двигаем по всем осям, но ApplyGravity в Update по-прежнему будет влиять на _moveDirection.y.
         _controller.Move(_moveDirection * Time.deltaTime);

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    // --- По окончании отбрасывания постепенно уменьшаем горизонтальную скорость до нуля ---
    // Сохраняем вертикальную скорость (гравитация должна управлять ей)
    float originalYVelocity = _moveDirection.y;
    Vector3 horizontalVelocity = new Vector3(_moveDirection.x, 0, _moveDirection.z);
    float slowDownDuration = 0.2f; // Длительность затухания
    float elapsedSlowDownTime = 0f;

    while (elapsedSlowDownTime < slowDownDuration)
    {
        float t = elapsedSlowDownTime / slowDownDuration;
        Vector3 newHorizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, t);
        _moveDirection = new Vector3(newHorizontalVelocity.x, originalYVelocity, newHorizontalVelocity.z);
        // ApplyGravity в Update продолжает работать
        _controller.Move(_moveDirection * Time.deltaTime);

        elapsedSlowDownTime += Time.deltaTime;
        yield return null;
    }

    // --- Убедимся, что горизонтальная скорость обнулена, а вертикальная остается под гравитацией ---
    _moveDirection = new Vector3(0, _moveDirection.y, 0); // Обнуляем только горизонтальную составляющую
    _isKnockedback = false;
}

    public bool IsGrounded() => _controller.isGrounded;
    public Vector3 GetMoveDirection() => _moveDirection;
}