using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public BossHP bossHp;
    public Image playerHP;

    public DialogueManager dialogueManager;

    public AbilityManager abilityManager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerHP(float currentHP, float maxHP)
    {
        playerHP.fillAmount = currentHP / maxHP;
    }


    public void StartDialogue(string[] lines)
    {
        dialogueManager.StartDialogue(lines);
    }

    public void UnlockAbility(int index)
    {
        abilityManager.Unlock(index);
    }

    public void ActiveAbility(int index)
    {
        abilityManager.ActivateAbility(index);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
