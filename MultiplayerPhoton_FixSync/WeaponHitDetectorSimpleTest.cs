// WeaponHitDetectorSimpleTest.cs
using UnityEngine;
using Photon.Pun;
using System.Collections; // ��� �������, ���� �����������, �� �� � ���� ������� �����.

/// <summary>
/// ������� ������ ��� ����������� ��������� ������� � ��������� ����� �� ����.
/// ������������ ��� �������� ������������.
/// ���������� � ��������� GameObject (��������) �� ������ ���������.
/// </summary>
[RequireComponent(typeof(Collider))] // ��������, ��� �� ������� ���� ���������
[RequireComponent(typeof(Rigidbody))] // ��������, ��� ���� Rigidbody ��� OnTriggerEnter (����� ��������������)
public class WeaponHitDetectorSimpleTest : MonoBehaviourPunCallbacks
{
    [Tooltip("����, ��������� ���� ������� ��� ���������.")]
    public int weaponDamage = 10;

    [Tooltip("�������� ����� ���������� ����� ����� � ��� �� ������� ������ � ���� �� ������ (��� �������������� ������������� ����� �� ���� ����).")]
    public float hitCooldown = 0.5f;

    private float lastHitTime; // ����� ���������� ���������
    private PhotonView playerPhotonView; // PhotonView ������������� ������ (��������� ������)

    void Awake()
    {
        // ����������, ��� �� ���� GameObject ���� Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Collider component not found! Please add a Collider to the weapon's hitbox.");
            enabled = false;
            return;
        }
        col.isTrigger = true; // �����: ������ ��� ���������

        // ����������, ��� �� ���� GameObject ���� Rigidbody, ����� OnTriggerEnter �� ��������� �������.
        // ������ ��� ��������������, ����� �� �� ���������� �� ������, � ������ �����������.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // ����� ����� ��� ������
        rb.useGravity = false; // ��������� ����������

        // �������� PhotonView �� ������������� ������� ������.
        // ������ - ��� �������� ������, ������� PhotonView ������ ���� �� ������-��������.
        playerPhotonView = GetComponentInParent<PhotonView>();
        if (playerPhotonView == null)
        {
            Debug.LogError($"[WeaponHitDetectorSimpleTest:{gameObject.name}] PhotonView not found on parent player! Networked damage will not work. Ensure this script is a child of a GameObject with PhotonView.");
            enabled = false;
        }

        // � ������ ����� ��������� ������ ����� ���� �������, ����� ����� ��������� ���������.
        // � �������� ���� ��� ����� ����� ��������/��������� ����� Animation Events.
        // col.enabled = false; // ����� ������ � �����������, ���� ������ �������� �������.
    }

    /// <summary>
    /// ����������, ����� ������� ���������� ������ ���������� ������ ���������.
    /// </summary>
    /// <param name="other">���������, � ������� ��������� �����������.</param>
    void OnTriggerEnter(Collider other)
    {
        // === �������� ������� ������ ===
        // 1. ����������, ��� �� - �������� ������, ������� ������ ��� ������.
        // ������ �������� ������ ������������ ��������� ����� �� ����.
        if (playerPhotonView == null || !playerPhotonView.IsMine)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Not owner of attacking player. Skipping damage logic.");
            return;
        }

        // 2. ��������� �������, ����� �� �������� ���� ����������� �� ���� "�������"
        if (Time.time < lastHitTime + hitCooldown)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] On cooldown. Skipping damage.");
            return;
        }

        // �������� �������� ��������� PlayerHealth �� ����, � ��� �����������.
        // �� ����� ���� �� ������ GameObject, ������� ���������� GetComponentInParent.
        PlayerHealth otherPlayerHealth = other.GetComponent<PlayerHealth>();
        EnemyController enemyController = other.GetComponent<EnemyController>();
      //  Rigidbody rigidbody = enemyController.gameObject.GetComponent<Rigidbody>();


        // 3. ���� �� ����� PlayerHealth � ��� �� ��� ����������� �����.
        if (otherPlayerHealth != null && otherPlayerHealth.photonView.Owner != playerPhotonView.Owner)
        {
            Debug.Log($"[WeaponHitDetectorSimpleTest:{gameObject.name}] Weapon hit player {otherPlayerHealth.photonView.Owner.NickName}. Calling RPC_TakeDamage.");

            // === �������� ��� ������������ PunRPC ===
            otherPlayerHealth.photonView.RPC("TakeDamage", RpcTarget.All, weaponDamage);

            lastHitTime = Time.time; // ���������� �������
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
        // ������ �������� ����� ������ ��� �������� ��������� (����, �������).
    }
}