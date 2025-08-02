using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GogoGaga.OptimizedRopesAndCables
{
    // Новый класс для настроек столкновений

    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour
    {
        public event Action OnPointsChanged;

        [Header("Rope Transforms")]
        [Tooltip("The rope will start at this point")]
        [SerializeField] private Transform startPoint;
        public Transform StartPoint => startPoint;

        [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
        [SerializeField] private Transform midPoint;
        public Transform MidPoint => midPoint;

        [Tooltip("The rope will end at this point")]
        [SerializeField] private Transform endPoint;
        public Transform EndPoint => endPoint;

        [Header("Rope Settings")]
        [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
        [Range(2, 100)] public int linePoints = 10;

        [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
        public float stiffness = 350f;

        [Tooltip("0 is no damping, 50 is a lot")]
        public float damping = 15f;

        [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
        public float ropeLength = 15;

        [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
        public float ropeWidth = 0.1f;

        [Header("Rational Bezier Weight Control")]
        [Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
        [Range(1, 15)] public float midPointWeight = 1f;
        private const float StartPointWeight = 1f;
        private const float EndPointWeight = 1f;

        [Header("Midpoint Position")]
        [Tooltip("Position of the midpoint along the line between start and end points")]
        [Range(0.25f, 0.75f)] public float midPointPosition = 0.5f;

        // --- НОВОЕ: Настройки столкновений ---
        [Header("Collision Settings")]
        public CollisionSettings collisionSettings;
        // --- КОНЕЦ НОВОГО ---

        private Vector3 currentValue;
        private Vector3 currentVelocity;
        private Vector3 targetValue; // Эта переменная теперь критична для проверки столкновений
        public Vector3 otherPhysicsFactors { get; set; }

        private const float valueThreshold = 0.01f;
        private const float velocityThreshold = 0.01f;

        private LineRenderer lineRenderer;
        private bool isFirstFrame = true;

        private Vector3 prevStartPointPosition;
        private Vector3 prevEndPointPosition;
        private float prevMidPointPosition;
        private float prevMidPointWeight;
        private float prevLineQuality;
        private float prevRopeWidth;
        private float prevstiffness;
        private float prevDampness;
        private float prevRopeLength;

        public bool IsPrefab => gameObject.scene.rootCount == 0;

        private void Start()
        {
            InitializeLineRenderer();
            if (AreEndPointsValid())
            {
                currentValue = GetMidPoint();
                targetValue = currentValue;
                currentVelocity = Vector3.zero;
                SetSplinePoint(); // Ensure initial spline point is set correctly
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                InitializeLineRenderer();
                if (AreEndPointsValid())
                {
                    RecalculateRope();
                    // SimulatePhysics вызывается внутри RecalculateRope косвенно через SetSplinePoint
                }
                else
                {
                    lineRenderer.positionCount = 0;
                }
            }
        }

        private void InitializeLineRenderer()
        {
            if (!lineRenderer)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }

        private void Update()
        {
            if (IsPrefab)
            {
                return;
            }
            if (AreEndPointsValid())
            {
                SetSplinePoint(); // Также вызывает CheckForCollisionsAndAdjust
                if (!Application.isPlaying && (IsPointsMoved() || IsRopeSettingsChanged()))
                {
                    SimulatePhysics(); // Также вызывает CheckForCollisionsAndAdjust
                    NotifyPointsChanged();
                }
                prevStartPointPosition = startPoint.position;
                prevEndPointPosition = endPoint.position;
                prevMidPointPosition = midPointPosition;
                prevMidPointWeight = midPointWeight;
                prevLineQuality = linePoints;
                prevRopeWidth = ropeWidth;
                prevstiffness = stiffness;
                prevDampness = damping;
                prevRopeLength = ropeLength;
            }
        }

        private bool AreEndPointsValid()
        {
            return startPoint != null && endPoint != null;
        }

        private void SetSplinePoint()
        {
            if (lineRenderer.positionCount != linePoints + 1)
            {
                lineRenderer.positionCount = linePoints + 1;
            }

            Vector3 mid = GetMidPoint();
            targetValue = mid; // Сохраняем целевую позицию перед коррекцией
            mid = currentValue;

            // --- НОВОЕ: Проверка столкновений перед отрисовкой ---
            // Корректируем currentValue, если targetValue или сам currentValue внутри коллайдера
            if (Application.isPlaying) // Проверки столкновений обычно нужны только во время игры
            {
                 CheckForCollisionsAndAdjust();
            }
            // --- КОНЕЦ НОВОГО ---

            if (midPoint != null)
            {
                midPoint.position = GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, midPointPosition, StartPointWeight, midPointWeight, EndPointWeight);
            }

            for (int i = 0; i < linePoints; i++)
            {
                Vector3 p = GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, i / (float)linePoints, StartPointWeight, midPointWeight, EndPointWeight);
                lineRenderer.SetPosition(i, p);
            }
            lineRenderer.SetPosition(linePoints, endPoint.position);
        }


        private float CalculateYFactorAdjustment(float weight)
        {
            //float k = 0.360f; //after testing this seemed to be a good value for most cases, more accurate k is available.
            float k = Mathf.Lerp(0.493f, 0.323f, Mathf.InverseLerp(1, 15, weight)); //K calculation that is more accurate, interpolates between precalculated values.
            float w = 1f + k * Mathf.Log(weight);
            return w;
        }

        private Vector3 GetMidPoint()
        {
            Vector3 startPointPosition = startPoint.position;
            Vector3 endPointPosition = endPoint.position;
            Vector3 midpos = Vector3.Lerp(startPointPosition, endPointPosition, midPointPosition);
            float yFactor = (ropeLength - Mathf.Min(Vector3.Distance(startPointPosition, endPointPosition), ropeLength)) / CalculateYFactorAdjustment(midPointWeight);
            midpos.y -= yFactor;
            return midpos;
        }

        private Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0, float w1, float w2)
        {
            Vector3 wp0 = w0 * p0;
            Vector3 wp1 = w1 * p1;
            Vector3 wp2 = w2 * p2;
            float denominator = w0 * Mathf.Pow(1 - t, 2) + 2 * w1 * (1 - t) * t + w2 * Mathf.Pow(t, 2);
            Vector3 point = (wp0 * Mathf.Pow(1 - t, 2) + wp1 * 2 * (1 - t) * t + wp2 * Mathf.Pow(t, 2)) / denominator;
            return point;
        }

        public Vector3 GetPointAt(float t)
        {
            if (!AreEndPointsValid())
            {
                Debug.LogError("StartPoint or EndPoint is not assigned.", gameObject);
                return Vector3.zero;
            }
            return GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
        }

        private void FixedUpdate()
        {
            if (IsPrefab)
            {
                return;
            }
            if (AreEndPointsValid())
            {
                if (!isFirstFrame)
                {
                    SimulatePhysics(); // Также вызывает CheckForCollisionsAndAdjust
                }
                isFirstFrame = false;
            }
        }

        private void SimulatePhysics()
        {
            float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
            Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
            currentVelocity = currentVelocity * dampingFactor + acceleration + otherPhysicsFactors;
            currentValue += currentVelocity * Time.fixedDeltaTime;

            // --- НОВОЕ: Проверка столкновений после обновления физики ---
            if (Application.isPlaying)
            {
                CheckForCollisionsAndAdjust();
            }
            // --- КОНЕЦ НОВОГО ---

            if (Vector3.Distance(currentValue, targetValue) < valueThreshold && currentVelocity.magnitude < velocityThreshold)
            {
                currentValue = targetValue;
                currentVelocity = Vector3.zero;
            }
        }

        // --- НОВЫЙ МЕТОД: Проверка столкновений и коррекция ---
        /// <summary>
        /// Проверяет, не находится ли точка провисания внутри коллайдера, и корректирует её позицию.
        /// </summary>
        private void CheckForCollisionsAndAdjust()
        {
            if (collisionSettings == null || !AreEndPointsValid()) return;

            Vector3 startPointPos = startPoint.position;
            Vector3 endPointPos = endPoint.position;

            // Определяем "идеальную" точку провисания (targetValue)
            Vector3 idealMidPoint = targetValue; // targetValue уже рассчитана в SetSplinePoint до вызова этого метода

            // Направление от начальной к конечной точке
            Vector3 lineDirection = (endPointPos - startPointPos).normalized;
            if (lineDirection == Vector3.zero) lineDirection = Vector3.up; // На случай, если точки совпадают

            // Вектор, перпендикулярный линии и направленный вниз (примерная плоскость провисания)
            Vector3 down = Vector3.down;
            Vector3 perpendicular = Vector3.Cross(lineDirection, down);
            if (perpendicular == Vector3.zero)
            {
                // Если линия вертикальна, используем правый вектор
                perpendicular = Vector3.right;
            }
            else
            {
                perpendicular = perpendicular.normalized;
            }

            // Смещение в сторону, чтобы избежать застревания в поверхности
            Vector3 offset = perpendicular * collisionSettings.collisionOffset;

            int attempts = 0;
            Vector3 adjustedMidPoint = currentValue; // Начинаем с текущей позиции

            while (attempts < collisionSettings.maxAdjustmentAttempts)
            {
                attempts++;

                // Проверяем, находится ли скорректированная точка внутри коллайдера
                Vector3 checkPosition = adjustedMidPoint + offset;

                // Создаем луч от идеальной точки к проверочной точке
                Vector3 rayOrigin = idealMidPoint;
                Vector3 rayDirection = checkPosition - idealMidPoint;
                float rayDistance = rayDirection.magnitude;
                rayDirection.Normalize();

                // Если направление нулевое (редкий случай), пропускаем проверку на этом шаге
                if (rayDirection == Vector3.zero)
                {
                     //Debug.LogWarning("Ray direction is zero, skipping collision check step.");
                     break;
                }


                // Выполняем рейкаст
                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, rayDistance, collisionSettings.collisionLayerMask))
                {
                    // Если луч попал в коллайдер, корректируем точку провисания
                    // Перемещаем её немного назад от точки столкновения
                    adjustedMidPoint = hitInfo.point - rayDirection * collisionSettings.collisionOffset;

                    // Простая коррекция по Y: убедиться, что точка не выше идеальной
                    // Это помогает избежать "выпрыгивания" вверх при столкновениях с боковыми поверхностями
                    // if (adjustedMidPoint.y > idealMidPoint.y)
                    // {
                    //     adjustedMidPoint.y = idealMidPoint.y;
                    // }

                     //Debug.Log($"Collision detected at {hitInfo.point}. Adjusting mid point to {adjustedMidPoint}. Attempt: {attempts}");
                     // Можно добавить более сложную логику скольжения здесь, если enableSliding == true

                     // Простая попытка: если точка скорректирована, выходим из цикла.
                     // Для более сложного поведения можно продолжать итерации.
                     // Пока что остановимся на одной коррекции за фрейм для стабильности.
                     break;
                }
                else
                {
                    // Если на этом шаге столкновения нет, точка считается безопасной
                    //Debug.Log($"No collision detected. Mid point is valid at {adjustedMidPoint}. Attempts: {attempts}");
                    break;
                }
            }

            // Применяем скорректированную позицию
            currentValue = adjustedMidPoint;
        }
        // --- КОНЕЦ НОВОГО МЕТОДА ---


        private void OnDrawGizmos()
        {
            if (!AreEndPointsValid())
                return;

            Vector3 midPos = GetMidPoint();
            // Uncomment if you need to visualize midpoint
            // Gizmos.color = Color.red;
            // Gizmos.DrawSphere(midPos, 0.2f);

            // --- НОВОЕ: Визуализация для отладки ---
            if (Application.isPlaying && collisionSettings != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(currentValue, 0.1f); // Текущая скорректированная точка провисания

                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(targetValue, 0.1f); // Идеальная точка провисания

                // Визуализация направления коррекции
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(targetValue, currentValue);

                // Визуализация рейкаста (пример)
                 Vector3 startPointPos = startPoint ? startPoint.position : transform.position;
                 Vector3 endPointPos = endPoint ? endPoint.position : transform.position;
                 Vector3 lineDirection = (endPointPos - startPointPos).normalized;
                 if(lineDirection != Vector3.zero){
                     Vector3 down = Vector3.down;
                     Vector3 perpendicular = Vector3.Cross(lineDirection, down);
                     if (perpendicular == Vector3.zero) { perpendicular = Vector3.right; } else { perpendicular = perpendicular.normalized; }
                     Vector3 offset = perpendicular * (collisionSettings?.collisionOffset ?? 0.05f);
                     Vector3 checkPosition = currentValue + offset;
                     Gizmos.color = Color.blue;
                     Gizmos.DrawLine(targetValue, checkPosition);
                     Gizmos.DrawSphere(checkPosition, 0.05f);
                 }

            }
            // --- КОНЕЦ НОВОГО ---
        }

        // New API methods for setting start and end points
        // with instantAssign parameter to recalculate the rope immediately, without
        // animating the rope to the new position.
        // When newStartPoint or newEndPoint is null, the rope will be recalculated immediately
        public void SetStartPoint(Transform newStartPoint, bool instantAssign = false)
        {
            startPoint = newStartPoint;
            prevStartPointPosition = startPoint == null ? Vector3.zero : startPoint.position;
            if (instantAssign || newStartPoint == null)
            {
                RecalculateRope();
            }
            NotifyPointsChanged();
        }

        public void SetMidPoint(Transform newMidPoint, bool instantAssign = false)
        {
            midPoint = newMidPoint;
            prevMidPointPosition = midPoint == null ? 0.5f : midPointPosition;
            if (instantAssign || newMidPoint == null)
            {
                RecalculateRope();
            }
            NotifyPointsChanged();
        }

        public void SetEndPoint(Transform newEndPoint, bool instantAssign = false)
        {
            endPoint = newEndPoint;
            prevEndPointPosition = endPoint == null ? Vector3.zero : endPoint.position;
            if (instantAssign || newEndPoint == null)
            {
                RecalculateRope();
            }
            NotifyPointsChanged();
        }

        public void RecalculateRope()
        {
            if (!AreEndPointsValid())
            {
                lineRenderer.positionCount = 0;
                return;
            }
            currentValue = GetMidPoint();
            targetValue = currentValue;
            currentVelocity = Vector3.zero;
            SetSplinePoint(); // Это вызовет CheckForCollisionsAndAdjust
        }

        private void NotifyPointsChanged()
        {
            OnPointsChanged?.Invoke();
        }

        private bool IsPointsMoved()
        {
            var startPointMoved = startPoint.position != prevStartPointPosition;
            var endPointMoved = endPoint.position != prevEndPointPosition;
            return startPointMoved || endPointMoved;
        }

        private bool IsRopeSettingsChanged()
        {
            var lineQualityChanged = !Mathf.Approximately(linePoints, prevLineQuality);
            var ropeWidthChanged = !Mathf.Approximately(ropeWidth, prevRopeWidth);
            var stiffnessChanged = !Mathf.Approximately(stiffness, prevstiffness);
            var dampnessChanged = !Mathf.Approximately(damping, prevDampness);
            var ropeLengthChanged = !Mathf.Approximately(ropeLength, prevRopeLength);
            var midPointPositionChanged = !Mathf.Approximately(midPointPosition, prevMidPointPosition);
            var midPointWeightChanged = !Mathf.Approximately(midPointWeight, prevMidPointWeight);
            return lineQualityChanged
                   || ropeWidthChanged
                   || stiffnessChanged
                   || dampnessChanged
                   || ropeLengthChanged
                   || midPointPositionChanged
                   || midPointWeightChanged;
        }
    }
}
[Serializable]
public class CollisionSettings
{
    [Tooltip("LayerMask to check collisions against. Set this to the layers your walls/obstacles are on.")]
    public LayerMask collisionLayerMask = Physics.AllLayers; // По умолчанию проверяет все слои

    [Tooltip("Distance to offset the collision check point from the line to avoid catching on the surface.")]
    public float collisionOffset = 0.05f;

    [Tooltip("If true, the rope will attempt to slide along the surface of obstacles.")]
    public bool enableSliding = true; // Эта опция пока не реализована полностью в этом простом примере, но заложена для будущего расширения

    [Tooltip("Maximum number of raycast attempts to find a valid non-colliding point. Prevents infinite loops.")]
    public int maxAdjustmentAttempts = 5;
}