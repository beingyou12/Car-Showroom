using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class DoorPhysicsOptimizer : MonoBehaviour
{
    private Rigidbody _rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _interactable;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (_interactable != null)
        {
            _interactable.selectEntered.AddListener(OnGrab);
            _interactable.selectExited.AddListener(OnRelease);
        }

        // Default to kinematic to prevent twitching when not grabbed
        _rb.isKinematic = true;
    }

    private void OnDestroy()
    {
        if (_interactable != null)
        {
            _interactable.selectEntered.RemoveListener(OnGrab);
            _interactable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _rb.isKinematic = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _rb.isKinematic = true;
    }
}