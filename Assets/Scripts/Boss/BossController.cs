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

    public string[] lines;
    
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
        
        UIManager.Instance.StartDialogue(lines);
    }
    
    // Вызывается в конце анимации атаки (только для дальней атаки)
    public void OnAttackEnd()
    {
        isAttacking = false;
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
        BossActions.onBossDied -= Die;
    }
    
    void OnDisable()
    {
        // Останавливаем все корутины при отключении
        StopAllCoroutines();
    }
}