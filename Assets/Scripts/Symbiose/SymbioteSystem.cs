using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class SymbioteSystem : MonoBehaviour
{
    [Header("Body Slots")]
    public BodySlot[] bodySlots;
    
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;
    private void Awake()
    {
        InitializeBodySlots();
    }

    private void InitializeBodySlots()
    {
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
        // Проверяем, есть ли уже такая часть у игрока
        foreach (BodySlot slot in bodySlots)
        {
            if (slot.isOccupied && slot.currentPart == part)
            {
                Debug.Log($"У игрока уже есть часть: {part.partName}");
                return false;
            }
        }
    
        // Находим подходящий слот
        BodySlot targetSlot = bodySlots.FirstOrDefault(slot => slot.slotType == part.slotType);
    
        if (targetSlot == null || !targetSlot.CanEquip(part))
        {
            Debug.Log($"Cannot equip {part.partName} to this slot");
            return false;
        }
    
        // Экипируем часть
        targetSlot.EquipPart(part);
    
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
    
        Debug.Log($"Equipped {part.partName} to {part.slotType}");
        return true;
    }
    
    private void CheckCombinations(IEnumerable<SymbiotePart> parts)
    {
        var partList = parts.ToList();

        // Проверка на кенгуру-ноги (уже была)
        int kangarooLegs = partList.Count(p => p.mobeType == BodySlot.MobeType.Cangaroo && 
                                               (p.slotType == BodySlot.SlotType.LeftLeg || 
                                                p.slotType == BodySlot.SlotType.RightLeg));
    
        playerMovement.jumpForce = 8 + 2 * kangarooLegs;

        if (kangarooLegs >= 2)
        {
            playerMovement.jumpCount = 2;
            Debug.Log("Комбинация активирована: двойной прыжок");
        }
        else
        {
            playerMovement.jumpCount = 1; // сброс, если комбинация неактивна
        }

        // 🔍 Новая проверка: обе руки - паучьи
        bool hasSpiderLeftArm = partList.Any(p => p.slotType == BodySlot.SlotType.LeftArm && p.mobeType == BodySlot.MobeType.Spider);
        bool hasSpiderRightArm = partList.Any(p => p.slotType == BodySlot.SlotType.RightArm && p.mobeType == BodySlot.MobeType.Spider);

        if (hasSpiderLeftArm && hasSpiderRightArm)
        {
            Debug.Log("Комбинация активирована: обе руки — паучьи!");
            // Здесь можно добавить эффекты, бонусы и т.д.
            // Например: playerAttack.EnableWebShooting(true);
        }
        else
        {
            // Отключить эффект, если комбинация потеряна
            // playerAttack.EnableWebShooting(false);
        }
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
            List<SymbiotePart> equippedParts = new List<SymbiotePart>();
        
            // Собираем все экипированные части
            foreach (BodySlot bodySlot in bodySlots)
            {
                if (bodySlot.isOccupied)
                {
                    equippedParts.Add(slot.currentPart);
                }
            }
        
            // Проверяем комбинации
            CheckCombinations(equippedParts);
        }
    }
}