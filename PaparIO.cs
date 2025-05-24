using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Для использования .Last()

public class PaperIO : MonoBehaviour
{
    public float moveSpeed = 5f; // Скорость движения игрока
    public LineRenderer lineRenderer; // Ссылка на компонент LineRenderer
    public Material lineMaterial; // Материал для LineRenderer (для линии)
    public float lineWidth = 0.1f; // Ширина линии
    public Color playerColor = Color.red; // Цвет этого игрока/команды
    public Material territoryMaterial; // Материал для окрашивания захваченной территории
    public float lineDrawMinDistance = 0.1f; // Минимальное расстояние для добавления новой точки в линию

    private List<Vector3> currentLinePoints = new List<Vector3>(); // Список точек для текущей рисуемой линии
    private bool isDrawing = false; // Флаг, указывающий, рисует ли игрок в данный момент
    private Vector3 lastMoveDirection; // Последнее направление движения
    private bool wasMovingLastFrame = false; // Был ли игрок в движении в предыдущем кадре

    public Transform playerModel; // Ссылка на модель игрока (для поворота)
    public float rotationSpeed = 10f; // Скорость поворота игрока

    public Transform[] spawnPoints; // Массив точек для возрождения игроков

    void Start()
    {
        // Проверяем, назначен ли LineRenderer
        if (lineRenderer == null)
        {
            // Если LineRenderer не назначен, создаем новый GameObject с LineRenderer
            GameObject lineObject = new GameObject("Line_" + gameObject.name);
            lineObject.transform.SetParent(this.transform); // Делаем LineRenderer дочерним объектом игрока
            lineRenderer = lineObject.AddComponent<LineRenderer>();
        }

        // Настраиваем LineRenderer
        lineRenderer.material = lineMaterial;
        lineRenderer.widthCurve = AnimationCurve.Constant(0, 0, lineWidth);
        lineRenderer.startColor = playerColor; // Цвет линии соответствует цвету игрока
        lineRenderer.endColor = playerColor;
        lineRenderer.useWorldSpace = true; // Используем мировые координаты
        lineRenderer.positionCount = 0; // Начинаем с 0 точек
    }

    void Update()
    {
        // Получаем ввод от игрока
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Вычисляем направление движения
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Двигаем игрока
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Если игрок двигается
        if (moveDirection != Vector3.zero)
        {
            // Поворачиваем модель игрока в направлении движения
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            lastMoveDirection = moveDirection; // Сохраняем направление
        }

        bool isMoving = moveDirection != Vector3.zero;

        // Логика начала/остановки рисования линии при движении
        if (isMoving && !wasMovingLastFrame && !isDrawing) // Игрок начал двигаться и не рисует
        {
            StartDrawing();
            
        }
        else if (!isMoving && wasMovingLastFrame && isDrawing) // Игрок остановился и рисует
        {
            EndDrawing();
        }
        else if (isMoving && isDrawing) // Игрок продолжает двигаться и рисует
        {
            AddPointToLine();
            lineRenderer.SetPositions(currentLinePoints.ToArray());
            lineRenderer.material = lineMaterial;
        }

        wasMovingLastFrame = isMoving;

        // Проверка столкновений с линиями других игроков
        CheckOtherPlayerLines();
    }

    void StartDrawing()
    {
        isDrawing = true;
        currentLinePoints.Clear(); // Начинаем новый контур
        lineRenderer.positionCount = 0;
        // Сбрасываем количество точек LineRenderer

        AddPointToLine(); // Добавляем первую точку

    }

    void EndDrawing()
    {
        if (!isDrawing) return; // Проверка, чтобы избежать повторного вызова
        isDrawing = false;

        // Очищаем LineRenderer
        lineRenderer.positionCount = 0;

        // Если точек достаточно, пытаемся захватить территорию
        if (currentLinePoints.Count > 2) // Полигон должен иметь как минимум 3 точки
        {
            CaptureTerritory(currentLinePoints);
        }
        currentLinePoints.Clear(); // Очищаем точки после обработки
    }

