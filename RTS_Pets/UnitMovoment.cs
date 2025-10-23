using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private SkinnedMeshRenderer unitRenderer; // ��� ������������ ���������
    private Color originalColor;

    public Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unitRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        originalColor = unitRenderer.material.color;
    }

    


    void Update()
    {
        if (agent.isStopped)
        {
            animator.SetBool("Walk", false);
        }
        else
        {
            animator.SetBool("Walk", true);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Agent is close to the destination, stop or perform other actions
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }

        // ���������� ����������, ����� ������ ������� �� ��������
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }



    // ���������� ����������, ����� ��������/������ ���������
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            unitRenderer.material.color = Color.green; // ���������� ���������
            Debug.Log($"���� {gameObject.name} ������.");
        }
        else
        {
            unitRenderer.material.color = originalColor; // ����� ���������
        }
    }
}

