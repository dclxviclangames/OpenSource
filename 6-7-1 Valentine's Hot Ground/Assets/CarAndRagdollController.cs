using UnityEngine;

public class CarAndRagdollController : MonoBehaviour
{
    [Header("Настройки движения машины")]
    public float speed = 20f;
    private bool isCrashed = false;

    [Header("Настройки вылета (Ragdoll)")]
    public GameObject ragdollRoot; // Главный объект рагдолла (Hips)
    public Rigidbody[] ragdollParts;
    public float minForce = 10f;
    public float maxForce = 35f;
    public float explosionRadius = 7f;

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();

        // В начале рагдолл «спит» и привязан к машине
        SetRagdollKinematic(true);
    }

    void FixedUpdate()
    {
        // Пока не врезались — едем вперед
        if (!isCrashed)
        {
            Vector3 moveVelocity = -transform.forward * speed;
            moveVelocity.y = carRb.velocity.y; // Сохраняем гравитацию
            carRb.velocity = moveVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Если врезались во что-то (кроме земли) и еще не в аварии
        if (!isCrashed && collision.gameObject.CompareTag("Player"))
        {
            HandleCrash();
        }
    }

    private void HandleCrash()
    {
        Vector3 randomDirection = Random.insideUnitSphere;

        // Прикладываем случайную силу (используем ForceMode.VelocityChange для резкости)
        carRb.AddForce(randomDirection * 55f, ForceMode.VelocityChange);
        isCrashed = true;

        // 1. Отсоединяем рагдолл от машины
        ragdollRoot.transform.SetParent(null);

        // 2. Включаем физику всем частям тела
        SetRagdollKinematic(false);

        // 3. Создаем взрывную волну
        ExplodeNearby();
    }

    private void SetRagdollKinematic(bool state)
    {
        foreach (var rb in ragdollParts)
        {
            if (rb != null) rb.isKinematic = state;
            Vector3 randomDirection = Random.insideUnitSphere;

            // Прикладываем случайную силу (используем ForceMode.VelocityChange для резкости)
            rb.AddForce(randomDirection * 15f, ForceMode.VelocityChange);
        }
    }

    private void ExplodeNearby()
    {
        // Находим всё живое в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;

                float randomForce = Random.Range(minForce, maxForce);

                // Сила взрыва: расталкивает объекты от центра машины
                rb.AddExplosionForce(randomForce, transform.position, explosionRadius, 2f, ForceMode.Impulse);
            }
        }
        Destroy(this.gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

