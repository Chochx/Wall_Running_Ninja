using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D swordCollider;
    [SerializeField] private GameObject bloodPrefab;

    public event System.Action onEndAnimation;

    private void Start()
    {
        // Ensure collider starts disabled
        swordCollider.enabled = false;
    }

    // Called by animation events
    public void EnableSwordCollider()
    {
        swordCollider.enabled = true;
    }

    public void DisableSwordCollider()
    {
        swordCollider.enabled = false;
    }

    public void EndingAnimation(AnimationClip anim)
    {
        Debug.Log($"<color=red>Animation {anim.name}</color> has ended. Sending event.");

        onEndAnimation?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && swordCollider.IsTouching(other))
        {
            Instantiate(bloodPrefab, transform.position, Quaternion.identity);
            ScoreManager.Instance.OnEnemyKilled();
            Destroy(other.gameObject);
        }
    }
}
