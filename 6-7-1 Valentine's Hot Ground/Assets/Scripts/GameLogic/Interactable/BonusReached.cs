using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusReached : MonoBehaviour
{
    public Engine gameEngine;

    public Material awardedMaterial;

    private bool bonusWasReached = false;

    void Start() 
    {
        gameEngine = GameObject.Find("Engine").GetComponent<Engine>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (bonusWasReached) return;
        
        // Access the colliding GameObject for further actions
        GameObject collidingObject = collision.gameObject;

        // Perform actions based on the colliding object's type or properties
        if (collidingObject.tag == "Player")
        {
            bonusWasReached = true;
            Debug.Log("GAME Bonus was hit >> "+collision.gameObject.name);
            HeroLogic heroLogic = collidingObject.GetComponent<HeroLogic>();
            heroLogic.PlayBonusParticles();
            gameEngine.UpdateScores(20);

            Renderer thisRenderer = this.gameObject.GetComponent<Renderer>();
            thisRenderer.material = awardedMaterial;
        }
    }
}
