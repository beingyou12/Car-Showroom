using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-10)]
public class IgnoreCarCollision : MonoBehaviour
{
    [SerializeField] private GameObject carRoot;
    [SerializeField] private bool ignoreFloor = true;

    private void Awake()
    {
        if (carRoot == null)
        {
            // Cache car root by checking parents only once
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name.Contains("RMCar26"))
                {
                    carRoot = current.gameObject;
                    break;
                }
                current = current.parent;
            }
        }

        if (carRoot == null) return;

        Collider[] doorColliders = GetComponentsInChildren<Collider>();
        
        // Use a more efficient way to find environment colliders if needed
        // Instead of FindObjectsByType<Collider>, we'll look for specific tags or layers if possible, 
        // but for now, we'll at least use a targeted search or just the car's children.
        
        List<Collider> targets = new List<Collider>();
        targets.AddRange(carRoot.GetComponentsInChildren<Collider>());

        if (ignoreFloor)
        {
            // Optimization: Only search for specific objects that are likely to be ground
            // This is still broad but better than finding ALL colliders in the entire world.
            // We can check common names in a more constrained way if we have a reference.
            // For now, let's look for "Floor" or "Plane" objects specifically in the root scene.
            foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                string lowerName = go.name.ToLower();
                if (lowerName.Contains("floor") || lowerName.Contains("plane") || lowerName.Contains("ground"))
                {
                    targets.AddRange(go.GetComponentsInChildren<Collider>());
                }
            }
        }

        foreach (var doorCol in doorColliders)
        {
            foreach (var targetCol in targets)
            {
                if (targetCol == null || doorCol == null || targetCol == doorCol) continue;
                
                // Don't ignore collision with itself or its children
                if (targetCol.transform.IsChildOf(transform)) continue;

                Physics.IgnoreCollision(doorCol, targetCol);
            }
        }
        
        Debug.Log($"[IgnoreCarCollision] {name} ignoring {targets.Count} colliders.");
    }
}

