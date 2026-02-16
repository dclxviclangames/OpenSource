using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    public Transform target; // The object to follow
    public Vector3 offset; // The offset from the target

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
