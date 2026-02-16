using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public GameObject[] chunkPrefabs; // Массив префабов комнат
    public int worldSeed = 12345;
    public Transform player;
    public int drawDistance = 2; // На сколько комнат вперед генерировать
    public float chunkSize = 10f; // Размер комнаты (10х10)

    private Dictionary<Vector2Int, GameObject> spawnedChunks = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        // Считаем, в какой координате сетки находится игрок
        int currentX = Mathf.RoundToInt(player.position.x / chunkSize);
        int currentZ = Mathf.RoundToInt(player.position.z / chunkSize);

        for (int x = -drawDistance; x <= drawDistance; x++)
        {
            for (int z = -drawDistance; z <= drawDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(currentX + x, currentZ + z);
                if (!spawnedChunks.ContainsKey(chunkCoord))
                {
                    SpawnChunk(chunkCoord);
                }
            }
        }
    }

    void SpawnChunk(Vector2Int coord)
    {
        // Магия сида: смешиваем мировой сид с координатами чанка
        int chunkSeed = worldSeed + (coord.x * 10000) + coord.y;
        Random.InitState(chunkSeed); 

        // Теперь Random.Range выдаст "случайный", но всегда один и тот же индекс для этой точки
        int index = Random.Range(0, chunkPrefabs.Length);
        
        Vector3 spawnPos = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        GameObject newChunk = Instantiate(chunkPrefabs[index], spawnPos, Quaternion.identity);
        
        spawnedChunks.Add(coord, newChunk);
    }
}
