using System.Collections.Generic;
using UnityEngine;

public class LevelSegment : MonoBehaviour
{
    [Header("Segment Properties")]
    public float segmentWidth;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints; // Empty GameObjects marking spawn locations
    [SerializeField] private bool shouldSpawnObjects = true;

    private void OnValidate()
    {
        // Auto-calculate width if not set
        if (segmentWidth <= 0)
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                segmentWidth = collider.size.x * transform.localScale.x;
            }
        }
    }

    public List<Transform> GetSpawnPoints()
    {
        return spawnPoints;
    }

    public bool ShouldSpawnObjects()
    {
        return shouldSpawnObjects;
    }
}