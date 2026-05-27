using UnityEngine;
using Unity.XR.CoreUtils;

public class CarRemoteTeleport : MonoBehaviour
{
    [Tooltip("Drag your XR Origin from the Hierarchy here")]
    public XROrigin playerOrigin;

    [Tooltip("Drag the CarSeatAnchor empty GameObject here")]
    public Transform carSeatTarget;

    // We will call this function using the remote's grab interactable
    public void TeleportIntoCar()
    {
        if (playerOrigin != null && carSeatTarget != null)
        {
            // MoveCameraToWorldLocation perfectly aligns the VR headset to the target position
            // regardless of where the player is physically standing in their real room.
            playerOrigin.MoveCameraToWorldLocation(carSeatTarget.position);
            
            // This rotates the player to look out the windshield
            playerOrigin.MatchOriginUpCameraForward(carSeatTarget.up, carSeatTarget.forward);
        }
        else
        {
            Debug.LogWarning("Missing XR Origin or Car Seat Target on the Remote script!");
        }
    }
}