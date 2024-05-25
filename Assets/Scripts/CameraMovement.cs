using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player; // Reference to the player transform
    public float smoothSpeed = 0.125f; // Smoothing speed
    public Vector3 offset; // Offset value for the camera position

    private float initialZ; // To store the initial z position of the camera

    void Start()
    {
        // Store the initial z position of the camera
        initialZ = transform.position.z;
    }

    void FixedUpdate()
    {
        // Calculate the desired position based on the player position and offset
        Vector3 desiredPosition = player.position + offset;

        // Set the z position to the initial z position to keep it constant
        desiredPosition.z = initialZ;

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the camera position
        transform.position = smoothedPosition;
    }
}
