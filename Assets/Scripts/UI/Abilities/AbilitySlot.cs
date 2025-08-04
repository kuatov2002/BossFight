using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    [Header("UI References")]
    public Image abilityIcon;
    public Image cooldownOverlay;
    public Image lockOverlay;
    
    [Header("Ability Settings")]
    public float cooldownTime = 5f;
    public KeyCode activationKey = KeyCode.Space;
    
    private bool isOnCooldown = false;
    private float currentCooldown;
    
    void Start()
    {
        // Инициализация UI
        ResetCooldownUI();
    }
    
    void Update()
    {
        // Проверка активации способности
        if (Input.GetKeyDown(activationKey) && !isOnCooldown)
        {
            ActivateAbility();
        }
        
        // Обновление кулдауна
        if (isOnCooldown)
        {
            UpdateCooldown();
        }
    }

    public void Unlock()
    {
        lockOverlay.gameObject.SetActive(false);
    }
    public void ActivateAbility()
    {
        // Здесь ваш код активации способности
        Debug.Log("Ability activated!");
        
        // Запуск кулдауна
        StartCoroutine(StartCooldown());
    }
    
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = cooldownTime;
        
        while (currentCooldown > 0)
        {
            UpdateCooldownUI();
            yield return null;
        }
        
        ResetCooldownUI();
        isOnCooldown = false;
    }
    
    private void UpdateCooldown()
    {
        currentCooldown -= Time.deltaTime;
        UpdateCooldownUI();
    }
    
    private void UpdateCooldownUI()
    {
        // Обновление оверлея (затемнение)
        float fillAmount = currentCooldown / cooldownTime;
        cooldownOverlay.fillAmount = fillAmount;
    }
    
    private void ResetCooldownUI()
    {
        cooldownOverlay.fillAmount = 0f;
    }
}