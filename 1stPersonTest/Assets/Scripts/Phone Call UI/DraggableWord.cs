using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SentenceWordEntry sentenceWordEntry;
    private RectTransform rectTransform;

    private Vector3 storedPosition;
    private Transform storedParent;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool hasPlaceholder = false;

    public bool isDraggable = true;
    public bool isBeingDragged = false;
    public bool isInSentencePanel = false;    

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

        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;

        storedPosition = rectTransform.anchoredPosition;
        storedParent = transform.parent;

        GameObject dragLayer = GameObject.Find("DragLayer");
        if (dragLayer != null)
            transform.SetParent(dragLayer.transform, false);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (isInSentencePanel && sentenceBuilder.wordList.Contains(rectTransform))
            sentenceBuilder.RemoveWord(rectTransform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return; // prevent drag entirely

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
       
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return; // prevent drag entirely

        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null)
        {
            if (dropTarget.CompareTag("SentencePanel"))
            {
                // Insert into sentence
                transform.SetParent(sentenceBuilder.transform, false);

                
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    sentenceBuilder.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint                
                );

                var width = rectTransform.rect.width;
                var wordMidX = localPoint.x + (width * 0.5f);

                int insertIndex = sentenceBuilder.GetInsertionIndex(wordMidX);
                sentenceBuilder.InsertWordAt(rectTransform, insertIndex);
                sentenceBuilder.TestSingularOrPlural(sentenceWordEntry);

                isInSentencePanel = true;
                return;
            }

            if (dropTarget.GetComponentInParent<WordBank>() != null)
            {
                // Back to word bank
                WordBank wb = dropTarget.GetComponentInParent<WordBank>();
                transform.SetParent(wb.transform, true);

                //rectTransform.pivot = new Vector2(0.5f, 0.5f);
                isInSentencePanel = false;
                return;
            }
        }

        // Drop failed ? return to wordbank
        transform.SetParent(wordBank.transform, false);
        rectTransform.anchoredPosition = storedPosition;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        isInSentencePanel = false;
    }
}



