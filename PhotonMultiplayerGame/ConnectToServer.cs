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
        SceneManager.LoadScene("Menu");
    }
}
