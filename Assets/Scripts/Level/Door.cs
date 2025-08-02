using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
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
        // Можно добавить проверку на игрока:
        if (other.CompareTag("Player")) LoadRandomSceneFromBuild();
    }

    private void LoadRandomSceneFromBuild()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        
        if (sceneCount <= 0)
        {
            Debug.LogError("Нет сцен в Build Settings!");
            return;
        }

        // Если только одна сцена, загружаем её же (или можно не загружать ничего)
        if (sceneCount == 1)
        {
            Debug.Log("В билде только одна сцена, загружаю её...");
            SceneManager.LoadScene(0);
            return;
        }

        // Получаем индекс текущей сцены, чтобы не загружать ту же самую
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int randomSceneIndex;
        
        // Генерируем случайный индекс, отличный от текущей сцены
        do
        {
            randomSceneIndex = Random.Range(0, sceneCount);
        } 
        while (randomSceneIndex == currentSceneIndex && sceneCount > 1);

        // Получаем имя сцены по индексу
        string scenePath = SceneUtility.GetScenePathByBuildIndex(randomSceneIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        
        Debug.Log($"Загрузка случайной сцены: {sceneName} (индекс: {randomSceneIndex})");
        
        // Загружаем случайную сцену
        SceneManager.LoadScene(randomSceneIndex);
    }
}