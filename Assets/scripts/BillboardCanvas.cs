using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    [Tooltip("Leave this empty and it will automatically find your VR headset")]
    public Transform playerCamera;

    [Tooltip("Check this if you want the canvas to stay perfectly upright without tilting up or down")]
    public bool lockVertical = true;

    [Tooltip("How many degrees to tilt the canvas on the X-axis (Pitch)")]
    public float tiltAngleX = 20f;

    void Start()
    {
        // If you didn't assign the camera manually, find the Main Camera (VR Headset) automatically
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Calculate the direction the camera is looking
        Vector3 forwardDirection = playerCamera.forward;

        // If we want the UI to stand up straight (usually better for reading)
        if (lockVertical)
        {
            forwardDirection.y = 0; 
        }

        // Make the canvas look in the exact same direction as the camera, preventing backwards text
        transform.LookAt(transform.position - forwardDirection);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        
        // Apply our custom X tilt, while keeping the correct Y and Z rotations
        transform.rotation = Quaternion.Euler(tiltAngleX, currentRotation.y, currentRotation.z);
    }
}