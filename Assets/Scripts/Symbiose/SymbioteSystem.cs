using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SymbioteSystem : MonoBehaviour
{
    [Header("Body Slots")]
    public BodySlot[] bodySlots;
    
    [Header("References")]
    public PlayerStats playerStats;
    
    private void Awake()
    {
        InitializeBodySlots();
    }
    
    private void InitializeBodySlots()
    {
        /*bodySlots = new BodySlot[6];
        bodySlots[0] = new BodySlot(BodySlot.SlotType.Head);
        bodySlots[1] = new BodySlot(BodySlot.SlotType.LeftArm);
        bodySlots[2] = new BodySlot(BodySlot.SlotType.RightArm);
        bodySlots[3] = new BodySlot(BodySlot.SlotType.Back);
        bodySlots[4] = new BodySlot(BodySlot.SlotType.LeftLeg);
        bodySlots[5] = new BodySlot(BodySlot.SlotType.RightLeg);*/
        
        List<SymbiotePart> equippedParts = new List<SymbiotePart>();
        
        // Собираем все экипированные части
        foreach (BodySlot slot in bodySlots)
        {
            if (slot.isOccupied)
            {
                equippedParts.Add(slot.currentPart);
            }
        }
        CheckCombinations(equippedParts);
    }
    
    public bool EquipSymbiote(SymbiotePart part)
    {
        // Находим подходящий слот
        BodySlot targetSlot = bodySlots.FirstOrDefault(slot => slot.slotType == part.slotType);
        
        if (targetSlot == null || !targetSlot.CanEquip(part))
        {
            Debug.Log($"Cannot equip {part.partName} to this slot");
            return false;
        }
        
        // Экипируем часть
        targetSlot.EquipPart(part);
        
        // Применяем эффекты
        ApplySymbioteEffects();
        
        Debug.Log($"Equipped {part.partName} to {part.slotType}");
        return true;
    }
    
    private void ApplySymbioteEffects()
    {
        // Сброс эффектов
        playerStats.ResetModifiers();
        
        List<SymbiotePart> equippedParts = new List<SymbiotePart>();
        
        // Собираем все экипированные части
        foreach (BodySlot slot in bodySlots)
        {
            if (slot.isOccupied)
            {
                equippedParts.Add(slot.currentPart);
            }
        }
        
        // Проверяем комбинации
        CheckCombinations(equippedParts);
    }
    
    private void CheckCombinations(List<SymbiotePart> parts)
    {
        bool hasCangaroo = parts.Any(p => p.mobeType == BodySlot.MobeType.Cangaroo);
        if (hasCangaroo)
        {
            // Применяем специальный эффект
            //playerStats.canCreateSteamTrail = true;
            Debug.Log("Комбинация активирована: Супер прыжок");
        }
        
        // Добавьте больше комбинаций по аналогии
    }
    
    public List<SymbiotePart> GetEquippedParts()
    {
        return bodySlots.Where(slot => slot.isOccupied)
            .Select(slot => slot.currentPart)
            .ToList();
    }
    
    public void RemoveSymbiote(BodySlot.SlotType slotType)
    {
        BodySlot slot = bodySlots.FirstOrDefault(s => s.slotType == slotType);
        if (slot != null)
        {
            slot.RemovePart();
            ApplySymbioteEffects();
        }
    }
}