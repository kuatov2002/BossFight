using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;

    private float attackCooldownTimer = 0f;

    void Update()
    {
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
    }

    public void PerformAttack()
    {
        if (!CanAttack()) return;

        attackCooldownTimer = attackCooldown;

        // Позиция атаки - немного впереди игрока
        Vector3 attackPosition = transform.position + transform.forward * 1f;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, attackRange, enemyLayer);
        
        foreach (Collider enemy in hitEnemies)
        {
            // Здесь можно вызвать метод у врага
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
            Debug.Log($"Атакован враг: {enemy.name}");
        }
    }

    public bool CanAttack() => attackCooldownTimer <= 0;

    // Визуализация радиуса атаки в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + transform.forward * 1f;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}