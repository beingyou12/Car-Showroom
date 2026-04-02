using UnityEngine;

public class GazeInfoPanel : MonoBehaviour
{
    [Tooltip("Drag your World Space Canvas here")]
    public GameObject infoPanel;
    
    [Tooltip("How far the player can be to trigger the label")]
    public float maxLookDistance = 10f;

    void Update()
    {
        // Create a ray pointing forward from the VR headset
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Shoot the raycast
        if (Physics.Raycast(ray, out hit, maxLookDistance))
        {
            // Check if the object we are looking at has the "Car" tag
            if (hit.collider.CompareTag("Car"))
            {
                infoPanel.SetActive(true);
            }
            else
            {
                infoPanel.SetActive(false);
            }
        }
        else
        {
            // Turn it off if we are looking at nothing
            infoPanel.SetActive(false);
        }
    }
}