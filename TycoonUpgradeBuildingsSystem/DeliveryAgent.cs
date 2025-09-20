using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent
using System; // Required for Action delegates

public class DeliveryAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator; // Optional: For controlling agent's animations

    // Events to notify other scripts about delivery status
    public event Action<string, int> OnDeliveryCompleted; // resourceType, amount
    public event Action OnReturnCompleted;

    private DeliveryPoint currentTargetPoint; // The point agent is currently heading to
    private int deliveredAmount;
    private string deliveredResourceType;
    private Transform returnPointTransform; // Reference to the actual return point GameObject's transform

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Get Animator if exists

        // Ensure Rigidbody is kinematic if you don't want physics simulation,
        // but it's required for OnTriggerEnter with other non-kinematic colliders.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Agent is controlled by NavMeshAgent, not physics forces
            rb.useGravity = false; // NavMeshAgent handles vertical positioning
        }
        else
        {
            Debug.LogError("DeliveryAgent: Rigidbody component is missing! OnTriggerEnter will not work correctly without it.", this);
        }

        // Ensure agent's collider is NOT a trigger itself, unless you have specific needs.
        // It needs to be a regular collider to detect trigger collisions.
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning($"DeliveryAgent on {gameObject.name}: Agent's collider is set as trigger. It should usually be a regular collider for OnTriggerEnter to fire on other triggers.", this);
        }
    }

    /// <summary>
    /// Initializes the delivery agent for a new delivery task.
    /// </summary>
    /// <param name="start">The starting position of the delivery.</param>
    /// <param name="deliveryPoint">The DeliveryPoint GameObject for delivery.</param>
    /// <param name="returnPoint">The DeliveryPoint GameObject for return.</param>
    /// <param name="amount">The amount of resources to deliver.</param>
    /// <param name="resourceType">The type of resource being delivered.</param>
    public void SetupDelivery(Vector3 start, DeliveryPoint deliveryPoint, DeliveryPoint returnPoint) //int amount)
    {
        transform.position = start; // Place agent at the start point
       // deliveredAmount = amount;
        //deliveredResourceType = resourceType;

        // Store references to the actual DeliveryPoint objects
        // We'll use these to check which trigger was entered
        currentTargetPoint = deliveryPoint; // Start by heading to the delivery point
        returnPointTransform = returnPoint.transform; // We only need transform for return destination

        gameObject.SetActive(true); // Activate agent if it was pooled
        agent.SetDestination(deliveryPoint.transform.position); // Set initial destination
        SetAnimationState(true); // Start moving animation
      //  Debug.Log($"Delivery Agent: Starting delivery of {amount} {resourceType} to {deliveryPoint.name}.");
    }

    /// <summary>
    /// Called when the agent's collider enters a trigger collider.
    /// </summary>
    /// <param name="other">The other Collider involved in the trigger collision.</param>
    void OnTriggerEnter(Collider other)
    {
        // Try to get the DeliveryPoint component from the other collider
        DeliveryPoint enteredPoint = other.GetComponent<DeliveryPoint>();

        if (enteredPoint != null)
        {
            if (enteredPoint.type == DeliveryPoint.PointType.Delivery && currentTargetPoint == enteredPoint)
            {
                // Agent reached the delivery point
                Debug.Log($"Delivery Agent: Entered Delivery Point: {enteredPoint.name}");
               // OnDeliveryCompleted?.Invoke(deliveredResourceType, deliveredAmount); // Invoke the delivery event

                // Now, set destination to the return point
                currentTargetPoint = null; // Clear current target as we're now heading to return
                agent.SetDestination(returnPointTransform.position);
                Debug.Log($"Delivery Agent: Delivered. Now heading back to {returnPointTransform.name}.");
            }
            else if (enteredPoint.type == DeliveryPoint.PointType.Return)// && currentTargetPoint == enteredPoint)//agent.destination == returnPointTransform.position)
            {
                // Agent reached the return point
                Debug.Log($"Delivery Agent: Entered Return Point: {enteredPoint.name}");
             //   OnReturnCompleted?.Invoke(); // Invoke the return event

                SetAnimationState(false); // Stop moving animation
                Debug.Log("Delivery Agent: Returned to base. Task complete.");
                // For pooling, deactivate the agent
                // gameObject.SetActive(false);
                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// Sets the animation state for the agent.
    /// Assumes an Animator with a boolean parameter "IsMoving".
    /// </summary>
    /// <param name="isMoving">True to set moving animation, false to stop.</param>
    private void SetAnimationState(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    // Optional: Visualize path in editor
    void OnDrawGizmos()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow;
            Vector3 lastCorner = transform.position;
            foreach (var corner in agent.path.corners)
            {
                Gizmos.DrawLine(lastCorner, corner);
                lastCorner = corner;
            }
        }
    }
}