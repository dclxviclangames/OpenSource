// PlayerHealth.cs
using UnityEngine;
using Photon.Pun;
using System.Collections; // Для корутин

/// <summary>
/// Управляет здоровьем игрока и синхронизирует его по сети.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public int maxHealth = 100;

    [SerializeField]
    private int currentHealth;

    private PhotonView photonView;
    private Collider playerCollider; // Для отключения при смерти
    private Renderer playerRenderer; // Для отключения при смерти

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>(); // Или MeshRenderer, SkinnedMeshRenderer
    }

    void OnEnable()
    {
        // Инициализируем здоровье при активации персонажа (например, после респавна)
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log($"[{photonView.Owner.NickName}] Инициализировано здоровье: {currentHealth}/{maxHealth}");

        // При активации включаем коллайдер и рендерер, если они были выключены
        if (playerCollider != null) playerCollider.enabled = true;
        if (playerRenderer != null) playerRenderer.enabled = true;
    }

    /// <summary>
    /// Получает урон. Должен быть вызван владельцем объекта, который затем инициирует RPC.
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    public void TakeDamage(int amount)
    {
        if (photonView.IsMine)
        {
            // Вызываем RPC, чтобы урон был синхронизирован на всех клиентах
            photonView.RPC("RPC_ApplyDamage", RpcTarget.All, amount);
            Debug.Log($"[{photonView.Owner.NickName}] Локально запущен RPC на урон: {amount}");
        }
        // Если не IsMine, то урон должен быть нанесен владельцем снаряда, который вызвал TakeDamage.
        // Или другим игроком, который вызвал RPC на нас.
        // Здесь важно избегать многократного вызова RPC.
    }

    /// <summary>
    /// RPC-метод для применения урона. Вызывается по сети на всех клиентах.
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    [PunRPC]
    public void RPC_ApplyDamage(int amount)
    {
        if (currentHealth <= 0) return; // Уже мертв

        currentHealth -= amount;
        Debug.Log($"[{photonView.Owner.NickName}] Получил урон: {amount}. Текущее здоровье: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Логика смерти игрока. Вызывается по сети.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[{photonView.Owner.NickName}] Умер!");

        // Отключаем коллайдер и рендерер, чтобы игрок стал "невидимым" и неактивным
        if (playerCollider != null) playerCollider.enabled = false;
        if (playerRenderer != null) playerRenderer.enabled = false;

        // Если это наш локальный игрок, сообщаем GameManager о необходимости респавна
        if (photonView.IsMine)
        {
            // Запускаем корутину респавна через GameManager
            GameManager.Instance?.StartRespawn(photonView.Owner);
        }
    }

    /// <summary>
    /// Метод для респавна (сброса состояния и активации).
    /// </summary>
    public void ResetHealthAndActivate()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Включаем обратно коллайдер и рендерер
        if (playerCollider != null) playerCollider.enabled = true;
        if (playerRenderer != null) playerRenderer.enabled = true;

        Debug.Log($"[{photonView.Owner.NickName}] Респавнен с полным здоровьем: {currentHealth}/{maxHealth}");
    }

    private void UpdateHealthUI()
    {
        // ... (логика обновления UI здоровья, если есть)
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
