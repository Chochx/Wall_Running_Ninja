using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string userId;
        public string username;
        public float score;
        public int rank;
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
        if (AuthManager.Instance == null)
        {
            Debug.LogError("AuthManager instance is null");
            return;
        }

        dbReference = AuthManager.Instance.GetDatabaseReference();
        Debug.Log($"Database reference initialized: {dbReference != null}");
    }

    public async Task RefreshLeaderboard(TimeFilter timeFilter)
    {
        try
        {
            if (dbReference == null)
            {
                Initialize();
                if (dbReference == null) throw new Exception("Database reference initialization failed");
            }

            string scoreField = timeFilter switch
            {
                TimeFilter.Daily => "dailyBestScore",
                TimeFilter.Weekly => "weeklyBestScore",
                _ => "highScore"
            };

            var snapshot = await dbReference.Child("users").OrderByChild(scoreField).LimitToLast(50).GetValueAsync();
            if (snapshot == null) throw new Exception("Database snapshot is null");

            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

            foreach (var userSnapshot in snapshot.Children.Reverse())
            {
                if (userSnapshot == null) continue;
                string rawJson = userSnapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(rawJson)) continue;

                UserData userData = JsonUtility.FromJson<UserData>(rawJson);
                if (userData == null) continue;

                float relevantScore = GetRelevantScore(userData, timeFilter);
                if (relevantScore <= 0) continue;

                entries.Add(new LeaderboardEntry
                {
                    userId = userData.userId,
                    username = string.IsNullOrEmpty(userData.username) ? "Anonymous" : userData.username,
                    score = relevantScore
                });
            }

            entries = entries.OrderByDescending(e => e.score).Take(50).ToList();
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].rank = i + 1;
            }

            OnLeaderboardUpdated?.Invoke(entries);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to refresh leaderboard: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private float GetRelevantScore(UserData userData, TimeFilter timeFilter)
    {
        if (userData == null) return 0;

        try
        {
            switch (timeFilter)
            {
                case TimeFilter.Daily:
                    if (string.IsNullOrEmpty(userData.dailyBestDate)) return 0;
                    if (DateTime.Parse(userData.dailyBestDate).Date == DateTime.UtcNow.Date)
                        return userData.dailyBestScore;
                    break;

                case TimeFilter.Weekly:
                    if (string.IsNullOrEmpty(userData.weeklyBestDate)) return 0;
                    var scoreDate = DateTime.Parse(userData.weeklyBestDate);
                    var weekNumber = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.UtcNow);
                    var scoreWeekNumber = System.Globalization.ISOWeek.GetWeekOfYear(scoreDate);
                    if (weekNumber == scoreWeekNumber)
                        return userData.weeklyBestScore;
                    break;

                case TimeFilter.AllTime:
                    return userData.highScore;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error parsing date for user {userData.userId}: {e.Message}");
        }
        return 0;
    }

    public int GetCurrentUserRank(List<LeaderboardEntry> entries)
    {
        string currentUserId = AuthManager.Instance.GetUserId();
        var userEntry = entries.FirstOrDefault(n => n.userId == currentUserId);
        return userEntry?.rank ?? -1;
    }
}