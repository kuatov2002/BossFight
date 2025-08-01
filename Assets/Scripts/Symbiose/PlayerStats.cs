using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float baseDamage = 10f;
    public float baseSpeed = 5f;
    
    [Header("Current Stats")]
    public float currentHealth;
    public float currentDamage;
    public float currentSpeed;
    
    private SymbioteSystem symbioteSystem;
    
    private void Awake()
    {
        symbioteSystem = GetComponent<SymbioteSystem>();
        ResetStats();
    }
    
    private void ResetStats()
    {
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
    }
    
    public void ResetModifiers()
    {
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentHealth = baseHealth;
    }
}