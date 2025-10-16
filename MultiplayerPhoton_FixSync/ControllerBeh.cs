using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllerBeh : MonoBehaviour
{
    public PlayersController playersController;
    public PlayerMovement playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            playersController.enabled = true;
            playerMovement.enabled = false;
        }
        else
        {
            playerMovement.enabled = true;
            playersController.enabled = false;
        }
    }

    // Update is called once per frame
   
}
