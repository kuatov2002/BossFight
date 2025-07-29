using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackTrigger : MonoBehaviour
{
    public float damage = 50f;
    private readonly List<Collider> currentlyInside = new List<Collider>();
    
    private void OnTriggerEnter(Collider other)
    {
        // Добавляем в список объектов внутри триггера
        if (!currentlyInside.Contains(other))
            currentlyInside.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // Убираем из списка объектов внутри триггера
        currentlyInside.Remove(other);
    }

    private void OnDisable()
    {
        // Наносим урон всем объектам, которые находятся внутри триггера
        foreach (Collider col in currentlyInside)
        {
            if (col == null) continue; // Проверка на случай, если объект был уничтожен
            
                
            // Проверяем, является ли объект игроком или врагом
            if (col.CompareTag("Player"))
            {
                // Наносим урон
                IDamageable damageable = col.GetComponent<IDamageable>();
                damageable?.TakeDamage(damage);
            }
        }
        
        // Очищаем списки
        currentlyInside.Clear();
    }
}