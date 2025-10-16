using System;
using UnityEngine;
using Photon.Pun; // Используется для контекста мультиплеера
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
            Debug.Log("[C#] Запрос на открытие YouTube-канала.");
            OpenExternalUrl(youtubeUrl);
#endif
    }

    /*  void Start()
      {
          RunSyncedEvents();
      }

      public void RunSyncedEvents()
      {
          // Используем DateTime.UtcNow для получения ГЛОБАЛЬНО СИНХРОНИЗИРОВАННОГО времени.
          // Это гарантирует, что "сегодня" начнется и закончится одновременно в Бразилии и Китае.
          DateTime utcNow = DateTime.UtcNow;

          // Получаем номер дня месяца по UTC
          int utcDayOfMonth = utcNow.Day;

          // Проверяем четность дня по UTC
          bool isEven = (utcDayOfMonth % 2) == 0;

          if (isEven)
          {
              Debug.Log($"[UTC День: {utcDayOfMonth}] Сегодня ЧЕТНЫЙ день UTC. Запуск Ивента А.");
              for(int i = 0; i < isnTEven.Length; i++)
              {
                  isnTEven[i].SetActive(false);

              }
              // Включаем PlayersController.cs
              for (int i = 0; i < isEveSnT.Length; i++)
              {
                  isEveSnT[i].SetActive(true);

              }
          }
          else
          {
              Debug.Log($"[UTC День: {utcDayOfMonth}] Сегодня НЕЧЕТНЫЙ день UTC. Запуск Ивента Б.");
              for (int i = 0; i < isEveSnT.Length; i++)
              {
                  isEveSnT[i].SetActive(false);

              }
              // Включаем PlayersController.cs
              for (int i = 0; i < isnTEven.Length; i++)
              {
                  isnTEven[i].SetActive(true);

              }
              // Отключаем PlayersController.cs
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
