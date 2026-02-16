using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using YTGameSDK;

public class Engine : MonoBehaviour
{
    [Serializable]
    public class LoadedScores
    {
        public int BestScore;
        public float BestTime;
    }

    public YTGameWrapper ytGameWrapper;

    public bool gameIsPaused = false;

    public string currentSaveData = "0";

    public Text scoreTxt;
    public Text timeTxt;
    public Text bestScoreTxt;
    public Text bestTimeTxt;

    public AudioSource audioSource;

    public AudioClip MoveClip;
    public AudioClip WinClip;
    public AudioClip LoseClip;
    public AudioClip BonusClip;

    public GameObject endScreenGO;
    public GameObject canvasGO;
    public GameObject loadingScreenGO;
    public GameObject eventSystemGO;
    public GameObject ytGameWrapperGO;

    private GameObject winText;
    private GameObject loseText;

    public float volume = 0.8f;

    public float timer = 0f;
    public int battleScore = 0;

    public bool gameIsOver = false;

    public bool IsAudioOn = true;

    public bool InPlayablesEnv = true;

    private void Awake()
    {
        // Send the initial First Frame Ready event now that Unity has started
        ytGameWrapper.SendGameFirstFrameReady();

        GameObject.DontDestroyOnLoad(this.gameObject);
        GameObject.DontDestroyOnLoad(canvasGO);
        GameObject.DontDestroyOnLoad(eventSystemGO);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSaveGameDataReturned("{\"BestScore\": \"0\",\"BestTime\": \"0.00\"}");

        string sdkVersion = ytGameWrapper.GetTheYTGameSDKVersion();

        ytGameWrapper.SetOnResumeCallback(ResumeTheGameCallback);
        ytGameWrapper.SetOnPauseCallback(PauseTheGameCallback);

        ytGameWrapper.LoadGameSaveData(LoadSaveGameDataReturned);

        IsAudioOn = ytGameWrapper.IsYTGameAudioEnabled();
        InPlayablesEnv = ytGameWrapper.InPlayablesEnv();
        ytGameWrapper.SetOnAudioEnabledChangeCallback(OnYTAudioChangedCallback);

        timer = 0f;

        winText = endScreenGO.transform.Find("WinTxt").gameObject;
        loseText = endScreenGO.transform.Find("LoseTxt").gameObject;
        endScreenGO.SetActive(false);
        scoreTxt.text = battleScore.ToString() + " / 220";

        // Send the Game Ready event now that the game is ready to present
        ytGameWrapper.SendGameIsReady();
    }

    void Update()
    {
        if (gameIsPaused || gameIsOver) return;
        timer += Time.deltaTime;
        timeTxt.text = timer.ToString("0.00");
    }

    public void SaveGameData(string saveString, string saveKey = "")
    {
        if (string.IsNullOrEmpty(saveString) || !InPlayablesEnv) return;

        Debug.Log("Saving Game Data >> " + saveString);
        ytGameWrapper.SendGameSaveData(saveString);
    }

    public void LoadSaveGameDataReturned(string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            currentSaveData = data;
            Debug.Log("LoadSaveGameDataReturned > last level reached was > " + currentSaveData);
            LoadedScores loadedScores = JsonUtility.FromJson<LoadedScores>(currentSaveData);
            bestScoreTxt.text = loadedScores.BestScore.ToString();
            bestTimeTxt.text = loadedScores.BestTime.ToString("0.00");
        }
    }

    // Resume the game
    public void ResumeTheGameCallback()
    {
        gameIsPaused = false;
        Debug.Log("ResumeTheGameCallback was called!!!");
    }

    // Pause the game
    public void PauseTheGameCallback()
    {
        gameIsPaused = true;
        Debug.Log("PauseTheGameCallback was called!!!");
    }

    public void UpdateScores(int scoreAmt)
    {
        battleScore += scoreAmt;
        scoreTxt.text = battleScore.ToString() + " / 220";
        Debug.Log("UpdateScores for the game " + battleScore);
        if (InPlayablesEnv)
        {
            ytGameWrapper.SendGameScore(battleScore);
        }

        SaveGameData("{\"BestScore\": \"" + battleScore.ToString() + "\",\"BestTime\": \"" + timer.ToString() + "\"}");
    }

    public void OnYTAudioChangedCallback(bool isAudioEnabled)
    {
        Debug.Log("In Unity: Audio change callback >> " + isAudioEnabled);
        IsAudioOn = isAudioEnabled;
    }

    public void PlaySFX(string sfxName)
    {
        // suppress audio when audio is muted
        if (!IsAudioOn) return;

        if (string.Equals(sfxName, "MoveClip"))
        {
            audioSource.PlayOneShot(MoveClip, volume);
        }
        else if (string.Equals(sfxName, "WinClip"))
        {
            audioSource.PlayOneShot(WinClip, volume);
        }
        else if (string.Equals(sfxName, "LoseClip"))
        {
            audioSource.PlayOneShot(LoseClip, volume);
        }
        else if (string.Equals(sfxName, "BonusClip"))
        {
            audioSource.PlayOneShot(BonusClip, volume);
        }
    }

    public void GameWon()
    {
        endScreenGO.SetActive(true);
        winText.SetActive(true);
        loseText.SetActive(false);

        RequestInterstitialAd();
    }

    public void GameLose()
    {
        endScreenGO.SetActive(true);
        winText.SetActive(false);
        loseText.SetActive(true);

        RequestInterstitialAd();
    }

    public void RequestInterstitialAd()
    {
        // At the end of a round send a request to show an interstitial Ad, if one is available 
        //  an Ad will be shown and Pause Game Callback should be called.
        ytGameWrapper.RequestInterstitialAd();
    }

    public void ResetGame()
    {
        endScreenGO.SetActive(false);
        gameIsOver = false;
        timer = 0f;
        battleScore = 0;
        scoreTxt.text = battleScore.ToString() + " / 220";

        SceneManager.LoadScene("MainScene");
        ytGameWrapper.LoadGameSaveData(LoadSaveGameDataReturned);
    }

    public void StartGame()
    {
        loadingScreenGO.SetActive(false);
        SceneManager.LoadScene("MainScene");
    }
}
