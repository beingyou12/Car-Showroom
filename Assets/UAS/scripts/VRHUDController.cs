using UnityEngine;
using UnityEngine.InputSystem;

public class VRHUDController : MonoBehaviour
{
    [Tooltip("Drag your TipsCanvas here")]
    public GameObject tipsUI;

    [Tooltip("Select the controller button to toggle the tips")]
    public InputActionProperty toggleButton;

    [Header("HUD Positioning")]
    [Tooltip("Leave empty to auto-find the VR headset")]
    public Transform playerCamera;
    
    [Tooltip("How far in front of the face the UI floats")]
    public float forwardDistance = 1.5f;
    
    [Tooltip("Negative numbers move it Left, Positive move it Right")]
    public float leftOffset = -0.5f;
    
    [Tooltip("Positive numbers move it Up, Negative move it Down")]
    public float upOffset = 0.4f;
    
    [Tooltip("How smoothly the UI catches up to head movement")]
    public float smoothSpeed = 8.0f;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
            
        // Tell the new input system to listen for this button
        if (toggleButton.action != null)
        {
            toggleButton.action.Enable();
        }
    }

    void Update()
    {
        // 1. Handle the Toggle Button Press
        if (toggleButton.action != null && toggleButton.action.WasPressedThisFrame())
        {
            if (tipsUI != null)
            {
                // Flips the active state (if on, turn off. if off, turn on)
                tipsUI.SetActive(!tipsUI.activeSelf);
            }
        }

        // 2. Handle the Smooth Follow Logic
        if (playerCamera != null && tipsUI != null && tipsUI.activeSelf)
        {
            // Calculate the exact target position in the upper left
            Vector3 targetPosition = playerCamera.position 
                                   + (playerCamera.forward * forwardDistance) 
                                   + (playerCamera.right * leftOffset) 
                                   + (playerCamera.up * upOffset);

            // Smoothly glide the Canvas to that position
            tipsUI.transform.position = Vector3.Lerp(tipsUI.transform.position, targetPosition, Time.deltaTime * smoothSpeed);

            // Make the Canvas rotate to face the same direction the camera is facing
            tipsUI.transform.LookAt(tipsUI.transform.position + playerCamera.forward);
        }
    }
}