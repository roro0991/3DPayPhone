using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //public Word wordData; // Reference to the semantic word
    public SentenceWordEntry sentenceWordEntry; // Reference to surface word
    private RectTransform rectTransform;
    private RectTransform placeholder;
    private Vector3 storedPosition;
    private Transform storedParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

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
        sentenceWordEntry.Surface = this.GetComponent<TMP_Text>().text.ToString();
        sentenceWordEntry.Word = WordDataBase.Instance.GetWord(this.GetComponent<TMP_Text>().text.ToString());
        //wordData = WordDataBase.Instance.GetWord(this.GetComponent<TMP_Text>().text.ToString());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        storedPosition = rectTransform.anchoredPosition;
        storedParent = transform.parent;

        if (sentenceWordEntry.hasArticle)
        {
            sentenceBuilder.RemoveArticle(rectTransform, sentenceWordEntry);            
        }

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
                sentenceBuilder.InsertWordAt(rectTransform, sentenceWordEntry, insertIndex);
                sentenceBuilder.TestSingularOrPlural(sentenceWordEntry);
                isInSentencePanel = true;
            }
            else if (dropTarget.GetComponentInParent<WordBank>() != null)
            {
                // Snap into WordBank at drop position
                WordBank wb = dropTarget.GetComponentInParent<WordBank>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                transform.SetParent(wb.transform, true); // true = keep world position                
                isInSentencePanel = false;
            }            
        }
        else
        {
            // No drop target — return to WordBank
            transform.SetParent(wordBank.transform, false);
            rectTransform.anchoredPosition = storedPosition;
            //if (isInSentencePanel) sentenceBuilder.UpdateWordPositions();
            isInSentencePanel = false;
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
        placeholderScript.sentenceWordEntry.Word = this.sentenceWordEntry.Word;

        if (sentenceWordEntry.Word.PartOfSpeech == PartsOfSpeech.Noun &&
            sentenceWordEntry.Word.IsSingular(sentenceWordEntry.Surface))
        {
            TMP_Text tmp = placeholderGO.GetComponent<TMP_Text>();

            string baseWord = placeholderScript.sentenceWordEntry.Surface;

            bool startsWithVowel = "aeiou".Contains(char.ToLower(baseWord[0]));
            string article = startsWithVowel ? "an" : "a";

            tmp.text = $"{article} {baseWord}";
        }

        return placeholderGO.GetComponent<RectTransform>();
    }
}



