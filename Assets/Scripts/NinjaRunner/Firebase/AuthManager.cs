using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class AuthManager : MonoBehaviour
{
    // Singleton instance
    private static AuthManager _instance;
    public static AuthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AuthManager");
                _instance = go.AddComponent<AuthManager>();
            }
            return _instance;
        }
    }

    // Firebase references
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private string userId;
    private bool isInitialized = false;

    // Constants
    private const string DEVICE_ID_KEY = "device_id";

    // Events that other scripts can subscribe to
    public event Action OnFirebaseInitialized;
    public event Action<string> OnUserAuthenticated;
    public event Action<string> OnAuthenticationFailed;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebaseAsync();
    }

    private async void InitializeFirebaseAsync()
    {
        try
        {
            // Initialize Firebase
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                OnAuthenticationFailed?.Invoke("Failed to initialize Firebase dependencies");
                return;
            }

            // Get Firebase instances
            auth = FirebaseAuth.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            isInitialized = true;
            OnFirebaseInitialized?.Invoke();

            // Start authentication only after Firebase is initialized
            await AuthenticateUser();
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase initialization failed: {e.Message}");
            OnAuthenticationFailed?.Invoke("Failed to initialize Firebase");
        }
    }

    public bool IsFirebaseInitialized()
    {
        return isInitialized;
    }

    public DatabaseReference GetDatabaseReference()
    {
        if (!isInitialized)
        {
            Debug.LogError("Trying to access database before Firebase initialization!");
            return null;
        }
        return dbReference;
    }

    private async Task AuthenticateUser()
    {
        try
        {
            // Get the device ID
            string deviceId = GetDeviceId();
            Debug.Log($"Device ID: {deviceId}");

            // Sign in anonymously with Firebase
            var authResult = await auth.SignInAnonymouslyAsync();
            userId = authResult.User.UserId;

            // Store the device-to-user mapping
            await SaveDeviceMapping(deviceId, userId);

            Debug.Log($"Authentication successful. User ID: {userId}");
            
            OnUserAuthenticated?.Invoke(userId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Authentication failed: {e.Message}");
            OnAuthenticationFailed?.Invoke(e.Message);
        }
    }

    private string GetDeviceId()
    {
        // Try to get existing device ID
        string deviceId = PlayerPrefs.GetString(DEVICE_ID_KEY, "");

        if (string.IsNullOrEmpty(deviceId))
        {
            // Generate new device ID based on platform and available information
            deviceId = SystemInfo.deviceUniqueIdentifier;

            // If SystemInfo.deviceUniqueIdentifier returns empty or a default value
            if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
            {
                // Create a composite ID using multiple device characteristics
                string deviceModel = SystemInfo.deviceModel;
                string operatingSystem = SystemInfo.operatingSystem;
                string processorType = SystemInfo.processorType;

                // Combine these values to create a unique identifier
                string combinedInfo = $"{deviceModel}-{operatingSystem}-{processorType}";

                // Generate a deterministic GUID based on the combined info
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(combinedInfo);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                deviceId = new Guid(hashBytes).ToString();
            }

            // Save the device ID
            PlayerPrefs.SetString(DEVICE_ID_KEY, deviceId);
            PlayerPrefs.Save();

            Debug.Log($"Generated new device ID: {deviceId}");
        }

        return deviceId;
    }

    private async Task SaveDeviceMapping(string deviceId, string userId)
    {
        try
        {
            // Create device info object
            var deviceInfo = new DeviceInfo
            {
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                lastLogin = DateTime.UtcNow.ToString("o"),
                userId = userId
            };

            // Save device mapping
            await dbReference.Child("device_mappings")
                           .Child(deviceId)
                           .SetValueAsync(userId);

            // Save device info
            string deviceInfoJson = JsonUtility.ToJson(deviceInfo);
            await dbReference.Child("devices")
                           .Child(deviceId)
                           .SetRawJsonValueAsync(deviceInfoJson);

            Debug.Log("Device mapping saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to store device mapping: {e.Message}");
            // Non-critical error, we can continue from here
        }
    }

    public string GetUserId()
    {
        // Return cached ID if available, otherwise get from current user
        if (string.IsNullOrEmpty(userId) && auth?.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
        }
        return userId;
    }

    public bool IsAuthenticated()
    {
        return auth?.CurrentUser != null;
    }

    public async Task ReauthenticateAsync()
    {
        await AuthenticateUser();
    }
}

// Class to store device information
[Serializable]
public class DeviceInfo
{
    public string deviceModel;
    public string operatingSystem;
    public string lastLogin;
    public string userId;
}