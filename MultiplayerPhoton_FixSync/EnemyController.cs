using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviourPunCallbacks//, IPunObservable
{
    // Компоненты
    private NavMeshAgent navMeshAgent;
    private PhotonView photonView;
    public Transform[] enemyRespawn;
    public SkinnedMeshRenderer enemyMesh;
    Rigidbody rigidbody;

    // Здоровье
    [SerializeField]
    private float health = 100f;
    [SerializeField]
    private float maxHealth = 100f;

    // Поведение
    private Transform targetPlayer;
    public Transform[] shotPos;
    public bool shotNPC = false;
    [SerializeField]
    private float updateTargetInterval = 3f;
    public Animator animator;

    private float timeForFind = 0;
    public bool diedBot = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();
        rigidbody = GetComponent<Rigidbody>();
        targetPlayer = null;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(FindNewTargetRoutine());
        }
    }

    private IEnumerator FindNewTargetRoutine()
    {
        while (true)
        {
            if (SceneManager.GetActiveScene().buildIndex != 4)
                FindClosestPlayer();
            else
                FindClosesPlayer();
            yield return new WaitForSeconds(updateTargetInterval);
        }
    }

    private void FindClosestPlayer()
    {
       // if (SceneManager.GetActiveScene().buildIndex != 4)

            // Находим все объекты с тегом "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform newTarget = null;

        foreach (GameObject playerObject in players)
        {
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                newTarget = playerObject.transform;
            }
            // Убеждаемся, что это игрок с PlayerMovement и он локальный
            // PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
            // if (playerMovement != null && playerObject.GetComponent<PhotonView>().IsMine)




        }

        if (newTarget != null)
        {
            targetPlayer = newTarget;
            photonView.RPC("RPC_SetNewDestination", RpcTarget.All, targetPlayer.position);
            //  navMeshAgent.SetDestination(targetPlayer.position);
        }
        
    }

    private void FindClosesPlayer()
    {
        // if (SceneManager.GetActiveScene().buildIndex != 4)

        // Находим все объекты с тегом "Player"
        if(shotNPC == false)
        {
            GameObject players = GameObject.FindGameObjectWithTag("Truck");

            targetPlayer = players.transform;
            if (players != null)
                photonView.RPC("RPC_SetNewDestination", RpcTarget.All, targetPlayer.position);
        }
        else
        {
            int randomPoint = Random.Range(0, shotPos.Length);
            targetPlayer = shotPos[randomPoint];
            photonView.RPC("RPC_SetNewDestination", RpcTarget.All, targetPlayer.position);
        }
       

        

    }

    private void Update()
    {
        
      /*  if (PhotonNetwork.IsMasterClient && timeForFind > 5f)
        {
            if (photonView.IsMine && targetPlayer != null && health > 0)
            {
               // navMeshAgent.SetDestination(targetPlayer.position);
               // enemyMesh.enabled = true;
            }

            

            
            timeForFind = 0;
        }
        else
        {
            timeForFind += Time.deltaTime;
        } */

        if(targetPlayer != null)
        {
            if (targetPlayer.gameObject.activeInHierarchy == false)
            {
                targetPlayer = null;
                return;
            }
        }

        if(targetPlayer != null && health > 0)
        {
            

            if (Vector3.Distance(transform.position, targetPlayer.position) < 3.5f)
                photonView.RPC("RPC_SetAttack", RpcTarget.All);
        } 

        
        
    }

    [PunRPC]
    private void RPC_SetNewDestination(Vector3 destination)
    {
        // Все игроки просто устанавливают конечную точку для своего локального NavMeshAgent
        navMeshAgent.SetDestination(destination);
    }

    [PunRPC]
    public void TakeDamages(int damage)
    {
        if(diedBot == false && health > 0)
        {
           // navMeshAgent.speed = -30;
            health -= damage;
            Debug.Log($"Враг получил {damage} урона. Здоровье: {health}");
          //  StartCoroutine(FakeAddForceMotion());
           // rigidbody.isKinematic = false;
           // rigidbody.AddForce(Vector3.right * 100);
            animator.SetTrigger("Damage");
            //  rigidbody.isKinematic = true;
            if (navMeshAgent.speed > 0.1f)
                navMeshAgent.speed -= 0.3f;
        }
        
        if (health <= 0 && diedBot == false)
        {
            animator.SetTrigger("Die");
            Die();
            diedBot = true;
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
    } */

    private void Die()
    {
        Debug.Log("Враг уничтожен.");
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
              // Отправляем позицию и здоровье
              stream.SendNext(transform.position);
              stream.SendNext(transform.rotation);
              stream.SendNext(health);
          }
          else
          {
              // Получаем данные от мастера и применяем их
              Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
              Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
              float receivedHealth = (float)stream.ReceiveNext();

              // Интерполируем, чтобы движение было плавным
              transform.position = Vector3.Lerp(transform.position, receivedPosition, Time.deltaTime * 5f);
              transform.rotation = Quaternion.Lerp(transform.rotation, receivedRotation, Time.deltaTime * 5f);
              health = receivedHealth;
          }
      } */
}