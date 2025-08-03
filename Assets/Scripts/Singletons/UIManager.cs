using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public BossHP bossHp;
    public Image playerHP;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
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
}
