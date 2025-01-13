using UnityEngine;
using System.Collections.Generic;

public class WallManager : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float wallMarginPercent = 0.1f;
    [SerializeField] private float wallSegmentHeightPercent = 0.2f;
    [SerializeField] private GameObject wallSegmentPrefab;
    [SerializeField] private int initialWallSegments = 3;
    [SerializeField] private Camera gameCamera;

    private float screenWidth;
    private float screenHeight;
    private float wallSegmentHeight;
    private List<GameObject> wallSegments = new List<GameObject>();
    private float totalScrollDistance;

    // Track camera bounds
    private float screenTop;
    private float screenBottom;

    // Public properties
    public float HorizontalSnapDistance { get; private set; }
    public float ScrollSpeed => scrollSpeed;

    private void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        CalculateScreenDimensions();

        // Spawn initial segments to fill screen plus buffer
        float startY = screenBottom;
        float endY = screenTop + wallSegmentHeight; // One extra segment above screen

        float currentY = startY;
        while (currentY < endY)
        {
            SpawnNewWallSegment(currentY);
            currentY += wallSegmentHeight;
        }
    }

    private void CalculateScreenDimensions()
    {
        Vector2 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, gameCamera.nearClipPlane));
        Vector2 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1, 1, gameCamera.nearClipPlane));

        screenWidth = topRight.x - bottomLeft.x;
        screenHeight = topRight.y - bottomLeft.y;

        // Cache screen bounds
        screenTop = topRight.y;
        screenBottom = bottomLeft.y;

        HorizontalSnapDistance = screenWidth * (0.5f - wallMarginPercent);
        wallSegmentHeight = screenHeight * wallSegmentHeightPercent;
    }

    private void Update()
    {
        // Handle resolution changes
        if (Screen.width != screenWidth || Screen.height != screenHeight)
        {
            CalculateScreenDimensions();
            AdjustExistingWalls();
        }

        ScrollWalls();
        ManageWallSegments();
    }

    private void ScrollWalls()
    {
        foreach (GameObject segment in wallSegments)
        {
            segment.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
        }

        totalScrollDistance += scrollSpeed * Time.deltaTime;
    }

    private void ManageWallSegments()
    {
        // Remove segments that are fully below screen
        wallSegments.RemoveAll(segment =>
        {
            if (segment == null) return true;

            // Check if segment is completely below screen
            bool shouldRemove = segment.transform.position.y + wallSegmentHeight < screenBottom;

            if (shouldRemove)
            {
                Destroy(segment);
            }

            return shouldRemove;
        });

        // Get highest segment's Y position
        float highestY = GetHighestSegmentY();

        // Spawn new segments if needed
        while (highestY < screenTop + wallSegmentHeight)
        {
            SpawnNewWallSegment(highestY + wallSegmentHeight);
            highestY += wallSegmentHeight;
        }
    }

    private void SpawnNewWallSegment(float yPosition)
    {
        // Create left wall
        GameObject leftWall = Instantiate(wallSegmentPrefab, transform);
        Vector3 scale = leftWall.transform.localScale;
        scale.x = screenWidth * wallMarginPercent;
        scale.y = wallSegmentHeight;
        leftWall.transform.localScale = scale;
        leftWall.transform.position = new Vector3(
            -screenWidth * 0.5f + scale.x * 0.5f,
            yPosition,
            0
        );
        wallSegments.Add(leftWall);

        // Create right wall
        GameObject rightWall = Instantiate(wallSegmentPrefab, transform);
        rightWall.transform.localScale = scale;
        rightWall.transform.position = new Vector3(
            screenWidth * 0.5f - scale.x * 0.5f,
            yPosition,
            0
        );
        wallSegments.Add(rightWall);
    }

    private float GetHighestSegmentY()
    {
        float highest = float.MinValue;
        foreach (GameObject segment in wallSegments)
        {
            if (segment != null && segment.transform.position.y > highest)
            {
                highest = segment.transform.position.y;
            }
        }
        return highest;
    }

    private void AdjustExistingWalls()
    {
        foreach (GameObject segment in wallSegments)
        {
            if (segment == null) continue;

            Vector3 scale = segment.transform.localScale;
            scale.x = screenWidth * wallMarginPercent;
            segment.transform.localScale = scale;

            if (segment.transform.position.x != 0)
            {
                float direction = Mathf.Sign(segment.transform.position.x);
                segment.transform.position = new Vector3(
                    direction * (screenWidth * 0.5f - scale.x * 0.5f),
                    segment.transform.position.y,
                    segment.transform.position.z
                );
            }
        }
    }

    public float GetScore() => totalScrollDistance;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw screen bounds
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-screenWidth / 2, screenTop, 0), new Vector3(screenWidth / 2, screenTop, 0));
        Gizmos.DrawLine(new Vector3(-screenWidth / 2, screenBottom, 0), new Vector3(screenWidth / 2, screenBottom, 0));
    }
}