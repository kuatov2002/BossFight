using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
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
    
    private CancellationTokenSource cancellationTokenSource;
    private bool isAttacking = false;
    private bool isRecovering = false;
    private float lastAttackTime = 0f;
    
    void Start()
    {
        BossActions.onBossDied += Die;
        
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        cancellationTokenSource = new CancellationTokenSource();
        
        // Запускаем основную логику босса
        BossLogicAsync(cancellationTokenSource.Token).Forget();
    }
    
    async UniTask BossLogicAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (health > 0 && !cancellationToken.IsCancellationRequested)
            {
                // Постоянный поворот к игроку
                RotateToPlayer();
                
                // Атака (только если не атакуем и не восстанавливаемся и прошел кулдаун)
                if (!isAttacking && !isRecovering && Time.time - lastAttackTime >= attackCooldown)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                    
                    if (distanceToPlayer <= meleeRange)
                    {
                        await MeleeAttackAsync(cancellationToken);
                    }
                    else if (distanceToPlayer <= rangedRange)
                    {
                        await RangedAttackAsync(cancellationToken);
                    }
                }
                
                // Небольшая задержка перед следующей итерацией
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Игнорируем отмену задачи
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
    
    async UniTask MeleeAttackAsync(CancellationToken cancellationToken)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("meleeAttack");
        
        // Спавним префаб атаки
        SpawnMeleeAttack();
        
        // Ждем указанную длительность атаки, затем выключаем префаб
        await UniTask.Delay((int)(meleeAttackDuration * 1000), cancellationToken: cancellationToken);
        DeactivateMeleeAttack();
        
        // Завершаем атаку
        isAttacking = false;
        
        // Восстановление после атаки
        await RecoveryAsync(cancellationToken);
    }
    
    async UniTask RangedAttackAsync(CancellationToken cancellationToken)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("rangedAttack");
        
        // Ждем завершения анимации атаки (вызов OnAttackEnd)
        await UniTask.WaitUntil(() => !isAttacking, cancellationToken: cancellationToken);
        
        // Восстановление после атаки
        await RecoveryAsync(cancellationToken);
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
    
    async UniTask RecoveryAsync(CancellationToken cancellationToken)
    {
        isRecovering = true;
        await UniTask.Delay((int)(recoveryTime * 1000), cancellationToken: cancellationToken);
        isRecovering = false;
    }
    
    // Вызывается в конце анимации атаки (только для дальней атаки)
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Босс получил {damage} урона. Осталось здоровья: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Босс побежден!");
        // Проигрываем анимацию смерти (если есть)
        // animator.SetTrigger("die");
        
        // Отменяем все задачи
        cancellationTokenSource?.Cancel();
        
        // Отключаем объект (или делаем другую логику смерти)
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
    
    void OnDisable()
    {
        cancellationTokenSource?.Cancel();
    }
}

// Интерфейс для объектов, которые могут получать урона
public interface IDamageable
{
    void TakeDamage(float damage);
}