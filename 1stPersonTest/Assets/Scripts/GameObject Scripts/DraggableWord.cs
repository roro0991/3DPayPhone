using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Word wordData; // Reference to the semantic word
    private RectTransform rectTransform;
    private RectTransform placeholder;
    private Vector3 storedPosition;
    private Transform storedParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public bool isBeingDragged = false;
    public bool isInSentencePanel = false;

    private SentenceBuilder sentenceBuilder;

    private void Awake()
    {
        sentenceBuilder = FindObjectOfType<SentenceBuilder>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        storedPosition = rectTransform.anchoredPosition;
        storedParent = transform.parent;

        // Move to top-level DragLayer for drag clarity
        GameObject dragLayer = GameObject.Find("DragLayer");
        if (dragLayer != null) transform.SetParent(dragLayer.transform, false);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (isInSentencePanel && sentenceBuilder.wordList.Contains(rectTransform))
            sentenceBuilder.RemoveWord(rectTransform);

        if (isInSentencePanel) rectTransform.pivot = new Vector2(0f, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (sentenceBuilder == null) return;

        GameObject hoverTarget = eventData.pointerEnter;
        bool isOverSentencePanel = hoverTarget != null && hoverTarget.CompareTag("SentencePanel");

        if (isOverSentencePanel)
        {
            Vector2 localPoint;
            RectTransform sentenceRect = sentenceBuilder.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(sentenceRect, eventData.position, eventData.pressEventCamera, out localPoint);

            int insertIndex = sentenceBuilder.GetInsertionIndex(localPoint.x);
            if (placeholder == null) placeholder = CreatePlaceholder();
            sentenceBuilder.ShowPlaceholderAt(insertIndex, placeholder);
        }
        else if (placeholder != null)
        {
            sentenceBuilder.RemovePlaceholder();
            Destroy(placeholder.gameObject);
            placeholder = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        if (placeholder != null)
        {
            sentenceBuilder.RemovePlaceholder();
            Destroy(placeholder.gameObject);
            placeholder = null;
        }

        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null)
        {
            if (dropTarget.CompareTag("SentencePanel"))
            {
                // Dropped in SentencePanel
                transform.SetParent(sentenceBuilder.transform, false);
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    sentenceBuilder.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint
                );
                int insertIndex = sentenceBuilder.GetInsertionIndex(localPoint.x);
                sentenceBuilder.InsertWordAt(rectTransform, wordData, insertIndex);
                isInSentencePanel = true;
            }
            else if (dropTarget.GetComponentInParent<WordBank>() != null)
            {
                // Snap into WordBank at drop position
                WordBank wb = dropTarget.GetComponentInParent<WordBank>();
                transform.SetParent(wb.transform, true); // true = keep world position
                isInSentencePanel = false;
            }
            else
            {
                // Not dropped in a valid panel — return to previous parent
                transform.SetParent(storedParent, false);
                rectTransform.anchoredPosition = storedPosition;
                if (isInSentencePanel) sentenceBuilder.UpdateWordPositions();
            }
        }
        else
        {
            // No drop target — return to previous parent
            transform.SetParent(storedParent, false);
            rectTransform.anchoredPosition = storedPosition;
            if (isInSentencePanel) sentenceBuilder.UpdateWordPositions();
        }
    }


    private RectTransform CreatePlaceholder()
    {
        GameObject placeholderGO = Instantiate(gameObject, sentenceBuilder.transform);
        placeholderGO.name = "Placeholder";
        CanvasGroup cg = placeholderGO.GetComponent<CanvasGroup>() ?? placeholderGO.AddComponent<CanvasGroup>();
        cg.alpha = 0.5f;
        cg.blocksRaycasts = false;

        // Copy Word reference
        DraggableWord placeholderScript = placeholderGO.GetComponent<DraggableWord>();
        placeholderScript.wordData = this.wordData;

        return placeholderGO.GetComponent<RectTransform>();
    }
}


