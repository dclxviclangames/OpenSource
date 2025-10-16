// CameraFollow.cs
using UnityEngine;

/// <summary>
/// Скрипт для камеры, которая следует за игроком от третьего лица.
/// Позволяет вращать камеру мышью вокруг игрока.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Объект, за которым будет следовать камера (обычно игрок).")]
    public Transform target;

    [Header("Позиция камеры")]
    [Tooltip("Расстояние камеры от игрока.")]
    public float distance = 7.0f;
    [Tooltip("Высота камеры над игроком.")]
    public float height = 3.0f;
    [Tooltip("Смещение камеры от игрока по горизонтали.")]
    public float horizontalOffset = 0f;

    [Header("Скорость и сглаживание")]
    [Tooltip("Скорость, с которой камера будет двигаться к целевой позиции.")]
    public float followSpeed = 5.0f;
    [Tooltip("Скорость, с которой камера будет вращаться вокруг игрока.")]
    public float rotationSpeed = 3.0f;

    [Header("Настройки вращения")]
    [Tooltip("Минимальный угол наклона камеры (ограничение вверх).")]
    public float minVerticalAngle = -30.0f;
    [Tooltip("Максимальный угол наклона камеры (ограничение вниз).")]
    public float maxVerticalAngle = 60.0f;

    // Внутренние переменные для отслеживания углов вращения
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void LateUpdate() // LateUpdate лучше для камеры, чтобы избежать "дерганья"
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Целевой объект (target) не назначен. Отключите скрипт или назначьте target.");
            return;
        }

        // Получаем ввод для вращения камеры мышью
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;

        // Ограничиваем вертикальное вращение камеры
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        // Вычисляем вращение камеры
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Quaternion rotationS = Quaternion.Euler(0, 0, 0);


        // Вычисляем целевую позицию камеры
        Vector3 targetPosition = target.position;
        // Добавляем смещение камеры относительно игрока
        Vector3 desiredPosition = targetPosition + rotation * new Vector3(horizontalOffset, height, -distance);
        Vector3 desiredPositionS = targetPosition + rotationS * new Vector3(horizontalOffset, height, -distance);

        // Плавное перемещение камеры к целевой позиции
        transform.position = Vector3.Lerp(transform.position, desiredPositionS, followSpeed * Time.deltaTime); //Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Камера всегда смотрит на игрока (с учетом высоты)
        transform.LookAt(targetPosition + Vector3.up * height * 0.5f); // Немного выше центра игрока
    }
} 


