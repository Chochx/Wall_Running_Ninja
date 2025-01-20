using UnityEngine;
using UnityEngine.Events;
public class UsernameFlowManager : MonoBehaviour
{
    [SerializeField] private GameObject usernameInputPanel;
    [SerializeField] private GameObject mainGameUI;

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
        // Initially hide both UIs
        usernameInputPanel.SetActive(false);
        mainGameUI.SetActive(false);

        // Subscribe to the username check event
        UserDataManager.Instance.OnUsernameCheckCompleted += HandleUsernameCheck;

        // Check username state immediately
        CheckUsernameState();
    }

    private void OnEnable()
    {
        // Also check when object is enabled (happens on scene load)
        CheckUsernameState();
    }

    private void OnDestroy()
    {
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnUsernameCheckCompleted -= HandleUsernameCheck;
        }
    }

    private void CheckUsernameState()
    {
        if (UserDataManager.Instance != null)
        {
            string currentUsername = UserDataManager.Instance.GetUsername();
            HandleUsernameCheck(!string.IsNullOrEmpty(currentUsername));
        }
    }

    public void HandleUsernameCheck(bool hasUsername)
    {
        Debug.Log($"Username check completed. Has username: {hasUsername}");
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
        mainGameUI.SetActive(true);
    }

    public void ShowUsernameInput()
    {
        Debug.Log("Showing username input UI");
        usernameInputPanel.SetActive(true);
        mainGameUI.SetActive(false);
    }
}
