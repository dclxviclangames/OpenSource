using UnityEngine;
using UnityEngine.UI;
using YTGameSDK; // 1. Добавляем пространство имен из твоего файла
using UnityEngine.SceneManagement;

public class YTPlayableController : MonoBehaviour
{
    public Text scoreText;
    public int _currentScore = 0;

    // Ссылка на компонент SDK
    private YTGameWrapper _ytWrapper;
    public GameObject menu;
    public GameObject pause;

    [System.Serializable]
    public class MySaveData { public int bestScore; }

    void OnEnable()
    {
        // Подписываемся на события при включении объекта
        if (_ytWrapper != null)
        {
            _ytWrapper.SetOnPauseCallback(OnPauseGame);
            _ytWrapper.SetOnResumeCallback(OnResumeGame);
        }
    }

    void OnDisable()
    {
        // Отписываемся при уничтожении/выключении, чтобы избежать ошибок
        if (_ytWrapper != null)
        {
            _ytWrapper.SetOnPauseCallback(null);
            _ytWrapper.SetOnResumeCallback(null);
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene(0);
    }


    void Start()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        // 2. Находим объект YTGameWrapper на сцене
        _ytWrapper = FindObjectOfType<YTGameWrapper>();

        if (_ytWrapper != null)
        {
            // 3. Используем ТОЧНЫЕ имена методов из твоего файла
            _ytWrapper.SendGameFirstFrameReady();

            // Загрузка (передаем метод-колбэк OnDataLoaded)
            _ytWrapper.LoadGameSaveData(OnDataLoaded);

            _ytWrapper.SendGameIsReady();
            scoreText.text = "Best: " + _currentScore.ToString();
        }
        else
        {
            Debug.LogError("Объект YTGameWrapper не найден на сцене!");
        }
    }

    // --- СОХРАНЕНИЕ ---
    public void SaveGame()
    {
        if (_ytWrapper == null) return;

        MySaveData data = new MySaveData { bestScore = _currentScore };
        string json = JsonUtility.ToJson(data);

        // В твоем SDK метод называется SendGameSaveData
        _ytWrapper.SendGameSaveData(json);
    }

    // --- ЗАГРУЗКА (Callback) ---
    public void OnDataLoaded(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}") return;

        try
        {
            _currentScore = JsonUtility.FromJson<MySaveData>(json).bestScore;
            scoreText.text = "Best: " + _currentScore.ToString();
        }
        catch
        {
            scoreText.text = "Best: " + _currentScore.ToString();
            Debug.Log("Похоже, сохранений еще нет.");
        }
    }

    // --- ПАУЗА / РЕЗЮМЕ ---
    // Вызывай эти методы через события, которые прокидывает YTGameWrapper
    public void OnPauseGame()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        menu.SetActive(false);
        pause.SetActive(true);
        SaveGame();
    }

    public void OnResumeGame()
    {
        Time.timeScale = 1;
        menu.SetActive(true);
        pause.SetActive(false);
        AudioListener.pause = false;
    }
}
