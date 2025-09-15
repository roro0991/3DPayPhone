using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        raycaster.Raycast(pointerData, results);

        bool droppedInSentencePanel = false;
        bool droppedInWordBankPanel = false;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("SentencePanel"))
            {
                droppedInSentencePanel = true;
                HandleDropInSentencePanel(result.gameObject, pointerData);
                break;
            }
            else if (result.gameObject.CompareTag("WordBankPanel"))
            {
                droppedInWordBankPanel = true;
                HandleDropInWordBankPanel(result.gameObject);
                break;
            }
        }

        if (!droppedInSentencePanel && !droppedInWordBankPanel)
        {
            // Invalid drop — revert position
            RevertPosition();
        }
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

