using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Объект, за которым следим
    public Vector3 offset;         // Смещение камеры относительно игрока
    public float smoothTime = 0.3f; // Время сглаживания (меньше = быстрее)

    private Vector3 _velocity = Vector3.zero;

    // Важно: используем LateUpdate, чтобы камера двигалась ПОСЛЕ игрока
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        // Плавно перемещаем камеру к цели
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
    }
}

