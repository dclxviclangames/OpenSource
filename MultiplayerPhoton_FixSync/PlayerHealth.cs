// PlayerHealth.cs
using UnityEngine;
using Photon.Pun;
using System.Collections;
using ExitGames.Client.Photon; // ��������� ��� Custom Event
using Photon.Realtime; // ��������� ��� RaiseEventOptions, ReceiverGroup
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// ��������� ��������� ������, ���������� ����� � ���������� �������.
/// </summary>
public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable // ��������� IPunObservable
{
    [Tooltip("������������ �������� ������.")]
    public int maxHealth = 100;

    [Tooltip("������� �������� ������.")]
   // [SerializeField]
    public int currentHealth;

    [Tooltip("��������� PlayerMovement ��� ���������� ��� ������.")]
    public PlayerMovement playerMovement; // ���������� ���� ��������� PlayerMovement � ����������

    private PhotonView photonView;
    float timeHeal = 0;

    // ���������������� ��� ������� ��� ������ ������. ������ ���� ����������.
    private const byte PlayerDiedEventCode = 100;

    public Slider slider;

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
        // �������������� �������� ������ ��� ������ ������/������� � ������ �� ���������.
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
    /// ���������� �������� �� ��������� � ���������� ������.
    /// ���������� GameManager ��� ������/�������.
    /// </summary>
    public void ResetHealthAndActivate()
    {
        currentHealth = maxHealth;
        if (playerMovement != null)
        {
            playerMovement.enabled = true; // ����������, ��� �������� ��������
        }
        SetPlayerVisuals(true); // ���������� ������ ������
        Debug.Log($"[PlayerHealth:{gameObject.name}] Health reset to {maxHealth}. Player activated.");
    }

    /// <summary>
    /// ������� ���� ������. ���������� �� RPC.
    /// </summary>
    /// <param name="damage">���������� ���������� �����.</param>
    [PunRPC]
    public void TakeDamage(int damage)
    {
        // ���� �������������� ������ �� ��������� PhotonView.
        // ��� �����������, ��� ������ �������� ������, ����� �� �������.
        if (!photonView.IsMine)
        {
            Debug.Log($"[PlayerHealth:{gameObject.name}] Received damage RPC but not owner. Skipping direct damage application. Current health (synced): {currentHealth}");
            return;
        }
        playerMovement.animator.SetTrigger("Damage");
        

        currentHealth -= damage;
        Debug.Log($"[PlayerHealth:{gameObject.name}] Took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

        slider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    public void Heal(int damage)
    {
        // ���� �������������� ������ �� ��������� PhotonView.
        // ��� �����������, ��� ������ �������� ������, ����� �� �������.
        if (!photonView.IsMine)
        {
            Debug.Log($"[PlayerHealth:{gameObject.name}] Received damage RPC but not owner. Skipping direct damage application. Current health (synced): {currentHealth}");
            return;
        }
       // playerMovement.animator.SetTrigger("Damage");
        

        
        Debug.Log($"[PlayerHealth:{gameObject.name}] Took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

        

        if (currentHealth >= 0)
        {
            SetPlayerStatus(true);
            currentHealth += damage;
            slider.value = currentHealth;
        }
    }

    /// <summary>
    /// ������ ������ ������. ���������� �������� �� ��������� ����� ��������� ������������ �����.
    /// ���������� ������� ����� GameManager (�� ������-�������).
    /// </summary>
    private void Die()
    {
        Debug.Log($"[PlayerHealth:{gameObject.name}] Player {photonView.Owner.NickName} has died!");

        // ��������� �������� � ���������� ������� ��������
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        SetPlayerVisuals(false); // �������� ������ ������
        if(photonView.IsMine)
        {
            SetPlayerStatus(false); // NEW CHANGE FOR ALIVE CHECK
            playerMovement.animator.SetTrigger("Death");
            GameManager.Instance.StartRespawn(photonView.Owner);
        }
            

        // --- �������� �����������: ����������� ������-������� � ������ ---
        /*   if (PhotonNetwork.IsMasterClient)
           {
               // ���� �� ���� ������-������, �� �������� ������� ��������.
               GameManager.Instance.StartRespawn(photonView.Owner);
               Debug.Log($"[PlayerHealth:{gameObject.name}] Master Client (owner) initiating respawn for {photonView.Owner.NickName}.");
           }
           else // ���� �� �� ������-������, �� ��� ��� �����, �� ���������� ������� ������-�������.
           {
               // ���������� Custom Event ������-�������, ����� �� ����������� �������.
               // ���������� ActorNumber �������� ������, ����� ������-������ ����, ���� ����������.
               object[] content = new object[] { photonView.Owner.ActorNumber };
               RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
               PhotonNetwork.RaiseEvent(PlayerDiedEventCode, content, raiseEventOptions, SendOptions.SendReliable);
               Debug.Log($"[PlayerHealth:{gameObject.name}] Non-Master Client (owner) sent death event for {photonView.Owner.NickName} to Master Client.");
           } */
    }

    private void SetPlayerStatus(bool isAlive)
    {
        Hashtable props = new Hashtable { { "IsAlive", isAlive } };
        // ���������� �������� ���� ������� � �������
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void Update()
    {
        slider.value = currentHealth;
        if (timeHeal > 7 && currentHealth < maxHealth)
        {
            photonView.RPC("Heal", RpcTarget.All, 20);
            timeHeal = 0;
        }
        else
        {
            timeHeal += Time.deltaTime;
        }
    }

    /// <summary>
    /// ����������/������������ ���������� ���������� ������.
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

    // IPunObservable ���������� ��� ������������� ��������
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // �� (��������) ���������� ���� ������� ��������
            stream.SendNext(currentHealth);
        }
        else
        {
            // �� (��-��������) �������� �������� �� ���������
            int receivedHealth = (int)stream.ReceiveNext();

            // ��������� �������� ������ ���� ��� ����������
            if (currentHealth != receivedHealth)
            {
                currentHealth = receivedHealth;
                Debug.Log($"[PlayerHealth:{gameObject.name}] Received health sync for {photonView.Owner.NickName}: {currentHealth}.");
            }

            // �� ������ ����������� ��������, ��������� ���������� ������� ��� ��-���������
            if (currentHealth <= 0 && gameObject.activeSelf) // ���� �������� 0 ��� ������, � ������ �������
            {
                SetPlayerVisuals(false);
                if (playerMovement != null) playerMovement.enabled = false; // ��������� �������� ���������� ������
                Debug.Log($"[PlayerHealth:{gameObject.name}] Remote player {photonView.Owner.NickName} died, hiding visuals.");
            }
            else if (currentHealth > 0 && !gameObject.activeSelf) // ���� �������� > 0, � ������ ���������
            {
                SetPlayerVisuals(true);
                if (playerMovement != null) playerMovement.enabled = true; // �������� �������� ���������� ������
                Debug.Log($"[PlayerHealth:{gameObject.name}] Remote player {photonView.Owner.NickName} respawned, showing visuals.");
            }
        }
    }
}
