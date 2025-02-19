using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel; 
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private LeaderboardUIManager leaderboardUIManager;
    [SerializeField] private GameObject settingsPanel;
    
    public void OnPauseButtonPressed()
    {
        Time.timeScale = 0; 
        menuPanel.SetActive(true);
    }

    public void OnResumeButtonPressed()
    {
        Time.timeScale = 1;
        menuPanel.SetActive(false);
    }

    public void OnQuitButtonPressed()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }

    public void OnPlayButtonPressed()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(1);
    }

    public void OnRetryButtonPressed()
    {
        Time.timeScale = 1;
        DifficultyManager.Instance.ResetDifficulty();
        DifficultyManager.Instance.StartDifficulty();
        SceneManager.LoadSceneAsync(1);
    }

    public void OnSettingsButtonPressed() 
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void OnStatsButtonPressed()
    {

    }

    public void OnLeaderBoardButtonPressed()
    {
        leaderboardUIManager.Show();
    }
}
