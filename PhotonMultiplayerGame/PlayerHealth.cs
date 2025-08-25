// PlayerHealth.cs
using UnityEngine;
using Photon.Pun;
using System.Collections; // ��� �������

/// <summary>
/// ��������� ��������� ������ � �������������� ��� �� ����.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public int maxHealth = 100;

    [SerializeField]
    private int currentHealth;

    private PhotonView photonView;
    private Collider playerCollider; // ��� ���������� ��� ������
    private Renderer playerRenderer; // ��� ���������� ��� ������

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>(); // ��� MeshRenderer, SkinnedMeshRenderer
    }

    void OnEnable()
    {
        // �������������� �������� ��� ��������� ��������� (��������, ����� ��������)
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log($"[{photonView.Owner.NickName}] ���������������� ��������: {currentHealth}/{maxHealth}");

        // ��� ��������� �������� ��������� � ��������, ���� ��� ���� ���������
        if (playerCollider != null) playerCollider.enabled = true;
        if (playerRenderer != null) playerRenderer.enabled = true;
    }

    /// <summary>
    /// �������� ����. ������ ���� ������ ���������� �������, ������� ����� ���������� RPC.
    /// </summary>
    /// <param name="amount">���������� �����.</param>
    public void TakeDamage(int amount)
    {
        if (photonView.IsMine)
        {
            // �������� RPC, ����� ���� ��� ��������������� �� ���� ��������
            photonView.RPC("RPC_ApplyDamage", RpcTarget.All, amount);
            Debug.Log($"[{photonView.Owner.NickName}] �������� ������� RPC �� ����: {amount}");
        }
        // ���� �� IsMine, �� ���� ������ ���� ������� ���������� �������, ������� ������ TakeDamage.
        // ��� ������ �������, ������� ������ RPC �� ���.
        // ����� ����� �������� ������������� ������ RPC.
    }

    /// <summary>
    /// RPC-����� ��� ���������� �����. ���������� �� ���� �� ���� ��������.
    /// </summary>
    /// <param name="amount">���������� �����.</param>
    [PunRPC]
    public void RPC_ApplyDamage(int amount)
    {
        if (currentHealth <= 0) return; // ��� �����

        currentHealth -= amount;
        Debug.Log($"[{photonView.Owner.NickName}] ������� ����: {amount}. ������� ��������: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ������ ������ ������. ���������� �� ����.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[{photonView.Owner.NickName}] ����!");

        // ��������� ��������� � ��������, ����� ����� ���� "���������" � ����������
        if (playerCollider != null) playerCollider.enabled = false;
        if (playerRenderer != null) playerRenderer.enabled = false;

        // ���� ��� ��� ��������� �����, �������� GameManager � ������������� ��������
        if (photonView.IsMine)
        {
            // ��������� �������� �������� ����� GameManager
            GameManager.Instance?.StartRespawn(photonView.Owner);
        }
    }

    /// <summary>
    /// ����� ��� �������� (������ ��������� � ���������).
    /// </summary>
    public void ResetHealthAndActivate()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // �������� ������� ��������� � ��������
        if (playerCollider != null) playerCollider.enabled = true;
        if (playerRenderer != null) playerRenderer.enabled = true;

        Debug.Log($"[{photonView.Owner.NickName}] ��������� � ������ ���������: {currentHealth}/{maxHealth}");
    }

    private void UpdateHealthUI()
    {
        // ... (������ ���������� UI ��������, ���� ����)
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
