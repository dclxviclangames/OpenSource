using UnityEngine;
using Unity.AI.Navigation; // Необходим пакет AI Navigation

public class WorldGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float depth = 20f;

    [Header("Префабы")]
    public GameObject playerBasePrefab;
    public GameObject enemyPrefab;
    public GameObject treePrefab;
    public GameObject bossPrefab;

    [Header("Настройки спавна")]
    public int enemyCount = 10;
    public int decorationCount = 50;

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        GenerateWorld();

        // 1. Запекаем навигацию ПОСЛЕ генерации высот
        navMeshSurface.BuildNavMesh();

        // 2. Спавним объекты
        SpawnObjects();
    }

    void GenerateWorld()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
    }

    TerrainData GenerateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        float[,] heights = new float[width, height];
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    void SpawnObjects()
    {
        // Спавним базу игрока в центре (или рандомно)
        SpawnAtRandomLocation(playerBasePrefab, 1);

        SpawnAtRandomLocation(bossPrefab, 1);

        // Спавним врагов
        SpawnAtRandomLocation(enemyPrefab, enemyCount);

        // Спавним декорации (деревья/камни)
        SpawnAtRandomLocation(treePrefab, decorationCount);
    }

    void SpawnAtRandomLocation(GameObject prefab, int count)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(10f, width - 10f);
            float z = Random.Range(10f, height - 10f);

            // Ключевой момент: получаем высоту Terrain в этой точке
            float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));

            Vector3 spawnPos = new Vector3(x, y, z);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}

