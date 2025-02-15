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
            auth.StateChanged += AuthStateChanged;
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

    private void AuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != null && string.IsNullOrEmpty(userId))
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Auth state changed. Current user: {userId}");
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
            // Check if we already have a user
            if (auth.CurrentUser != null)
            {
                userId = auth.CurrentUser.UserId;
                Debug.Log($"Using existing auth. User ID: {userId}");
                OnUserAuthenticated?.Invoke(userId);
                return;
            }

            // No existing user, create new anonymous user
            var authResult = await auth.SignInAnonymouslyAsync();
            userId = authResult.User.UserId;

            Debug.Log($"New authentication successful. User ID: {userId}");
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
#if UNITY_WEBGL
            // For web builds, generate a random GUID and store it
            deviceId = Guid.NewGuid().ToString();
#elif UNITY_IOS && !UNITY_EDITOR
            // Use IDFV for iOS
            deviceId = UnityEngine.iOS.Device.vendorIdentifier;
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            // For PC builds, use deviceUniqueIdentifier but ensure it's stored
            deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
            {
                deviceId = Guid.NewGuid().ToString();
            }
#else
            // Use deviceUniqueIdentifier for Android and other platforms
            deviceId = SystemInfo.deviceUniqueIdentifier;
#endif

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