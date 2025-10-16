using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BossFlee : MonoBehaviour
{
    public Rigidbody BossRigidbody;
    public float FleeForce = 2f; // ���� ������ ����, ��� � �������, ����� �������
    public float RotationSpeed = 3f; // �������� ��������

    private Transform closestPlayer; // ��������� �����, �� �������� �� �����
    PhotonView photonView;
    
    
    void Start() => photonView = GetComponent<PhotonView>();

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 1. ������� ���������� ������ (���������� ���� ������ ������ �����)
            closestPlayer = FindClosestTarget();

            if (closestPlayer != null)
            {
                FleeTarget(closestPlayer);
            }
        }
    }

    void FleeTarget(Transform target)
    {
        // 1. ���������� ����������� � ���� (��� ������)
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;

        // 2. ���������� ����������� �� ���� (��������!)
        Vector3 directionToFlee = -directionToTarget.normalized;

        // 3. ������������ ����������� ������� (����� ����������� � ������� �� ����)
        Quaternion targetRotation = Quaternion.LookRotation(directionToFlee);

        // 4. ������ ������������ �����
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * RotationSpeed);

        // 5. ������������ ���� ������ (���� ��� ���������)
        BossRigidbody.AddForce(transform.forward * FleeForce, ForceMode.Acceleration);
    }

    // C# - NPCEnemyLogic.cs (����������� ������� ������ ����)

    private Transform FindClosestTarget()
    {
        PlayerTarget[] allTargets = FindObjectsOfType<PlayerTarget>();

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (PlayerTarget potentialTarget in allTargets)
        {
            // ���������, ��� ��� �� ��� ����������� NPC � ��� ���� �������
            if (potentialTarget.gameObject.activeInHierarchy)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude; // ������� ���������� (�������, ��� Distance)

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.transform;
                }
            }
        }
        return bestTarget;
    }
}
