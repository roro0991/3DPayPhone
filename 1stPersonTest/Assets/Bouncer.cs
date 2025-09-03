using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public List<Bouncer> allBouncers = new List<Bouncer>();

    public bool isBeingDragged = false;
    public float speed = 200f;
    public float driftStrength = 0.5f; // How much the direction can change per second

    public Vector2 direction;
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    public float collisionCooldown = 0.1f;

    // Track last collision time with each other bouncer
    private Dictionary<Bouncer, float> lastCollisionTime = new Dictionary<Bouncer, float>();

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = transform.parent.GetComponent<RectTransform>();

        // Start with a random normalized direction
        direction = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (isBeingDragged)
        {
            return;
        }

        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        DriftDirection();
        Move();
        CheckBouncerCollisions();
        CleanupOldCollisions();            
    }

    void DriftDirection()
    {
        // Add a small random change to the direction
        Vector2 drift = new Vector2(
            Random.Range(-driftStrength, driftStrength),
            Random.Range(-driftStrength, driftStrength)
        ) * Time.deltaTime;

        direction += drift;
        direction.Normalize();
    }

    void Move()
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        rectTransform.anchoredPosition += new Vector2(movement.x, movement.y);

        Vector2 pos = rectTransform.anchoredPosition;
        Vector2 size = rectTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.rect.size;

        bool bounced = false;

        // Check horizontal bounds
        if (pos.x - size.x / 2 < -canvasSize.x / 2)
        {
            pos.x = -canvasSize.x / 2 + size.x / 2;
            direction.x *= -1;
            bounced = true;
        }
        else if (pos.x + size.x / 2 > canvasSize.x / 2)
        {
            pos.x = canvasSize.x / 2 - size.x / 2;
            direction.x *= -1;
            bounced = true;
        }

        // Check vertical bounds
        if (pos.y - size.y / 2 < -canvasSize.y / 2)
        {
            pos.y = -canvasSize.y / 2 + size.y / 2;
            direction.y *= -1;
            bounced = true;
        }
        else if (pos.y + size.y / 2 > canvasSize.y / 2)
        {
            pos.y = canvasSize.y / 2 - size.y / 2;
            direction.y *= -1;
            bounced = true;
        }

        // Apply position after bounds check
        rectTransform.anchoredPosition = pos;

        // Optional: Add a small random nudge on bounce
        if (bounced)
        {
            direction += Random.insideUnitCircle * 0.1f;
            direction.Normalize();
        }
    }

    void CheckBouncerCollisions()
    {
        foreach (var other in allBouncers)
        {
            if (other == this) continue;

            float timeSinceLastCollision = 0f;
            lastCollisionTime.TryGetValue(other, out timeSinceLastCollision);

            if (Time.time - timeSinceLastCollision < collisionCooldown)
                continue; // Still cooling down

            if (IsOverlapping(other))
            {
                // Update last collision time for both
                lastCollisionTime[other] = Time.time;
                other.lastCollisionTime[this] = Time.time;

                // Calculate collision normal
                Vector2 normal = (rectTransform.anchoredPosition - other.rectTransform.anchoredPosition).normalized;

                // Reflect both directions
                direction = Vector2.Reflect(direction, normal);
                other.direction = Vector2.Reflect(other.direction, -normal);

                direction.Normalize();
                other.direction.Normalize();

                // Nudge apart slightly
                Vector2 nudge = normal * 2f;
                rectTransform.anchoredPosition += nudge;
                other.rectTransform.anchoredPosition -= nudge;
            }
        }
    }

    void CleanupOldCollisions(float threshold = 1.0f)
    {
        List<Bouncer> toRemove = new List<Bouncer>();

        foreach (var kvp in lastCollisionTime)
        {
            if (Time.time - kvp.Value > threshold)
            {
                toRemove.Add(kvp.Key);
            }
        }

        // Remove outdated entries
        foreach (var bouncer in toRemove)
        {
            lastCollisionTime.Remove(bouncer);
        }
    }

    bool IsOverlapping(Bouncer other)
    {
        Rect rectA = GetWorldRect(rectTransform);
        Rect rectB = GetWorldRect(other.rectTransform);
        return rectA.Overlaps(rectB);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 size = new Vector2(
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
        return new Rect(corners[0], size);
    }
}

