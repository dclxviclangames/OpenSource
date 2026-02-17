using UnityEngine;
using YTGameSDK;

public class AudioManager : MonoBehaviour 
{
    void Start() 
    {
        // Подписываемся на событие изменения громкости от YouTube
        Playables.OnVolumeChange += HandleVolumeChange;
    }

    void HandleVolumeChange(float volume) 
    {
        // volume будет 0, если нажали Mute, и 1, если звук включен
        AudioListener.volume = volume; 
        
        // Если volume == 0, Unity полностью выключит все звуки сама
        Debug.Log("YouTube сменил громкость на: " + volume);
    }
}
using UnityEngine;
using YTGameSDK; // Убедись, что имя SDK верное

public class MenuInit : MonoBehaviour 
{
    void Start() 
    {
        // 1. Сначала говорим: "Я рисую кадры"
        Playables.FirstFrameReady();
        
        // 2. СРАЗУ ЖЕ говорим: "Я готов, нажимай на кнопки меню"
        Playables.GameReady();
    }
}

//Anchors 
//Timp
