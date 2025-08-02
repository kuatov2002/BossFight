using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float health = 100f;
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
        BossActions.onBossDied.Invoke();
        Debug.Log("Босс погиб!");
        // Здесь логика смерти игрока
    }
}
