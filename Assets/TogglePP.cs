using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TogglePP : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private GameObject currentBuilding;
    private float buildingWidth;
    public bool isPlayerAligned = false;
    private Volume volume;

    void Start()
    {
        LevelManager.instance.onBuildingSpawned += OnBuildingSpawned;
        volume = GetComponent<Volume>();
    }

    private void OnBuildingSpawned(GameObject building, float width)
    {
        if (width > 400)
        {
            currentBuilding = building;
            buildingWidth = width;
            
        }
    }

    private void Update()
    {
        if (currentBuilding == null) return;
        float buildingLeft = currentBuilding.transform.position.x - (buildingWidth / 2);
        float buildingRight = currentBuilding.transform.position.x + (buildingWidth / 2);
        float playerX = player.transform.position.x;

        // Check if player is within building's x bounds
        bool isNowAligned = playerX >= buildingLeft && playerX <= buildingRight;

        // Detect change in alignment
        if (isNowAligned != isPlayerAligned)
        {
            isPlayerAligned = isNowAligned;
            if (isPlayerAligned)
            {
                volume.enabled = true;
            }
            else
            {
                volume.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.onBuildingSpawned -= OnBuildingSpawned;
        }
        volume.enabled = false;
    }
}


