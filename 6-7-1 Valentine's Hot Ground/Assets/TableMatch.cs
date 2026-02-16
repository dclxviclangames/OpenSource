using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableMatch : MonoBehaviour
{
    public float damage = 20;
    public bool isRight = false;
    public bool isLeft = false;
    public Transform leftT;
    public Transform righT;
    public Transform tableRot;

    
    private YTPlayableController yTPlayableController;
    // Start is called before the first frame update
    void Start() => yTPlayableController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<YTPlayableController>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Dude"))
        {
            yTPlayableController._currentScore += 150;
            yTPlayableController.SaveGame();
            
            Rigidbody targetRb = other.GetComponent<Rigidbody>();

            if (targetRb != null)
            {
                // 2. Œ—“¿Õ¿¬À»¬¿≈Ã ÙËÁËÍÛ
                targetRb.isKinematic = true;
                targetRb.velocity = Vector3.zero;
                targetRb.angularVelocity = Vector3.zero;
                
            }
            if (isRight == true)
            {

                other.transform.position = leftT.position;
                other.transform.SetParent(leftT);
                Debug.Log("Deleted");
                Destroy(this.gameObject, 7f);
            }
            if(isLeft == true)
            {
                other.transform.position = righT.position;
                other.transform.SetParent(righT);
                Debug.Log("Deleted");
                Destroy(this.gameObject, 7f);
            }
            if(!isLeft && !isRight)
            {
                other.transform.position = leftT.position;
                other.transform.SetParent(leftT);
                isLeft = true;
            }
        }
    }

    void FixedUpdate()
    {
        transform.Translate(-Vector3.forward * 3 * Time.fixedDeltaTime, Space.Self);
    }


    private void Update()
    {
        tableRot.Rotate(0, 20 * Time.deltaTime, 0f);
    }
}
