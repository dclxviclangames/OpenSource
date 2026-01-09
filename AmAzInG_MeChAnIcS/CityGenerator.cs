#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class ManualBakeGenerator : MonoBehaviour
{
    [Header("Настройки мира")]
    public float mapSize = 150f;
    public float groundYLevel = 0f;

    [Header("Массивы префабов")]
    public GameObject[] buildingPrefabs;
    public GameObject[] playerBasePrefabs;
    public GameObject[] enemyBasePrefabs;

    [Header("Настройки генерации")]
    public int buildingCount = 50;
    public int enemyBaseCount = 3;
    public float minSpawnDistance = 10f;

    private NavMeshSurface navMeshSurface;
    private List<Vector3> occupiedPositions = new List<Vector3>();

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            Debug.LogError("На объекте генератора отсутствует компонент NavMeshSurface!");
            return;
        }

        GenerateObjectsOnGround();
        Debug.Log("Генерация завершена. Нажмите 'Bake NavMesh' в инспекторе вручную.");
    }

    void GenerateObjectsOnGround()
    {
        foreach (Transform child in transform)
        {
            if (child != null)
            {
                // В редакторе лучше использовать DestroyImmediate, если чистите сцену кнопкой
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
        occupiedPositions.Clear();

        PlaceObjectsRandomly(playerBasePrefabs, 1, minSpawnDistance * 2f);
        PlaceObjectsRandomly(enemyBasePrefabs, enemyBaseCount, minSpawnDistance * 2f);
        PlaceObjectsRandomly(buildingPrefabs, buildingCount, minSpawnDistance);
    }

    void PlaceObjectsRandomly(GameObject[] prefabs, int count, float minDistance)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = Vector3.zero;
            bool canSpawn = false;
            int attempts = 0;
            int maxAttempts = 500;

            while (!canSpawn && attempts < maxAttempts)
            {
                float halfSize = mapSize / 2f;
                float x = Random.Range(-halfSize, halfSize);
                float z = Random.Range(-halfSize, halfSize);
                randomPos = new Vector3(x, groundYLevel, z);

                if (CheckDistance(randomPos, minDistance))
                {
                    canSpawn = true;
                }
                attempts++;
            }

            if (canSpawn)
            {
                GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];
                Instantiate(prefabToSpawn, randomPos, GetRandomRotation(), transform);
                occupiedPositions.Add(randomPos);
            }
        }
    }

    bool CheckDistance(Vector3 position, float minDistance)
    {
        foreach (Vector3 occupiedPos in occupiedPositions)
        {
            if (Vector3.Distance(position, occupiedPos) < minDistance)
                return false;
        }
        return true;
    }

    Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
    }

    public void BakeNavMeshManually()
    {
        // Сначала пробуем получить компонент, если он еще не найден
        if (navMeshSurface == null) navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface != null)
        {
            navMeshSurface.RemoveData();
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh запечен успешно!");
        }
        else
        {
            Debug.LogError("NavMeshSurface не найден на объекте!");
        }
    }
}

// Код редактора
#if UNITY_EDITOR
[CustomEditor(typeof(ManualBakeGenerator))]
public class ManualBakeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ManualBakeGenerator generator = (ManualBakeGenerator)target;
        if(GUILayout.Button("Bake NavMesh Manually"))
        {
            generator.BakeNavMeshManually();
        }
    }
}
#endif
