using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float scrollSpeed = 5f;
    [SerializeField] private float spawnOffset = 2f;
    [SerializeField] private float despawnOffset = -2f;
    [SerializeField] private float yOffset = 0f;

    [Header("Building Components")]
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private List <GameObject> leftFacadePrefab;
    [SerializeField] private List <GameObject> rightFacadePrefab;
    [SerializeField] private List<float> buildingPosY;
    [SerializeField] private float minBuildingWidth = 20f;
    [SerializeField] private float maxBuildingWidth = 50f;

    [Header("Gap Settings")]
    [SerializeField] private float minGapSize = 2f;
    [SerializeField] private float maxGapSize = 5f;
    [SerializeField] private float gapChance = 1f;

    private List<GameObject> activeBuildings = new List<GameObject>();
    private Camera mainCamera;
    private float spawnPositionX;
    private float despawnPositionX;
    private PlayerController controller;

    private void Start()
    {
        controller = FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;
        UpdateScreenBounds();
        SpawnInitialBuilding();
    }

    private void Update()
    {
        if (controller.playerHasLanded && controller.isAlive)
        {
            MoveAndCheckBuildings();
        }
        CheckAndSpawnNewBuilding();
    }

    private void SpawnBuilding(Vector3 position, float buildingWidth)
    {
        GameObject buildingParent = new GameObject("Building");
        buildingParent.tag = "Ground";
        buildingParent.layer = 6;
        buildingParent.transform.position = position;

        // Spawn and stretch base
        GameObject baseSection = Instantiate(basePrefab, buildingParent.transform);
        baseSection.layer = buildingParent.layer;
        baseSection.tag = buildingParent.tag;
        baseSection.transform.localPosition = Vector3.zero;

        // Update sprite tiling for width
        SpriteRenderer baseSprite = baseSection.GetComponent<SpriteRenderer>();
        if (baseSprite != null)
        {
            baseSprite.size = new Vector2(buildingWidth, baseSprite.size.y);
        }

        // Update collider to match new size
        BoxCollider2D baseCollider = baseSection.GetComponent<BoxCollider2D>();
        if (baseCollider != null)
        {
            baseCollider.size = baseSprite.size;
        }

        // Get the actual width after scaling
        float actualWidth = baseSection.GetComponent<BoxCollider2D>().bounds.size.x;
        int setRandomBuildingNr = Random.Range(0, 3);

        // Spawn facades
        GameObject leftFacade = Instantiate(leftFacadePrefab[setRandomBuildingNr], buildingParent.transform);
        GameObject rightFacade = Instantiate(rightFacadePrefab[setRandomBuildingNr], buildingParent.transform);

        leftFacade.layer = buildingParent.layer;
        leftFacade.tag = buildingParent.tag;
        rightFacade.layer = buildingParent.layer;
        rightFacade.tag = buildingParent.tag;

        // Position facades at the edges of the base
        leftFacade.transform.localPosition = new Vector3(-actualWidth * 0.5f, 0, 0);
        rightFacade.transform.localPosition = new Vector3(actualWidth * 0.5f, 0, 0);

        activeBuildings.Add(buildingParent);
    }

    private void SpawnInitialBuilding()
    {
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float spawnY = buildingPosY[0];
        float initialWidth = maxBuildingWidth; 
        SpawnBuilding(new Vector3(-bottomRight.x, spawnY, 0), initialWidth);
    }

    private void MoveAndCheckBuildings()
    {
        foreach (var building in activeBuildings.ToArray())
        {
            building.transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

            float rightEdge = GetBuildingRightEdge(building);
            if (rightEdge < despawnPositionX)
            {
                activeBuildings.Remove(building);
                Destroy(building);
            }
        }
    }

    private void CheckAndSpawnNewBuilding()
    {
        if (activeBuildings.Count == 0 || ShouldSpawnNewBuilding())
        {
            float buildingWidth = Random.Range(minBuildingWidth, maxBuildingWidth);
            float gapSize = (Random.value < gapChance && activeBuildings.Count > 0) ?
                Random.Range(minGapSize, maxGapSize) : 0f;

            float spawnX = CalculateNextSpawnPosition() + (buildingWidth * 0.5f) + gapSize;

            int previousBuilding = activeBuildings.Count - 1;
            float previousBuildingHeight = activeBuildings[previousBuilding].transform.position.y;

            int newSpawnPosY;
            
            if ((int)previousBuildingHeight == buildingPosY[0])
            {
                newSpawnPosY = Random.Range(0, 2);
                Debug.Log("new pos: " + newSpawnPosY);
            }
            else if ((int)previousBuildingHeight == buildingPosY[1])
            {
                newSpawnPosY = Random.Range(0, 3);
            }
            else 
            {
                newSpawnPosY = Random.Range(0, 3);
            }

            Vector3 spawnPosition = new Vector3(spawnX, buildingPosY[newSpawnPosY], 0);

            SpawnBuilding(spawnPosition, buildingWidth);
        }
    }

    private bool ShouldSpawnNewBuilding()
    {
        if (activeBuildings.Count == 0) return true;

        GameObject rightmostBuilding = GetRightmostBuilding();
        return GetBuildingRightEdge(rightmostBuilding) < spawnPositionX;
    }

    private float CalculateNextSpawnPosition()
    {
        if (activeBuildings.Count == 0) return spawnPositionX;

        GameObject rightmostBuilding = GetRightmostBuilding();
        return GetBuildingRightEdge(rightmostBuilding);
    }

    private GameObject GetRightmostBuilding()
    {
        return activeBuildings.Count > 0 ?
            activeBuildings[activeBuildings.Count - 1] : null;
    }

    private float GetBuildingRightEdge(GameObject building)
    {
        float rightmostEdge = float.MinValue;

        foreach (Transform child in building.transform)
        {
            Collider2D collider = child.GetComponent<Collider2D>();
            if (collider != null)
            {
                float edge = collider.bounds.max.x;
                rightmostEdge = Mathf.Max(rightmostEdge, edge);
            }
        }

        return rightmostEdge;
    }

    private void UpdateScreenBounds()
    {
        Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        spawnPositionX = rightEdge.x + spawnOffset;
        despawnPositionX = leftEdge.x + despawnOffset;
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateScreenBounds();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            UpdateScreenBounds();
        }
    }
#endif

    public void RefreshScreenBounds()
    {
        UpdateScreenBounds();
    }
}