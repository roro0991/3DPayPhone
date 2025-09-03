using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform panelRect;
    private Vector2 pointerOffset;
    private Bouncer bouncer;

    private Vector2 lastPosition;
    private Vector2 dragVelocity;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        panelRect = transform.parent.GetComponent<RectTransform>();
        bouncer = GetComponent<Bouncer>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (bouncer != null)
            bouncer.isBeingDragged = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );

        lastPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition - pointerOffset;
        }

        // Calculate velocity
        Vector2 currentPosition = rectTransform.anchoredPosition;
        dragVelocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (bouncer != null)
        {
            bouncer.isBeingDragged = false;

            // Calculate direction based on drag
            Vector2 currentPosition = rectTransform.anchoredPosition;
            Vector2 dragDir = (currentPosition - lastPosition).normalized;

            if (dragDir != Vector2.zero)
            {
                bouncer.direction = dragDir;
                // Do NOT modify bouncer.speed
            }
            else if (bouncer.direction == Vector2.zero)
            {
                bouncer.direction = Random.insideUnitCircle.normalized;
            }
        }
    }
}