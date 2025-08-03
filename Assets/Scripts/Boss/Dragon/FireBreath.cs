using UnityEngine;

public class FireBreath : MonoBehaviour
{
    public float speed = 10f;           // Скорость полета огня
    public float lifetime = 2.5f;         // Время жизни
    public int damage = 10;             // Урон
    public LayerMask targetLayers;      // Слои целей
    
    private Vector3 direction;
    private Transform target;
    private float timer = 0f;

    public float knockForce = 0f;
    void Start()
    {
        direction = transform.forward;
    }
    
    void Update()
    {
        // Если есть цель, следуем за ней
        if (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            transform.position += directionToTarget * speed * Time.deltaTime;
        }
        else
        {
            // Иначе летим по прямой
            transform.position += direction * speed * Time.deltaTime;
        }
        
        // Таймер жизни
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }
    
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Проверяем, попали ли мы в цель
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            // Наносим урон
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            var playerMovementScript=other.GetComponent<PlayerMovement>();
            Vector3 knockDirection = (other.transform.position - transform.position).normalized;
            // Можно немного поднять игрока вверх
            knockDirection.y = 2f; // Настройте по желанию
            playerMovementScript.Knockback(knockDirection);
            
            // Уничтожаем огненное дыхание
            Destroy(gameObject);
        }
    }
}