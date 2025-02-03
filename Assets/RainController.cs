using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private DifficultyManager difficultyManager;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        difficultyManager = DifficultyManager.Instance;

        UpdateRainVelocity();
    }

    private void Update()
    {
        UpdateRainVelocity(); 
    }

    private void UpdateRainVelocity()
    {
        if (difficultyManager != null) return;
        
        var velocityModule = _particleSystem.velocityOverLifetime;
        velocityModule.enabled = true;

        float rainSpeed = difficultyManager.CurrentScrollSpeed;

        velocityModule.x = new ParticleSystem.MinMaxCurve(-rainSpeed, -rainSpeed); 
    }
}
