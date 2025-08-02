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
    public float smoothMoveSpeed = 30f; // Скорость плавного перемещения
    public float smoothRotationSpeed = 50f; // Скорость плавного поворота
    public GameObject webPrefab;
    
    
    // --- Ссылка на скрипт игрока ---
    private PlayerMovement playerMovementScript;

    private float _lastAttack;
    private bool _hitWall = false; // Флаг столкновения со стеной
    private int _currentWayPointIndex = 0; // Индекс текущей точки назначения

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
        StartCoroutine(SmoothMoveToWayPoint(0));
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
                elapsed += Time.deltaTime;
                yield return null;
            }
            // После 3 секунд или столкновения со стеной цикл повторяется - снова получаем новое направление
        }
    }

    /// <summary>
    /// Плавное перемещение к следующей точке в списке wayPoints с плавным поворотом
    /// </summary>
    public IEnumerator SmoothMoveToNextWayPoint()
    {
        if (wayPoints.Count == 0)
        {
            Debug.LogWarning("WayPoints list is empty!");
            yield break;
        }

        // Получаем следующую точку
        Transform targetWayPoint = wayPoints[_currentWayPointIndex];
        
        // Плавное перемещение и поворот
        while (Vector3.Distance(transform.position, targetWayPoint.position) > 0.1f)
        {
            // Плавный поворот к точке
            Vector3 directionToTarget = (targetWayPoint.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
            
            // Плавное перемещение к точке
            transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position, smoothMoveSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Переход к следующей точке (циклически)
        _currentWayPointIndex = (_currentWayPointIndex + 1) % wayPoints.Count;
    }

    /// <summary>
    /// Плавное перемещение к конкретной точке с плавным поворотом
    /// </summary>
    /// <param name="targetPosition">Целевая позиция</param>
    public IEnumerator SmoothMoveToPoint(Vector3 targetPosition)
    {
        // Создаем временную точку для перемещения
        GameObject tempTarget = new GameObject("TempMoveTarget");
        tempTarget.transform.position = targetPosition;
        
        // Плавное перемещение и поворот
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // Плавный поворот к точке
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
            
            // Плавное перемещение к точке
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, smoothMoveSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Удаляем временную точку
        Destroy(tempTarget);
    }

    /// <summary>
    /// Плавное перемещение к точке с заданным индексом
    /// </summary>
    /// <param name="wayPointIndex">Индекс точки в списке wayPoints</param>
    public IEnumerator SmoothMoveToWayPoint(int wayPointIndex)
    {
        if (wayPoints.Count == 0)
        {
            Debug.LogWarning("WayPoints list is empty!");
            yield break;
        }
        
        if (wayPointIndex < 0 || wayPointIndex >= wayPoints.Count)
        {
            Debug.LogWarning($"WayPoint index {wayPointIndex} is out of range!");
            yield break;
        }

        Transform targetWayPoint = wayPoints[wayPointIndex];
        _currentWayPointIndex = wayPointIndex;
        
        // Плавное перемещение и поворот
        while (Vector3.Distance(transform.position, targetWayPoint.position) > 0.1f)
        {
            // Плавный поворот к ориентации точки
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWayPoint.rotation, smoothRotationSpeed * Time.deltaTime);
            
            // Плавное перемещение к точке
            transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position, smoothMoveSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Когда достигли точки, копируем её точный поворот
        transform.rotation = targetWayPoint.rotation;
    }

    private void Die()
    {
        // Остановка всех корутин при смерти
        StopAllCoroutines();
        // Здесь должна быть логика смерти босса
        // Например: animator.SetTrigger("Die"); или Destroy(gameObject);
        throw new System.NotImplementedException();
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