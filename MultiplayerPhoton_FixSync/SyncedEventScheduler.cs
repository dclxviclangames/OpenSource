using System;
using UnityEngine;
using Photon.Pun; // ������������ ��� ��������� ������������
using System.Runtime.InteropServices;

public class SyncedEventScheduler : MonoBehaviour
{

   /* [DllImport("__Internal")]
    private static extern void OpenNewTab(string url); */

    public GameObject[] isnTEven;
    public GameObject[] isEveSnT;

    public GameObject unlockBanner;

    [DllImport("__Internal")]
    private static extern void OpenExternalUrl(string url);

    public void OpenYouTubeChannel()
    {
        string youtubeUrl = "https://www.youtube.com/@dclxviclan";

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[C#] ������ �� �������� YouTube-������.");
            OpenExternalUrl(youtubeUrl);
#endif
    }

    /*  void Start()
      {
          RunSyncedEvents();
      }

      public void RunSyncedEvents()
      {
          // ���������� DateTime.UtcNow ��� ��������� ��������� ������������������� �������.
          // ��� �����������, ��� "�������" �������� � ���������� ������������ � �������� � �����.
          DateTime utcNow = DateTime.UtcNow;

          // �������� ����� ��� ������ �� UTC
          int utcDayOfMonth = utcNow.Day;

          // ��������� �������� ��� �� UTC
          bool isEven = (utcDayOfMonth % 2) == 0;

          if (isEven)
          {
              Debug.Log($"[UTC ����: {utcDayOfMonth}] ������� ������ ���� UTC. ������ ������ �.");
              for(int i = 0; i < isnTEven.Length; i++)
              {
                  isnTEven[i].SetActive(false);

              }
              // �������� PlayersController.cs
              for (int i = 0; i < isEveSnT.Length; i++)
              {
                  isEveSnT[i].SetActive(true);

              }
          }
          else
          {
              Debug.Log($"[UTC ����: {utcDayOfMonth}] ������� �������� ���� UTC. ������ ������ �.");
              for (int i = 0; i < isEveSnT.Length; i++)
              {
                  isEveSnT[i].SetActive(false);

              }
              // �������� PlayersController.cs
              for (int i = 0; i < isnTEven.Length; i++)
              {
                  isnTEven[i].SetActive(true);

              }
              // ��������� PlayersController.cs
          }
      }

      public void OpenCharacters()
      {
          unlockBanner.SetActive(false);
          GameMonetize.Instance.ShowAd();
      }

      */

    /*  public void openIt(string url)
      {
  #if !UNITY_EDITOR && UNITY_WEBGL
          OpenNewTab(url);
  #endif
      } */
}
