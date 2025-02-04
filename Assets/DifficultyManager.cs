using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class DifficultyParameters
{
    public float baseScrollSpeed = 5f;
    public float maxScrollSpeed = 15f;
    public float scrollSpeedIncreaseRate = 0.1f; // Units per second

    public float baseMinGapSize = 3f;
    public float baseMaxGapSize = 6f;
    public float minGapSizeLimit = 2f;
    public float maxGapSizeLimit = 4f;
    public float gapSizeDecreaseRate = 0.05f; // Units per second

    // You can add more parameters here as needed
    public float difficultyMultiplier = 1f;
    public float maxDifficultyMultiplier = 2f;
    public float difficultyRampUpTime = 300f; // Time in seconds to reach max difficulty
}

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyParameters parameters;
    [SerializeField] private LevelManager levelManager; // Reference to level manager

    public float currentScrollSpeed;
    private float currentMinGapSize;
    private float currentMaxGapSize;
    private float gameTime;
    private bool isDifficultyActive;

    public float CurrentScrollSpeed => currentScrollSpeed;
    public float CurrentMinGapSize => currentMinGapSize;
    public float CurrentMaxGapSize => currentMaxGapSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeDifficulty();
        SetupSceneReferences();

        // Subscribe to ScoreManager events if you want difficulty tied to score/distance
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceUpdated += OnDistanceUpdated;
        }

        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupSceneReferences();
    }

    private void SetupSceneReferences()
    {
        // Find LevelManager in new scene if not already assigned
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!isDifficultyActive) return;

        UpdateDifficulty();
    }

    public void ResetDifficulty()
    {
        currentScrollSpeed = parameters.baseScrollSpeed;
        currentMinGapSize = parameters.baseMinGapSize;
        currentMaxGapSize = parameters.baseMaxGapSize;
        gameTime = 0f;
        isDifficultyActive = false;

        // Immediately update LevelManager with base values
        if (levelManager != null)
        {
            levelManager.UpdateDifficultyParameters(
                parameters.baseScrollSpeed,
                parameters.baseMinGapSize,
                parameters.baseMaxGapSize
            );
        }
    }

    private void InitializeDifficulty()
    {
        ResetDifficulty();
    }

    public void StartDifficulty()
    {
        InitializeDifficulty();
        isDifficultyActive = true;
    }

    public void PauseDifficulty()
    {
        isDifficultyActive = false;
    }

    public void ResumeDifficulty()
    {
        isDifficultyActive = true;
    }

    private void UpdateDifficulty()
    {
        gameTime += Time.deltaTime;

        // Calculate difficulty multiplier based on time
        float normalizedTime = Mathf.Clamp01(gameTime / parameters.difficultyRampUpTime);
        float difficultyMultiplier = Mathf.Lerp(1f, parameters.maxDifficultyMultiplier, normalizedTime);

        // Update scroll speed
        currentScrollSpeed = Mathf.Lerp(
            parameters.baseScrollSpeed,
            parameters.maxScrollSpeed,
            normalizedTime
        );

        // Update gap sizes
        currentMinGapSize = Mathf.Lerp(
            parameters.baseMinGapSize,
            parameters.minGapSizeLimit,
            normalizedTime
        );

        currentMaxGapSize = Mathf.Lerp(
            parameters.baseMaxGapSize,
            parameters.maxGapSizeLimit,
            normalizedTime
        );

        // Apply updates to LevelManager
        if (levelManager != null)
        {
            levelManager.UpdateDifficultyParameters(
                currentScrollSpeed,
                currentMinGapSize,
                currentMaxGapSize
            );
        }
    }

    private void OnDistanceUpdated(float distance)
    {
        // You could add additional difficulty modifications based on distance here
        // For example, adding sudden difficulty spikes at certain milestones
    }

    public float GetCurrentDifficultyPercentage()
    {
        return Mathf.Clamp01(gameTime / parameters.difficultyRampUpTime) * 100f;
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceUpdated -= OnDistanceUpdated;
        }
    }
}