using UnityEngine;
using System.Collections.Generic; // Для использования HashSet

/// <summary>
/// Отслеживает объекты, перекрывающие обзор от камеры к игроку,
/// и делает их прозрачными с помощью скрипта ObstructingWall.
/// </summary>
public class PlayerViewObstructor : MonoBehaviour
{
    [Tooltip("Ссылка на Transform игрока, чье поле зрения отслеживается.")]
    public Transform playerTransform;

    [Tooltip("Ссылка на Transform камеры, которая смотрит на игрока.")]
    public Transform cameraTransform;

    [Tooltip("Слой(и), на которых находятся объекты, которые могут быть сделаны прозрачными (например, 'Walls', 'Roofs').")]
    public LayerMask obstructingLayers;

    [Tooltip("Насколько прозрачными должны стать объекты (0.0 - полностью прозрачный, 1.0 - полностью непрозрачный).")]
    [Range(0.0f, 1.0f)]
    public float transparentAlpha = 0.3f; // Например, 30% непрозрачности

    [Tooltip("Длительность анимации исчезновения/появления в секундах.")]
    public float fadeDuration = 0.2f;

    // Используем HashSet для эффективного отслеживания стен, которые в данный момент прозрачны
    private HashSet<ObstructingWall> currentlyTransparentWalls = new HashSet<ObstructingWall>();
    private HashSet<ObstructingWall> previouslyTransparentWalls = new HashSet<ObstructingWall>();

    void Update()
    {
        if (playerTransform == null || cameraTransform == null)
        {
            Debug.LogWarning("Player Transform или Camera Transform не назначены в PlayerViewObstructor. Пожалуйста, назначьте их в инспекторе.", this);
            return;
        }

        // Очищаем текущий список для нового кадра
        currentlyTransparentWalls.Clear();
        //previouslyTransparentWalls.Clear();
        // Направление от камеры к игроку
        Vector3 direction = playerTransform.position - cameraTransform.position;
        float distance = direction.magnitude;

        // Выполняем RaycastAll, чтобы получить все объекты между камерой и игроком
        // (RaycastAll нужен, чтобы задеть все объекты, а не только первый)
        RaycastHit[] hits = Physics.RaycastAll(cameraTransform.position, direction.normalized, distance, obstructingLayers);

        // Перебираем все объекты, которые были задеты лучом
        foreach (RaycastHit hit in hits)
        {
            // Проверяем, есть ли на задетом объекте наш скрипт ObstructingWall
            ObstructingWall wall = hit.collider.GetComponent<ObstructingWall>();
            if (wall != null)
            {
                
                // Если это стена, которую мы хотим сделать прозрачной, запускаем анимацию прозрачности
                wall.SetTransparent(transparentAlpha, fadeDuration);
                currentlyTransparentWalls.Add(wall); // Добавляем в список текущих прозрачных стен
            }
        }
        
        
        // Проверяем стены, которые были прозрачными в предыдущем кадре,
        // но больше не находятся в списке текущих (т.е., они больше не перекрывают обзор)
        foreach (ObstructingWall wall in previouslyTransparentWalls)
        {
            // Убеждаемся, что стена все еще существует и не находится в списке текущих прозрачных стен
            if (wall != null && !currentlyTransparentWalls.Contains(wall))
            {
                

                wall.SetOpaque(fadeDuration); // Возвращаем стену к непрозрачност
                
            }
        } 

        // Копируем текущие прозрачные стены в список "предыдущих" для следующего кадра
        // Важно: создаем новый HashSet, чтобы избежать проблем с изменением во время итерации
        previouslyTransparentWalls = new HashSet<ObstructingWall>(currentlyTransparentWalls); 
    }

    /// <summary>
    /// Отображает желтую линию между камерой и игроком для отладки в режиме Scene.
    /// </summary>
    void OnDrawGizmos()
    {
        if (playerTransform != null && cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cameraTransform.position, playerTransform.position);
            Gizmos.DrawSphere(playerTransform.position, 0.1f); // Player position
            Gizmos.DrawSphere(cameraTransform.position, 0.1f); // Camera position
        }
    }
}
