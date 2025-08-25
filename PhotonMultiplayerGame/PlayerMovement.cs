// PlayerMovement.cs
using UnityEngine;
using Photon.Pun; // ��� ������ � Photon

/// <summary>
/// ��������� ��������� ������ �� �������� ���� � ���������� ����� ��� ������������.
/// ������� ���������� CharacterController � PhotonView.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("��������� ��������")]
    [Tooltip("�������� ����������� ������.")]
    public float moveSpeed = 5f;
    [Tooltip("�������� �������� ������ ������ ����� ���.")]
    public float rotationSpeed = 10f;
    [Tooltip("���� ����������, ����������� �� ������.")]
    public float gravity = -9.81f;
    [Tooltip("������ ������ ������.")]
    public float jumpHeight = 2f;

    [Header("��������� ����� ��� ������������")]
    [Tooltip("����, ��������� ������� ������ ��� ������������.")]
    public int collisionDamage = 10;
    [Tooltip("�������� ����� ��������� ���������� ����� ���� �� ������ ��� ������������.")]
    public float collisionDamageCooldown = 0.5f;

    private CharacterController controller;
    private PhotonView photonView;
    private Vector3 playerVelocity; // ������ �������� ������
    private bool isGrounded; // ��������� �� ����� �� �����
    float horizontalInput = 0;
    float verticalInput = 0;

    public Animator animator;

    // ������ ��� ������������ �������� ����� ��� ������������ � ���������� �������
    private float lastDamageTime = 0f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();

        // ���� ��� �� ��� ��������� �����, ��������� ������, ����� �� ��������� ������ �����������.
        if (!photonView.IsMine)
        {
            enabled = false;
        }
    }

    void Update()
    {
        // ������ ���� ��� ��� ��������� �����, ������������ ����.
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
        // ���������, ��������� �� ����� �� �����
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // ������� ��������� � �����, ����� �������� ������� � isGrounded
        }

        // �������� ���� � ���������� (Horizontal - A/D ��� Left/Right Arrow, Vertical - W/S ��� Up/Down Arrow)
        horizontalInput = SimpleInput.GetAxis("Horizontal");
        verticalInput = SimpleInput.GetAxis("Vertical");

        // ������� ������ �������� ������������ ����������� ������.
        // ����� ����������� ������, �������� Y, �����������, ����� ��������� �� ���������.
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // ��������� ��������
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // ������� ������ � ����������� ��������
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // ��������� ����������
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded) // "Jump" �� ��������� Spacebar
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    /// <summary>
    /// ���������� ��� ������������ CharacterController � ������ �����������.
    /// ������������ ��� ��������� ����� ������ �������.
    /// </summary>
    /// <param name="hit">���������� � ������������.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // ������ ���� ��� ��� ��������� ����� � �� ����� ������� ����
        if (!photonView.IsMine) return;

        // ���������, ��� ����������� � ������ �������
        PlayerHealth otherPlayerHealth = hit.collider.GetComponent<PlayerHealth>();
        if (otherPlayerHealth != null)
        {
            // �������� PhotonView ������� ������
            PhotonView otherPhotonView = hit.collider.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine == false) // ����������, ��� ��� �� ��� ����������� ������ � ��� ������� ������
            {
                // ��������� �������, ����� �� �������� ���� ���������
                if (Time.time >= lastDamageTime + collisionDamageCooldown)
                {
                    Debug.Log($"������� ���� ������ {otherPhotonView.Owner.NickName}!");
                    // �������� TakeDamage �� ������ ������.
                    // ����� TakeDamage � PlayerHealth ��� �������� RPC.
                    otherPlayerHealth.TakeDamage(collisionDamage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}

