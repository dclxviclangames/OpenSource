using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text; // Äëÿ StringBuilder
using System.Collections;
using Photon.Realtime;


public class MyNetworkScript : MonoBehaviourPunCallbacks
{
    TMP_Text tMP_Text;
    ScrollRect scrollView;
    TMP_InputField inputField;
    private PhotonView photonView;
    private Button myButton;
    private readonly List<string> currentChatLines = new List<string>();
    private int maxChatLines = 3;

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityEngine.WebGLInput.mobileKeyboardSupport = true;
#endif
        photonView = GetComponent<PhotonView>();
        tMP_Text = GameObject.FindWithTag("ChatText").GetComponent<TMP_Text>();
        inputField = GameObject.FindWithTag("Chat").GetComponent<TMP_InputField>();
        myButton = GameObject.FindWithTag("Send").GetComponent<Button>();
        myButton.onClick.AddListener(delegate { SendChatMessage(inputField.text); });
    }
    // Call this method to send a message to all other players
    public void SendChatMessage(string message)
    {
        photonView.RPC("ReceiveChatMessage", RpcTarget.All, inputField.text);
    }

    // This method will be called on all clients when the RPC is received
    [PunRPC]
    public void ReceiveChatMessage(string message)
    {
        Debug.Log("Chat Message received: " + message);
        string NameHero = photonView.Owner.NickName;
        currentChatLines.Add(NameHero + ":" + message);
        StringBuilder sb = new StringBuilder();
        while (currentChatLines.Count > maxChatLines)
        {
            currentChatLines.RemoveAt(0);
        }
        foreach (string line in currentChatLines)
        {
            sb.AppendLine(line);
        }
        tMP_Text.text = sb.ToString();
        StartCoroutine(Remove());
        // Update UI or perform other actions with the message
    }

    IEnumerator Remove()
    {
        yield return new WaitForSeconds(5f);
        if(!inputField.isFocused)
        {
            inputField.text = " ";
        }
        

    }
}
