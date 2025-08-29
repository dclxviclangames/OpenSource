// PlayerHealth.cs
using UnityEngine;
using Photon.Pun;
using System.Collections;
using ExitGames.Client.Photon; // Добавлено для Custom Event
using Photon.Realtime; // Добавлено для RaiseEventOptions, ReceiverGroup

/// <summary>
/// Управляет здоровьем игрока, получением урона и инициирует респаун.
/// </summary>
public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable // Добавлено IPunObservable
{
    [Tooltip("Максимальное здоровье игрока.")]
    public int maxHealth = 100;

    [Tooltip("Текущее здоровье игрока.")]
    [SerializeField]
    private int currentHealth;

    [Tooltip("Компонент PlayerMovement для отключения при смерти.")]
    public PlayerMovement playerMovement; // Перетащите сюда компонент PlayerMovement в инспекторе

    private PhotonView photonView;

    // Пользовательский код события для смерти игрока. Должен быть уникальным.
    private const byte PlayerDiedEventCode = 100;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning($"[PlayerHealth:{gameObject.name}] PlayerMovement component not assigned and not found on this GameObject!");
            }
        }
    }

    void Start()
    {
        // Инициализируем здоровье только при первом спавне/реснапе и только на владельце.
        if (photonView.IsMine)
        {
            ResetHealthAndActivate();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Сбрасывает здоровье до максимума и активирует игрока.
    /// Вызывается GameManager при спавне/реснапе.
    /// </summary>
    public void ResetHealthAndActivate()
    {
        currentHealth = maxHealth;
        if (playerMovement != null)
        {
            playerMovement.enabled = true; // Убеждаемся, что движение включено
        }
        SetPlayerVisuals(true); // Показываем модель игрока
        Debug.Log($"[PlayerHealth:{gameObject.name}] Health reset to {maxHealth}. Player activated.");
    }

    /// <summary>
    /// Наносит урон игроку. Вызывается по RPC.
    /// </summary>
    /// <param name="damage">Количество наносимого урона.</param>
    [PunRPC]
    public void TakeDamage(int damage)
    {
        // Урон обрабатывается только на владельце PhotonView.
        // Это гарантирует, что только владелец решает, когда он умирает.
        if (!photonView.IsMine)
        {
            Debug.Log($"[PlayerHealth:{gameObject.name}] Received damage RPC but not owner. Skipping direct damage application. Current health (synced): {currentHealth}");
            return;
        }

        if (currentHealth <= 0)
        {
            Debug.Log($"[PlayerHealth:{gameObject.name}] Already dead. Skipping damage. Current health: {currentHealth}");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"[PlayerHealth:{gameObject.name}] Took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Логика смерти игрока. Вызывается локально на владельце после получения смертельного урона.
    /// Инициирует респаун через GameManager (на Мастер-клиенте).
    /// </summary>
    private void Die()
    {
        Debug.Log($"[PlayerHealth:{gameObject.name}] Player {photonView.Owner.NickName} has died!");

        // Отключаем движение и визуальные эффекты ЛОКАЛЬНО
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        SetPlayerVisuals(false); // Скрываем модель игрока
        if(photonView.IsMine)
            GameManager.Instance.StartRespawn(photonView.Owner);

        // --- КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ: Информируем Мастер-клиента о смерти ---
        /*   if (PhotonNetwork.IsMasterClient)
           {
               // Если мы сами Мастер-клиент, то вызываем респаун напрямую.
               GameManager.Instance.StartRespawn(photonView.Owner);
               Debug.Log($"[PlayerHealth:{gameObject.name}] Master Client (owner) initiating respawn for {photonView.Owner.NickName}.");
           }
           else // Если мы НЕ Мастер-клиент, но это наш игрок, то отправляем событие Мастер-клиенту.
           {
               // Отправляем Custom Event Мастер-клиенту, чтобы он инициировал респаун.
               // Отправляем ActorNumber умершего игрока, чтобы Мастер-клиент знал, кого респавнить.
               object[] content = new object[] { photonView.Owner.ActorNumber };
               RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
               PhotonNetwork.RaiseEvent(PlayerDiedEventCode, content, raiseEventOptions, SendOptions.SendReliable);
               Debug.Log($"[PlayerHealth:{gameObject.name}] Non-Master Client (owner) sent death event for {photonView.Owner.NickName} to Master Client.");
           } */
    }

    /// <summary>
    /// Активирует/деактивирует визуальные компоненты игрока.
    /// </summary>
    /// <param name="active"></param>
    private void SetPlayerVisuals(bool active)
    {
      /*  MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in renderers)
        {
            mr.enabled = active;
        }
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = active;
        }
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = active; */
    }

    // IPunObservable реализация для синхронизации здоровья
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Мы (владелец) отправляем наше текущее здоровье
            stream.SendNext(currentHealth);
        }
        else
        {
            // Мы (не-владелец) получаем здоровье от владельца
            int receivedHealth = (int)stream.ReceiveNext();

            // Обновляем здоровье только если оно изменилось
            if (currentHealth != receivedHealth)
            {
                currentHealth = receivedHealth;
                Debug.Log($"[PlayerHealth:{gameObject.name}] Received health sync for {photonView.Owner.NickName}: {currentHealth}.");
            }

            // На основе полученного здоровья, обновляем визуальные эффекты для не-владельца
            if (currentHealth <= 0 && gameObject.activeSelf) // Если здоровье 0 или меньше, и объект активен
            {
                SetPlayerVisuals(false);
                if (playerMovement != null) playerMovement.enabled = false; // Отключаем движение удаленного игрока
                Debug.Log($"[PlayerHealth:{gameObject.name}] Remote player {photonView.Owner.NickName} died, hiding visuals.");
            }
            else if (currentHealth > 0 && !gameObject.activeSelf) // Если здоровье > 0, и объект неактивен
            {
                SetPlayerVisuals(true);
                if (playerMovement != null) playerMovement.enabled = true; // Включаем движение удаленного игрока
                Debug.Log($"[PlayerHealth:{gameObject.name}] Remote player {photonView.Owner.NickName} respawned, showing visuals.");
            }
        }
    }
}
