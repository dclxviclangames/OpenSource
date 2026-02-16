using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalReached : MonoBehaviour
{
    public Engine gameEngine;

    private bool goalWasReached = false;
    
    void Start() 
    {
        gameEngine = GameObject.Find("Engine").GetComponent<Engine>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (goalWasReached) return;
        
        // Access the colliding GameObject for further actions
        GameObject collidingObject = collision.gameObject;

        // Perform actions based on the colliding object's type or properties
        if (collidingObject.tag == "Player")
        {
            goalWasReached = true;
            Debug.Log("GAME WON!!! Collision detected with: Player  >> "+collision.gameObject.name);
            HeroLogic heroLogic = collidingObject.GetComponent<HeroLogic>();
            heroLogic.PlayWinParticles();
            gameEngine.UpdateScores(100);
            gameEngine.gameIsOver = true;
        }
    }
}
