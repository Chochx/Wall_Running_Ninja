using Firebase.Database;
using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;

[Serializable]
public class UserData
{
    public string userId;
    public string username;
    public float highScore;
    public int totalGamesPlayed;
    public float totalDistance;
    public int totalEnemiesKilled;
    public string lastPlayed;

    public UserData(string userId)
    {
        this.userId = userId;
        this.username = ""; 
        highScore = 0;
        totalGamesPlayed = 0;
        totalDistance = 0;
        totalEnemiesKilled = 0;
        lastPlayed = DateTime.Now.ToString("O");
    }
}

public class UserDataManager : MonoBehaviour
{
    private static UserDataManager _instance;
    public static UserDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UserDataManager");
                _instance = go.AddComponent<UserDataManager>();
            }
            return _instance;
        }
    }
    public event System.Action<bool> OnUsernameCheckCompleted;

    private DatabaseReference dbReference;
    private UserData currentUserData;
    private const int MIN_USERNAME_LENGTH = 3;
    private const int MAX_USERNAME_LENGTH = 20;
    public enum UsernameCheckResult
    {
        Valid,
        TooShort,
        TooLong,
        InvalidCharacters,
        AlreadyTaken
    }

    public event Action<UserData> OnUserDataUpdated;

    private void Awake()
    {
        // Ensure singleton
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

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        InitializeFirebaseReferences();
        // Wait for authentication
        AuthManager.Instance.OnUserAuthenticated += LoadUserData;
    }

    public bool HasUsername()
    {
        return currentUserData != null && !string.IsNullOrEmpty(currentUserData.username);
    }

    public async Task<UsernameCheckResult> SetUsername(string newUsername)
    {
        // Check if database reference is initialized
        if (dbReference == null)
        {
            Debug.LogError("Database reference is null. Attempting to reinitialize...");
            InitializeFirebaseReferences();

            if (dbReference == null)
            {
                Debug.LogError("Failed to initialize database reference");
                return UsernameCheckResult.AlreadyTaken;
            }
        }

        // Validate Auth Manager
        if (AuthManager.Instance == null)
        {
            Debug.LogError("AuthManager instance is null");
            return UsernameCheckResult.AlreadyTaken;
        }

        // Get current user ID
        string userId = AuthManager.Instance.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty");
            return UsernameCheckResult.AlreadyTaken;
        }

        Debug.Log($"Current User ID: {userId}");

        // Initialize currentUserData if null
        if (currentUserData == null)
        {
            Debug.Log("CurrentUserData is null, creating new instance...");
            currentUserData = new UserData(userId);
        }

        try
        {
            // Basic validation checks
            if (string.IsNullOrEmpty(newUsername))
            {
                Debug.LogError("Username is null or empty");
                return UsernameCheckResult.TooShort;
            }

            Debug.Log($"Checking username: {newUsername}");
            Debug.Log($"Current user data - UserId: {currentUserData.userId}, Current username: {currentUserData.username}");

            // Check if username exists in database
            var snapshot = await dbReference.Child("usernames")
                                          .Child(newUsername.ToLower())
                                          .GetValueAsync();

            Debug.Log($"Username check snapshot exists: {snapshot?.Exists}");

            if (snapshot != null && snapshot.Exists)
            {
                string existingUserId = snapshot.Value?.ToString();
                Debug.Log($"Existing userId for username: {existingUserId}");

                if (!string.IsNullOrEmpty(existingUserId) && existingUserId != currentUserData.userId)
                {
                    Debug.Log("Username is taken by another user");
                    return UsernameCheckResult.AlreadyTaken;
                }
            }

            // Store old username for cleanup
            string oldUsername = currentUserData.username;
            Debug.Log($"Old username: {oldUsername}");

            // Update the username
            currentUserData.username = newUsername;

            // Save to Firebase
            await SaveUserData();
            Debug.Log("Saved user data successfully");

            // Remove old username reservation if it existed
            if (!string.IsNullOrEmpty(oldUsername))
            {
                try
                {
                    await dbReference.Child("usernames")
                                    .Child(oldUsername.ToLower())
                                    .RemoveValueAsync();
                    Debug.Log($"Removed old username: {oldUsername}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to remove old username: {e.Message}");
                }
            }

            // Reserve new username
            try
            {
                await dbReference.Child("usernames")
                                .Child(newUsername.ToLower())
                                .SetValueAsync(currentUserData.userId);
                Debug.Log($"Reserved new username: {newUsername}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to reserve new username: {e.Message}");
                // Try to rollback
                currentUserData.username = oldUsername;
                await SaveUserData();
                return UsernameCheckResult.AlreadyTaken;
            }

            return UsernameCheckResult.Valid;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set username: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            return UsernameCheckResult.AlreadyTaken;
        }
    }

    public string GetUsername()
    {
        return currentUserData?.username ?? "";
    }

    private async void LoadUserData(string userId)
    {
        try
        {
            var snapshot = await dbReference.Child("users")
                                          .Child(userId)
                                          .GetValueAsync();

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                currentUserData = JsonUtility.FromJson<UserData>(json);

                // Check if username exists
                bool hasUsername = !string.IsNullOrEmpty(currentUserData.username);
                OnUsernameCheckCompleted?.Invoke(hasUsername);

                if (hasUsername)
                {
                    await dbReference.Child("usernames")
                                    .Child(currentUserData.username.ToLower())
                                    .SetValueAsync(userId);
                }
            }
            else
            {
                currentUserData = new UserData(userId);
                await SaveUserData();
                OnUsernameCheckCompleted?.Invoke(false); // New user, no username
            }

            OnUserDataUpdated?.Invoke(currentUserData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load user data: {e.Message}");
            OnUsernameCheckCompleted?.Invoke(false); // Error case, assume no username
        }
    }


