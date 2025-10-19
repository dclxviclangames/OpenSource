using UnityEngine;
using Photon.Pun;
using System.Collections; // ��������� ��� �������
using Photon.Realtime;

public class CarsController : MonoBehaviourPun
{
    //FIX ERRORS TOMORROW 17.10.25
    // ������������� ���������
    public float motorSpeed = 15f;
    public float rotationSpeed = 100f;
    public float gravity = 20f;
    // �����: ���������, ��� � ������ ���� CharacterController.

    private CharacterController carController;
    private Vector3 moveDirection = Vector3.zero;
    private Transform driverSeat;

    private int driverID = 0;

    void Awake()
    {
        carController = GetComponent<CharacterController>();
        driverSeat = transform.Find("DriverSeat") ?? transform;
    }

    void Update()
    {
        // 1. ������� �����: ������ �������� (��������) ��������� ������
        if (!photonView.IsMine)
        {
            return;
        }

        if (this.driverID == 0)
            return;

        // 2. ��������� �����
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 3. ������ �������� � ��������

        transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        Vector3 forward = transform.forward * verticalInput * motorSpeed;

        // ���������� (��� CharacterController)
        if (carController.isGrounded)
        {
            moveDirection = forward;
        }
        else
        {
            moveDirection.x = forward.x;
            moveDirection.z = forward.z;
            moveDirection.y -= gravity * Time.deltaTime;
        }

        carController.Move(moveDirection * Time.deltaTime);

        // 4. ������������� ��������
        if (driverSeat.childCount > 0)
        {
            // ����� �������������� ������� ������ � ������� (������ ��� "���������" ���������)
            driverSeat.GetChild(0).position = driverSeat.position;
            driverSeat.GetChild(0).rotation = driverSeat.rotation;
        }

        // 5. ����� �� ������ (�������� ��� ��� ������ ������, ��������, "E")
        if ( Input.GetKeyDown(KeyCode.F))
        {
            ExitCar(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // =================================================================
    // ������ �����/������ (������ ���� ���������� ��� ������ �� ������ ��������)
    // =================================================================

    // ���������� �������� �� ������, ������� ����� ����� � ������
    public void EnterCar(PhotonView playerView)
    {
        // ���������, ��� ��� ��� ����������� ����� � ��� ������ �� ������
        if (!playerView.IsMine || driverSeat.childCount > 0) return;

        

        // 1. ����������� �������� �������. (��� ��������� ��� ���������!)
        photonView.RequestOwnership();

        this.driverID = playerView.ViewID;

        // 2. ��������� ���������� ����� �������
        PlayerMovement playerMovement = playerView.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // *******************************************************************
        // * �����������: ��������� ��� CharacterController, ����� ����� �������� ������ *
        // *******************************************************************
        playerView.GetComponent<CharacterController>().enabled = false;

        // 3. �������� RPC, ����� ��� ������� ������� ������ �������� �������� ������
        photonView.RPC("RPC_SetDriver", RpcTarget.All, playerView.ViewID);
    }

    // ���������� �������� �� ��������� ������
    public void ExitCar(int driverActorNumber)
    {
        // 1. ������ �������� ����, ���� �� �� �������� (�� ������ ������)
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        // 2. �������� RPC, ����� ��� ������� "��������" ������
        photonView.RPC("RPC_RemoveDriver", RpcTarget.All, driverID);

        // 3. ���������� �������� ������ (���� ��� ����� ����� � ����� ����)
        // photonView.TransferOwnership(driverViewID); // ����� �������� ��� ������
    }


    // =================================================================
    // RPC-������ (������������� �� ����)
    // =================================================================

    // �������������� ������� ������
    [PunRPC]
    private void RPC_SetDriver(int driverViewID)
    {
        PhotonView driverView = PhotonView.Find(driverViewID);
        this.driverID = driverViewID;
        if (driverView != null)
        {
            Transform seat = transform.Find("DriverSeat") ?? transform;

            driverView.transform.SetParent(seat);
            driverView.transform.localPosition = Vector3.zero;
            driverView.transform.localRotation = Quaternion.identity;
        }
    }

    // �������������� ������� ������
    [PunRPC]
    private void RPC_RemoveDriver(int driverViewID) // <--- ��������� ViewID
    {
        // ������� ������ �� ViewID
        PhotonView driverView = PhotonView.Find(driverViewID);

        if (driverView != null)
        {
            // ������ ���� � ������� ���� CharacterController, �� ����� ��� ��������
            CharacterController playerCC = driverView.GetComponent<CharacterController>();

            if (playerCC != null) // <--- ��� �������� ������� MissingComponentException
            {
                // 1. ���������� ������ � ���������� �����
                Vector3 exitPosition = transform.position - transform.forward * 2f;
                driverView.transform.SetParent(null);
                driverView.transform.position = exitPosition;

                // 2. ������ ��������� ����� �������� ���� ���������� �������
                if (driverView.IsMine)
                {
                    PlayerMovement playerMovement = driverView.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.enabled = true;
                    }
                    playerCC.enabled = true; // �������� CC, ������� �� ��������� ��� �����
                }
            }
            else
            {
                Debug.LogError($"[CAR] �� ������� � ViewID {driverViewID} ��� CharacterController. ��������, ��� NPC ��� ������.");
            }
        }

        // 3. ���������� ID �������� �� ���� ��������
        this.driverID = 0;
    }
}
