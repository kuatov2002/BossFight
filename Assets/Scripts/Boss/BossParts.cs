using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossParts : MonoBehaviour
{
    [Header("Boss Parts")]
    [SerializeField] private List<SymbiotePart> bossParts;
    [SerializeField] private SymbioteSystem playerSymbioteSystem;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Die();
        }
    }

    private void Die()
    {
        if (bossParts != null && bossParts.Count > 0 && playerSymbioteSystem != null)
        {
            // Выбираем случайную часть из списка
            int randomIndex = Random.Range(0, bossParts.Count);
            SymbiotePart randomPart = bossParts[randomIndex];
            
            // Даем часть игроку
            bool success = playerSymbioteSystem.EquipSymbiote(randomPart);
            
            if (success)
            {
                Debug.Log($"Босс дал игроку часть: {randomPart.partName}");
                // Можно добавить визуальные эффекты или звук получения части
            }
            else
            {
                Debug.Log($"Не удалось получить часть: {randomPart.partName}");
            }
        }
        else
        {
            Debug.LogWarning("BossParts: Не настроены части босса или система симбиота игрока");
        }
    }
}