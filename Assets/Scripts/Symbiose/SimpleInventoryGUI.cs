using UnityEngine;
using System.Linq;

public class SimpleInventoryGUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SymbioteSystem symbioteSystem;
    
    private bool showInventory = false;
    private Rect windowRect = new Rect(100, 100, 400, 500);
    
    private void Update()
    {
        // Открытие/закрытие инвентаря по клавише E
        if (Input.GetKeyDown(KeyCode.E))
        {
            showInventory = !showInventory;
        }
    }
    
    private void OnGUI()
    {
        if (showInventory && symbioteSystem != null)
        {
            windowRect = GUI.Window(0, windowRect, InventoryWindow, "СИМБИОТ ИНВЕНТАРЬ");
        }
    }
    
    private void InventoryWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // Заголовок
        GUILayout.Label("=== ХАРАКТЕРИСТИКИ ===", GUI.skin.box);
        GUILayout.Space(10);
        
        // Отображение характеристик
        /*GUILayout.Label($"Здоровье: {playerStats.currentHealth:F0}/{playerStats.baseHealth:F0}");
        GUILayout.Label($"Урон: {playerStats.currentDamage:F1} (базовый: {playerStats.baseDamage:F1})");
        GUILayout.Label($"Скорость: {playerStats.currentSpeed:F1} (базовая: {playerStats.baseSpeed:F1})");*/
        
        GUILayout.Space(20);
        GUILayout.Label("=== СЛОТЫ ТЕЛА ===", GUI.skin.box);
        GUILayout.Space(10);
        
        // Отображение слотов
        if (symbioteSystem.bodySlots != null)
        {
            foreach (var slot in symbioteSystem.bodySlots)
            {
                string slotInfo = GetSlotInfo(slot);
                GUILayout.Label(slotInfo);
            }
        }
        
        GUILayout.Space(20);
        GUILayout.Label("=== ЭКИПИРОВАННЫЕ ЧАСТИ ===", GUI.skin.box);
        GUILayout.Space(10);
        
        // Отображение экипированных частей
        var equippedParts = symbioteSystem.GetEquippedParts();
        if (equippedParts.Count > 0)
        {
            foreach (var part in equippedParts)
            {
                GUILayout.Label($"• {part.partName} ({part.slotType})");
                if (!string.IsNullOrEmpty(part.description))
                {
                    GUILayout.Label($"  {part.description}");
                }
            }
        }
        else
        {
            GUILayout.Label("Нет экипированных частей");
        }
        
        GUILayout.Space(20);
        
        // Кнопка закрытия
        if (GUILayout.Button("Закрыть [E]"))
        {
            showInventory = false;
        }
        
        GUILayout.EndVertical();
        
        // Делаем окно перетаскиваемым
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
    
    private string GetSlotInfo(BodySlot slot)
    {
        if (slot.isOccupied)
        {
            return $"[{slot.slotType}] ЗАНЯТ: {slot.currentPart.partName} ({slot.currentPart.mobeType})";
        }
        else
        {
            return $"[{slot.slotType}] ПУСТОЙ";
        }
    }
}