using UnityEngine;

public class ToyGrabber : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "Dude"; // Тег для игрушек
    public float grabRange = 20f;
    public float pullSpeed = 10f;
    public float stopDistance = 1.5f;

    [Header("References")]
    public LineRenderer beam; // Сюда перетащи LineRenderer
    public Transform grabPoint; // Точка, откуда вылетает луч

    private GameObject targetToy;
    private bool isPulling = false;
    private YTPlayableController yTPlayableController;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        if (beam) beam.enabled = false;
        yTPlayableController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<YTPlayableController>();
    }

    void Update()
    {
        // Нажми Пробел или кнопку мыши, чтобы начать захват
        transform.Translate(-Vector3.forward * 8.5f * Time.deltaTime, Space.Self);
        FindTarget();
        if(targetToy != null)
        {
            UpdateBeam();
            spriteRenderer.flipX = targetToy.transform.position.x < transform.position.x;
        }
       
        
        if (isPulling && targetToy != null)
        {
            
            PullToy();
        }
    }

    void FindTarget()
    {
        // Ищем все объекты с нужным тегом
        GameObject[] toys = GameObject.FindGameObjectsWithTag(targetTag);
        float closestDist = grabRange;
        GameObject closestToy = null;

        foreach (var toy in toys)
        {
            float dist = Vector3.Distance(grabPoint.position, toy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestToy = toy;
            }
        }

        if (closestToy != null)
        {
            targetToy = closestToy;
            isPulling = true;
            beam.enabled = true;

            // Если на игрушке есть Rigidbody, отключаем гравитацию для плавности
            if (targetToy.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.useGravity = false;
                rb.isKinematic = false; // Позволяем двигать кодом
            }
        }
    }

    void PullToy()
    {
        float distance = Vector3.Distance(targetToy.transform.position, grabPoint.position);

        if (distance > stopDistance)
        {
            // Плавно перемещаем игрушку к точке захвата
            targetToy.transform.position = Vector3.MoveTowards(
                targetToy.transform.position,
                grabPoint.position,
                pullSpeed * Time.deltaTime
            );
        }
        else
        {
            // Притянули!
            StopGrabbing();
        }
    }

    void UpdateBeam()
    {
        beam.SetPosition(0, grabPoint.position);
        beam.SetPosition(1, targetToy.transform.position);
    }

    void StopGrabbing()
    {
        isPulling = false;
        beam.enabled = false;

        if (targetToy != null && targetToy.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.useGravity = true;
        }
        targetToy = null;
    }

    public void KupiDie()
    {
        yTPlayableController._currentScore += 150;
        Destroy(this.gameObject);
    }
}

