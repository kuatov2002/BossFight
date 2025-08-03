using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    private float health;

    private void Start()
    {
        health = maxHealth;
        UIManager.Instance.SetPlayerHP(health, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Игрок получил {damage} урона. Осталось здоровья: {health}");
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
        UIManager.Instance.SetPlayerHP(health, maxHealth);
    }
    
    void Die()
    {
        Debug.Log("Игрок погиб!");
        // Здесь логика смерти игрока
    }
}

// Интерфейс для объектов, которые могут получать урона
public interface IDamageable
{
    void TakeDamage(float damage);
}