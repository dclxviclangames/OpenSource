using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BossFlee : MonoBehaviour
{
    public Rigidbody BossRigidbody;
    public float FleeForce = 2f; // Чуть больше силы, чем у игроков, чтобы убегать
    public float RotationSpeed = 3f; // Скорость поворота

    private Transform closestPlayer; // Ближайший игрок, от которого мы бежим
    PhotonView photonView;
    
    
    void Start() => photonView = GetComponent<PhotonView>();

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 1. Находим ближайшего игрока (используем вашу логику поиска целей)
            closestPlayer = FindClosestTarget();

            if (closestPlayer != null)
            {
                FleeTarget(closestPlayer);
            }
        }
    }

    void FleeTarget(Transform target)
    {
        // 1. Определяем направление К цели (как раньше)
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;

        // 2. Определяем направление ОТ цели (инверсия!)
        Vector3 directionToFlee = -directionToTarget.normalized;

        // 3. Рассчитываем необходимый поворот (нужно повернуться в сторону ОТ цели)
        Quaternion targetRotation = Quaternion.LookRotation(directionToFlee);

        // 4. Плавно поворачиваем Босса
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * RotationSpeed);

        // 5. Прикладываем силу вперед (Босс сам двигается)
        BossRigidbody.AddForce(transform.forward * FleeForce, ForceMode.Acceleration);
    }

    // C# - NPCEnemyLogic.cs (Обновленная функция поиска цели)

    private Transform FindClosestTarget()
    {
        PlayerTarget[] allTargets = FindObjectsOfType<PlayerTarget>();

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (PlayerTarget potentialTarget in allTargets)
        {
            // Убедитесь, что это не ваш собственный NPC и что цель активна
            if (potentialTarget.gameObject.activeInHierarchy)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude; // Квадрат расстояния (быстрее, чем Distance)

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
