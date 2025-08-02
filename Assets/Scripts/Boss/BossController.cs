using System;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Параметры босса")]
    public float health = 1000f;
    public float rangedDamage = 30f;
    public float attackCooldown = 2f;
    public float recoveryTime = 2f; // Время восстановления после атаки
    
    [Header("Дистанции атак")]
    public float meleeRange = 3f;
    public float rangedRange = 15f;
    
    [Header("Префабы атак")]
    public GameObject meleeAttackPrefab; // Префаб для ближней атаки
    public float meleeAttackDuration = 1f; // Длительность атаки в секундах
    
    [Header("Ссылки")]
    public Transform player;
    public Animator animator;
    public Door door;
    
    private bool isAttacking = false;
    private bool isRecovering = false;
    private float lastAttackTime = 0f;
    private bool isDying = false; // Add this flag
    private Coroutine bossLogicCoroutine;
    
    void Start()
    {
        BossActions.onBossDied += Die;
        
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        // Запускаем основную логику босса
        bossLogicCoroutine = StartCoroutine(BossLogicCoroutine());
    }
    
    System.Collections.IEnumerator BossLogicCoroutine()
    {
        while (health > 0)
        {
            // Постоянный поворот к игроку
            RotateToPlayer();
            
            // Атака (только если не атакуем и не восстанавливаемся и прошел кулдаун)
            if (!isAttacking && !isRecovering && Time.time - lastAttackTime >= attackCooldown)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer <= meleeRange)
                {
                    yield return StartCoroutine(MeleeAttackCoroutine());
                }
                else if (distanceToPlayer <= rangedRange)
                {
                    yield return StartCoroutine(RangedAttackCoroutine());
                }
            }
            
            // Небольшая задержка перед следующей итерацией
            yield return null;
        }
    }
    
    void RotateToPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    System.Collections.IEnumerator MeleeAttackCoroutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("meleeAttack");
        
        // Спавним префаб атаки
        SpawnMeleeAttack();
        
        // Ждем указанную длительность атаки, затем выключаем префаб
        yield return new WaitForSeconds(meleeAttackDuration);
        DeactivateMeleeAttack();
        
        // Завершаем атаку
        isAttacking = false;
        
        // Восстановление после атаки
        yield return StartCoroutine(RecoveryCoroutine());
    }
    
    System.Collections.IEnumerator RangedAttackCoroutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("rangedAttack");
        
        // Ждем завершения анимации атаки (вызов OnAttackEnd)
        while (isAttacking)
        {
            yield return null;
        }
        
        // Восстановление после атаки
        yield return StartCoroutine(RecoveryCoroutine());
    }
    
    void SpawnMeleeAttack()
    {
        if (meleeAttackPrefab)
        {
            meleeAttackPrefab.SetActive(true);
        }
    }
    
    void DeactivateMeleeAttack()
    {
        if (meleeAttackPrefab)
        {
            meleeAttackPrefab.SetActive(false);
        }
    }
    
    System.Collections.IEnumerator RecoveryCoroutine()
    {
        isRecovering = true;
        yield return new WaitForSeconds(recoveryTime);
        isRecovering = false;
    }
    
    // Вызывается в конце анимации атаки (только для дальней атаки)
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
    
    public void TakeDamage(float damage)
    {
        // Don't take damage if already dying
        if (isDying) return;
        
        health -= damage;
        Debug.Log($"Босс получил {damage} урона. Осталось здоровья: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // Prevent multiple death calls
        if (isDying) return;
        isDying = true;
        
        Debug.Log("Босс побежден!");
        // Проигрываем анимацию смерти (если есть)
        // animator.SetTrigger("die");
        
        // Останавливаем основную логику
        if (bossLogicCoroutine != null)
        {
            StopCoroutine(bossLogicCoroutine);
        }
        
        // Отключаем объект (или делаем другую логику смерти)
        gameObject.SetActive(false);
        door.gameObject.SetActive(true);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        BossActions.onBossDied -= Die;
    }
    
    void OnDisable()
    {
        // Останавливаем все корутины при отключении
        StopAllCoroutines();
    }
}