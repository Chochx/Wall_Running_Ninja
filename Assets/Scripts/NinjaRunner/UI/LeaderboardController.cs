using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


public class LeaderboardController : MonoBehaviour
{
    private float minPositionY;
    private float maxPositionY;
    private float maxPositionYOffset = 2; 
    private int lastChild;
    private Vector2 originalPos;
    private bool hasUpdatedLeaderboard; 

    LeaderboardUIManager leaderboardUiManager;

    private List<Transform> children = new List<Transform>();
    private void Awake()
    {
        EnhancedTouchSupport.Enable(); 
    }

    private void Start()
    {
        maxPositionY = Camera.main.ScreenToWorldPoint(new Vector3(0, maxPositionYOffset, 0)).y;
        originalPos = transform.position;
        if (leaderboardUiManager == null)
        {
            leaderboardUiManager = FindFirstObjectByType<LeaderboardUIManager>();
        }
        
        leaderboardUiManager.OnLeaderBoardUpdated += GetChildrenOfThisObject;
    }

    void Update()
    {
        foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches) {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    HandleTouchStart(touch);
                    break; 
                case UnityEngine.InputSystem.TouchPhase.Moved:
                    HandleTouchMoved(touch);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                    HandleTouchEnded(touch);
                    break;
            }
        }
        if (hasUpdatedLeaderboard)
        {
            CheckPositionOfLeaderboard();
        }
    }

    private void CheckPositionOfLeaderboard()
    {
        if (transform.position.y < originalPos.y)
        {
            transform.position = originalPos;
        }

        if (children[lastChild].transform.position.y > maxPositionY && children.Count > 10)
        {
            float difference = (children[lastChild].transform.position.y - maxPositionY);
            transform.position = new Vector3(transform.position.x, transform.position.y - difference, transform.position.z);
        }

    }

    private void HandleTouchEnded(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        
    }

    private void HandleTouchMoved(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {       
        Vector2 delta = touch.delta;

        Vector3 panelYpos = new(0,delta.y);
        transform.position += panelYpos; 
    }

    private void HandleTouchStart(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        
    }
    
    private void GetChildrenOfThisObject(List<GameObject> entries)
    {
        children.Clear();
        foreach (GameObject entry in entries)
        {
            children.Add(entry.transform);
        }
        if (children.Count > 0)
        {
            minPositionY = children[0].transform.position.y;
            lastChild = children.Count - 1; 

        }
        transform.position = originalPos;
        hasUpdatedLeaderboard = true;
    }

    private void OnDestroy()
    {
        leaderboardUiManager.OnLeaderBoardUpdated -= GetChildrenOfThisObject;
    }
}
