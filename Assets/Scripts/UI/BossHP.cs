using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHP : MonoBehaviour
{
    public TextMeshProUGUI bossName;
    public Image bossHPImage;
    public TextMeshProUGUI bossHPNum;

    public void SetHP(float currentHP,float maxHP)
    {
        bossHPImage.fillAmount = currentHP / maxHP;
        bossHPNum.text = $"{currentHP}/{maxHP}";
    }
    public void SetName(string bossName)
    {
        this.bossName.text = bossName;
    }
}
