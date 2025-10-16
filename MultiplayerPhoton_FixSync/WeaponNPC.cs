using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponNPC : MonoBehaviour
{
    //private NavMeshAgent navMeshAgent;
    public GameObject BulletPrefab;
    public Transform FirePoint; // Точка, откуда вылетает пуля (пустой дочерний объект NPC)
    float BulletSpeed = 15f;
    public Transform truckPos;
    PhotonView photonView;
    private float timeToShot = 0;
    bool canShoot = false;

    public float MaxViewAngle = 45f; // Максимальный угол обзора (45° влево и 45° вправо)
    public float MaxRange = 7f; // Максимальная дистанция стрельбы
    public LayerMask ObstacleMask; // Слой, который нужно проверять на преграды (стены)

    void Start() => photonView = GetComponent<PhotonView>();
    // Метод для создания пули
    void FireLocal()
    {
        // 1. Создаем пулю в точке FirePoint
        GameObject bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);

        // 2. Пуля летит в направлении transform.forward (куда смотрит FirePoint)
        bullet.GetComponent<Rigidbody>().velocity = FirePoint.forward * BulletSpeed;

        // ВАЖНО: Урон должен рассчитываться на Master Client!
        // Пуля должна сообщить Master Client, когда она попадет.
    }

    // C# - NPCWeapon.cs (Продолжение)

    void TryToFire()
    {
        // Только Master Client может решать, когда NPC стреляет.
        //  if (!PhotonNetwork.IsMasterClient) return;

        // ... (Проверка, что цель в радиусе и не на кулдауне) ...

        // Говорим всем: "Сейчас этот NPC стреляет!"
        if (CanSeeAndShoot(truckPos)) 
            photonView.RPC("RPC_Fire", RpcTarget.All);
    }

    // ЭТОТ МЕТОД ВЫЗЫВАЕТСЯ НА ВСЕХ КЛИЕНТАХ
    [PunRPC]
    void RPC_Fire()
    {
        // Теперь каждый клиент локально создает и запускает пулю.
        // Это экономит трафик, потому что синхронизируется только событие, а не сам снаряд.
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
        // 1. Проверка дистанции
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > MaxRange)
        {
            return false; // Слишком далеко
        }

        // 2. Проверка Угла Обзора (FOV)
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Вычисляем угол между направлением, куда смотрит NPC (transform.forward) и направлением на цель
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (angle > MaxViewAngle)
        {
            return false; // Цель находится вне угла обзора
        }

        // 3. Проверка Преграды (Raycast)
        RaycastHit hit;

        // Выпускаем луч от NPC к цели
        if (Physics.Raycast(transform.position, directionToTarget, out hit, MaxRange, ObstacleMask))
        {
            // Если луч попадает в что-то...

            // ...проверяем, что это что-то — это именно наша фура.
            if (hit.collider.CompareTag("Truck"))
            {
                return true; // Цель видна и нет преград
            }
            else
            {
                return false; // Луч попал в стену/столб/другой объект
            }
        }

        // Если Raycast ничего не задел (ошибка или слишком далеко), но дистанция была ОК, 
        // возвращаем false или true в зависимости от того, как вы хотите обрабатывать крайние случаи. 
        // Лучше вернуть false, чтобы избежать стрельбы в никуда.
        return false;
    }
}
