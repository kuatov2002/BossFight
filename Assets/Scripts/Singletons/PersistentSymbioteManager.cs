using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSymbioteManager : MonoBehaviour
{
    public static PersistentSymbioteManager Instance;

    // Синглтон для доступа к менеджеру из других скриптов
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Подписываемся на событие загрузки сцены
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // Уничтожаем дубликаты
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Вызывается каждый раз после загрузки новой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
         StartCoroutine(WaitForPlayerAndApplyData());
    }

    // Ждем один кадр, чтобы объекты сцены (включая игрока) успели инициализироваться
    private IEnumerator WaitForPlayerAndApplyData()
    {
        yield return null; 

        // Найдем игрока в новой сцене
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SymbioteSystem playerSymbioteSystem = player.GetComponent<SymbioteSystem>();
            if (playerSymbioteSystem != null)
            {
                // Применяем сохраненные данные к новому экземпляру SymbioteSystem
                ApplySavedSymbioteData(playerSymbioteSystem);
                // Или делаем сам SymbioteSystem DontDestroyOnLoad и пере-инициализируем его
                // ReinitializeSymbioteSystem(playerSymbioteSystem); 
            }
            else
            {
                Debug.LogWarning("PersistentSymbioteManager: Player found but SymbioteSystem component is missing.");
            }
        }
        else
        {
            Debug.LogWarning("PersistentSymbioteManager: Player not found in the loaded scene.");
        }
    }


    // --- Хранилище данных ---
    // Храним список экипированных частей (их ScriptableObject)
    // Предполагаем, что ScriptableObjects находятся в Resources или управляются другим способом
    // Для простоты храним их напрямую. В реальном проекте лучше хранить уникальные идентификаторы (ID или имена)
    private List<SymbiotePart> _savedEquippedParts = new List<SymbiotePart>();

    // --- Методы для сохранения и применения ---
    public void SaveSymbioteData(SymbioteSystem symbioteSystem)
    {
        _savedEquippedParts.Clear();
        if (symbioteSystem != null)
        {
            _savedEquippedParts = new List<SymbiotePart>(symbioteSystem.GetEquippedParts()); // Копируем список
            // Debug.Log($"Saved {_savedEquippedParts.Count} parts.");
        }
    }

    private void ApplySavedSymbioteData(SymbioteSystem symbioteSystem)
    {
        if (symbioteSystem == null || _savedEquippedParts == null) return;

        // Предполагаем, что слоты инициализируются в Awake/Start SymbioteSystem
        // и что у нас есть доступ к оригинальным ScriptableObject частей
        // (либо они находятся в Resources, либо передаются как-то иначе)
        // Для этого примера предположим, что _savedEquippedParts содержит корректные ссылки

        // Сначала очистим текущие слоты (на всякий случай, если что-то было)
        // Это зависит от вашей логики инициализации SymbioteSystem в новой сцене
        // Если SymbioteSystem.Awake() уже инициализирует слоты как пустые, этот шаг может быть не нужен
        // foreach(var slot in symbioteSystem.bodySlots)
        // {
        //     if(slot.isOccupied) slot.RemovePart();
        // }

        // Экипируем сохраненные части
        foreach (var part in _savedEquippedParts)
        {
            // Debug.Log($"Trying to equip saved part: {part?.partName}");
            if (part != null)
            {
                 // EquipSymbiote внутри вызывает CheckCombinations
                 bool success = symbioteSystem.EquipSymbiote(part);
                 if (!success)
                 {
                     Debug.LogWarning($"Failed to re-equip part {part.partName} in the new scene.");
                 }
            }
        }
        Debug.Log($"Applied {_savedEquippedParts.Count} saved parts to the player in the new scene.");
    }
}
