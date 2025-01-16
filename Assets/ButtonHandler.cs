using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel; 
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

    public void OnSettingsButtonPressed() 
    {

    }
}
