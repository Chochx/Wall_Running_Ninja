using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string username;
    public float highScore;
    public string lastPlayed;
    public float totalDistance;
    public int totalEnemiesKilled;
}

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager _instance;
    public static LeaderboardManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("LeaderboardManager");
                _instance = go.AddComponent<LeaderboardManager>();
            }
            return _instance;
        }
    }

    private DatabaseReference dbReference;
    public event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;

    public enum TimeFilter
    {
        Daily,
        Weekly,
        AllTime
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AuthManager.Instance.OnFirebaseInitialized += Initialize;
    }

    private void Initialize()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboard(TimeFilter timeFilter, int limit = 50)
    {
        try
        {
            DatabaseReference scoresRef = dbReference.Child("users");
            Query filteredQuery;

            DateTime now = DateTime.UtcNow;
            string startTime = "";

            switch (timeFilter)
            {
                case TimeFilter.Daily:
                    startTime = now.Date.ToString("O");
                    filteredQuery = scoresRef.OrderByChild("lastPlayed").StartAt(startTime);
                    break;
                case TimeFilter.Weekly:
                    startTime = now.AddDays(-7).ToString("O");
                    filteredQuery = scoresRef.OrderByChild("lastPlayed").StartAt(startTime);
                    break;
                default: // AllTime
                    filteredQuery = scoresRef.OrderByChild("highScore");
                    break;
            }

            // Add limit and order by high score
            filteredQuery = filteredQuery.LimitToLast(limit);
            var snapshot = await filteredQuery.GetValueAsync();

            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

            foreach (var child in snapshot.Children)
            {
                try
                {
                    var userData = JsonUtility.FromJson<UserData>(child.GetRawJsonValue());

                    // Skip entries with no username
                    if (string.IsNullOrEmpty(userData.username)) continue;

                    var entry = new LeaderboardEntry
                    {
                        userId = userData.userId,
                        username = userData.username,
                        highScore = userData.highScore,
                        lastPlayed = userData.lastPlayed,
                        totalDistance = userData.totalDistance,
                        totalEnemiesKilled = userData.totalEnemiesKilled
                    };

                    entries.Add(entry);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing leaderboard entry: {e.Message}");
                }
            }

            // Sort by high score in descending order
            entries.Sort((a, b) => b.highScore.CompareTo(a.highScore));

            OnLeaderboardUpdated?.Invoke(entries);
            return entries;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching leaderboard: {e.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    public async Task RefreshLeaderboard(TimeFilter timeFilter)
    {
        await GetLeaderboard(timeFilter);
    }
}
