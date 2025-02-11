using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemySpawnParameters
{
    public float baseSpawnSpacing = 6f;
    public float maxSpawnSpacing = 10;
    public int baseEnemySpawnChance = 3;
    public int maxEnemySpawnChance = 0; 
}


public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }
    [SerializeField] EnemySpawnParameters enemySpawnParameters;
    [SerializeField] private GameObject enemyPrefab; 
    private List<Vector3> spawnPoints = new List<Vector3>();
    private float currentSpawnSpacing;
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
    }

    private void Update()
    {
        
    }

    private void GenerateSpawnPoints(GameObject parentBuilding, float buildingWidth)
    {
        Collider2D buildingCollider = parentBuilding.GetComponent<Collider2D>();
        float buildingTop = buildingCollider.bounds.extents.y;
        Vector3 centerSpawnReference = buildingCollider.bounds.center + new Vector3(0, buildingTop, 0);

        for (float i = -buildingWidth/2; i < buildingWidth/2; i+=10) 
        {
            spawnPoints.Add(new Vector3(i, 0, 0));
        }
        System.Random rand = new System.Random();
        var randomSpawnPoints = spawnPoints.Where(n => rand.Next(3) == 0).ToList();

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
