using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;
    public Vector3 offset; 
    private float _attackCooldownTimer = 0f;

    void Update()
    {
        if (_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;
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