using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class TruckEscort : MonoBehaviourPunCallbacks//, IPunObservable
{
    // ����������
    private NavMeshAgent navMeshAgent;
    private PhotonView photonView;
   // public Transform[] enemyRespawn;
   // public SkinnedMeshRenderer enemyMesh;
    Rigidbody rigidbody;

    // ��������
    [SerializeField]
    private float health = 100f;
    [SerializeField]
    private float maxHealth = 100f;

    // ���������
    public Transform[] targetPlayer;
    [SerializeField]
    private float updateTargetInterval = 3f;
  //  public Animator animator;

    private float timeForFind = 0;
    public bool diedBot = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
       // targetPlayer = null;
    }

    private void Start()
    {
        if(photonView.IsMine)
        {
            int randomPoint = Random.Range(0, targetPlayer.Length);
            photonView.RPC("RPC_SerNewDestination", RpcTarget.All, targetPlayer[randomPoint].position);
        }
            
    }

    

   
    [PunRPC]
    private void RPC_SerNewDestination(Vector3 destination)
    {
        // ��� ������ ������ ������������� �������� ����� ��� ������ ���������� NavMeshAgent
        navMeshAgent.SetDestination(destination);
    }



    [PunRPC]
    public void TruckDamage(int damage)
    {
        if (diedBot == false && health > 0)
        {
            // navMeshAgent.speed = -30;
            health -= damage;
            Debug.Log($"���� ������� {damage} �����. ��������: {health}");
            //  StartCoroutine(FakeAddForceMotion());
            // rigidbody.isKinematic = false;
            // rigidbody.AddForce(Vector3.right * 100);
           // animator.SetTrigger("Damage");
            //  rigidbody.isKinematic = true;
            if (navMeshAgent.speed > 0.1f)
                navMeshAgent.speed -= 0.01f;
        }

        CheckForGameOver();

        /*  if (health <= 0 && diedBot == false)
          {

             // animator.SetTrigger("Die");
             // Die();
              diedBot = true;
          } */
    }

    void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            if(diedBot == false)
                photonView.RPC("TruckDamage", RpcTarget.All, 2);
        }
    } 

    void Update()
    {
        if (health <= 0)
            diedBot = true;

        
    }

    private void CheckForGameOver()
    {
        if (health <= 0 && !diedBot)
        {
            diedBot = true;

            // 1. ����������, ��� ������ Master Client ��������� � ����� ����
            if (PhotonNetwork.IsMasterClient)
            {
                // 2. �������� RPC �� GameManager
                GameManager.Instance.photonView.RPC("RPC_GameOver", RpcTarget.All, "EscortFailed");
            }
        }
    }

    /* float forceAmount = 105f;

     IEnumerator FakeAddForceMotion()
     {
         float i = 0.01f;
         while (forceAmount > i)
         {
             rigidbody.velocity = new Vector3(forceAmount / i, rigidbody.velocity.y, forceAmount/i); // !! For X axis positive force
             i = i + Time.deltaTime;
             yield return new WaitForEndOfFrame();
         }
         rigidbody.velocity = Vector3.zero;
         yield return null;
     }

     // Call this when you want to apply force
     void AddForce()
     {
         StartCoroutine(FakeAddForceMotion());
     } 

    private void Die()
    {
        Debug.Log("���� ���������.");
        if (photonView.IsMine)
        {
            navMeshAgent.speed = 0f;
            //   animator.SetTrigger("Die");
            StartCoroutine(Respawn());
            //  PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RPC_SetAlive()
    {
        animator.SetTrigger("Alive");
        enemyMesh.enabled = true;
    }

    [PunRPC]
    public void RPC_SetAttack()
    {
        animator.SetTrigger("Attack");
    }

    [PunRPC]
    public void RPC_SetMesh()
    {
        enemyMesh.enabled = false;
    }

    private IEnumerator Respawn()
    {
        //   animator.SetTrigger("Die");
        yield return new WaitForSeconds(3f);
        photonView.RPC("RPC_SetMesh", RpcTarget.All);
        int randomSpawn = Random.Range(0, enemyRespawn.Length);
        transform.position = enemyRespawn[randomSpawn].position;
        yield return new WaitForSeconds(3f);
        photonView.RPC("RPC_SetAlive", RpcTarget.All);
        health = 100;
        navMeshAgent.speed = 2.5f;
        diedBot = false;
        navMeshAgent.speed = 3f;
    }



    /*  public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
          if (stream.IsWriting)
          {
              // ���������� ������� � ��������
              stream.SendNext(transform.position);
              stream.SendNext(transform.rotation);
              stream.SendNext(health);
          }
          else
          {
              // �������� ������ �� ������� � ��������� ��
              Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
              Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
              float receivedHealth = (float)stream.ReceiveNext();

              // �������������, ����� �������� ���� �������
              transform.position = Vector3.Lerp(transform.position, receivedPosition, Time.deltaTime * 5f);
              transform.rotation = Quaternion.Lerp(transform.rotation, receivedRotation, Time.deltaTime * 5f);
              health = receivedHealth;
          }
      } */
}
