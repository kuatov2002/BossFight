using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public BossHP bossHp;
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
}
