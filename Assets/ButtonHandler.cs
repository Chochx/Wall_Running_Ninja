using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel; 
    public void OnSettingsButtonPressed()
    {
        Time.timeScale = 0; 
        menuPanel.SetActive(true);
    }

    public void OnResumeButtonPressed()
    {
        Time.timeScale = 1;
        menuPanel.SetActive(false);
    }
}
