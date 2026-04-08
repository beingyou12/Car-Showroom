using UnityEngine;

public class FloatingIndicator : MonoBehaviour
{
    [Tooltip("How fast the arrow moves up and down")]
    public float bobSpeed = 2.0f;

    [Tooltip("How high and low the arrow travels")]
    public float bobHeight = 0.25f;

    private Vector3 startPosition;

    void Start()
    {
        // Remember where we placed the arrow in the scene so it bobs around this exact point
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new bobbing height using a Sine wave
        float newY = startPosition.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

        // Apply the new position, keeping the original X and Z coordinates
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}