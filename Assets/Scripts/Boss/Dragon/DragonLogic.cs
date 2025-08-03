using System.Collections;
using UnityEngine;

public class DragonLogic : MonoBehaviour
{
    [Header("Точки полета")]
    public Transform[] flightPoints;        // Точки, куда дракон может взлететь
    public Transform[] landingPoints;       // Точки посадки
    
    [Header("Параметры огненного дыхания")]
    public GameObject fireBreathPrefab;     // Префаб огненного дыхания
    public Transform firePoint;             // Точка выстрела
    public Transform playerTarget;          // Цель (игрок)
    public float fireDuration = 3f;         // Длительность стрельбы
    public float fireRate = 0.5f;           // Частота выстрелов
    
    [Header("Тайминги")]
    public float landingWaitTime = 6f;      // Время ожидания на земле
    public float flightSpeed = 5f;          // Скорость полета
    public float fireDelay = 0.2f;          // Задержка перед началом стрельбы

    public Door door;
    private Transform currentTarget;        // Текущая цель для полета
    private bool isFlying = false;          // Флаг полета
    [SerializeField] private Animator animator;              // Аниматор (если используется)
    private float lastFireTime = 0f;        // Время последнего выстрела
    
    void Start()
    {
        BossActions.onBossDied += Die;
        door.gameObject.SetActive(false);
        if (flightPoints.Length == 0 || landingPoints.Length == 0)
        {
            Debug.LogError("Не заданы точки полета или посадки!");
            return;
        }
        
        if (fireBreathPrefab == null)
        {
            Debug.LogError("Не задан префаб огненного дыхания!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("Не задана точка выстрела!");
            return;
        }
        
        if (playerTarget == null)
        {
            Debug.LogError("Не задана цель для стрельбы!");
            return;
        }
        
        // Получаем компонент аниматора если есть
        animator = GetComponent<Animator>();
        
        // Начинаем цикл поведения
        StartCoroutine(BossBehaviour());
    }
    
    IEnumerator BossBehaviour()
    {
        while (true)
        {
            // 1. Взлетаем к случайной точке
            yield return StartCoroutine(FlyToRandomPoint());
            
            // 2. Стреляем огнем
            yield return StartCoroutine(FireBreath());
            
            // 3. Садимся на случайную точку
            yield return StartCoroutine(LandOnRandomPoint());
            
            // 4. Ждем 6 секунд
            yield return new WaitForSeconds(landingWaitTime);
        }
    }
    
    IEnumerator FlyToRandomPoint()
    {
        // Выбираем случайную точку полета
        int randomIndex = Random.Range(0, flightPoints.Length);
        currentTarget = flightPoints[randomIndex];
        
        isFlying = true;
        SetFlyingAnimation(true);
        
        // Летим к точке
        while (Vector3.Distance(transform.position, currentTarget.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                currentTarget.position, 
                flightSpeed * Time.deltaTime
            );
            
            // Поворачиваем дракона в сторону движения (только по Y)
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                // Создаем вектор направления, но обнуляем Y компонент для поворота только по горизонтали
                Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
                if (horizontalDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        Time.deltaTime * 5f
                    );
                }
            }
            
            yield return null;
        }
        
        isFlying = false;
    }
    
    IEnumerator FireBreath()
    {
        float fireTimer = 0f;
        
        SetFireAnimation(true);
        
        // Добавляем задержку перед началом стрельбы
        yield return new WaitForSeconds(fireDelay);
        
        while (fireTimer < fireDuration)
        {
            // Поворачиваем дракона к игроку (только по Y)
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            // Создаем вектор направления, но обнуляем Y компонент для поворота только по горизонтали
            Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            if (horizontalDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 3f
                );
            }
            
            // Стреляем огнем с заданной частотой
            if (Time.time - lastFireTime >= fireRate)
            {
                ShootFireAtPlayer();
                lastFireTime = Time.time;
            }
            
            fireTimer += Time.deltaTime;
            yield return null;
        }
        
        SetFireAnimation(false);
    }
    
    IEnumerator LandOnRandomPoint()
    {
        // Выбираем случайную точку посадки
        int randomIndex = Random.Range(0, landingPoints.Length);
        currentTarget = landingPoints[randomIndex];
        
        isFlying = true;
        SetFlyingAnimation(true);
        
        // Летим к точке посадки
        while (Vector3.Distance(transform.position, currentTarget.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                currentTarget.position, 
                flightSpeed * Time.deltaTime
            );
            
            // Поворачиваем дракона в сторону движения (только по Y)
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                // Создаем вектор направления, но обнуляем Y компонент для поворота только по горизонтали
                Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
                if (horizontalDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        Time.deltaTime * 5f
                    );
                }
            }
            
            yield return null;
        }
        
        isFlying = false;
        SetFlyingAnimation(false);
    }
    
    void ShootFireAtPlayer()
    {
        if (fireBreathPrefab != null && firePoint != null && playerTarget != null)
        {
            // Создаем огненное дыхание
            GameObject fire = Instantiate(fireBreathPrefab, firePoint.position, Quaternion.identity);
            
            // Настраиваем направление полета к игроку
            FireBreath fireScript = fire.GetComponent<FireBreath>();
            if (fireScript != null)
            {
                // Вычисляем направление к игроку
                Vector3 directionToPlayer = (playerTarget.position - firePoint.position).normalized;
                fireScript.SetDirection(directionToPlayer);
                
                // Устанавливаем цель для отслеживания
                fireScript.SetTarget(playerTarget);
            }
            
            // Уничтожаем через некоторое время
            Destroy(fire, 5f);
        }
    }
    
    void SetFlyingAnimation(bool flying)
    {
        if (animator != null)
        {
            animator.SetBool("IsFlying", flying);
        }
    }
    
    void SetFireAnimation(bool firing)
    {
        if (animator != null)
        {
            animator.SetBool("IsFiring", firing);
        }
    }

    private void Die()
    {
        BossActions.onBossDied -= Die;
        door.gameObject.SetActive(true);
    }
}