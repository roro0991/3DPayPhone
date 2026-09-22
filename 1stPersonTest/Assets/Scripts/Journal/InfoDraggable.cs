using Ink.Parsed;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class InfoDraggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public string InfoAsString = string.Empty;
    public List<SentenceWordEntry> InfoEntries = new List<SentenceWordEntry>();
    
    private Canvas Canvas;
    public CanvasGroup CanvasGroup;
    private RectTransform RectTransform;



    private SentenceBuilder SentenceBuilder;

    private void Awake()
    {
        Canvas = GetComponentInParent<Canvas>();
        CanvasGroup = GetComponentInChildren<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
        SentenceBuilder = FindFirstObjectByType<SentenceBuilder>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;

        GameObject hover = eventData.pointerEnter;

        SentenceBuilder.HandleHoveringInfo(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasGroup.blocksRaycasts = true;
        GameObject dropTarget = eventData.pointerEnter;

        SentenceBuilder.HandleInfoDropped(this, eventData);
    }
}
