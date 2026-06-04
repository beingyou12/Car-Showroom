using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CarRemoteTeleport : MonoBehaviour
{
    [Tooltip("Drag your XR Origin from the Hierarchy here")]
    public XROrigin playerOrigin;

    [Tooltip("Drag the CarSeatAnchor empty GameObject here")]
    public Transform carSeatTarget;

    [Tooltip("The socket the joystick lives in. After teleporting, the joystick is released from the hand and snapped back here so it does not follow the player to the car.")]
    public XRSocketInteractor homeSocket;

    [Tooltip("The grab interactable on this remote. Auto-assigned from this GameObject if left empty.")]
    public XRGrabInteractable grabInteractable;

    void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();
    }

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

            // Make sure the joystick stays behind in its socket instead of being
            // dragged along to the car in the player's hand.
            ReturnJoystickToSocket();
        }
        else
        {
            Debug.LogWarning("Missing XR Origin or Car Seat Target on the Remote script!");
        }
    }

    void ReturnJoystickToSocket()
    {
        if (grabInteractable == null)
            return;

        var manager = grabInteractable.interactionManager;
        if (manager == null)
            return;

        // 1. Release the joystick from whatever is currently holding it (the hand).
        //    This must happen before forcing the socket to select it, otherwise the
        //    socket would see the joystick selected by two interactors and drop it.
        if (grabInteractable.isSelected)
            manager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);

        // 2. Force the joystick back into its home socket so it snaps into place and
        //    does not follow the player. The socket keeps a normal (non-manual)
        //    selection, so the player can still grab it out of the socket again later.
        if (homeSocket != null)
            manager.SelectEnter((IXRSelectInteractor)homeSocket, (IXRSelectInteractable)grabInteractable);
    }
}