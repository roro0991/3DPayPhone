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

    private Vector3 storedPosition;
    private Transform storedParent;

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private RectTransform placeholder;

    public bool isPlaceholder = false;
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

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        float normalizedPivotX = Mathf.Clamp01((localPoint.x / rectTransform.rect.width) + 0.5f);
        float normalizedPivotY = Mathf.Clamp01((localPoint.y / rectTransform.rect.height) + 0.5f);

        Vector2 oldSize = rectTransform.rect.size;
        Vector2 pivotDelta = new Vector2(
            normalizedPivotX - rectTransform.pivot.x,
            normalizedPivotY - rectTransform.pivot.y
            );


        if (placeholder != null)
        {
            sentenceBuilder.RemovePlaceholder();
            placeholder = null;
        }

        if (isInSentencePanel && sentenceBuilder.wordList.Contains(rectTransform))
            sentenceBuilder.RemoveWord(rectTransform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        GameObject hover = eventData.pointerEnter;
        bool overSentence = hover != null && hover.CompareTag("SentencePanel");

        if (overSentence && sentenceBuilder != null)
        {
            if (placeholder == null)
            {
                placeholder = sentenceBuilder.CreatePlaceHolder(rectTransform);
            }

            // Convert pointer to local X
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sentenceBuilder.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
            float pointerX = localPoint.x;

            // Get raw insertion index
            int insertIndex = sentenceBuilder.GetInsertionIndex(pointerX);

            // --- Adjust insertion index to prevent splitting noun + article ---
            if (sentenceBuilder.wordList != null &&
                insertIndex < sentenceBuilder.wordList.Count)
            {
                var targetRect = sentenceBuilder.wordList[insertIndex];
                if (targetRect != null)
                {
                    var targetWord = targetRect.GetComponent<DraggableWord>();
                    if (targetWord != null && targetWord.rectTransform != null &&
                        targetWord.sentenceWordEntry != null &&
                        targetWord.sentenceWordEntry.Word != null)
                    {
                        // If the word is a noun with an article and cursor is to the left, shift insertion index right
                        if (targetWord.sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Noun) &&
                            targetWord.sentenceWordEntry.hasArticle)
                        {
                            float nounLeftX = targetWord.rectTransform.anchoredPosition.x;
                            if (pointerX < nounLeftX) // cursor is to the left of noun
                            {
                                insertIndex++; // force placeholder to go after article+noun
                            }
                        }
                    }
                }
            }

            // Show placeholder at corrected index
            sentenceBuilder.ShowPlaceholderAt(insertIndex, placeholder);
        }
        else
        {
            if (placeholder != null && sentenceBuilder != null)
            {
                sentenceBuilder.RemovePlaceholder();
                placeholder = null;
            }
        }
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
                sentenceBuilder.RemovePlaceholder();
                placeholder = null;
                transform.SetParent(sentenceBuilder.transform, false);

                
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    sentenceBuilder.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint                
                );

                float pointerX = localPoint.x;

                int insertIndex = sentenceBuilder.GetInsertionIndex(pointerX);
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



