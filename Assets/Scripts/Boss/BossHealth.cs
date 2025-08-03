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
            Debug.Log("Здоровье босса достигло нуля, вызываю Die()");
            Die();
        }
    
        BossActions.onBossHit?.Invoke();
        UIManager.Instance.bossHp.SetHP(_health, maxHealth);
    }

    void Die()
    {
        Debug.Log("BossHealth.Die() вызван, отправляю событие onBossDied");
        BossActions.onBossDied?.Invoke();
        Debug.Log("Событие onBossDied отправлено!");
        Debug.Log("Босс погиб!");
    }
}
