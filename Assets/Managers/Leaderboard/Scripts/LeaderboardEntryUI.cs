using UnityEngine;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetEntry(LeaderboardManager.LeaderboardEntry entry)
    {
        if (entry == null || rankText == null || usernameText == null || scoreText == null) return;

        rankText.text = $"#{entry.rank}";
        usernameText.text = string.IsNullOrEmpty(entry.username) ? "Anonymous" : entry.username;
        scoreText.text = entry.score.ToString("N0");

        string currentUserId = AuthManager.Instance?.GetUserId();
        bool isCurrentUser = !string.IsNullOrEmpty(currentUserId) && entry.userId == currentUserId;
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = isCurrentUser ? new Color(1, 1, 0.8f) : Color.white;
        }
    }
}