using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemySpawnParameters
{
    public float baseSpawnSpacing = 4f;
    public float maxSpawnSpacing = 10f;
    public int baseEnemySpawnChance = 3;
    public int maxEnemySpawnChance = 0; 
}


public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }
    private DifficultyParameters difficultyParameters;
    [SerializeField] EnemySpawnParameters enemySpawnParameters;
    [SerializeField] private GameObject enemyPrefab; 
    private List<Vector3> spawnPoints = new List<Vector3>();
    [SerializeField]private float currentSpawnSpacing;
    private float currentEnemySpawnChance;
    

    public float CurrentSpawnSpacing => currentSpawnSpacing;
    public float CurrentEnemySpawnChance => currentEnemySpawnChance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LevelManager.instance.onBuildingSpawned += GenerateSpawnPoints;
        difficultyParameters = new DifficultyParameters();
    }

 
    private void GenerateSpawnPoints(GameObject parentBuilding, float buildingWidth)
    {
        float normalizedScrollSpeed = DifficultyManager.Instance.currentScrollSpeed / DifficultyManager.Instance.maxScrollSpeed;
        currentSpawnSpacing = Mathf.Lerp(enemySpawnParameters.baseSpawnSpacing, enemySpawnParameters.maxSpawnSpacing, normalizedScrollSpeed);
        Collider2D buildingCollider = parentBuilding.GetComponent<Collider2D>();
        float buildingTop = buildingCollider.bounds.extents.y;
        Vector3 centerSpawnReference = buildingCollider.bounds.center + new Vector3(0, buildingTop, 0);

        for (float i = -buildingWidth/2; i < buildingWidth/2; i+= currentSpawnSpacing) 
        {
            spawnPoints.Add(new Vector3(i, 0, 0));
        }

        float spawnChance;
        if (buildingWidth > 400)
        {
            spawnChance = 0.45f;
        }
        else spawnChance = 0.25f;

        var randomSpawnPoints = spawnPoints.Where(n => UnityEngine.Random.value < spawnChance).ToList();

        foreach (Vector3 randomSpawnPoint in randomSpawnPoints)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, parentBuilding.transform);
            newEnemy.transform.position = randomSpawnPoint + centerSpawnReference;
        }
        spawnPoints.Clear();
    }

    

    private void OnDisable()
    {
        LevelManager.instance.onBuildingSpawned -= GenerateSpawnPoints;
    }
}
