using System;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    private void Start()
    {
        _health = maxHealth;
        UIManager.Instance.bossHp.SetHP(_health, maxHealth);
        UIManager.Instance.bossHp.SetName(bossName);
    }

    public float maxHealth = 100f;
    public string bossName;
    private float _health;
    
    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log($"Босс получил {damage} урона. Осталось здоровья: {_health}");
        
        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
        
        UIManager.Instance.bossHp.SetHP(_health, maxHealth);
    }
    
    void Die()
    {
        BossActions.onBossDied.Invoke();
        Debug.Log("Босс погиб!");
        // Здесь логика смерти игрока
    }
}
