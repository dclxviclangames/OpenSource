// PlayerMovement.cs
using UnityEngine;
using Photon.Pun; // ��� ������ � Photon
//using UnityEngine.UI;
//using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

/// <summary>
/// ��������� ��������� ������ �� �������� ���� � ���������� ����� ��� ������������.
/// ������� ���������� CharacterController � PhotonView.
/// </summary>

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("��������� ��������")]
    [Tooltip("�������� ����������� ������.")]
    public float moveSpeed = 5f;
    public float boostSpeed = 55f;
    [Tooltip("�������� �������� ������ ������ ����� ���.")]
    public float rotationSpeed = 10f;
    [Tooltip("���� ����������, ����������� �� ������.")]
    public float gravity = -9.81f;
    [Tooltip("������ ������ ������.")]
    public float jumpHeight = 2f;
    public float boostJump = 55f;

    [Header("��������� ����� ��� ������������")]
    [Tooltip("����, ��������� ������� ������ ��� ������������.")]
    public int collisionDamage = 10;
    [Tooltip("�������� ����� ��������� ���������� ����� ���� �� ������ ��� ������������.")]
    public float collisionDamageCooldown = 0.5f;

    public CharacterController controller;
    private PhotonView photonView;
    private Vector3 playerVelocity; // ������ �������� ������
    private bool isGrounded; // ��������� �� ����� �� �����
    float horizontalInput = 0;
    float verticalInput = 0;

    public Animator animator;
    public ParticleSystem blood;
    // ������ ��� ������������ �������� ����� ��� ������������ � ���������� �������
    private float lastDamageTime = 0f;

    private FixedButton attackButton;
    private FixedButton jumpButton;

    public BoxCollider attackWeapon;
    public PlayerHealth playerHealth;

    public GameObject particleDust;

    float timeDam = 0;
    bool DamGet = false;
    //  public Slider healthSlider;

    void Awake()
    {
       /* if (SceneManager.GetActiveScene().buildIndex == 4)
            this.enabled = false; */

        photonView = GetComponent<PhotonView>();
        
        // ���� ��� �� ��� ��������� �����, ��������� ������, ����� �� ��������� ������ �����������.
        if (!photonView.IsMine)
        {
            enabled = false;
            
        }
        else
        {
            controller = GetComponent<CharacterController>();
            controller.enabled = true;
            attackButton = GameObject.FindWithTag("Fight").GetComponent<FixedButton>();
            jumpButton = GameObject.FindWithTag("Jump").GetComponent<FixedButton>();
        }
       
        
    }

  /*  void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 4)
            this.enabled = false;
    } */

    

    void Update()
    {
        // ������ ���� ��� ��� ��������� �����, ������������ ����.
        if (photonView.IsMine)
        {
            if (timeDam > 1.5f && DamGet == true)
            {
                photonView.RPC("TakeDamage", RpcTarget.All, 10);
                DamGet = false;
                timeDam = 0;
            }
            else
            {
                timeDam += Time.deltaTime;
            }

            if (attackButton.Pressed)
            {
                animator.SetTrigger("Attack");
                attackWeapon.enabled = true;
            }
            else
            {
                attackWeapon.enabled = false;
            }
                

            if (jumpButton.Pressed)
                mobileJump();

            if(controller != null)
                HandleMovement();
            HandleJump();
            // healthSlider.value = playerHealth.currentHealth;
            

            if (verticalInput != 0 || horizontalInput != 0)
            {
                animator.SetBool("Walk", true);
                if(transform.position.y < 25)
                    particleDust.SetActive(true);
                else
                    particleDust.SetActive(false);
            } 
            else
            {
                animator.SetBool("Walk", false);
                particleDust.SetActive(false);
            }
                
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
            AddScore(5);
        }
    }

    public void mobileJump()
    {
        if (isGrounded) // "Jump" �� ��������� Spacebar
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void JumpBoost()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward + right).normalized;

        // ��������� ��������
        controller.Move(-moveDirection * moveSpeed * Time.deltaTime);
        playerVelocity.y = Mathf.Sqrt(boostJump * -2f * gravity);


    }

    [PunRPC]
    public void AddScore(int amountToAdd)
    {
        // �������� ������� ��������
        Hashtable currentProps = PhotonNetwork.LocalPlayer.CustomProperties;

        int currentScore = 0;
        // ���������, ���������� �� �������� � �������� ��� ��������
        if (currentProps.ContainsKey(GameManager.PlayerScoreKey))
        {
            currentScore = (int)currentProps[GameManager.PlayerScoreKey];
        }

        // ������ ����� ���-������� � ���������� ���������
        Hashtable newProps = new Hashtable();
        newProps[GameManager.PlayerScoreKey] = currentScore + amountToAdd;

        // ���������� �� ������ ��� �������������
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);
        Debug.Log($"���� ������ {PhotonNetwork.LocalPlayer.NickName} ������� �� {currentScore + amountToAdd}");
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

        string hitTag = hit.gameObject.tag;

        if (hitTag == "Enemy" && photonView.IsMine)
        {
            DamGet = true;
            blood.Play();
        }
           

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
                  //  otherPlayerHealth.TakeDamage(collisionDamage);
                    blood.Play();
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}

