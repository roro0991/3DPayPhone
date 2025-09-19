using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform placeholder;
    private Vector3 storedPosition;
    private Transform storedParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public bool isBeingDragged = false;
    public bool isInSentencePanel = false;

    // Assign this in inspector: drag your SentencePanel GameObject here (with SentenceBuilder script)
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

        // Move to top-level DragLayer
        GameObject dragLayer = GameObject.Find("DragLayer");
        if (dragLayer != null)
        {
            transform.SetParent(dragLayer.transform, false);
        }
        else
        {
            Debug.LogWarning("DragLayer not found in scene.");
        }

        // Center pivot for better cursor alignment
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // If the word is in the sentence, remove it temporarily
        if (isInSentencePanel && sentenceBuilder.wordList.Contains(rectTransform))
        {
            sentenceBuilder.RemoveWord(rectTransform);
        }

        // Restore pivot if necessary
        if (isInSentencePanel)
        {
            rectTransform.pivot = new Vector2(0f, 0.5f); // Left-middle
        }

    }




    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        if (sentenceBuilder == null) return;

        // Get the pointer's current target
        GameObject hoverTarget = eventData.pointerEnter;

        // Check if we're currently over the SentencePanel
        bool isOverSentencePanel = hoverTarget != null && hoverTarget.CompareTag("SentencePanel");

        if (isOverSentencePanel)
        {
            Vector2 localPoint;
            RectTransform sentencePanelRect = sentenceBuilder.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sentencePanelRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            int insertIndex = sentenceBuilder.GetInsertionIndex(localPoint.x);

            // Only create placeholder if it doesn't exist
            if (placeholder == null)
            {
                placeholder = CreatePlaceholder();
            }

            sentenceBuilder.ShowPlaceholderAt(insertIndex, placeholder);
        }
        else
        {
            // If no longer over sentence panel, remove placeholder
            if (placeholder != null)
            {
                sentenceBuilder.RemovePlaceholder();
                Destroy(placeholder.gameObject);
                placeholder = null;
            }
        }
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        GameObject dropTarget = eventData.pointerEnter;

        // Always clean up placeholder
        if (sentenceBuilder != null && placeholder != null)
        {
            sentenceBuilder.RemovePlaceholder();
            Destroy(placeholder.gameObject);
            placeholder = null;
        }

        if (dropTarget != null && dropTarget.CompareTag("SentencePanel"))
        {
            transform.SetParent(sentenceBuilder.transform, false);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sentenceBuilder.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            int insertIndex = sentenceBuilder.GetInsertionIndex(localPoint.x);
            sentenceBuilder.InsertWordAt(rectTransform, insertIndex);
            isInSentencePanel = true;
        }
        else if (dropTarget != null && dropTarget.CompareTag("WordBankPanel"))
        {
            HandleDropInWordBankPanel(dropTarget);
        }
        else
        {
            // Revert to original location
            transform.SetParent(storedParent, false);
            rectTransform.anchoredPosition = storedPosition;

            if (isInSentencePanel)
            {
                sentenceBuilder.UpdateWordPositions();
            }
        }
    }




    private RectTransform CreatePlaceholder()
    {
        GameObject placeholderGO = Instantiate(gameObject, sentenceBuilder.transform);
        placeholderGO.name = "Placeholder";
        var cg = placeholderGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = placeholderGO.AddComponent<CanvasGroup>();
        cg.alpha = 0.5f; // semi-transparent
        cg.blocksRaycasts = false;
        return placeholderGO.GetComponent<RectTransform>();
    }


    private void HandleDropInSentencePanel(GameObject sentencePanelGO, PointerEventData pointerData)
    {
        isInSentencePanel = true;

        // Set parent to sentence panel (no world position change)
        transform.SetParent(sentencePanelGO.transform, false);

        RectTransform sentencePanelRect = sentencePanelGO.GetComponent<RectTransform>();

        // Convert screen pointer position to local position in sentence panel
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(sentencePanelRect, pointerData.position, pointerData.pressEventCamera, out localPoint);

        // Determine insert index based on local X position relative to existing words
        int insertIndex = 0;
        for (int i = 0; i < sentenceBuilder.wordList.Count; i++)
        {
            var wordRect = sentenceBuilder.wordList[i];
            if (localPoint.x > wordRect.anchoredPosition.x)
                insertIndex = i + 1;
            else
                break;
        }

        // Insert this word into the sentenceBuilder's list at correct index
        sentenceBuilder.InsertWordAt(rectTransform, insertIndex);
    }

    private void HandleDropInWordBankPanel(GameObject wordBankPanelGO)
    {
        isInSentencePanel = false;

        RectTransform wordBankRect = wordBankPanelGO.GetComponent<RectTransform>();

        Camera cam = null;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wordBankRect,
            Input.mousePosition,
            cam,
            out localPoint);

        transform.SetParent(wordBankRect, false);

        // Pivot is center, so localPoint should be centered on the word
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Apply a small offset correction to align with cursor better (optional)
        float offsetX = rectTransform.rect.width * rectTransform.localScale.x * 0.5f;
        Vector2 correctedPosition = new Vector2(localPoint.x - offsetX, localPoint.y);

        // You might want to test with or without this offset to see if it helps your case
        rectTransform.anchoredPosition = correctedPosition;

        // Clamp within bounds to avoid floating out of panel
        Vector2 clampedPosition = rectTransform.anchoredPosition;

        Vector2 panelSize = wordBankRect.rect.size;
        Vector2 wordSize = rectTransform.rect.size;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -panelSize.x / 2 + wordSize.x / 2, panelSize.x / 2 - wordSize.x / 2);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -panelSize.y / 2 + wordSize.y / 2, panelSize.y / 2 - wordSize.y / 2);

        rectTransform.anchoredPosition = clampedPosition;

        if (sentenceBuilder.wordList.Contains(rectTransform))
        {
            sentenceBuilder.RemoveWord(rectTransform);
        }
    }


    private void RevertPosition()
    {
        transform.SetParent(storedParent, false);
        rectTransform.anchoredPosition = storedPosition;

        // If in sentence panel, snap back to sentence positions
        if (isInSentencePanel)
        {
            sentenceBuilder.UpdateWordPositions();
        }
    }
}

