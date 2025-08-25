// PlayerMovement.cs
using UnityEngine;
using Photon.Pun; // Для работы с Photon

/// <summary>
/// Управляет движением игрока от третьего лица и нанесением урона при столкновении.
/// Требует компоненты CharacterController и PhotonView.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Настройки движения")]
    [Tooltip("Скорость перемещения игрока.")]
    public float moveSpeed = 5f;
    [Tooltip("Скорость вращения игрока вокруг своей оси.")]
    public float rotationSpeed = 10f;
    [Tooltip("Сила гравитации, действующая на игрока.")]
    public float gravity = -9.81f;
    [Tooltip("Высота прыжка игрока.")]
    public float jumpHeight = 2f;

    [Header("Настройки урона при столкновении")]
    [Tooltip("Урон, наносимый другому игроку при столкновении.")]
    public int collisionDamage = 10;
    [Tooltip("Задержка перед повторным нанесением урона тому же игроку при столкновении.")]
    public float collisionDamageCooldown = 0.5f;

    private CharacterController controller;
    private PhotonView photonView;
    private Vector3 playerVelocity; // Вектор скорости игрока
    private bool isGrounded; // Находится ли игрок на земле
    float horizontalInput = 0;
    float verticalInput = 0;

    public Animator animator;

    // Таймер для отслеживания кулдауна урона при столкновении с конкретным игроком
    private float lastDamageTime = 0f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();

        // Если это не наш локальный игрок, отключаем скрипт, чтобы не управлять чужими персонажами.
        if (!photonView.IsMine)
        {
            enabled = false;
        }
    }

    void Update()
    {
        // Только если это наш локальный игрок, обрабатываем ввод.
        if (photonView.IsMine)
        {
            HandleMovement();
            HandleJump();
            if (verticalInput != 0 || horizontalInput != 0)
                animator.SetBool("Walk", true);
            else
                animator.SetBool("Walk", false);
        }
    }

    private void HandleMovement()
    {
        // Проверяем, находится ли игрок на земле
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Немного прижимаем к земле, чтобы избежать проблем с isGrounded
        }

        // Получаем ввод с клавиатуры (Horizontal - A/D или Left/Right Arrow, Vertical - W/S или Up/Down Arrow)
        horizontalInput = SimpleInput.GetAxis("Horizontal");
        verticalInput = SimpleInput.GetAxis("Vertical");

        // Создаем вектор движения относительно направления камеры.
        // Берем направление камеры, обнуляем Y, нормализуем, чтобы двигаться по плоскости.
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // Применяем скорость
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Вращаем игрока в направлении движения
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Применяем гравитацию
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded) // "Jump" по умолчанию Spacebar
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    /// <summary>
    /// Вызывается при столкновении CharacterController с другим коллайдером.
    /// Используется для нанесения урона другим игрокам.
    /// </summary>
    /// <param name="hit">Информация о столкновении.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Только если это наш локальный игрок и мы хотим нанести урон
        if (!photonView.IsMine) return;

        // Проверяем, что столкнулись с другим игроком
        PlayerHealth otherPlayerHealth = hit.collider.GetComponent<PlayerHealth>();
        if (otherPlayerHealth != null)
        {
            // Получаем PhotonView другого игрока
            PhotonView otherPhotonView = hit.collider.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine == false) // Убеждаемся, что это не наш собственный объект и это сетевой объект
            {
                // Проверяем кулдаун, чтобы не наносить урон постоянно
                if (Time.time >= lastDamageTime + collisionDamageCooldown)
                {
                    Debug.Log($"Наносим урон игроку {otherPhotonView.Owner.NickName}!");
                    // Вызываем TakeDamage на другом игроке.
                    // Метод TakeDamage в PlayerHealth сам запустит RPC.
                    otherPlayerHealth.TakeDamage(collisionDamage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}

