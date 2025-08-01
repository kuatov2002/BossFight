using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbiote", menuName = "Symbiote/SymbiotePart")]
public class SymbiotePart : ScriptableObject
{
    public string partName;
    public string description;
    public BodySlot.SlotType slotType;
    public BodySlot.MobeType mobeType;
    public Sprite icon;

    public float jumpForceModifier;
    public float speedModifier;
}