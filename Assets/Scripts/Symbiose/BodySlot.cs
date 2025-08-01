using System;

[Serializable]
public class BodySlot
{
    public enum SlotType
    {
        Head,
        LeftArm,
        RightArm,
        Back,
        LeftLeg,
        RightLeg
    }

    public enum MobeType
    {
        Cangaroo,
    }
    public SlotType slotType;
    public SymbiotePart currentPart;
    public bool isOccupied => currentPart != null;
    
    public BodySlot(SlotType type)
    {
        slotType = type;
        currentPart = null;
    }
    
    public bool CanEquip(SymbiotePart part)
    {
        return part.slotType == slotType;
    }
    
    public void EquipPart(SymbiotePart part)
    {
        currentPart = part;
    }
    
    public void RemovePart()
    {
        currentPart = null;
    }
}