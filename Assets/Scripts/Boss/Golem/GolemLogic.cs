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
    public float attackRadius = 3f;
    public GameObject warning;
    
    [Header("Анимации")]
    public Animator animator;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool shouldMoveAfterAttack = false;

    // Новый флаг смерти
    private bool isDead = false;

    // Имена параметров аниматора
    private const string PARAM_IS_MOVING = "isMoving";
    private const string PARAM_IS_ATTACKING = "isAttacking";
    private const string PARAM_TRIGGER_HIT = "OnHit";
    private const string PARAM_TRIGGER_DEATH = "OnDeath";

    void Start()
    {
        warning.SetActive(false);
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        lastAttackTime = -attackCooldown;

        BossActions.onBossDied += Die;
    }

    void Update()
    {
        // Если мёртв — ничего не делаем
        if (isDead) return;

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inDetectionRange = distanceToPlayer <= detectionRange;
        bool inAttackRange = distanceToPlayer <= attackRange;

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

        if (inDetectionRange && !isAttacking)
        {
            if (inAttackRange)
            {
                Attack();
            }
            else
            {
                MoveTowardsPlayer();
                SetAnimationMovement(true);
            }
        }
        else if (!isAttacking)
        {
            SetAnimationMovement(false);
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null || isAttacking || isDead) return;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void Attack()
    {
        warning.SetActive(true);
        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking && !isDead)
        {
            StartCoroutine(AttackSequence());
        }
    }

    System.Collections.IEnumerator AttackSequence()
    {
        isAttacking = true;
        shouldMoveAfterAttack = false;

        SetAnimationAttack(true);

        float animationLength = 3.0f;
        float damageDelay = 0.9f;

        yield return new WaitForSeconds(damageDelay);
        warning.SetActive(false);
        if (!isDead && IsPlayerInAttackArea())
        {
            Vector3 knockDirection = (player.transform.position - transform.position).normalized;
            var playerMovementScript = player.GetComponent<PlayerMovement>();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            knockDirection.y = 0.5f; // Настройте по желанию
            playerMovementScript.Knockback(knockDirection * 2f);
        }

        lastAttackTime = Time.time;
        float remainingTime = animationLength - damageDelay;
        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        SetAnimationAttack(false);
        SetAnimationMovement(false);

        yield return new WaitForSeconds(4f);

        isAttacking = false;

        if (isDead) yield break;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange && distanceToPlayer <= detectionRange)
            {
                shouldMoveAfterAttack = true;
                SetAnimationMovement(true);
            }
            else
            {
                SetAnimationMovement(false);
            }
        }
        else
        {
            SetAnimationMovement(false);
        }
    }

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

    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(PARAM_TRIGGER_HIT);
        }
    }

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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    private void Die()
    {
        if (isDead) return;
        warning.SetActive(false);
        isDead = true;
        PlayDeathAnimation();

        // Останавливаем все корутины, если нужно
        StopAllCoroutines();

        // Отписываемся от события
        BossActions.onBossDied -= Die;
    }
}