using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerMovement movement;

    private static readonly int Hit1 = Animator.StringToHash("hit1");
    private static readonly int Hit2 = Animator.StringToHash("hit2");
    private static readonly int Hit3 = Animator.StringToHash("hit3");

    private float _lastAttackTime = 0f;
    public float maxComboDelay = 1f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;
    public Vector3 offset;

    void Start()
    {
        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>() ?? GetComponent<PlayerMovement>();
        if (anim == null)
            anim = GetComponent<Animator>();
        if (anim == null || movement == null)
            Debug.LogError("PlayerAttack: Missing required components (Animator or PlayerMovement)");
    }

    void Update()
    {
        if (Time.time - _lastAttackTime > maxComboDelay)
        {
            // Мы можем сбросить триггеры здесь, если комбо "протухло"
            // Это поможет избежать "залипания" триггеров.
            // Однако будьте осторожны, это может прервать начатую анимацию.
            // Лучше сбрасывать только если аниматор в состоянии Idle или других "базовых" состояниях.
            // Например, проверить текущее состояние аниматора.
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Idle") || stateInfo.IsName("Walk")) // Или любые другие "не-атакующие" состояния
            {
                 // Ничего не делаем, комбо и так сброшено
            }
            // Или более грубый способ (может вызвать проблемы):
            // anim.ResetTrigger(Hit1);
            // anim.ResetTrigger(Hit2);
            // anim.ResetTrigger(Hit3);
        }
    }

    public void PerformAttack()
    {
        _lastAttackTime = Time.time;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        // Получаем следующий возможный удар в зависимости от текущего состояния
        int nextHitTrigger = -1;

        if (stateInfo.IsName("Idle") || stateInfo.IsName("Walk")) // Начинаем комбо
        {
            nextHitTrigger = Hit1;
        }
        else if (stateInfo.IsName("Hit1")) // Продолжаем комбо
        {
            nextHitTrigger = Hit2;
        }
        else if (stateInfo.IsName("Hit2")) // Завершаем комбо
        {
            nextHitTrigger = Hit3;
        }
        // Если мы в состоянии Hit3, можно либо игнорировать, либо начать новое комбо
        // В вашем случае, судя по контроллеру, после Hit3 идет переход в Idle,
        // и новое комбо начнется с Hit1. Но если мы нажмем атаку во время Hit3,
        // можно поставить в очередь новое комбо (как у вас было).
        else if (stateInfo.IsName("Hit3"))
        {
             // Можно ничего не делать, и комбо начнется сначала после завершения Hit3
             // Или можно поставить в очередь (но тогда нужна логика в OnHit3Finished)
             // Пока просто ничего не делаем.
        }


        if (nextHitTrigger != -1)
        {
            // Сбрасываем все триггеры перед установкой нового, чтобы избежать конфликтов
            anim.ResetTrigger(Hit1);
            anim.ResetTrigger(Hit2);
            anim.ResetTrigger(Hit3);
            anim.SetTrigger(nextHitTrigger);
            // Debug.Log($"Attack: Trigger {nextHitTrigger} set directly.");
        }
    }

    // Эти методы теперь нужны только для вызова ExecuteAttack и, возможно, сброса комбо
    // если анимация действительно завершается (например, если анимация прервана или т.п.)
    // Но основная логика перехода теперь в PerformAttack.
    public void OnHit1Finished()
    {
        ExecuteAttack();
        // Debug.Log("Attack: Hit1 animation finished");
    }

    public void OnHit2Finished()
    {
        ExecuteAttack();
        // Debug.Log("Attack: Hit2 animation finished");
    }

    public void OnHit3Finished()
    {
        ExecuteAttack();
        // Debug.Log("Attack: Hit3 animation finished");
        // После завершения последнего удара можно явно сбросить комбо, если нужно
        // Хотя PerformAttack теперь сам это контролирует.
        // ResetCombo(); // Может быть избыточно
    }

    public void ExecuteAttack()
    {
        // Учитываем поворот объекта при определении позиции атаки
        Vector3 attackPosition = transform.position + transform.TransformDirection(offset);

        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.GetComponent<BossHealth>() != null)
            {
                StartCoroutine(PauseEffect(enemy.GetComponent<BossHealth>(),attackDamage));
            }
            else if(enemy.GetComponent<Zombie>() != null)
            {
                enemy.GetComponent<Zombie>().TakeDamage();
            }
        }

#if UNITY_EDITOR
        Debug.DrawLine(attackPosition, attackPosition + Vector3.up * 0.5f, Color.red, 0.1f);
#endif
    }

    private IEnumerator PauseEffect(BossHealth bossHealth, float damage)
    {
        // Замедляем время вместо полной остановки
        anim.speed = 0;
        yield return new WaitForSecondsRealtime(0.1f); // Реальное время
        anim.speed = 1;
    
        bossHealth.TakeDamage(damage);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Также учитываем поворот при отрисовке Gizmos
        Vector3 attackPosition = transform.position + transform.TransformDirection(offset);

        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
    // private void ResetCombo() { ... } // Можно удалить или оставить для других целей
    // void OnDrawGizmosSelected() { ... } // Оставить как есть
}