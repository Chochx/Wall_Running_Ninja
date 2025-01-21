using UnityEngine;
using UnityEngine.Events;

public class UsernameFlowManager : MonoBehaviour
{
    [SerializeField] private GameObject usernameInputPanel;
    [SerializeField] private GameObject mainGameUI;
    [SerializeField] private GameObject loadingPanel;

    private static UsernameFlowManager _instance;

    public static UsernameFlowManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UsernameFlowManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        // Subscribe to events
        AuthManager.Instance.OnFirebaseInitialized += HandleFirebaseInitialized;
        AuthManager.Instance.OnUserAuthenticated += HandleUserAuthenticated;
        AuthManager.Instance.OnAuthenticationFailed += HandleAuthenticationFailed;
        UserDataManager.Instance.OnUsernameCheckCompleted += HandleUsernameCheck;

        // Initialize UI state - moved to Start to ensure all components are ready
        InitializeUIState();
    }

    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnFirebaseInitialized -= HandleFirebaseInitialized;
            AuthManager.Instance.OnUserAuthenticated -= HandleUserAuthenticated;
            AuthManager.Instance.OnAuthenticationFailed -= HandleAuthenticationFailed;
        }

        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnUsernameCheckCompleted -= HandleUsernameCheck;
        }
    }

    private void InitializeUIState()
    {
        // First ensure everything is hidden
        usernameInputPanel.SetActive(false);
        mainGameUI.SetActive(false);
        loadingPanel.SetActive(true);  // Show loading while we check state

        // If Firebase is ready, check username immediately
        if (AuthManager.Instance != null && AuthManager.Instance.IsFirebaseInitialized() && UserDataManager.Instance != null)
        {
            string currentUsername = UserDataManager.Instance.GetUsername();
            HandleUsernameCheck(!string.IsNullOrEmpty(currentUsername));
        }
    }

    private void HandleFirebaseInitialized()
    {
        Debug.Log("Firebase initialized");
        if (UserDataManager.Instance != null)
        {
            string currentUsername = UserDataManager.Instance.GetUsername();
            HandleUsernameCheck(!string.IsNullOrEmpty(currentUsername));
        }
    }

    private void HandleUserAuthenticated(string userId)
    {
        Debug.Log($"User authenticated with ID: {userId}");
        // UserDataManager will automatically load data and trigger OnUsernameCheckCompleted
    }

    private void HandleAuthenticationFailed(string error)
    {
        Debug.LogError($"Authentication failed: {error}");
        loadingPanel.SetActive(false);
        // You might want to show an error UI here
    }

    public void HandleUsernameCheck(bool hasUsername)
    {
        Debug.Log($"Username check completed. Has username: {hasUsername}");

        loadingPanel.SetActive(false);

        if (hasUsername)
        {
            ShowMainGame();
        }
        else
        {
            ShowUsernameInput();
        }
    }

    public void ShowMainGame()
    {
        Debug.Log("Showing main game UI");
        usernameInputPanel.SetActive(false);
        loadingPanel.SetActive(false);
        mainGameUI.SetActive(true);
    }

    public void ShowUsernameInput()
    {
        Debug.Log("Showing username input UI");
        mainGameUI.SetActive(false);
        loadingPanel.SetActive(false);
        usernameInputPanel.SetActive(true);
    }
}