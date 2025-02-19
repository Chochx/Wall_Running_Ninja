using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class PlayerScore
{
    public float distanceScore;
    public float bonusScore;
    public float totalScore;
    public float furthestDistance;
    public DateTime lastPlayed;

    public PlayerScore()
    {
        distanceScore = 0f;
        bonusScore = 0f;
        totalScore = 0f;
        furthestDistance = 0f;
        lastPlayed = DateTime.Now;
    }
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private float pointsPerMeter = 10f;
    [SerializeField] private float highScoreBonus = 100f;
    [SerializeField] private float enemyKillBonus = 1000f;

    [Header("Level References")]
    [SerializeField] private LevelManager levelManager;
    private PlayerController playerController;
    private int currentLevelId = 0; 

    // Game state
    private float totalDistanceTraveled = 0f;
    [HideInInspector]public bool isGameActive = false;
    private int enemiesKilledThisRun = 0;

    // Current session score data
    private PlayerScore currentScore = new PlayerScore();

    // Events
    public event UnityAction<float> OnDistanceUpdated;
    public event UnityAction<float> OnScoreUpdated;
    public event UnityAction<float> OnHighScoreAchieved;
    
    public event UnityAction<int> OnKillsUpdated; 

    // Properties
    public float DistanceScore => currentScore.distanceScore;
    public float BonusScore => currentScore.bonusScore;
    public float TotalScore => currentScore.totalScore;
    public float FurthestDistance => totalDistanceTraveled;
    public int EnemiesKilled => enemiesKilledThisRun;

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
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        StartNewGame();
    }
    public void ReinitializeGame()
    {
        // Reinitialize game references
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        StartNewGame();
    }

    private void Update()
    {
        if (!isGameActive || levelManager == null || playerController == null) return;

        if (playerController.playerHasLanded && playerController.isAlive)
        {
            UpdateDistance();
        }
    }

    private void OnEnable()
    {
        // Subscribe to scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Reinitialize when a new scene is loaded
        ReinitializeGame();
    }

    private void UpdateDistance()
    {
        float distanceThisFrame = levelManager.scrollSpeed * Time.deltaTime;
        totalDistanceTraveled += distanceThisFrame;

        currentScore.distanceScore = totalDistanceTraveled * pointsPerMeter;

        UserData userData = UserDataManager.Instance.GetCurrentUserData();
        if (userData != null)
        {
            // Check if current run is further than previous best
            if (totalDistanceTraveled > userData.totalDistance)
            {
                float newDistanceBonus = distanceThisFrame * highScoreBonus;
                currentScore.bonusScore += newDistanceBonus;
                OnHighScoreAchieved?.Invoke(totalDistanceTraveled);
            }
        }

        currentScore.totalScore = currentScore.distanceScore + currentScore.bonusScore;
        currentScore.furthestDistance = totalDistanceTraveled;

        OnDistanceUpdated?.Invoke(totalDistanceTraveled);
        OnScoreUpdated?.Invoke(currentScore.totalScore);

       
    }

    public void OnEnemyKilled()
    {
        enemiesKilledThisRun++;
        OnKillsUpdated(enemiesKilledThisRun);
        AddBonus(enemyKillBonus);

        Debug.Log("Enemy killed added: " + enemyKillBonus + " points");
    }

    public void AddBonus(float amount)
    {
        if (!isGameActive) return;

        currentScore.bonusScore += amount;
        currentScore.totalScore = currentScore.distanceScore + currentScore.bonusScore;
        OnScoreUpdated?.Invoke(currentScore.totalScore);
    }

    public async void GameOver()
    {
        Debug.Log($"GameOver CALLED - Current State: " +
                  $"isGameActive: {isGameActive}, " +
                  $"Total Score: {currentScore.totalScore}, " +
                  $"Total Distance: {totalDistanceTraveled}, " +
                  $"Enemies Killed: {enemiesKilledThisRun}");

        // Capture values BEFORE resetting anything
        float finalTotalScore = currentScore.totalScore;
        float finalTotalDistance = totalDistanceTraveled;
        int finalEnemiesKilled = enemiesKilledThisRun;

        // Explicitly mark game as not active FIRST
        isGameActive = false;

        try
        {
            // Debug log to verify values before sending to UserDataManager
            Debug.Log($"GameOver - Sending to UserDataManager: " +
                      $"Score: {finalTotalScore}, " +
                      $"Distance: {finalTotalDistance}, " +
                      $"Enemies Killed: {finalEnemiesKilled}");

            // Update persistent user data
            await UserDataManager.Instance.UpdateStats(
                finalTotalScore,
                finalTotalDistance,
                finalEnemiesKilled
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating stats on game over: {e.Message}");
        }

        // Move ResetScore to a separate method or call it after a delay
        ResetScore();
    }

    public void StartNewGame()
    {
        ResetScore();
        isGameActive = true;
    }

    private void ResetScore()
    {
        totalDistanceTraveled = 0f;
        enemiesKilledThisRun = 0;
        currentScore = new PlayerScore();
        isGameActive = false;
        //// Debug to track when and where reset occurs
        //Debug.Log($"ResetScore CALLED - Current State: " +
        //          $"Total Distance: {totalDistanceTraveled}, " +
        //          $"Current Score: {currentScore.totalScore}, " +
        //          $"Enemies Killed: {enemiesKilledThisRun}");

        //// Store current score
        ////float savedTotalScore = currentScore.totalScore;
        ////float savedTotalDistance = totalDistanceTraveled;

        //// Reset everything
        //totalDistanceTraveled = 0f;
        //currentScore = new PlayerScore();

        ////// Restore only some values
        ////currentScore.totalScore = savedTotalScore;
        ////currentScore.distanceScore = savedTotalDistance * pointsPerMeter;

        //isGameActive = false;

        ////Debug.Log($"ResetScore AFTER Reset - Saved Score: {savedTotalScore}, " +
        ////          $"Saved Distance: {savedTotalDistance}, " +
        ////          $"Saved Enemies: {savedEnemiesKilled}");
    }

    public string GetFormattedScore()
    {
        return $"Score: {Mathf.Floor(currentScore.totalScore):N0}";
    }

    public string GetFormattedDistance()
    {
        return $"Distance: {totalDistanceTraveled:N1}m";
    }
}