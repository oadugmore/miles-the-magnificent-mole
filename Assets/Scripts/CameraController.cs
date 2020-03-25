using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float xMargin = 1f;      // Distance in the x axis the player can move before the camera follows.
    public float yMargin = 1f;      // Distance in the y axis the player can move before the camera follows.
    public float xSmooth = 8f;      // How smoothly the camera catches up with its target movement in the x axis.
    public float ySmooth = 8f;      // How smoothly the camera catches up with its target movement in the y axis.
    public Vector2 maxXAndY;        // The maximum x and y coordinates the camera can have.
    public Vector2 minXAndY;		// The minimum x and y coordinates the camera can have.
    //public bool clampYPosition;
    public float yTrackingOffset;

    private bool playerLastDigging = false;
    private float endDigSmoothTime = 0f;
    private bool isTracking;
    private PlayerController player;        // Reference to the player.


    void Start()
    {
        isTracking = true;

        // Setting up the reference.
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }


    bool CheckXMargin()
    {
        // Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
        return Mathf.Abs(transform.position.x - player.transform.position.x) > xMargin;
    }


    bool CheckYMargin()
    {
        // Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
        return Mathf.Abs(transform.position.y - player.transform.position.y) > yMargin;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    void FixedUpdate()
    {
        if (isTracking)
            TrackPlayer();
    }

    float lerpValue = 0;
    void TrackPlayer()
    {
        // By default the target x and y coordinates of the camera are its current x and y coordinates.
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        // If the player has moved beyond the x margin...
        //if(CheckXMargin())
        //	// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
        //	targetX = Mathf.Lerp(transform.position.x, player.position.x, xSmooth * Time.fixedDeltaTime);

        // If the player has moved beyond the y margin...

        if (CheckYMargin())
        {
            if (player.digging)
            {
                if (playerLastDigging && Time.time > endDigSmoothTime)
                    //stay right on top of it
                    targetY = player.transform.position.y + yTrackingOffset;
                else
                {
                    if (!playerLastDigging)
                    {
                        endDigSmoothTime = Time.time + 0.5f;
                        lerpValue = 0;
                    }
                    lerpValue += .04f;
                    //Debug.Log("Lerp value: " + lerpValue);
                    targetY = Mathf.Lerp(transform.position.y, player.transform.position.y + yTrackingOffset, lerpValue);
                }
                //targetY = (player.transform.position.y + yTrackingOffset + transform.position.y) / 2;
                playerLastDigging = true;
            }
            else
            {
                // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
                targetY = Mathf.Lerp(transform.position.y, player.transform.position.y + yTrackingOffset, ySmooth * Time.fixedDeltaTime);
                playerLastDigging = false;
            }
        }

        // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
        //targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
        //if (clampYPosition) targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);

        // Set the camera's position to the target position with the same z component.
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}
