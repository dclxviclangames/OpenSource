using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponNPC : MonoBehaviour
{
    //private NavMeshAgent navMeshAgent;
    public GameObject BulletPrefab;
    public Transform FirePoint; // �����, ������ �������� ���� (������ �������� ������ NPC)
    float BulletSpeed = 15f;
    public Transform truckPos;
    PhotonView photonView;
    private float timeToShot = 0;
    bool canShoot = false;

    public float MaxViewAngle = 45f; // ������������ ���� ������ (45� ����� � 45� ������)
    public float MaxRange = 7f; // ������������ ��������� ��������
    public LayerMask ObstacleMask; // ����, ������� ����� ��������� �� �������� (�����)

    void Start() => photonView = GetComponent<PhotonView>();
    // ����� ��� �������� ����
    void FireLocal()
    {
        // 1. ������� ���� � ����� FirePoint
        GameObject bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);

        // 2. ���� ����� � ����������� transform.forward (���� ������� FirePoint)
        bullet.GetComponent<Rigidbody>().velocity = FirePoint.forward * BulletSpeed;

        // �����: ���� ������ �������������� �� Master Client!
        // ���� ������ �������� Master Client, ����� ��� �������.
    }

    // C# - NPCWeapon.cs (�����������)

    void TryToFire()
    {
        // ������ Master Client ����� ������, ����� NPC ��������.
        //  if (!PhotonNetwork.IsMasterClient) return;

        // ... (��������, ��� ���� � ������� � �� �� ��������) ...

        // ������� ����: "������ ���� NPC ��������!"
        if (CanSeeAndShoot(truckPos)) 
            photonView.RPC("RPC_Fire", RpcTarget.All);
    }

    // ���� ����� ���������� �� ���� ��������
    [PunRPC]
    void RPC_Fire()
    {
        // ������ ������ ������ �������� ������� � ��������� ����.
        // ��� �������� ������, ������ ��� ���������������� ������ �������, � �� ��� ������.
        FireLocal();
        canShoot = false;
    }

    private void Update()
    {
        if(photonView.IsMine)
        {

            if(Vector3.Distance(truckPos.position, transform.position) < 7f && canShoot == true)
            {
                TryToFire();
            }

            if(timeToShot > 1f)
            {
                canShoot = true;
                timeToShot = 0;
            }
            else
            {
                timeToShot += Time.deltaTime;
            }
        }
    }

    bool CanSeeAndShoot(Transform target)
    {
        // 1. �������� ���������
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > MaxRange)
        {
            return false; // ������� ������
        }

        // 2. �������� ���� ������ (FOV)
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // ��������� ���� ����� ������������, ���� ������� NPC (transform.forward) � ������������ �� ����
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (angle > MaxViewAngle)
        {
            return false; // ���� ��������� ��� ���� ������
        }

        // 3. �������� �������� (Raycast)
        RaycastHit hit;

        // ��������� ��� �� NPC � ����
        if (Physics.Raycast(transform.position, directionToTarget, out hit, MaxRange, ObstacleMask))
        {
            // ���� ��� �������� � ���-��...

            // ...���������, ��� ��� ���-�� � ��� ������ ���� ����.
            if (hit.collider.CompareTag("Truck"))
            {
                return true; // ���� ����� � ��� �������
            }
            else
            {
                return false; // ��� ����� � �����/�����/������ ������
            }
        }

        // ���� Raycast ������ �� ����� (������ ��� ������� ������), �� ��������� ���� ��, 
        // ���������� false ��� true � ����������� �� ����, ��� �� ������ ������������ ������� ������. 
        // ����� ������� false, ����� �������� �������� � ������.
        return false;
    }
}
