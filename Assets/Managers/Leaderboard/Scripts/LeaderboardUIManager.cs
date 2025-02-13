using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class LeaderboardUIManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TMP_Dropdown filterDropdown;
    [SerializeField] private TextMeshProUGUI playerRankText;

    public event Action<List<GameObject>> OnLeaderBoardUpdated;

    private void Start()
    {
        InitializeDropdown();
        LeaderboardManager.Instance.OnLeaderboardUpdated += UpdateLeaderboardUI;
        RefreshLeaderboard();
    }

    private void InitializeDropdown()
    {
        filterDropdown.ClearOptions();
        filterDropdown.AddOptions(new List<string> { "Daily", "Weekly", "All Time" });
        filterDropdown.onValueChanged.AddListener(OnFilterChanged);
    }

    public void OnFilterChanged(int index)
    {
        var filter = (LeaderboardManager.TimeFilter)index;
        RefreshLeaderboard(filter);
    }

    public async void RefreshLeaderboard(LeaderboardManager.TimeFilter filter = LeaderboardManager.TimeFilter.AllTime)
    {
        await LeaderboardManager.Instance.RefreshLeaderboard(filter);
    }

    private void UpdateLeaderboardUI(List<LeaderboardManager.LeaderboardEntry> entries)
    {
        if (!entriesContainer || !entryPrefab) return; 

        // Clear existing entries
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }

        List<GameObject> children = new List<GameObject>(); 

        // Create new entries
        foreach (var entry in entries)
        {
            if (!entriesContainer) return;
            GameObject entryObject = Instantiate(entryPrefab, entriesContainer);
            var entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetEntry(entry);
            }
            children.Add(entryObject);
        }

        // Update player rank
        if (playerRankText)
        {
            int playerRank = LeaderboardManager.Instance.GetCurrentUserRank(entries);
            playerRankText.text = playerRank > 0 ? $"Your Rank: #{playerRank}" : "Play a game to get a rank...";
        }
        OnLeaderBoardUpdated?.Invoke(children); 
    }

    public void Show()
    {
        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }

    public void Hide()
    {
        leaderboardPanel.SetActive(false);
    }

    public void RefreshLeaderboardScore()
    {
        RefreshLeaderboard();
    }
}