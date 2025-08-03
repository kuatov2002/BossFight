using UnityEngine;

public class GolemBoss : MonoBehaviour
{
    [Header("Цели и настройки")]
    public Transform player;
    public float moveSpeed = 2f;
    public float attackRange = 2f;
    public float detectionRange = 15f;

    [Header("Настройки атаки")]
    public float attackCooldown = 3f;
    public int attackDamage = 20;
    public float attackRadius = 3f; // Новая переменная — радиус зоны атаки

    [Header("Анимации")]
    public Animator animator;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool shouldMoveAfterAttack = false;

    // Имена параметров аниматора
    private const string PARAM_IS_MOVING = "isMoving";
    private const string PARAM_IS_ATTACKING = "isAttacking";
    private const string PARAM_TRIGGER_HIT = "OnHit"; // Если используется
    private const string PARAM_TRIGGER_DEATH = "OnDeath"; // Если используется

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        lastAttackTime = -attackCooldown;

        // Если Rise должна запускаться триггером, раскомментируйте строку ниже:
        // animator.SetTrigger(PARAM_TRIGGER_RISE);
        // В текущем YAML Rise уже является стартовым состоянием, так что триггер может и не понадобиться.
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inDetectionRange = distanceToPlayer <= detectionRange;
        bool inAttackRange = distanceToPlayer <= attackRange;

        // Поворачиваемся к игроку
        if (inDetectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        // Логика поведения
        if (inDetectionRange && !isAttacking) // Не двигаемся, если атакуем
        {
            if (inAttackRange)
            {
                Attack();
            }
            else
            {
                MoveTowardsPlayer();
                // Устанавливаем параметр для анимации ходьбы
                SetAnimationMovement(true);
            }
        }
        else if(!isAttacking) // Не в зоне обнаружения и не атакуем
        {
            // Устанавливаем параметр для анимации покоя
            SetAnimationMovement(false);
        }
        // Если isAttacking == true, то параметры движения уже установлены в AttackSequence
        // и будут сброшены там же.
    }

    void MoveTowardsPlayer()
    {
        if (player == null || isAttacking) return;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            StartCoroutine(AttackSequence());
        }
    }

    System.Collections.IEnumerator AttackSequence()
    {
        isAttacking = true;
        shouldMoveAfterAttack = false;

        // Устанавливаем параметр для анимации атаки
        SetAnimationAttack(true);

        float animationLength = 3.0f; // Предполагаемая длина анимации атаки
        float damageDelay = 0.8f; // Когда наносится урон

        yield return new WaitForSeconds(damageDelay);

        // Проверяем, находится ли игрок в зоне атаки
        if (IsPlayerInAttackArea())
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        lastAttackTime = Time.time;
        float remainingTime = animationLength - damageDelay;
        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        // Сбрасываем параметр атаки после завершения анимации
        SetAnimationAttack(false);
    
        // Теперь переходим в состояние Idle на 4 секунды
        SetAnimationMovement(false); // Останавливаемся

        yield return new WaitForSeconds(4f); // Ждём 4 секунды в состоянии покоя

        isAttacking = false;

        // После ожидания проверяем, нужно ли двигаться
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange && distanceToPlayer <= detectionRange)
            {
                shouldMoveAfterAttack = true;
                SetAnimationMovement(true); // Начинаем движение
            }
            else
            {
                SetAnimationMovement(false); // Остаёмся в Idle
            }
        }
        else
        {
            SetAnimationMovement(false); // Остаёмся в Idle, если игрока нет
        }
    }

    // Новый метод: проверяет, находится ли игрок в зоне атаки
    bool IsPlayerInAttackArea()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // Методы для управления анимациями через параметры
    void SetAnimationMovement(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_MOVING, isMoving);
        }
    }

    void SetAnimationAttack(bool isAttacking)
    {
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_ATTACKING, isAttacking);
        }
    }

    // Пример метода для проигрывания анимации получения урона (если нужно)
    public void PlayHitAnimation()
    {
         if (animator != null)
        {
            animator.SetTrigger(PARAM_TRIGGER_HIT);
        }
    }

    // Пример метода для проигрывания анимации смерти (если нужно)
    public void PlayDeathAnimation()
    {
         if (animator != null)
        {
            animator.SetTrigger(PARAM_TRIGGER_DEATH);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // Отображаем зону атаки
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}