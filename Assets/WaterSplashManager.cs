using UnityEngine;

public class WaterSplashManager : MonoBehaviour
{
    [SerializeField] private GameObject waterSplashPrefab;
    [SerializeField] private Transform feetPosition; // Reference to the position where splash should appear
    private LayerMask platformLayer; // Layer for the moving platforms

    private void Start()
    {
        // Set the layer mask for platforms - adjust the layer name as needed
        platformLayer = LayerMask.GetMask("Ground");
    }

    // Call this method from your animation event
    public void CreateWaterSplash()
    {
        // Cast a ray downward to detect the platform
        RaycastHit2D hit = Physics2D.Raycast(feetPosition.position, Vector2.down, 1f, platformLayer);

        if (hit.collider != null)
        {
            // Create the splash effect at the feet position
            GameObject splash = Instantiate(waterSplashPrefab, feetPosition.position, Quaternion.Euler(-90f, 0f, 0f));

            // Parent the splash to the platform that was hit
            splash.transform.SetParent(hit.collider.transform);

        }
    }
}