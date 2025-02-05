using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float effectDuration = 0.2f; 
    public float initialLength = 10f;  
    public float shrinkSpeed = 5f;

    private float timer;
    public bool isEffectActive;

    void Start()
    {
        // Ensure the LineRenderer is disabled initially
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isEffectActive)
        {
            // Update the timer
            timer -= Time.deltaTime;

            if (timer > 0)
            {
                // Shrink the line over time
                float currentLength = Mathf.Lerp(0, initialLength, timer / effectDuration);
                UpdateLineLength(currentLength);
            }
            else
            {
                // Disable the effect when the timer runs out
                isEffectActive = false;
                lineRenderer.enabled = false;
            }
        }
    }

    public void TriggerEffect(Vector2 startPosition, Vector2 direction)
    {
        // Set the start and end positions of the line
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition + direction * initialLength);

        lineRenderer.enabled = true;

        timer = effectDuration;
        isEffectActive = true;
    }

    private void UpdateLineLength(float length)
    {
        // Get the end position (this will stay fixed)
        Vector2 endPosition = lineRenderer.GetPosition(1);
        Vector2 direction = ( lineRenderer.GetPosition(0) - (Vector3)endPosition).normalized;

        // Calculate new start position by moving from the end position backwards
        Vector2 newStartPos = endPosition - (direction * length);

        // Update the start position instead of the end position
        lineRenderer.SetPosition(0, newStartPos);
    }
}