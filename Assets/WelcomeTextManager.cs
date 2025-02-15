using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WelcomeTextManager : MonoBehaviour
{
    private TextMeshProUGUI welcomeText;
    
    void Start()
    {
        welcomeText = GetComponent<TextMeshProUGUI>();
        welcomeText.text = $"Welcome ninja: {UserDataManager.Instance.GetUsername()}";
    }

    
}
