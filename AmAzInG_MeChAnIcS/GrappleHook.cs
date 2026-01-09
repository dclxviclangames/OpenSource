using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public string targetTag = "GrapplePoint"; // Тег объектов, за которые можно зацепиться
    public float maxDistance = 50f;          // Максимальная дистанция зацепа
    public LayerMask grappleLayer;           // Слой для проверки видимости (опционально)

    private SpringJoint joint;
    private LineRenderer lineRenderer;
    private Transform currentTarget;
    public Transform handPoint;

    public GameObject paint;

    void Awake()
    {
        // Подготовка LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            paint.SetActive(true);
            StartGrapple();
        }
       
        if (Input.GetMouseButtonUp(0))
        {
            paint.SetActive(false);
            StopGrapple();
        }
        
    }

    void LateUpdate()
    {
        // Обновляем линию каждый кадр, если зацепились
        if (currentTarget != null)
        {
            lineRenderer.SetPosition(0, handPoint.position);
            lineRenderer.SetPosition(1, currentTarget.position);
        }
    }

    void StartGrapple()
    {
        // 1. Поиск всех объектов с тегом
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float minDist = maxDistance;
        Vector3 currentPos = transform.position;

        // 2. Алгоритм поиска самого ближнего
        foreach (GameObject t in targets)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                closest = t;
                minDist = dist;
            }
        }

        // 3. Создание джоинта, если цель найдена
        if (closest != null)
        {
            currentTarget = closest.transform;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = closest.GetComponent<Rigidbody>();

            // Если у цели нет Rigidbody, цепляемся к точке в пространстве
            if (joint.connectedBody == null)
                joint.connectedAnchor = currentTarget.position;

            // Настройки упругости как у паутины
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lineRenderer.positionCount = 2;
        }
    }

    void StopGrapple()
    {
        Destroy(joint);
        currentTarget = null;
        lineRenderer.positionCount = 0;
    }
}

