using UnityEngine;

[System.Serializable]
public class SymbioteEffect
{
    public string description;
    public float damageModifier = 1f;
    public float speedModifier = 1f;
    public float healthModifier = 1f;
}

[CreateAssetMenu(fileName = "NewSymbiote", menuName = "Symbiote/SymbiotePart")]
public class SymbiotePart : ScriptableObject
{
    public string partName;
    public string description;
    public BodySlot.SlotType slotType;
    public BodySlot.MobeType mobeType;
    public Sprite icon;
}