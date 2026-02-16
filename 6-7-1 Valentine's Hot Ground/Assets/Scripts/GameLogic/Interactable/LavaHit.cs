using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaHit : MonoBehaviour
{
    public Engine gameEngine;

    private bool gameIsOver = false;
    
    void Start() 
    {
        gameEngine = GameObject.Find("Engine").GetComponent<Engine>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (gameIsOver) return;
        
        // Access the colliding GameObject for further actions
        GameObject collidingObject = collision.gameObject;

        // Perform actions based on the colliding object's type or properties
        if (collidingObject.tag == "Player")
        {
            gameIsOver = true;
            Debug.Log("GAME OVER, lost!!! Collision detected with: Player >> "+collision.gameObject.name);
            HeroLogic heroLogic = collidingObject.GetComponent<HeroLogic>();
            heroLogic.PlayLoseParticles();
            gameEngine.gameIsOver = true;
        }
    }
}
