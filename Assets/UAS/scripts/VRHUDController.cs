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
        // Handle the Toggle Button Press
        if (toggleButton.action != null && toggleButton.action.WasPressedThisFrame())
        {
            if (tipsUI != null)
            {
                bool turningOn = !tipsUI.activeSelf;

                // Flips the active state (if on, turn off. if off, turn on)
                tipsUI.SetActive(turningOn);

                // When opening, place the panel once in front of the player and FREEZE it.
                // A stationary panel is required so the player can reliably poke its buttons.
                if (turningOn)
                {
                    PositionInFrontOfPlayer();
                }
            }
        }
    }

    /// <summary>
    /// Snaps the tutorial panel to a comfortable spot in front of the player's head and
    /// faces it toward the camera. Called once on open; the panel then stays put so it can be poked.
    /// </summary>
    void PositionInFrontOfPlayer()
    {
        if (playerCamera == null || tipsUI == null)
            return;

        Vector3 targetPosition = playerCamera.position
                               + (playerCamera.forward * forwardDistance)
                               + (playerCamera.right * leftOffset)
                               + (playerCamera.up * upOffset);

        tipsUI.transform.position = targetPosition;

        // Make the Canvas face the same direction the camera is looking.
        tipsUI.transform.LookAt(tipsUI.transform.position + playerCamera.forward);
    }
}