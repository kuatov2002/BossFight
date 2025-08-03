using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    public int goToRoom = 0;
    
    private void Start()
    {
        gameObject.SetActive(false);
        // Включаем коллайдер как триггер
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Проверяем, есть ли сцены в билде
        if (SceneManager.sceneCountInBuildSettings <= 1)
        {
            Debug.LogWarning("В билде только одна сцена или они не добавлены!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {    
        if (other.CompareTag("Player"))
        {
            LoadRandomSceneFromBuild();
        }
    }

    private void LoadRandomSceneFromBuild()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        
        if (sceneCount <= 0)
        {
            Debug.LogError("Нет сцен в Build Settings!");
            return;
        }
        
        // Загружаем случайную сцену
        SceneManager.LoadScene(goToRoom);
    }
}