private async Task SaveUserData()
    {
        if (currentUserData == null) return;
        try
        {
            // Ensure userId is set
            if (string.IsNullOrEmpty(currentUserData.userId))
            {
                currentUserData.userId = AuthManager.Instance.GetUserId();
            }

            // Serialize entire object to ensure all fields are saved
            string json = JsonUtility.ToJson(currentUserData);

            await dbReference.Child("users")
                            .Child(currentUserData.userId)
                            .SetRawJsonValueAsync(json);

            Debug.Log("User data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save user data: {e.Message}");
        }
    }

    private void InitializeFirebaseReferences()
    {
        try
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Firebase Database Reference: {e.Message}");
        }
    }

    public async Task UpdateStats(float score, float distance, int enemiesKilled)
    {
        try
        {
            string userId = AuthManager.Instance?.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("Cannot update stats: No authenticated user");
                return;
            }

            Debug.Log($"UpdateStats RECEIVED - Score: {score}, Distance: {distance}, Enemies: {enemiesKilled}");

            // Reload the latest user data from Firebase before updating
            await ReloadUserData();

            if (currentUserData == null)
            {
                // If no existing data, create new user data
                currentUserData = new UserData(userId);
            }

            // Always update these values
            currentUserData.totalGamesPlayed++;
            currentUserData.totalDistance = Mathf.Max(currentUserData.totalDistance, distance);
            currentUserData.totalEnemiesKilled += enemiesKilled;

            // Update high score if new score is higher
            if (score > currentUserData.highScore)
            {
                currentUserData.highScore = score;
            }

            // Update last played time
            currentUserData.lastPlayed = DateTime.Now.ToString("O");

            Debug.Log($"UpdateStats BEFORE SAVE - Current Data: " +
                      $"Games: {currentUserData.totalGamesPlayed}, " +
                      $"Distance: {currentUserData.totalDistance}, " +
                      $"High Score: {currentUserData.highScore}, " +
                      $"Enemies: {currentUserData.totalEnemiesKilled}");

            await SaveUserData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update stats: {e.Message}");
        }
    }

    private async Task ReloadUserData()
    {
        try
        {
            string userId = AuthManager.Instance?.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("Cannot reload user data: No authenticated user");
                return;
            }

            if (dbReference == null)
            {
                InitializeFirebaseReferences();
                if (dbReference == null)
                {
                    Debug.LogError("Database reference is null");
                    return;
                }
            }

            var snapshot = await dbReference.Child("users")
                                          .Child(userId)
                                          .GetValueAsync();

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                currentUserData = JsonUtility.FromJson<UserData>(json);
            }
            else
            {
                // Create new user data if none exists
                currentUserData = new UserData(userId);
                await SaveUserData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to reload user data: {e.Message}");
        }
    }

    public UserData GetCurrentUserData()
    {
        return currentUserData;
    }
}