using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaveSpawn : MonoBehaviour
{
    [Header("Настройки объектов")]
    public GameObject[] prefabs;
    public string targetTag = "Dude"; // Тег, который считаем
    public int maxObjects = 5;       // Лимит объектов на сцене

    [Header("Настройки здоровья и времени")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float fastSpawnDelay = 2f; // Таймер при полном здоровье
    public float slowSpawnDelay = 5f; // Таймер при низком здоровье

    public Slider slider;

    public Text scoreText;
    private YTPlayableController yTPlayableController;
    private bool death = false;

    [Header("Зона спавна")]
    public Vector3 spawnArea = new Vector3(5, 0, 5);
    public GameObject gameOver;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
        yTPlayableController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<YTPlayableController>();
        slider.value = currentHealth;
        scoreText.text = yTPlayableController._currentScore.ToString();
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 1. Считаем, сколько объектов с нужным тегом сейчас в игре
            int currentCount = GameObject.FindGameObjectsWithTag(targetTag).Length;

            // 2. Если лимит не превышен — спавним
            if (currentCount < maxObjects)
            {
                SpawnObject();
            }

            // 3. Рассчитываем задержку в зависимости от здоровья
            // Чем меньше здоровья, тем дольше ждать (Lerp плавно меняет значение)
            float healthPercent = currentHealth / maxHealth;
            float currentDelay = Mathf.Lerp(slowSpawnDelay, fastSpawnDelay, healthPercent);

            yield return new WaitForSeconds(currentDelay);
        }
    }

    void SpawnObject()
    {
        int randomIndex = Random.Range(0, prefabs.Length);
        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.z, spawnArea.z)
        );

        Instantiate(prefabs[randomIndex], randomPos, Quaternion.identity);
    }

    public void AddScore()
    {
        yTPlayableController._currentScore += 20;
    }

    // Метод для получения урона (вызывайте его из других скриптов)
    public void TakeDamage(float damage)
    {
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        slider.value = currentHealth;
        scoreText.text = yTPlayableController._currentScore.ToString();
        if(currentHealth <= 0 && death == false)
        {
            gameOver.SetActive(true);
            Invoke("GameOver", 5f);
            death = true;
        }

    }

    public void GameOver()
    {
        SceneManager.LoadScene(0);
    }

    public void Hil(float damage)
    {

        currentHealth += damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        slider.value = currentHealth;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 1f, spawnArea.z * 2));
    }
}

