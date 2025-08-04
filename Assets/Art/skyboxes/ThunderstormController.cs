using System.Collections;
using UnityEngine;

public class ThunderstormController : MonoBehaviour
{
    [Header("Настройки Вспышки")]
    [Tooltip("Насколько яркой будет вспышка (значение Exposure). Стандартное значение ~1.")]
    [SerializeField] private float lightningExposure = 5f;

    [Tooltip("Длительность одной вспышки в секундах.")]
    [SerializeField] private float flashDuration = 0.1f;

    [Tooltip("Дополнительный источник света (например, Directional Light), который будет включаться во время вспышки. Необязательно.")]
    [SerializeField] private Light lightningLightSource;


    [Header("Настройки Времени")]
    [Tooltip("Минимальное время между сериями вспышек.")]
    [SerializeField] private float minTimeBetweenFlashes = 5f;

    [Tooltip("Максимальное время между сериями вспышек.")]
    [SerializeField] private float maxTimeBetweenFlashes = 20f;


    [Header("Настройки Серии Вспышек")]
    [Tooltip("Максимальное количество быстрых вспышек в одной серии (для эффекта мерцания).")]
    [SerializeField] private int maxFlashesInSequence = 3;

    [Tooltip("Максимальная задержка между быстрыми вспышками в одной серии.")]
    [SerializeField] private float maxDelayBetweenSubFlashes = 0.2f;


    [Header("Настройки Звука")]
    [Tooltip("Источник звука для раскатов грома.")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Массив со звуками грома. Будет проигрываться случайный.")]
    [SerializeField] private AudioClip[] thunderSounds;


    private float originalExposure; // Здесь сохраним оригинальное значение Exposure

    void Start()
    {
        // Проверяем, что Skybox вообще назначен в настройках рендера
        if (RenderSettings.skybox == null)
        {
            Debug.LogError("Не назначен Skybox в настройках рендера (Window -> Rendering -> Lighting -> Environment). Скрипт не может работать.", this);
            enabled = false; // Отключаем скрипт
            return;
        }

        // Пытаемся получить AudioSource, если он не назначен, и добавляем, если его нет
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if(audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Сохраняем начальное значение Exposure, чтобы вернуть его после вспышки
        originalExposure = RenderSettings.skybox.GetFloat("_Exposure");

        // Запускаем главный цикл грозы
        StartCoroutine(ThunderstormRoutine());
    }

    private IEnumerator ThunderstormRoutine()
    {
        // Бесконечный цикл, который будет работать, пока объект активен
        while (true)
        {
            // Ждем случайное количество времени до следующей серии вспышек
            float delay = Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes);
            yield return new WaitForSeconds(delay);

            // Запускаем корутину, которая отвечает за саму вспышку (или серию вспышек)
            StartCoroutine(LightningFlash());
        }
    }

    private IEnumerator LightningFlash()
    {
        // Определяем, сколько быстрых вспышек будет в этой серии
        int flashCount = Random.Range(1, maxFlashesInSequence + 1);

        for (int i = 0; i < flashCount; i++)
        {
            // --- ВСПЫШКА ВКЛ ---
            RenderSettings.skybox.SetFloat("_Exposure", lightningExposure);
            if (lightningLightSource != null) lightningLightSource.enabled = true;

            yield return new WaitForSeconds(flashDuration);

            // --- ВСПЫШКА ВЫКЛ ---
            RenderSettings.skybox.SetFloat("_Exposure", originalExposure);
            if (lightningLightSource != null) lightningLightSource.enabled = false;

            // Небольшая случайная задержка перед следующей вспышкой в серии
            float subFlashDelay = Random.Range(0.05f, maxDelayBetweenSubFlashes);
            yield return new WaitForSeconds(subFlashDelay);
        }
        
        // После серии вспышек проигрываем звук грома
        PlayThunderSound();
    }

    private void PlayThunderSound()
    {
        // Проверяем, есть ли у нас звуки для проигрывания
        if (audioSource != null && thunderSounds.Length > 0)
        {
            // Выбираем случайный клип из массива
            AudioClip randomThunderClip = thunderSounds[Random.Range(0, thunderSounds.Length)];
            // Проигрываем его
            audioSource.PlayOneShot(randomThunderClip);
        }
    }

    // При выключении объекта возвращаем Exposure в норму на всякий случай
    void OnDisable()
    {
        if (RenderSettings.skybox != null)
        {
             RenderSettings.skybox.SetFloat("_Exposure", originalExposure);
        }
    }
}