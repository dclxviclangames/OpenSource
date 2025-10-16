using System.Collections;
//using Photon.Pun;
using UnityEngine;

public class JumpUp : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
       
        PlayerMovement otherPlayer = other.GetComponent<PlayerMovement>();

        // 3. ���� �� ����� PlayerHealth � ��� �� ��� ����������� �����.
        if (otherPlayer != null)
        {
            
            otherPlayer.JumpBoost();
        }
        
    }
}
