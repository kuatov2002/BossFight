using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public bool isHaveShield = true;

    public float maxHealth = 100f;
    private float health;

    // --- Новые поля для кулдауна щита ---
    private bool _isShieldOnCooldown = false;
    private float _shieldCooldownDuration = 5.0f;
    private float _shieldCooldownEndTime = 0f;

    private void Start()
    {
        health = maxHealth;
        UIManager.Instance.SetPlayerHP(health, maxHealth);
    }

    private void Update()
    {
        // --- Проверяем, не закончился ли кулдаун щита ---
        if (_isShieldOnCooldown && Time.time >= _shieldCooldownEndTime)
        {
            _isShieldOnCooldown = false;
            // Можно добавить логику, если нужно что-то сделать по окончании кулдауна
            // Debug.Log("Щит снова доступен");
        }
    }

    public bool TakeDamage(float damage)
    {
        // --- Если есть щит и он не на кулдауне, урон игнорируется ---
        if (isHaveShield && !_isShieldOnCooldown)
        {
            Debug.Log("Урон заблокирован щитом!");
            
            // --- Активируем кулдаун щита ---
            _isShieldOnCooldown = true;
            _shieldCooldownEndTime = Time.time + _shieldCooldownDuration;
            
            // --- Щит использован, сбрасываем флаг наличия щита ---
            // Если щит одноразовый, раскомментируйте следующую строку:
            // isHaveShield = false; 
            UIManager.Instance.ActiveAbility(0);
            return false; // Урон не наносится
        }

        health -= damage;
        Debug.Log($"Игрок получил {damage} урона. Осталось здоровья: {health}");

        if (health <= 0)
        {
            health = 0;
            Die();
        }
        UIManager.Instance.SetPlayerHP(health, maxHealth);
        return true;
    }

    void Die()
    {
        Debug.Log("Игрок погиб!");
        // Здесь логика смерти игрока
    }

    // --- Вспомогательный метод для проверки, доступен ли щит ---
    public bool IsShieldAvailable()
    {
        return isHaveShield && !_isShieldOnCooldown;
    }
}

// Интерфейс для объектов, которые могут получать урона
public interface IDamageable
{
    bool TakeDamage(float damage);
}