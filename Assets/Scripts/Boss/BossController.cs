using UnityEngine;
using UnityEngine.InputSystem;

public class BossController : MonoBehaviour
{
    [Header("Параметры босса")]
    public float health = 1000f;
    public float meleeDamage = 50f;
    public float rangedDamage = 30f;
    public float attackCooldown = 2f;
    public float recoveryTime = 2f; // Время восстановления после атаки
    
    [Header("Дистанции атак")]
    public float meleeRange = 3f;
    public float rangedRange = 15f;
    
    [Header("Ссылки")]
    public Transform player;
    public Animator animator;
    
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isRecovering = false;
    private float recoveryStartTime;
    
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        if (health <= 0)
            return;
        
        // Проверка завершения восстановления
        if (isRecovering && Time.time - recoveryStartTime >= recoveryTime)
        {
            isRecovering = false;
        }
            
        // Поворот к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Атака (только если не атакуем и не восстанавливаемся)
        if (!isAttacking && !isRecovering && Time.time - lastAttackTime >= attackCooldown)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= meleeRange)
            {
                MeleeAttack();
            }
            else if (distanceToPlayer <= rangedRange)
            {
                RangedAttack();
            }
        }
    }
    
    void MeleeAttack()
    {
        isAttacking = true;
        animator.SetTrigger("meleeAttack");
        lastAttackTime = Time.time;
    }
    
    void RangedAttack()
    {
        isAttacking = true;
        animator.SetTrigger("rangedAttack");
        lastAttackTime = Time.time;
    }
    
    // Вызывается в конце анимации атаки
    public void OnAttackEnd()
    {
        isAttacking = false;
        isRecovering = true;
        recoveryStartTime = Time.time;
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
        
        // Отключаем скрипт
        this.enabled = false;
        
        // Отключаем объект (или делаем другую логику смерти)
        gameObject.SetActive(false);
    }
}