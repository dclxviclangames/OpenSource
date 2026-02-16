using UnityEngine;

public class ClickToSeek : MonoBehaviour
{
    public string targetTag = "Match";
    public float speed = 10f;        // Желаемая скорость полета
    public float jitterForce = 1.5f;

    private Rigidbody rb;
    private bool isSeeking = false;
    private Transform targetTransform;
    public Rigidbody[] rigidbodies;

    private WaveSpawn waveSpawn;
    private bool firstTimesCon = false;
    private bool firstDam = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Invoke("CloseBal", 3f);
        waveSpawn = GameObject.FindGameObjectWithTag("Wave").GetComponent<WaveSpawn>();
    }

    void OnMouseDown()
    {
        rb.AddForce(Vector3.up * 90f, ForceMode.Impulse);
        Invoke("Moving", 0.5f);
    }

    public void Moving()
    {
        GameObject closest = FindClosestTarget();

        if (closest != null)
        {
            targetTransform = closest.transform;
            isSeeking = true;

            // 1. Сбрасываем текущую скорость, чтобы старое движение не мешало рывку
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 2. Рассчитываем направление
            Vector3 direction = (targetTransform.position - transform.position).normalized;

            // 3. Прикладываем силу как изменение скорости (VelocityChange)
            // Это сделает полет предсказуемым независимо от массы объекта
            rb.AddForce(direction * speed, ForceMode.VelocityChange);

            Debug.Log($"Объект {name} летит к {closest.name}");
        }
    }

    void FixedUpdate()
    {
        // Если мы в полете, можно слегка "подруливать" к цели, если она движется
        if (isSeeking && targetTransform != null)
        {
            float distance = Vector3.Distance(transform.position, targetTransform.position);

            // Если почти долетели — выключаем режим поиска
            if (distance < 0.5f)
            {
                isSeeking = false;
                // Можно добавить небольшое замедление
                rb.velocity *= 0.9f;
            }
        }

        if(transform.position.y < -15f)
        {
            waveSpawn.TakeDamage(5f);
         //   firstTimesCon = false;
            Destroy(this.gameObject);
        }
    }

    // --- Ваш метод поиска цели (без изменений) ---
    GameObject FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject target in targets)
        {
            if (target == gameObject) continue;
            float diff = (target.transform.position - currentPos).sqrMagnitude;
            if (diff < distance)
            {
                closest = target;
                distance = diff;
            }
        }
        return closest;
    }

    public void CloseBal()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if(ball != null)
        {
            ball.SetActive(false);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Тряска при контакте
        if (collision.gameObject.CompareTag(targetTag) && !isSeeking)
        {
            isSeeking = false; // Прилетели — перестаем искать
            Vector3 randomDirection = Random.insideUnitSphere;
            rb.AddForce(randomDirection * jitterForce, ForceMode.VelocityChange);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && firstDam == true)
        {
            waveSpawn.TakeDamage(5f);
            //  firstTimesCon = false;
            firstDam = false;
            Destroy(this.gameObject, 3f);
        }

        if (collision.gameObject.CompareTag("Player") && firstTimesCon == false)
        {
          //  waveSpawn.Hil(10f);
            firstTimesCon = true;
          //  Destroy(this.gameObject, 3f);
        }
    }

  /*  private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Match"))
        {
            foreach(Rigidbody rbss in rigidbodies)
            {
                rbss.isKinematic = true;
                rbss.useGravity = false;
            }
        }

    } */
}


