using Ink.Parsed;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    DraggableWord dragScript;

    public RectTransform boundsRectTransform; // Assign in inspector (the panel)
    public Vector2 baseSpeed = new Vector2(100f, 100f);
    public float wanderStrength = 0.5f; // How strong the wandering effect is
    public float wanderSpeed = 0.5f;    // How quickly it changes

    private RectTransform rectTransform;
    private Vector2 direction;

    private float noiseOffsetX;
    private float noiseOffsetY;

    void Start()
    {
        dragScript = GetComponent<DraggableWord>();

        rectTransform = GetComponent<RectTransform>();
        direction = Random.insideUnitCircle.normalized;

        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (dragScript != null)
        {
            if (dragScript.isBeingDragged || dragScript.isInSentencePanel)
            {
                return;
            }
        }
        Float();
    }

    private void Float()
    {

        float time = Time.time;

        // Add smooth wandering using Perlin noise
        float wanderX = Mathf.PerlinNoise(noiseOffsetX, time * wanderSpeed) - 0.5f;
        float wanderY = Mathf.PerlinNoise(noiseOffsetY, time * wanderSpeed) - 0.5f;
        Vector2 wanderOffset = new Vector2(wanderX, wanderY) * wanderStrength;

        // Final direction with wandering
        Vector2 finalDirection = (direction + wanderOffset).normalized;

        // Move the object
        Vector2 movement = Vector2.Scale(finalDirection, baseSpeed) * Time.deltaTime;
        rectTransform.anchoredPosition += movement;

        // Check bounds
        Vector2 pos = rectTransform.anchoredPosition;
        Vector2 size = rectTransform.rect.size;
        Vector2 bounds = boundsRectTransform.rect.size;

        bool bounced = false;

        if (pos.x - size.x / 2 < -bounds.x / 2 || pos.x + size.x / 2 > bounds.x / 2)
        {
            direction.x *= -1;
            bounced = true;
        }

        if (pos.y - size.y / 2 < -bounds.y / 2 || pos.y + size.y / 2 > bounds.y / 2)
        {
            direction.y *= -1;
            bounced = true;
        }

        // Clamp to stay inside
        pos.x = Mathf.Clamp(pos.x, -bounds.x / 2 + size.x / 2, bounds.x / 2 - size.x / 2);
        pos.y = Mathf.Clamp(pos.y, -bounds.y / 2 + size.y / 2, bounds.y / 2 - size.y / 2);
        rectTransform.anchoredPosition = pos;

        // Optional: randomize direction slightly after a bounce
        if (bounced)
        {
            direction += Random.insideUnitCircle.normalized * 0.2f;
            direction.Normalize();
        }
    }

    public void ResumeFloatingAt(Vector2 newPosition)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.anchoredPosition = newPosition;

        // Reset movement direction and noise offsets
        direction = Random.insideUnitCircle.normalized;

        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
    }

}


