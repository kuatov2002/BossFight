using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Игрок получил {damage} урона. Осталось здоровья: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Игрок погиб!");
        // Здесь логика смерти игрока
    }
}