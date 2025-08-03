using UnityEngine;

public class WingScaler : MonoBehaviour
{
    [Header("Основные настройки")]
    [Tooltip("Аниматор, за которым нужно следить. Если оставить пустым, скрипт попытается найти его на этом же объекте.")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Точное имя состояния анимации полета/прыжка в Animator Controller")]
    [SerializeField] private string flightStateName = "Jump";

    [Header("Настройки масштабирования")]
    [Tooltip("Объект, который будет использоваться как центр масштабирования (пивот).")]
    [SerializeField] private Transform scalePivot; // <-- НОВОЕ ПОЛЕ

    [Tooltip("Обычный размер крыльев")]
    [SerializeField] private Vector3 normalScale = Vector3.one;

    [Tooltip("Увеличенный размер крыльев во время полета")]
    [SerializeField] private Vector3 flightScale = new Vector3(1.5f, 1.5f, 1.5f);
    
    [Tooltip("Скорость изменения размера")]
    [SerializeField] private float scaleSpeed = 5f;

    private Vector3 _targetScale;

    void Start()
    {
        // Поиск аниматора
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("WingScaler: Компонент Animator не найден! Скрипт отключен.", this);
            enabled = false;
            return;
        }

        // Проверка наличия пивота
        if (scalePivot == null)
        {
            Debug.LogError("WingScaler: Не назначен объект-пивот (Scale Pivot)! Масштабирование будет происходить от центра объекта. Назначьте пивот для корректной работы.", this);
        }

        // Установка начального размера
        transform.localScale = normalScale;
        _targetScale = normalScale;
    }

    void Update()
    {
        // Получаем информацию о текущем состоянии аниматора
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Определяем целевой размер в зависимости от состояния анимации
        if (stateInfo.IsName(flightStateName))
        {
            _targetScale = flightScale;
        }
        else
        {
            _targetScale = normalScale;
        }

        // Плавно интерполируем к целевому размеру
        Vector3 newScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * scaleSpeed);

        // Применяем масштабирование относительно пивота
        ScaleAround(scalePivot, newScale);
    }

    /// <summary>
    /// Масштабирует объект вокруг указанной точки (пивота).
    /// </summary>
    /// <param name="pivot">Точка, вокруг которой происходит масштабирование.</param>
    /// <param name="newScale">Новый размер объекта.</param>
    private void ScaleAround(Transform pivot, Vector3 newScale)
    {
        // Если пивот не назначен, используем стандартное масштабирование
        if (pivot == null)
        {
            transform.localScale = newScale;
            return;
        }

        Vector3 pivotPoint = pivot.position;
        Vector3 directionToPivot = transform.position - pivotPoint;

        // Рассчитываем, насколько изменился размер
        // Примечание: это работает корректно для равномерного масштабирования (когда x, y, z меняются одинаково)
        float relativeScaleChange = newScale.x / transform.localScale.x;

        // Новая позиция должна компенсировать сдвиг от масштабирования
        Vector3 finalPosition = pivotPoint + directionToPivot * relativeScaleChange;
        
        // Применяем новый размер и скорректированную позицию
        transform.localScale = newScale;
        transform.position = finalPosition;
    }
}