    void AddPointToLine()
    {
        Vector3 currentPosition = transform.position;
        // Добавляем точку, только если она достаточно далеко от предыдущей
        if (currentLinePoints.Count == 0 || Vector3.Distance(currentLinePoints.Last(), currentPosition) > lineDrawMinDistance)
        {
            currentLinePoints.Add(currentPosition);
            lineRenderer.positionCount = currentLinePoints.Count;
            lineRenderer.SetPositions(currentLinePoints.ToArray());
           // lineRenderer.material = territoryMaterial;
        }
    }

    void CaptureTerritory(List<Vector3> points)
    {
        // ВНИМАНИЕ: Это упрощенная логика захвата территории для Paper.io.
        // Она создает меш из замкнутого контура линии.
        // Для полноценной Paper.io требуется более сложный алгоритм заливки/объединения территорий,
        // который обрабатывает самопересечения, отверстия и слияние с существующими территориями.
        // Здесь используется простая триангуляция "веером", которая работает только для выпуклых полигонов.

        if (points.Count < 3) return; // Полигон должен иметь как минимум 3 точки

        // Добавляем первую точку в конец, чтобы замкнуть контур для триангуляции
        List<Vector3> closedLoopPoints = new List<Vector3>(points);
        closedLoopPoints.Add(points[0]);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Упрощенная триангуляция для выпуклых полигонов (не подходит для самопересекающихся/вогнутых)
        // Для общего случая используйте Triangle.NET или аналогичную библиотеку
        for (int i = 0; i < closedLoopPoints.Count - 2; i++)
        {
            // Используем первую точку как "центр" веера
            vertices.Add(closedLoopPoints[0]);
            vertices.Add(closedLoopPoints[i + 1]);
            vertices.Add(closedLoopPoints[i + 2]);

            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Создаем GameObject для отображения захваченной территории
        GameObject territoryObject = new GameObject("CapturedTerritory_" + this.name + "_" + Time.time); // Уникальное имя
        territoryObject.transform.position = playerModel.transform.position; //Vector3.zero; // Позиция меша относительно мировых координат
        territoryObject.transform.Rotate(180f, 0f, 0f);
        territoryObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = territoryObject.AddComponent<MeshRenderer>();
        renderer.material = territoryMaterial;
        renderer.material.color = playerColor; // Окрашиваем в цвет игрока
    }

    void CheckOtherPlayerLines()
    {
        PaperIO[] allPlayers = FindObjectsOfType<PaperIO>();
        foreach (PaperIO otherPlayer in allPlayers)
        {
            if (otherPlayer == this) continue; // Пропускаем себя

            // Проверяем, рисует ли другой игрок линию
            if (otherPlayer.isDrawing && otherPlayer.currentLinePoints.Count > 1)
            {
                // Упрощенная проверка столкновения: проверяем расстояние до точек линии другого игрока.
                // Для точной проверки столкновений с линиями (пересечения отрезков)
                // потребуется более сложный алгоритм или использование коллайдеров на линии.
                foreach (Vector3 linePoint in otherPlayer.currentLinePoints)
                {
                    // Если текущий игрок находится достаточно близко к точке на линии другого игрока
                    if (Vector3.Distance(transform.position, linePoint) < 0.5f) // 0.5f - это примерный радиус столкновения
                    {
                        Debug.Log(this.name + " столкнулся с линией " + otherPlayer.name + "!");
                        otherPlayer.Respawn(); // Уничтожаем игрока, чья линия была задета
                        return; // Выходим после первого столкновения, чтобы не уничтожать несколько раз
                    }
                }
            }
        }
    }

    public void Respawn()
    {
        Debug.Log(this.name + " был уничтожен и возрождается!");
        // Очищаем текущую линию
        currentLinePoints.Clear();
        lineRenderer.positionCount = 0;
        isDrawing = false;

        // Возрождаем в случайной точке
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[randomIndex].position;
        }
        else
        {
            Debug.LogWarning("Точки возрождения не назначены для " + this.name + ". Возрождение в (0,0,0).");
            transform.position = Vector3.zero;
        }
    }
}
