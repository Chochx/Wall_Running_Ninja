using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UsernameInputManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isProcessing = false;

    private void Start()
    {
        string currentUsername = UserDataManager.Instance.GetUsername();
        if (!string.IsNullOrEmpty(currentUsername))
        {
            usernameInput.text = currentUsername;
        }

        submitButton.onClick.AddListener(HandleUsernameSubmit);
        usernameInput.onValueChanged.AddListener(OnUsernameInputChanged);
    }

    private void OnUsernameInputChanged(string value)
    {
        statusText.text = "";

        if (value.Length < 3)
        {
            statusText.text = "Username must be at least 3 characters";
            statusText.color = Color.red;
            submitButton.interactable = false;
        }
        else if (value.Length > 10)
        {
            statusText.text = "Username must be less than 10 characters";
            statusText.color = Color.red;
            submitButton.interactable = false;
        }
        else
        {
            submitButton.interactable = true;
        }
    }

    private async void HandleUsernameSubmit()
    {
        if (isProcessing) return;

        isProcessing = true;
        submitButton.interactable = false;
        statusText.text = "Checking username...";
        statusText.color = Color.yellow;

        UserDataManager.UsernameCheckResult result =
            await UserDataManager.Instance.SetUsername(usernameInput.text);

        switch (result)
        {
            case UserDataManager.UsernameCheckResult.Valid:
                statusText.text = "Username set successfully!";
                statusText.color = Color.green;

                // Small delay to show success message before transitioning
                await Task.Delay(500);

                // Transition to main game
                UsernameFlowManager.Instance.ShowMainGame();
                break;

            case UserDataManager.UsernameCheckResult.TooShort:
                statusText.text = "Username is too short";
                statusText.color = Color.red;
                break;

            case UserDataManager.UsernameCheckResult.TooLong:
                statusText.text = "Username is too long";
                statusText.color = Color.red;
                break;

            case UserDataManager.UsernameCheckResult.InvalidCharacters:
                statusText.text = "Username can only contain letters, numbers, and underscores";
                statusText.color = Color.red;
                break;

            case UserDataManager.UsernameCheckResult.AlreadyTaken:
                statusText.text = "Username is already taken";
                statusText.color = Color.red;
                break;
        }

        isProcessing = false;
        submitButton.interactable = true;
    }
}