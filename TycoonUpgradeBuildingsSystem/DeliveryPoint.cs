using UnityEngine;

// This script identifies a GameObject as a delivery or return point.
// It requires a Collider component set as a trigger.
[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public enum PointType { Delivery, Return }
    [Tooltip("Type of this point: Delivery (where resources are dropped off) or Return (where agent goes back to).")]
    public PointType type;

    void Awake()
    {
        // Ensure the collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"DeliveryPoint on {gameObject.name}: Collider was not set as trigger. Setting it now.", this);
        }
    }

    // Optional: Visualize the point type in editor
    void OnDrawGizmos()
    {
        if (type == PointType.Delivery)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireSphere(transform.position, 0.75f);
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1f, type.ToString() + ".png", true); // Requires icons in project
    }
}
