using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public List<AbilitySlot> abilitySlots;

    public void ActivateAbility(int index)
    {
        if (index >= 0 && index < abilitySlots.Count && !abilitySlots[index].isLocked)
        {
            abilitySlots[index].ActivateAbility();
        }
    }

    public void Unlock(int index)
    {
        if (index >= 0 && index < abilitySlots.Count)
        {
            abilitySlots[index].Unlock();
        }
    }
}