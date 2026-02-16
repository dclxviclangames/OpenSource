using UnityEngine;
using System.Collections;

public class RandomSpawner : MonoBehaviour
{
    [Header("Настройки объектов")]
    public GameObject[] prefabs; // Массив из 3-х (или более) префабов

    [Header("Настройки времени (секунды)")]
    public float minDelay = 1f;
    public float maxDelay = 3f;

    [Header("Зона спавна")]
    public Vector3 spawnArea = new Vector3(5, 0, 5);

    void Start()
    {
        // Проверка: назначены ли префабы
        if (prefabs.Length > 0)
        {
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogError("Забудьте назначить префабы в инспекторе!");
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true) // Бесконечный цикл спавна
        {
            // 1. Ждем случайное время
            float randomWait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(randomWait);

            // 2. Выбираем случайный объект из массива
            int randomIndex = Random.Range(0, prefabs.Length);
            GameObject prefabToSpawn = prefabs[randomIndex];

            // 3. Вычисляем случайную позицию в зоне спавна
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                transform.position.y,
                Random.Range(-spawnArea.z, spawnArea.z)
            ) + transform.position;

            // 4. Создаем объект
            Instantiate(prefabToSpawn, randomPos, Quaternion.identity);
        }
    }

    // Визуализация зоны спавна в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 0.5f, spawnArea.z * 2));
    }
}

