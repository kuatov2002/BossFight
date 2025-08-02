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
                    Debug.Log($"Игрок столкнулся с высокой скоростью: {speed:F2} м/с");
                }
            }
        }
    }
}