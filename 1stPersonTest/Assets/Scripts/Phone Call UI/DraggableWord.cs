using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SentenceWordEntry sentenceWordEntry;
    private RectTransform rectTransform;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public bool isPlaceholder = false;
    public bool isDraggable = true;
    public bool isBeingDragged = false;
    public bool isInSentencePanel = false;
    public bool isOverSentencePanel;    

    private SentenceBuilder sentenceBuilder;
    private WordBank wordBank;
    private CallManager callManager;

    private void Awake()
    {
        wordBank = FindFirstObjectByType<WordBank>();
        sentenceBuilder = FindFirstObjectByType<SentenceBuilder>();
        callManager = FindFirstObjectByType<CallManager>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Start()
    {
        TMP_Text tmp = GetComponent<TMP_Text>();
        sentenceWordEntry.Surface = tmp.text;
        sentenceWordEntry.Word = WordDataBase.Instance.GetWord(tmp.text);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return; // prevent drag entirely

        if (isInSentencePanel)
        {
            sentenceBuilder.RemoveWord(rectTransform);
        }

        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        GameObject hover = eventData.pointerEnter;
        bool overSentence = hover != null && hover.CompareTag("SentencePanel");

        if (overSentence)
        {
            sentenceBuilder.HandleHoveringWord(this, eventData);
        }
        else
        {
            sentenceBuilder.RemovePlaceholder();
            sentenceBuilder.RemovePlaceholderTrailingPunctuation();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        GameObject dropTarget = eventData.pointerEnter;
        
        sentenceBuilder.HandleWordDropped(this, eventData);        
    }    
}



