using UnityEngine;

public class Zombie : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;

    [Header("Атака")] 
    public float attackRadius = 1.5f;
    public float damage = 10f;
    public Vector3 offset;
    public float attackCooldown = 1f;

    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private bool isDead = false; // Новое поле для отслеживания состояния смерти

    void Start()
    {
        BossActions.onBossDied += Die;
        
        animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Если зомби мертв, ничего не делаем
        if (isDead) return;
        
        if (player == null) return;

        // Проверяем расстояние до игрока с учетом оффсета
        Vector3 attackPosition = transform.position + offset;
        float distanceToPlayer = Vector3.Distance(attackPosition, player.position);

        // Если игрок в радиусе атаки и можно атаковать
        if (distanceToPlayer <= attackRadius && Time.time - lastAttackTime >= attackCooldown)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                animator.SetTrigger("Attack");
            }
        }
        else if (distanceToPlayer > attackRadius && !isAttacking)
        {
            // Если игрок вне радиуса атаки - идем к нему
            isAttacking = false;
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        // Дополнительная проверка на смерть
        if (isDead || player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
        
        // Поворачиваем зомби в сторону игрока
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        // Запускаем анимацию ходьбы
        if (animator != null)
            animator.SetBool("IsWalking", true);
    }

    public void ExecuteAttack()
    {
        // Если зомби мертв, не выполняем атаку
        if (isDead) return;
        
        // Вызывается через animation event
        lastAttackTime = Time.time;
        
        // Наносим урон всем объектам в радиусе атаки с учетом оффсета
        Vector3 attackPosition = transform.position + offset;
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius);
        
        foreach (Collider hit in hitColliders)
        {
            // Проверяем, что это игрок (или другой объект, которому можно нанести урон)
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>().TakeDamage(damage);
                Vector3 knockDirection = (hit.transform.position - transform.position).normalized;
                var playerMovementScript = hit.gameObject.GetComponent<PlayerMovement>();
                // Можно немного поднять игрока вверх
                knockDirection.y = 0f; // Настройте по желанию
                playerMovementScript.Knockback(knockDirection*2f);
                // Здесь можно вызвать метод получения урона у игрока
                // Например: hit.GetComponent<PlayerHealth>().TakeDamage(damage);
                Debug.Log($"Нанесено {damage} урона игроку");
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead) return; // Если уже мертв, не наносим дополнительный урон
        
        isDead = true; // Помечаем как мертвого
        animator.SetTrigger("OnDeath");
        
        // Отключаем коллайдер, чтобы зомби не мешал
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;
    }

    // Визуализация радиуса атаки в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + offset;
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }

    // Метод для завершения атаки (вызывается в конце анимации)
    public void FinishAttack()
    {
        if (isDead) return; // Если мертв, не завершаем атаку нормально
        
        isAttacking = false;
        if (animator != null)
            animator.SetBool("IsWalking", false);
    }

    private void Die()
    {
        BossActions.onBossDied -= Die;
        
        TakeDamage();
    }
}