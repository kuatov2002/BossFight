using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Animator anim;
    
    public static int noOfClicks = 0;
    float lastClickedTime = 0;
    float maxComboDelay = 1;
    
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;
    public Vector3 offset; 
    private float _attackCooldownTimer = 0f;

    void Update()
    {
        // Сброс анимаций после завершения
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime > 0.7f && stateInfo.IsName("hit1"))
            anim.SetBool("hit1", false);
        if (stateInfo.normalizedTime > 0.7f && stateInfo.IsName("hit2"))
            anim.SetBool("hit2", false);
        if (stateInfo.normalizedTime > 0.7f && stateInfo.IsName("hit3"))
        {
            anim.SetBool("hit3", false);
            noOfClicks = 0;
        }

        // Сброс комбо при задержке
        if (Time.time - lastClickedTime > maxComboDelay)
            noOfClicks = 0;

        // Кулдаун атаки
        if (_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;

        // Обработка клика
        if (Input.GetMouseButtonDown(0))
            OnClick();
    }

    void OnClick()
    {
        lastClickedTime = Time.time;
        noOfClicks++;

        if (noOfClicks == 1)
            anim.SetBool("hit1", true);
        else if (noOfClicks == 2)
            anim.SetBool("hit2", true);
        else if (noOfClicks == 3)
            anim.SetBool("hit3", true);

        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);
    }
    public void PerformAttack()
    {
        if (!CanAttack()) return;

        _attackCooldownTimer = attackCooldown;

        // Позиция атаки - немного впереди игрока
        Vector3 attackPosition = transform.position + offset;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, attackRange, enemyLayer);
        
        foreach (Collider enemy in hitEnemies)
        {
            // Здесь можно вызвать метод у врага
            enemy.GetComponent<BossHealth>()?.TakeDamage(attackDamage);
        }
    }

    public bool CanAttack() => _attackCooldownTimer <= 0;

    // Визуализация радиуса атаки в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + offset;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}