using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float scrollSpeed = 5f;
    [SerializeField] private float spawnOffset = 2f; // Extra units beyond screen edge 
    [SerializeField] private float despawnOffset = -2f; // Extra units beyond screen edge
    [SerializeField] private float yOffset = 0f;

    [Header("Level Prefabs")]
    [SerializeField] private List<GameObject> levelPrefabs;

    [Header("Object Spawning")]
    [SerializeField] private List<GameObject> spawnableObjects; // Prefabs of objects to spawn
    [SerializeField][Range(0, 1)] private float spawnChance = 0.5f;

    [Header("Gap Settings")]
    [SerializeField] private float minGapSize = 2f;  // Minimum space between platforms
    [SerializeField] private float maxGapSize = 5f;  // Maximum space between platforms
    [SerializeField] private float gapChance = 0.3f;

    private List<GameObject> activeSegments = new List<GameObject>();
    private Camera mainCamera;
    private float spawnPositionX;
    private float despawnPositionX;
    private PlayerController controller;


    private void Start()
    {
        controller = FindFirstObjectByType <PlayerController>();
        mainCamera = Camera.main;
        UpdateScreenBounds();
        SpawnInitialSegment();
    }

    private void Update()
    {
        if (controller.playerHasLanded && controller.isAlive)
        {
            MoveAndCheckSegments();
        }
        CheckAndSpawnNewSegment();
    }

    private void UpdateScreenBounds()
    {
        // Convert viewport points to world positions
        Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));

        // Set spawn and despawn positions with offset
        spawnPositionX = rightEdge.x + spawnOffset;
        despawnPositionX = leftEdge.x + despawnOffset;
    }

    private void SpawnInitialSegment()
    {
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float spawnY = bottomRight.y;
        Vector3 spawnPosition = new Vector3(0, spawnY, 0); // Start at center screen
        GameObject newSegment = Instantiate(levelPrefabs[0], spawnPosition, Quaternion.identity);
        activeSegments.Add(newSegment);
    }

    private void MoveAndCheckSegments()
    {
        foreach (var segment in activeSegments.ToArray())
        {
            // Move segment left
            segment.transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

            // Get the right edge of the platform
            float rightEdge = GetSegmentRightEdge(segment);

            // Only destroy if the entire platform is off screen
            if (rightEdge < despawnPositionX)
            {
                activeSegments.Remove(segment);
                Destroy(segment);
            }
        }
    }

    private void CheckAndSpawnNewSegment()
    {
        if (ShouldSpawnNewSegment())
        {
            SpawnNewSegment();
        }
    }

    private bool ShouldSpawnNewSegment()
    {
        if (activeSegments.Count == 0)
            return true;

        GameObject rightmostSegment = GetRightmostSegment();
        float rightEdgeX = GetSegmentRightEdge(rightmostSegment);

        // Check if the rightmost segment has moved far enough left to spawn a new one
        return rightEdgeX < spawnPositionX;
    }

    private GameObject GetRightmostSegment()
    {
        GameObject rightmost = activeSegments[0];
        float rightmostEdge = GetSegmentRightEdge(rightmost);

        foreach (var segment in activeSegments)
        {
            float segmentRightEdge = GetSegmentRightEdge(segment);
            if (segmentRightEdge > rightmostEdge)
            {
                rightmost = segment;
                rightmostEdge = segmentRightEdge;
            }
        }

        return rightmost;
    }

    private float GetSegmentRightEdge(GameObject segment)
    {
        // Try to get width from BoxCollider2D first
        BoxCollider2D collider = segment.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            return segment.transform.position.x + (collider.size.x * segment.transform.localScale.x) / 2;
        }

        // Fallback to SpriteRenderer
        SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return segment.transform.position.x + (renderer.bounds.size.x / 2);
        }

        // Last resort: just use position
        return segment.transform.position.x;
    }

    private void SpawnNewSegment()
    {
        GameObject prefabToSpawn = levelPrefabs[Random.Range(0, levelPrefabs.Count)];

        // Calculate spawn position accounting for the segment's width
        float spawnX = CalculateSpawnXPosition(prefabToSpawn);

        // Add random gap if rolled
        if (Random.value < gapChance && activeSegments.Count > 0)
        {
            float gapSize = Random.Range(minGapSize, maxGapSize);
            spawnX += gapSize;
        }

        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float spawnY = bottomRight.y + yOffset;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        GameObject newSegment = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        activeSegments.Add(newSegment);

        SpawnObjectsOnSegment(newSegment);
    }

    private float CalculateSpawnXPosition(GameObject prefabToSpawn)
    {
        if (activeSegments.Count == 0)
        {
            return spawnPositionX;
        }

        // Get the rightmost edge of the last spawned segment
        GameObject lastSegment = GetRightmostSegment();
        float lastSegmentRightEdge = GetSegmentRightEdge(lastSegment);

        // Calculate half width of the new segment we're about to spawn
        BoxCollider2D newSegmentCollider = prefabToSpawn.GetComponent<BoxCollider2D>();
        float halfWidth = (newSegmentCollider.size.x * prefabToSpawn.transform.localScale.x) / 2;

        // Return position that's right after the last segment, accounting for the new segment's center point
        return lastSegmentRightEdge + halfWidth;
    }


    private void SpawnObjectsOnSegment(GameObject segment)
    {
        LevelSegment levelSegment = segment.GetComponent<LevelSegment>();

        if (levelSegment != null && levelSegment.ShouldSpawnObjects())
        {
            List<Transform> spawnPoints = levelSegment.GetSpawnPoints();

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (Random.value < spawnChance)
                {
                    GameObject objectToSpawn = spawnableObjects[Random.Range(0, spawnableObjects.Count)];
                    Vector3 worldSpawnPoint = spawnPoint.position; // Use the world position of the spawn point

                    // Spawn as child but maintain original scale
                    GameObject spawnedObject = Instantiate(objectToSpawn, worldSpawnPoint, Quaternion.identity, segment.transform);

                    // Calculate and apply the compensated scale
                    Vector3 originalScale = objectToSpawn.transform.localScale;
                    Vector3 parentScale = segment.transform.lossyScale; // Use lossyScale to get the actual world scale

                    spawnedObject.transform.localScale = new Vector3(
                        originalScale.x / parentScale.x,
                        originalScale.y / parentScale.y,
                        originalScale.z / parentScale.z
                    );
                }
            }
        }
    }

    // Handle screen orientation changes
    private void OnRectTransformDimensionsChange()
    {
        UpdateScreenBounds();
    }

    // Editor-only validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            UpdateScreenBounds();
        }
    }
#endif

    // Public method to manually refresh bounds if needed
    public void RefreshScreenBounds()
    {
        UpdateScreenBounds();
    }
}

