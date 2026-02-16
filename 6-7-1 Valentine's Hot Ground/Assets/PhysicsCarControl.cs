using UnityEngine;

public class PhysicsCarControl : MonoBehaviour
{
    public float[] lanes = { -3.5f, 0f, 3.5f };
    public float sideForce = 50f;     // —ила рывка в бок
    public float dampening = 10f;    // Ќасколько сильно гасим инерцию

    private int currentLane = 1;
    private Rigidbody rb;
    public Rigidbody[] wheel;
    private Vector2 touchStart;
    private bool isSwiping = false;
    public Vector3 direction;
    public float boostForce;
    public GlobalLightChanger globalLightChanger;
    public WaveSpawn waveSpawner;
    public Transform basePos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleInput();
        Vector3 randomDirection = Random.insideUnitSphere;

        // ѕрикладываем случайную силу (используем ForceMode.VelocityChange дл€ резкости)
        rb.AddForce(randomDirection * 2, ForceMode.VelocityChange);
        if(transform.position.y > 6f)
        {
            transform.position = basePos.position;
        }
    }

    void FixedUpdate()
    {
        ApplyPhysicsMovement();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
            globalLightChanger.ChangeFOV(44f);
        }

        if (Input.GetMouseButton(0) && !isSwiping)
        {
            float diff = Input.mousePosition.x - touchStart.x;
            if (Mathf.Abs(diff) > 40)
            {
                if (diff > 0 && currentLane < lanes.Length - 1) currentLane++;
                else if (diff < 0 && currentLane > 0) currentLane--;
                isSwiping = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            globalLightChanger.ChangeFOV(58f);
            isSwiping = false;
        }
    }

    void ApplyPhysicsMovement()
    {
        float targetX = lanes[currentLane];
        float xDiff = targetX - transform.position.x;

        // 1. –ассчитываем желаемую скорость: чем дальше цель, тем быстрее летим
        // (xDiff * someMultiplier) Ч это и есть нужна€ нам скорость дл€ компенсации разницы
        float targetVelocityX = xDiff * 15f; // 15f - это резкость перестроени€

        // 2. ќграничиваем максимальную скорость перестроени€
        targetVelocityX = Mathf.Clamp(targetVelocityX, -sideForce, sideForce);

        // 3. ¬ычисл€ем разницу между текущей скоростью и желаемой
        float velocityDiff = targetVelocityX - rb.velocity.x;

        // 4. ѕрикладываем силу, котора€ мгновенно (VelocityChange) мен€ет скорость до нужной
        // Ёто уберет "плавание" и сделает переход на полосу четким
        rb.AddForce(Vector3.right * velocityDiff, ForceMode.VelocityChange);

        // ƒополнительно: если ты хочешь, чтобы текстура крутилась плавно
       // UpdateTextureScroll();
    }

    private void OnCollisionStay(Collision other)
    {
        Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // ѕрикладываем силу посто€нно, пока объект на полу
            targetRb.AddForce(direction * boostForce, ForceMode.Acceleration);
        }

        if (other.gameObject.CompareTag("Ground"))
        {
            float currentForce = 100 - waveSpawner.currentHealth;
            for (int i = 0; i < wheel.Length; i++)
            {
                wheel[i].AddForce(Vector3.up * currentForce, ForceMode.Impulse);
            }
            rb.AddForce(Vector3.up * currentForce/10, ForceMode.Impulse);
        }

    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            float currentForce = 100 - waveSpawner.currentHealth;
            for (int i = 0; i < wheel.Length; i++)
            {
                wheel[i].AddForce(Vector3.up * currentForce, ForceMode.Impulse);
            }
            rb.AddForce(Vector3.up * currentForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Heal"))
        {
            waveSpawner.Hil(10);
            waveSpawner.AddScore();
            Destroy(other.gameObject);
        }
    }
}

