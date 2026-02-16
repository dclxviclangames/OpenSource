using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnScreenJoystick : MonoBehaviour
{
    // References to UI elements
    public Image thumbImage;
    public Image thumbBGImage;

    // Joystick movement range
    public float movementRange = 100f;

    public Engine gamesEngine;

    // Vector2 to store the joystick's direction
    private Vector2 inputDirection = Vector2.zero;

    private void Start()
    {
        // Set the joystick's position to the center of the screen
        thumbImage.transform.position = Vector2.zero; 
        thumbBGImage.transform.position = Vector2.zero; 
        thumbImage.gameObject.SetActive(false);
        thumbBGImage.gameObject.SetActive(false);
    }

    private bool mouseTouchStarted = false;
    private Vector2 mouseTouchStartLoc;
    private Vector2 trackedPosition;
    private void Update()
    {
        if (gamesEngine.gameIsOver) return;

        // Reset the input direction each frame
        inputDirection = Vector2.zero;

#if UNITY_ANDROID || UNITY_IPHONE
        // Used for Mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Convert touch position to screen coordinates
            trackedPosition = touch.position;

            if (!mouseTouchStarted) {
                mouseTouchStartLoc = new Vector2(trackedPosition.x, trackedPosition.y);
                thumbBGImage.transform.position = mouseTouchStartLoc;
                mouseTouchStarted = true;
                thumbImage.gameObject.SetActive(true);
                thumbBGImage.gameObject.SetActive(true);
            }
        }
#elif UNITY_WEBGL
        //  Used for WebGL mobile
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Convert touch position to screen coordinates
                trackedPosition = touch.position;

                if (!mouseTouchStarted) {
                    mouseTouchStartLoc = new Vector2(trackedPosition.x, trackedPosition.y);
                    thumbBGImage.transform.position = mouseTouchStartLoc;
                    mouseTouchStarted = true;
                    thumbImage.gameObject.SetActive(true);
                    thumbBGImage.gameObject.SetActive(true);
                }
            }
        }
        // Check for left mouse button down.  Use for WebGL desktop
        else if (Input.GetMouseButton(0))
        {
            // Convert mouse position to screen coordinates
            trackedPosition = Input.mousePosition;

            if (!mouseTouchStarted) {
                mouseTouchStartLoc = new Vector2(trackedPosition.x, trackedPosition.y);
                thumbBGImage.transform.position = mouseTouchStartLoc;
                mouseTouchStarted = true;
                thumbImage.gameObject.SetActive(true);
                thumbBGImage.gameObject.SetActive(true);
            }
        }
#endif

        if (mouseTouchStarted) 
        {
            // Calculate the direction of the mouse relative to the joystick's center
            Vector2 direction = new Vector2(trackedPosition.x - mouseTouchStartLoc.x, trackedPosition.y - mouseTouchStartLoc.y);

            // Normalize the direction vector
            direction.Normalize();

            // Clamp the direction vector to the movement range
            direction *= movementRange;

            // Update the thumb image's position
            thumbImage.transform.position = mouseTouchStartLoc + direction;
        }

#if UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount == 0) {
            mouseTouchStarted = false;
            thumbImage.transform.position = mouseTouchStartLoc;
            thumbBGImage.transform.position = mouseTouchStartLoc; 
            thumbImage.gameObject.SetActive(false);
            thumbBGImage.gameObject.SetActive(false);
        }
#elif UNITY_WEBGL
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 0) 
            {
                mouseTouchStarted = false;
                thumbImage.transform.position = mouseTouchStartLoc;
                thumbBGImage.transform.position = mouseTouchStartLoc; 
                thumbImage.gameObject.SetActive(false);
                thumbBGImage.gameObject.SetActive(false);
            }
        }
        else if (Input.GetMouseButtonUp(0)) 
        {
            mouseTouchStarted = false;
            thumbImage.transform.position = mouseTouchStartLoc;
            thumbBGImage.transform.position = mouseTouchStartLoc; 
            thumbImage.gameObject.SetActive(false);
            thumbBGImage.gameObject.SetActive(false);
        }
#endif

        // Update the input direction based on the thumb image's position
        inputDirection = thumbImage.transform.position - thumbBGImage.transform.position;
        inputDirection.Normalize();
    }

    // Returns a vector that that is between -1 and 1 for both X and Y
    // inputDirection.x -1, 0 = Left
    // inputDirection.x 1, 0 = Right
    // inputDirection.y -1, 0 = down
    // inputDirection.y 1, 0 = Up
    public Vector2 GetInputDirection()
    {
        // Debug.Log("Check on things >>> Input Direction "+ inputDirection.ToString());
        return inputDirection;
    }
}
