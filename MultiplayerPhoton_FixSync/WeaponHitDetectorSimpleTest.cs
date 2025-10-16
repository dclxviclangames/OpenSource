// WeaponHitDetectorSimpleTest.cs
using UnityEngine;
using Photon.Pun;
using System.Collections; // Для корутин, если понадобится, но не в этом простом тесте.

/// <summary>
/// Простой скрипт для обнаружения попаданий оружием и нанесения урона по сети.
/// Предназначен для быстрого тестирования.
/// Прикрепите к дочернему GameObject (хитбоксу) на оружии персонажа.
/// </summary>
[RequireComponent(typeof(Collider))] // Убедимся, что на объекте есть коллайдер
[RequireComponent(typeof(Rigidbody))] // Убедимся, что есть Rigidbody для OnTriggerEnter (будет кинематическим)
public class WeaponHitDetectorSimpleTest : MonoBehaviourPunCallbacks
{
    [Tooltip("Урон, наносимый этим оружием при попадании.")]
    public int weaponDamage = 10;

    [Tooltip("Задержка между нанесением урона одним и тем же оружием одному и тому же игроку (для предотвращения многократного урона за один кадр).")]
    public float hitCooldown = 0.5f;

    private float lastHitTime; // Время последнего попадания
    private PhotonView playerPhotonView; // PhotonView родительского игрока (владельца оружия)

    void Awake()
    {
        // Убеждаемся, что на этом GameObject есть Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Collider component not found! Please add a Collider to the weapon's hitbox.");
            enabled = false;
            return;
        }
        col.isTrigger = true; // Важно: делаем его триггером

        // Убеждаемся, что на этом GameObject есть Rigidbody, иначе OnTriggerEnter не сработает надежно.
        // Делаем его кинематическим, чтобы он не реагировал на физику, а только обнаруживал.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Очень важно для оружия
        rb.useGravity = false; // Отключаем гравитацию

        // Получаем PhotonView от родительского объекта игрока.
        // Оружие - это дочерний объект, поэтому PhotonView должен быть на игроке-родителе.
        playerPhotonView = GetComponentInParent<PhotonView>();
        if (playerPhotonView == null)
        {
            Debug.LogError($"[WeaponHitDetectorSimpleTest:{gameObject.name}] PhotonView not found on parent player! Networked damage will not work. Ensure this script is a child of a GameObject with PhotonView.");
            enabled = false;
        }

        // В начале теста коллайдер оружия может быть активен, чтобы сразу проверять попадания.
        // В реальной игре его нужно будет включать/отключать через Animation Events.
        // col.enabled = false; // Можно начать с отключенным, если хотите включать вручную.
    }

    /// <summary>
    /// Вызывается, когда триггер коллайдера оружия пересекает другой коллайдер.
    /// </summary>
    /// <param name="other">Коллайдер, с которым произошло пересечение.</param>
    void OnTriggerEnter(Collider other)
    {
        // === КЛЮЧЕВАЯ СЕТЕВАЯ ЛОГИКА ===
        // 1. Убеждаемся, что мы - владелец игрока, который держит это оружие.
        // Только владелец должен инициировать нанесение урона по сети.
        if (playerPhotonView == null || !playerPhotonView.IsMine)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Not owner of attacking player. Skipping damage logic.");
            return;
        }

        // 2. Проверяем кулдаун, чтобы не наносить урон многократно за одно "касание"
        if (Time.time < lastHitTime + hitCooldown)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] On cooldown. Skipping damage.");
            return;
        }

        // Пытаемся получить компонент PlayerHealth от того, с чем столкнулись.
        // Он может быть на другом GameObject, поэтому используем GetComponentInParent.
        PlayerHealth otherPlayerHealth = other.GetComponent<PlayerHealth>();
        EnemyController enemyController = other.GetComponent<EnemyController>();
      //  Rigidbody rigidbody = enemyController.gameObject.GetComponent<Rigidbody>();


        // 3. Если мы нашли PlayerHealth и это НЕ наш собственный игрок.
        if (otherPlayerHealth != null && otherPlayerHealth.photonView.Owner != playerPhotonView.Owner)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Weapon hit player {otherPlayerHealth.photonView.Owner.NickName}. Calling RPC_TakeDamage.");

            // === ВЫЗЫВАЕМ ВАШ СУЩЕСТВУЮЩИЙ PunRPC ===
            otherPlayerHealth.photonView.RPC("TakeDamage", RpcTarget.All, weaponDamage);

            lastHitTime = Time.time; // Сбрасываем кулдаун
        }
        else if (otherPlayerHealth != null && otherPlayerHealth.photonView.Owner == playerPhotonView.Owner)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Weapon hit own player. Skipping damage.");
        }
        else
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Weapon hit {other.gameObject.name}, but no PlayerHealth component found.");
        }

        if (enemyController != null && playerPhotonView.IsMine)
        {
            enemyController.photonView.RPC("TakeDamages", RpcTarget.All, weaponDamage);
            photonView.RPC("AddScore", RpcTarget.All, 50);
            //  rigidbody.AddForce()
        }
        // Можете добавить здесь логику для эффектов попадания (звук, частицы).
    }
}