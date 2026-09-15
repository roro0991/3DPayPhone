
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JournalClickable : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public GameObject Journal;
    public GameObject DraggableWordPrefab;
    private void Awake()
    {
        Journal = GameObject.FindWithTag("Journal");        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GameObject newWord = Instantiate(DraggableWordPrefab, Journal.transform);
        
        RectTransform newWordRect = newWord.GetComponent<RectTransform>();
        newWordRect.pivot = new Vector2(0.5f, 0.5f);

        RectTransform journalRect = Journal.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            journalRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        newWordRect.anchoredPosition = localPoint;


        var newWordDraggable = newWord.GetComponent<DraggableWord>();
        newWordDraggable.ThisDraggableOrigin = DraggableWord.DraggableOrigin.Journal;
        newWordDraggable.isBeingDragged = true;
        newWordDraggable.canvasGroup.blocksRaycasts = false;

        newWordDraggable.sentenceWordEntry.Surface = "john";
        newWordDraggable.sentenceWordEntry.Word = WordDataBase.Instance.GetWord("john");
        newWordDraggable.sentenceWordEntry.Origin = DraggableWord.DraggableOrigin.Journal;



        TMP_Text textComponent = newWord.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = "john";
        }

        eventData.pointerDrag = newWord;
    }

    public void OnDrag(PointerEventData eventData)
    {        
    }
}


