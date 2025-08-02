using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLogic : MonoBehaviour
{
    public enum AttackType
    {
        Spin,
        WallAndShot,
        JustAttack
    }

    public Transform player;
    public Animator animator;
    public Door door;
    public float moveSpeed = 5f; // Скорость движения к игроку
    public float rotationSpeed = 180f; // Скорость вращения
    public float attackCooldown = 2f;
    
    // --- Новые поля для плавного перемещения ---
    public List<Transform> wayPoints = new List<Transform>(); // Список точек для перемещения
    public Transform defaultPosition;
    public float smoothMoveSpeed = 30f; // Скорость плавного перемещения
    public float smoothRotationSpeed = 50f; // Скорость плавного поворота
    public GameObject webPrefab;
    public Transform shootPoint; // Точка, из которой будет производиться выстрел
    public float returnToDefaultTime = 12f; // Время в секундах до возврата на позицию по умолчанию
    
    // --- Ссылка на скрипт игрока ---
    private PlayerMovement playerMovementScript;

    private float _lastAttack;
    private bool _hitWall = false; // Флаг столкновения со стеной
    private int _currentWayPointIndex = 0; // Индекс текущей точки назначения
    private bool _isAtWayPoint = false; // Флаг достижения точки
    private bool _isReturningToDefault = false; // Флаг возврата на позицию по умолчанию
    private Coroutine _moveCoroutine; // Ссылка на основную корутину
    private Coroutine _returnCoroutine; // Ссылка на корутину возврата

    void Start()
    {
        BossActions.onBossDied += Die;
        if (animator == null)
            animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // --- Получаем ссылку на скрипт игрока ---
        playerMovementScript = player.GetComponent<PlayerMovement>();
        if (playerMovementScript == null)
        {
            Debug.LogError("PlayerMovement script not found on the player object!");
        }

        //StartCoroutine(MoveTowardsPlayerRoutine());
        _moveCoroutine = StartCoroutine(SmoothMoveToWayPointWithShooting(0));
    }

    private IEnumerator MoveTowardsPlayerRoutine()
    {
        while (true) // Бесконечный цикл движения
        {
            // Получаем текущее направление к игроку
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // Обнуляем Y чтобы не летать вверх/вниз
            // Запоминаем это направление
            Vector3 moveDirection = directionToPlayer;
            // Сброс флага столкновения
            _hitWall = false;
            // Движение и вращение в течение 3 секунд или до столкновения со стеной
            float moveDuration = 3f;
            float elapsed = 0f;
            while (elapsed < moveDuration && !_hitWall)
            {
                // Движение в запомненном направлении
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
                elapsed += Time.deltaTime;
                yield return null;
            }
            // После 3 секунд или столкновения со стеной цикл повторяется - снова получаем новое направление
        }
    }

    /// <summary>
    /// Плавное перемещение к точке с заданным индексом с последующим стрельбой
    /// </summary>
    /// <param name="wayPointIndex">Индекс точки в списке wayPoints</param>
    public IEnumerator SmoothMoveToWayPointWithShooting(int wayPointIndex)
    {
        yield return new WaitForSeconds(3f);
        while (true) // Бесконечный цикл
        {
            if (wayPoints.Count == 0)
            {
                Debug.LogWarning("WayPoints list is empty!");
                yield return new WaitForSeconds(1f);
                continue;
            }
            
            if (wayPointIndex < 0 || wayPointIndex >= wayPoints.Count)
            {
                Debug.LogWarning($"WayPoint index {wayPointIndex} is out of range!");
                wayPointIndex = 0;
            }

            Transform targetWayPoint = wayPoints[wayPointIndex];
            _currentWayPointIndex = wayPointIndex;
            
            // Плавное перемещение и поворот
            while (Vector3.Distance(transform.position, targetWayPoint.position) > 0.1f && !_isReturningToDefault)
            {
                // Плавный поворот к ориентации точки
                Vector3 directionToTarget = (targetWayPoint.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
                
                // Плавное перемещение к точке
                transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position, smoothMoveSpeed * Time.deltaTime);
                
                yield return null;
            }
            
            if (_isReturningToDefault) break;
            
            // Когда достигли точки, копируем её точный поворот
            transform.rotation = targetWayPoint.rotation;
            
            // Устанавливаем флаг достижения точки
            _isAtWayPoint = true;
            
            // Стреляем в игрока
            ShootWebAtPlayer();
            
            // Запускаем корутину возврата через 12 секунд
            if (_returnCoroutine == null)
            {
                _returnCoroutine = StartCoroutine(ReturnToDefaultPositionAfterDelay());
            }
            
            // Ждем немного перед следующим перемещением
            yield return new WaitForSeconds(1f);
            
            // Переход к следующей точке (циклически)
            wayPointIndex = (wayPointIndex + 1) % wayPoints.Count;
            _isAtWayPoint = false;
        }
    }

    /// <summary>
    /// Возврат на позицию по умолчанию через заданное время
    /// </summary>
    private IEnumerator ReturnToDefaultPositionAfterDelay()
    {
        yield return new WaitForSeconds(returnToDefaultTime);
        
        if (defaultPosition != null && !_isReturningToDefault)
        {
            _isReturningToDefault = true;
            
            // Останавливаем основную корутину движения
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }
            
            // Плавное возвращение на позицию по умолчанию
            yield return StartCoroutine(SmoothMoveToDefaultPosition());
            
            // После возврата продолжаем движение по точкам
            _isReturningToDefault = false;
            _moveCoroutine = StartCoroutine(MoveTowardsPlayerRoutine());
        }
        
        _returnCoroutine = null;
    }

    /// <summary>
    /// Плавное перемещение на позицию по умолчанию
    /// </summary>
    private IEnumerator SmoothMoveToDefaultPosition()
    {
        if (defaultPosition == null) yield break;
        
        while (Vector3.Distance(transform.position, defaultPosition.position) > 0.1f)
        {
            // Плавный поворот к ориентации позиции по умолчанию
            Vector3 directionToTarget = (defaultPosition.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
            
            // Плавное перемещение к позиции по умолчанию
            transform.position = Vector3.MoveTowards(transform.position, defaultPosition.position, smoothMoveSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Устанавливаем точную позицию и поворот
        transform.position = defaultPosition.position;
        transform.rotation = defaultPosition.rotation;
    }

    /// <summary>
    /// Стрельба вебом в направлении игрока
    /// </summary>
    private void ShootWebAtPlayer()
    {
        if (webPrefab != null && player != null)
        {
            // Определяем точку выстрела (если не задана, используем позицию паука)
            Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;
            
            // Создаем веб
            GameObject webInstance = Instantiate(webPrefab, spawnPosition, Quaternion.identity);
            
            // Направляем веб в сторону игрока
            Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
            
            // Добавляем немного разброса для реалистичности (опционально)
            // directionToPlayer += new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
            
            // Получаем Rigidbody веба и применяем силу
            Rigidbody webRb = webInstance.GetComponent<Rigidbody>();
            if (webRb != null)
            {
                webRb.linearVelocity = directionToPlayer * 40f; // Скорость веба
            }
            else
            {
                Debug.LogWarning("Web prefab doesn't have Rigidbody component!");
            }
        }
        else
        {
            Debug.LogWarning("Web prefab or player is not assigned!");
        }
    }

    private void Die()
    {
        // Остановка всех корутин при смерти
        StopAllCoroutines();
        // Здесь должна быть логика смерти босса
        // Например: animator.SetTrigger("Die"); или Destroy(gameObject);
        // throw new System.NotImplementedException();
        Destroy(gameObject); // Простой пример уничтожения
    }

    // --- Обновленный OnTriggerStay ---
    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.tag);
        switch (other.tag)
        {
            case "Player":
                if (Time.time - _lastAttack > attackCooldown)
                {
                    _lastAttack = Time.time;
                    Debug.Log("больно в ноге");

                    // --- Вызываем отбрасывание у игрока ---
                    if (playerMovementScript != null)
                    {
                        // Вычисляем направление отбрасывания (от паука к игроку)
                        Vector3 knockDirection = (player.position - transform.position).normalized;
                        // Можно немного поднять игрока вверх
                        knockDirection.y = 0.5f; // Настройте по желанию
                        playerMovementScript.Knockback(knockDirection);
                    }
                }
                break;
            case "Wall":
                _hitWall = true; // Устанавливаем флаг столкновения со стеной
                break;
        }
    }

    // --- Обновленный OnTriggerEnter ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            _hitWall = true; // Также проверяем столкновение при входе в триггер
        }
    }

    void OnDestroy()
    {
        BossActions.onBossDied -= Die;
    }
}