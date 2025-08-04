using UnityEngine;

public class Web : MonoBehaviour
{
    [Tooltip("Минимальная скорость для срабатывания лога")]
    public float speedThreshold = 5f;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Получаем скорость объекта игрока
            Rigidbody Rb = GetComponent<Rigidbody>();
            
            if (Rb != null)
            {
                float speed = Rb.linearVelocity.magnitude;
                
                // Проверяем, превышает ли скорость пороговое значение
                if (speed >= speedThreshold)
                {
                    var _playerHealth = other.gameObject.GetComponent<PlayerHealth>();
                    // Можно немного поднять игрока вверх

                    if (_playerHealth.TakeDamage(10f))
                    {
                        // Вычисляем направление отбрасывания (от паука к игроку)
                        Vector3 knockDirection = (other.transform.position - transform.position).normalized;
                        var playerMovementScript = other.gameObject.GetComponent<PlayerMovement>();
                        knockDirection.y = 0.5f; // Настройте по желанию
                        playerMovementScript.Knockback(knockDirection*1.4f);
                    }
                    Debug.Log($"Игрок столкнулся с высокой скоростью: {speed:F2} м/с");
                }
            }
        }
    }
}