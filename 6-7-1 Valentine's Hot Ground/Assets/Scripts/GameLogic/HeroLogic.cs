using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroLogic : MonoBehaviour
{
    public Rigidbody rb;
    public float forceMultiplier = 100f;

    public ParticleSystem winParticles;
    public ParticleSystem loseParticles;

    public Engine gameEngine;

    public OnScreenJoystick osJoystick;

    private Vector2 joystickDirection;

    void Start() 
    {
        gameEngine = GameObject.Find("Engine").GetComponent<Engine>();
        osJoystick = GameObject.Find("Joystick").GetComponent<OnScreenJoystick>();
    }

    void FixedUpdate() 
    {
        if (gameEngine.gameIsOver) return;
        
#if UNITY_ANDROID || UNITY_IPHONE
        joystickDirection = osJoystick.GetInputDirection();

        // Move Right and Left
        if (joystickDirection.x > 0.1f) {
            rb.AddForce(Vector3.right * (forceMultiplier * joystickDirection.x));
        } else if (joystickDirection.x < -0.1f) {
            rb.AddForce(Vector3.left * (forceMultiplier * (joystickDirection.x * -1f)));

        }

        // Move Up and Down
        if (joystickDirection.y > 0.1f) {
            rb.AddForce(Vector3.forward * (forceMultiplier * joystickDirection.y));
        } else if (joystickDirection.y < -0.1f) {
            rb.AddForce(Vector3.back * (forceMultiplier * (joystickDirection.y * -1f)));
        }
#elif UNITY_WEBGL || UNITY_EDITOR
        joystickDirection = osJoystick.GetInputDirection();

        // Move Right and Left
        if (joystickDirection.x > 0.1f) {
            rb.AddForce(Vector3.right * (forceMultiplier * joystickDirection.x));
        } else if (joystickDirection.x < -0.1f) {
            rb.AddForce(Vector3.left * (forceMultiplier * (joystickDirection.x * -1f)));
        }

        // Move Up and Down
        if (joystickDirection.y > 0.1f) {
            rb.AddForce(Vector3.forward * (forceMultiplier * joystickDirection.y));
        } else if (joystickDirection.y < -0.1f) {
            rb.AddForce(Vector3.back * (forceMultiplier * (joystickDirection.y * -1f)));
        }

        // Move forward
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            rb.AddForce(Vector3.forward * forceMultiplier);
        }

        // Rotate left
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            rb.AddForce(Vector3.right * forceMultiplier);
        }

        // Rotate right
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            rb.AddForce(Vector3.left * forceMultiplier);
        }

        // Move backward
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            rb.AddForce(Vector3.back * forceMultiplier);
        }
#endif 
    }

    public void PlayLoseParticles (){
        loseParticles.transform.gameObject.SetActive(true);
        loseParticles.Play();
        gameEngine.PlaySFX("LoseClip");
        gameEngine.GameLose();
    }

    public void PlayWinParticles (){
        winParticles.transform.gameObject.SetActive(true);
        winParticles.Play();
        gameEngine.PlaySFX("WinClip");
        gameEngine.GameWon();
    }

    public void PlayBonusParticles (){
        gameEngine.PlaySFX("BonusClip");
    }
}
