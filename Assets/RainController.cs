using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    private new ParticleSystem particleSystem;
    private DifficultyManager difficultyManager;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        difficultyManager = DifficultyManager.Instance;

        UpdateRainVelocity();
    }

    private void Update()
    {
        UpdateRainVelocity(); 
    }

    private void UpdateRainVelocity()
    {
        if (difficultyManager == null) return;
        
        var velocityModule = particleSystem.velocityOverLifetime;
        velocityModule.enabled = true;

        float rainSpeed = difficultyManager.currentScrollSpeed*1.2f;

        velocityModule.x = new ParticleSystem.MinMaxCurve(-rainSpeed-10, -rainSpeed); 

        if (!playerController.isAlive)
        {
            velocityModule.x = new ParticleSystem.MinMaxCurve(0, 0);
        }
    }
}
