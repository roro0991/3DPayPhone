using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SentenceWordEntry sentenceWordEntry;
    private RectTransform rectTransform;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public bool isDraggable = true;
    public bool isBeingDragged = false;
    public bool isInSentencePanel = false;
    public bool isOverSentencePanel;

    private SentenceBuilder sentenceBuilder;    

    private void Awake()
    {
        sentenceBuilder = FindFirstObjectByType<SentenceBuilder>();

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
            // Remove from the sentence model first
            sentenceBuilder.RemoveDraggableFromSentence(sentenceWordEntry, eventData);
        }

        this.GetComponent<RectTransform>().position = eventData.position;
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        GameObject hover = eventData.pointerEnter;

        sentenceBuilder.HandleHoveringWord(this, eventData);        
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



