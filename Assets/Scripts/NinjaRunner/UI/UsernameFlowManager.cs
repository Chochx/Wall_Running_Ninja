using UnityEngine;
using UnityEngine.Events;

public class UsernameFlowManager : MonoBehaviour
{
    [SerializeField] private GameObject usernameInputPanel;
    [SerializeField] private GameObject mainGameUI;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject welcomeText;

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

        if (AuthManager.Instance.IsFirebaseInitialized() && UserDataManager.Instance.HasUsername())
        {
            ShowMainGame();
        }
        else
        {
            InitializeUIState();
        }
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
    }

    private void HandleFirebaseInitialized()
    {
        Debug.Log("Firebase initialized");
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
        welcomeText.SetActive(true);
    }

    public void ShowUsernameInput()
    {
        Debug.Log("Showing username input UI");
        mainGameUI.SetActive(false);
        welcomeText.SetActive(false);
        loadingPanel.SetActive(false);
        usernameInputPanel.SetActive(true);
    }
}