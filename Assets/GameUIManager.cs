using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameUIManager : MonoBehaviour
{
    [Header("In-Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text enemyKillsText;
    [SerializeField] private GameObject scorePanel;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text totalGamesText;
    [SerializeField] private TMP_Text totalDistanceText;
    [SerializeField] private TMP_Text totalKillsText;

    [Header("High Score UI")]
    [SerializeField] private GameObject highScorePanel; 

    private void Start()
    {
        // Subscribe to score events
        ScoreManager.Instance.OnScoreUpdated += UpdateScoreUI;
        ScoreManager.Instance.OnDistanceUpdated += UpdateDistanceUI;
        ScoreManager.Instance.OnKillsUpdated += UpdateKills; 
        
    }

    private void Update()
    {
        if (ScoreManager.Instance.isGameActive)
        {
            scorePanel.SetActive(true);
        }else {scorePanel.SetActive(false); }   
    }
    private void UpdateScoreUI(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = scoreText.text;
        }
    }

    private void UpdateDistanceUI(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {distance:N1}m";
        }

        if (totalDistanceText != null)
        {
            totalDistanceText.text = distanceText.text;
        }
    }

    private void UpdateKills(int kills)
    {
        int killsThisRun = kills;
        totalKillsText.text = $"Kills: {kills:N0}";
    }
    

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreUpdated -= UpdateScoreUI;
            ScoreManager.Instance.OnDistanceUpdated -= UpdateDistanceUI;
        }
    }
}
