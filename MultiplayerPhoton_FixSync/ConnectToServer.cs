using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
//using UnityEngine.WebGLModule;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Screen.fullScreen = true;
#if !UNITY_EDITOR && UNITY_WEBGL
        // disable WebGLInput.mobileKeyboardSupport so the built-in mobile keyboard support is disabled.
        WebGLInput.captureAllKeyboardInput = true;
#endif
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.SendRate = 8; // Снижаем до 8 пакетов/сек

        // Частота синхронизации обновления свойств (например, Custom Properties) (по умолчанию 10)
        PhotonNetwork.SerializationRate = 5; // Снижаем до 5 раз/сек
        SceneManager.LoadScene("Menu");
    }
}
