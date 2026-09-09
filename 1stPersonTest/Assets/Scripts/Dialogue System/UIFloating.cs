using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class FloatingText : MonoBehaviour
{
    public static List<FloatingText> activeWords = new List<FloatingText>();

    public Vector2 baseSpeed = new Vector2(100f, 100f);
    public float wanderStrength = 0.5f;
    public float wanderSpeed = 0.5f;

    private RectTransform boundsRectTransform;
    private RectTransform rectTransform;
    private DraggableWord dragScript;

    private Vector2 direction;
    private float noiseOffsetX;
    private float noiseOffsetY;

    private float lastBounceTime = 0f;
    private const float bounceCooldown = 0.1f;

    void OnEnable()
    {
        activeWords.Add(this);
    }

    void OnDisable()
    {
        activeWords.Remove(this);
    }

    void Start()
    {
        boundsRectTransform = GameObject.FindWithTag("WordBankPanel")?.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        dragScript = GetComponent<DraggableWord>();

        direction = Random.insideUnitCircle.normalized;
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (dragScript != null && (dragScript.isBeingDragged || dragScript.isInSentencePanel))
            return;

        Float();
    }

    private void Float()
    {
        float time = Time.time;

        // Add wandering movement via Perlin noise
        float wanderX = Mathf.PerlinNoise(noiseOffsetX, time * wanderSpeed) - 0.5f;
        float wanderY = Mathf.PerlinNoise(noiseOffsetY, time * wanderSpeed) - 0.5f;
        Vector2 wanderOffset = new Vector2(wanderX, wanderY) * wanderStrength;

        Vector2 finalDirection = (direction + wanderOffset).normalized;
        Vector2 movement = Vector2.Scale(finalDirection, baseSpeed) * Time.deltaTime;
        rectTransform.anchoredPosition += movement;

        // Keep inside bounds and bounce
        KeepInsideBounds();

        // Check and react to collisions with other floating words
        CheckCollisions();
    }

    private void KeepInsideBounds()
    {
        if (boundsRectTransform == null) return;

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

        // Clamp inside bounds
        pos.x = Mathf.Clamp(pos.x, -bounds.x / 2 + size.x / 2, bounds.x / 2 - size.x / 2);
        pos.y = Mathf.Clamp(pos.y, -bounds.y / 2 + size.y / 2, bounds.y / 2 - size.y / 2);
        rectTransform.anchoredPosition = pos;

        if (bounced)
        {
            direction += Random.insideUnitCircle.normalized * 0.2f;
            direction.Normalize();
        }
    }

    private void CheckCollisions()
    {
        float now = Time.time;

        foreach (FloatingText other in activeWords)
        {
            if (other == this) continue;

            if (other.dragScript != null && (other.dragScript.isBeingDragged || other.dragScript.isInSentencePanel))
                continue;

            // Null checks added here to avoid NullReferenceException
            if (rectTransform == null || other.rectTransform == null)
                continue;

            if (IsOverlapping(rectTransform, other.rectTransform))
            {
                if (now - lastBounceTime < bounceCooldown)
                    continue;

                // Repel away from the other word
                Vector2 toOther = rectTransform.anchoredPosition - other.rectTransform.anchoredPosition;

                if (toOther == Vector2.zero)
                    toOther = Random.insideUnitCircle; // Prevent zero direction

                direction += toOther.normalized * 0.5f;
                direction.Normalize();

                lastBounceTime = now;
            }
        }
    }

    // Null checks added here to avoid NullReferenceException
    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
            return false;

        Vector2 aMin = a.anchoredPosition - a.rect.size / 2f;
        Vector2 aMax = a.anchoredPosition + a.rect.size / 2f;
        Vector2 bMin = b.anchoredPosition - b.rect.size / 2f;
        Vector2 bMax = b.anchoredPosition + b.rect.size / 2f;

        return !(aMax.x < bMin.x || aMin.x > bMax.x || aMax.y < bMin.y || aMin.y > bMax.y);
    }

    public void ResumeFloatingAt(Vector2 newPosition)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.anchoredPosition = newPosition;

        direction = Random.insideUnitCircle.normalized;
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
    }
}


