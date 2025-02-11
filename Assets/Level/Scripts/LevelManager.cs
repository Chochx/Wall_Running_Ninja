using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance {  get; private set; }

    [Header("Spawn Settings")]
    public float scrollSpeed = 5f;
    [SerializeField] private float spawnOffset = 2f;
    [SerializeField] private float despawnOffset = -2f;
    [SerializeField] private float yOffset = 0f;

    [Header ("Enemy Spawn Settings")]
    [SerializeField] private List <GameObject> enemyPrefabs;
    [SerializeField] private int minSpawnPoints = 1;
    [SerializeField] private int maxSpawnPoints = 3;
    [SerializeField][Range(0f, 1f)] private float spawnChance = 0.7f;

    [Header("Building Components")]
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private List <GameObject> leftFacadePrefab;
    [SerializeField] private List <GameObject> rightFacadePrefab;
    [SerializeField] private List<float> buildingPosY;
    [SerializeField] private float minBuildingWidth = 20f;
    [SerializeField] private float maxBuildingWidth = 50f;
    [SerializeField] private GameObject rainDropSplashObject; 

    [Header("Gap Settings")]
    [SerializeField] private float minGapSize = 2f;
    [SerializeField] private float maxGapSize = 5f;
    [SerializeField] private float gapChance = 1f;

    private List<GameObject> activeBuildings = new List<GameObject>();
    private Camera mainCamera;
    private float spawnPositionX;
    private float despawnPositionX;
    private PlayerController controller;
    private int previousLevel = 0;
    private int currentLevel = 0;

    public event UnityAction <GameObject, float> onBuildingSpawned;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        DifficultyManager.Instance.StartDifficulty();
        DifficultyManager.Instance.OnNextLevelReached += OnLevelIncreased;
        controller = FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;
        UpdateScreenBounds();
        SpawnInitialBuilding();
    }

    private void OnLevelIncreased(int nextLevel)
    {
        currentLevel = nextLevel;
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

        //Spawn raindrop splash effect on ground.
        Vector3 rainSplashSpawnPos = baseCollider.bounds.center + new Vector3(0, baseCollider.bounds.extents.y, 0);
        GameObject rainSplash = Instantiate(rainDropSplashObject, buildingParent.transform);
        rainSplash.transform.position = rainSplashSpawnPos;
        var rainSplashVFX = rainSplash.GetComponent<ParticleSystem>();
        var rainSplashScale = rainSplashVFX.shape;
        rainSplashScale.scale = new Vector3(actualWidth + 2, 0, 0);

        // Spawn facades
        int setRandomBuildingNr = 4;
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
        onBuildingSpawned?.Invoke(baseSection, actualWidth);
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
            

            
            if (currentLevel != previousLevel)
            {
                float buildingWidth = 500;
                float gapSize = (Random.value < gapChance && activeBuildings.Count > 0) ?
                    Random.Range(minGapSize, maxGapSize) : 0f;

                float spawnX = CalculateNextSpawnPosition() + (buildingWidth * 0.5f) + gapSize;

                Vector3 spawnPosition = new Vector3(spawnX, buildingPosY[0], 0);

                SpawnBuilding(spawnPosition, buildingWidth);
                previousLevel = currentLevel;
            }
            else
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

    public void UpdateDifficultyParameters(float newScrollSpeed, float newMinGapSize, float newMaxGapSize)
    {
        scrollSpeed = newScrollSpeed;
        minGapSize = newMinGapSize;
        maxGapSize = newMaxGapSize;
    }
    //private void SpawnEnemiesOnBase(SpawnPointContainer container)
    //{
    //    List<SpawnPointContainer.SpawnPointData> availablePoints = new List<SpawnPointContainer.SpawnPointData>(container.spawnPoints);
    //    if (container.spawnPoints == null || container.spawnPoints.Length == 0 || enemyPrefabs.Count == 0)
    //        return;

    //    // Create a list of available spawn points
        

    //    // Determine how many spawn points to use
    //    int numPointsToUse = Random.Range(minSpawnPoints, Mathf.Min(maxSpawnPoints + 1, availablePoints.Count + 1));

    //    // Randomly select and use spawn points
    //    for (int i = 0; i < numPointsToUse; i++)
    //    {
    //        if (availablePoints.Count == 0) break;

    //        // Randomly select a spawn point
    //        int randomIndex = Random.Range(0, availablePoints.Count);
    //        SpawnPointContainer.SpawnPointData selectedPoint = availablePoints[randomIndex];
    //        availablePoints.RemoveAt(randomIndex);

    //        // Check spawn chance (multiplied by point weight)
    //        if (Random.value <= spawnChance * selectedPoint.weight)
    //        {
    //            SpawnEnemyAtPoint(selectedPoint.spawnPoint);
    //        }
    //    }
    //}

    //private void SpawnEnemyAtPoint(Transform spawnPoint)
    //{
    //    if (enemyPrefabs != null && enemyPrefabs.Count > 0)
    //    {
    //        // Randomly select an enemy prefab
    //        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Count);
    //        GameObject enemyPrefab = enemyPrefabs[randomEnemyIndex];

    //        // Spawn the enemy at the spawn point position
    //        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

    //        // Parent the enemy to the building so it moves with it
    //        enemy.transform.SetParent(spawnPoint.parent.parent);
    //    }
    //}

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