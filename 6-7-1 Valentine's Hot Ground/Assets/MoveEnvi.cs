using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEnvi : MonoBehaviour
{
    // Start is called before the first frame update
    void FixedUpdate()
    {
        transform.Translate(-Vector3.forward * 3 * Time.fixedDeltaTime, Space.Self);
    }
}
