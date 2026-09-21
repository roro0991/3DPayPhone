using Ink.Parsed;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using UnityEditor;

public class InformationClickable : MonoBehaviour, IBeginDragHandler, IDragHandler
{

    public GameObject Journal;
    public GameObject InfoDraggablePrefab;
    public string InformationAsString;
    public List<SentenceWordEntry> InformationEntries = new List<SentenceWordEntry>();    

    private void Awake()
    {
        Journal = GameObject.FindWithTag("Journal");
    }

    private void Start()
    {
        SentenceWordEntry firstEntry = new SentenceWordEntry();
        firstEntry.Word = WordDataBase.Instance.GetWord("john");
        firstEntry.Surface = "john";
        SentenceWordEntry secondEntry = new SentenceWordEntry();
        secondEntry.Word = WordDataBase.Instance.GetWord("work");
        secondEntry.Surface = "work";
        SentenceWordEntry thirdEntry = new SentenceWordEntry();
        thirdEntry.Surface = "accountant";
        thirdEntry.Word = WordDataBase.Instance.GetWord("accountant");

        InformationEntries.Add(firstEntry);
        InformationEntries.Add(secondEntry);
        InformationEntries.Add(thirdEntry);
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GameObject newInfoDraggable = Instantiate(InfoDraggablePrefab, Journal.transform);

        RectTransform newInfoDraggableRect = newInfoDraggable.GetComponent<RectTransform>();
        newInfoDraggableRect.pivot = new Vector2(0.5f, 0.5f);

        RectTransform journalRect = Journal.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            journalRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        newInfoDraggableRect.anchoredPosition = localPoint;

        var newInfoDraggableScript = newInfoDraggable.GetComponent<InfoDraggable>();
        newInfoDraggableScript.InfoEntries = InformationEntries;
        newInfoDraggableScript.CanvasGroup.blocksRaycasts = false;

        TMP_Text draggableText = newInfoDraggable.GetComponentInChildren<TMP_Text>();
        if (draggableText != null)
        {
            draggableText.text = InformationAsString;
        }

        eventData.pointerDrag = newInfoDraggable;
    }

    public void OnDrag(PointerEventData eventData)
    {        
    }

}
