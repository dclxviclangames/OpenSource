using UnityEngine;
using UnityEngine.AI;

public class NavMeshLineFollower : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxSpeed = 10f;
    public float minSpeed = 2f;
    public float stopDistance = 0.3f;
    public float lineOffset = 0.1f; // Высота линии над полом

    private NavMeshPath path;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        path = new NavMeshPath();
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        // Настройки линии, если забыли в инспекторе
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        // 1. РИСОВАНИЕ ПУТИ
        if (Input.GetMouseButton(0))
        {
            isMoving = false; // Останавливаемся при перерисовке
            UpdatePathToMouse();
        }

        // 2. СТАРТ ДВИЖЕНИЯ
        if (Input.GetMouseButtonUp(0))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                isMoving = true;
            }
        }

        // 3. ДВИЖЕНИЕ И ОБНОВЛЕНИЕ ЛИНИИ
        if (isMoving)
        {
            MoveAlongPath();
            // ВАЖНО: Обновляем визуализацию во время движения
            UpdateLineVisuals();
        }
    }

    void UpdatePathToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPosition = hit.point;
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

            // ВАЖНО: Обновляем визуализацию во время рисования
            UpdateLineVisuals();
        }
    }

    void UpdateLineVisuals()
    {
        if (path != null && path.corners.Length > 1)
        {
            lineRenderer.positionCount = path.corners.Length;

            for (int i = 0; i < path.corners.Length; i++)
            {
                // Поднимаем каждую точку чуть выше пола по Y
                Vector3 point = path.corners[i];
                point.y += lineOffset;
                lineRenderer.SetPosition(i, point);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    void MoveAlongPath()
    {
        // Постоянно пересчитываем путь к цели, чтобы он начинался из текущей позиции персонажа
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

        if (path.corners.Length < 2)
        {
            StopMovement();
            return;
        }

        Vector3 nextPoint = path.corners[1]; // Следующая точка после текущей позиции
        float distanceToFinalTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToFinalTarget <= stopDistance)
        {
            StopMovement();
            return;
        }

        // Движение и поворот
        float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, distanceToFinalTarget / 10f);
        transform.position = Vector3.MoveTowards(transform.position, nextPoint, currentSpeed * Time.deltaTime);

        Vector3 direction = nextPoint - transform.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }
    }

    void StopMovement()
    {
        isMoving = false;
        lineRenderer.positionCount = 0;
    }
}


