using UnityEngine;
using Photon.Pun;
using System.Collections; // Добавлено для корутин
using Photon.Realtime;

public class CarsController : MonoBehaviourPun
{
    //FIX ERRORS TOMORROW 17.10.25
    // Настраиваемые параметры
    public float motorSpeed = 15f;
    public float rotationSpeed = 100f;
    public float gravity = 20f;
    // ВАЖНО: Убедитесь, что у машины есть CharacterController.

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
        // 1. СЕТЕВОЙ ЗАМОК: ТОЛЬКО ВЛАДЕЛЕЦ (ВОДИТЕЛЬ) УПРАВЛЯЕТ ВВОДОМ
        if (!photonView.IsMine)
        {
            return;
        }

        if (this.driverID == 0)
            return;

        // 2. Обработка ввода
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 3. Расчет движения и поворота

        transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        Vector3 forward = transform.forward * verticalInput * motorSpeed;

        // Гравитация (для CharacterController)
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

        // 4. Синхронизация водителя
        if (driverSeat.childCount > 0)
        {
            // Здесь синхронизируем позицию игрока с креслом (делает его "невидимым" водителем)
            driverSeat.GetChild(0).position = driverSeat.position;
            driverSeat.GetChild(0).rotation = driverSeat.rotation;
        }

        // 5. Выход из машины (Добавьте это для кнопки выхода, например, "E")
        if ( Input.GetKeyDown(KeyCode.F))
        {
            ExitCar(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // =================================================================
    // МЕТОДЫ ВХОДА/ВЫХОДА (Должны быть публичными для вызова из других скриптов)
    // =================================================================

    // Вызывается локально на игроке, который хочет сесть в машину
    public void EnterCar(PhotonView playerView)
    {
        // Проверяем, что это наш собственный игрок и что машина не занята
        if (!playerView.IsMine || driverSeat.childCount > 0) return;

        

        // 1. Запрашиваем владение машиной. (Это позволяет нам управлять!)
        photonView.RequestOwnership();

        this.driverID = playerView.ViewID;

        // 2. Отключаем управление пешим игроком
        PlayerMovement playerMovement = playerView.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // *******************************************************************
        // * ОБЯЗАТЕЛЬНО: ОТКЛЮЧАЕМ ЕГО CharacterController, иначе будет конфликт физики *
        // *******************************************************************
        playerView.GetComponent<CharacterController>().enabled = false;

        // 3. Вызываем RPC, чтобы все клиенты сделали игрока дочерним объектом машины
        photonView.RPC("RPC_SetDriver", RpcTarget.All, playerView.ViewID);
    }

    // Вызывается локально на владельце машины
    public void ExitCar(int driverActorNumber)
    {
        // 1. Отдаем владение себе, если мы не владелец (на всякий случай)
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        // 2. Вызываем RPC, чтобы все клиенты "высадили" игрока
        photonView.RPC("RPC_RemoveDriver", RpcTarget.All, driverID);

        // 3. Возвращаем владение игроку (если это имеет смысл в вашей игре)
        // photonView.TransferOwnership(driverViewID); // Можно добавить эту строку
    }


    // =================================================================
    // RPC-МЕТОДЫ (Синхронизация по сети)
    // =================================================================

    // Синхронизирует посадку игрока
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

    // Синхронизирует высадку игрока
    [PunRPC]
    private void RPC_RemoveDriver(int driverViewID) // <--- Принимаем ViewID
    {
        // Находим игрока по ViewID
        PhotonView driverView = PhotonView.Find(driverViewID);

        if (driverView != null)
        {
            // Только если у объекта есть CharacterController, мы можем его высадить
            CharacterController playerCC = driverView.GetComponent<CharacterController>();

            if (playerCC != null) // <--- ЭТА ПРОВЕРКА УБИРАЕТ MissingComponentException
            {
                // 1. Высаживаем игрока в безопасное место
                Vector3 exitPosition = transform.position - transform.forward * 2f;
                driverView.transform.SetParent(null);
                driverView.transform.position = exitPosition;

                // 2. Только локальный игрок включает свое управление обратно
                if (driverView.IsMine)
                {
                    PlayerMovement playerMovement = driverView.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.enabled = true;
                    }
                    playerCC.enabled = true; // Включаем CC, который мы отключили при входе
                }
            }
            else
            {
                Debug.LogError($"[CAR] На объекте с ViewID {driverViewID} нет CharacterController. Возможно, это NPC или ошибка.");
            }
        }

        // 3. Сбрасываем ID водителя на всех клиентах
        this.driverID = 0;
    }
}
