using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Notepad : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages = new List<GameObject>();
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] Transform pagesParent;
    [SerializeField] TextMeshProUGUI note;
    TextMeshProUGUI newNote;
    [SerializeField] TMP_InputField noteInput;
    int currentPageIndex = 0;


    GraphicRaycaster rayCaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    bool isWriting = false;
    private void Start()
    {
        rayCaster = GetComponent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();       
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            noteInput.text = string.Empty;
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            rayCaster.Raycast(pointerEventData, results);

            if (results.Count == 0)
            {
                isWriting = false;
                return;
            }
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.tag == "buttons")
                {
                    isWriting = false;
                    return;
                }
                if (result.gameObject.tag == "pages")
                {
                    isWriting = true;
                    Debug.Log("You clicked on a page!");
                    newNote = Instantiate(note, Input.mousePosition, Quaternion.identity);
                    newNote.transform.SetParent(result.gameObject.transform);
                    noteInput.ActivateInputField();
                }
                else
                {
                    isWriting = false;
                    return;
                }
            }
        }
        if (newNote != null && isWriting)
        {
            newNote.text = noteInput.GetComponentInChildren<TMP_InputField>().text;
        }
    }


    public void FlipPageForward()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex++;
        Debug.Log(currentPageIndex);
        if (pages.Count < currentPageIndex+1)
        {
            GameObject newPage = Instantiate(pagePrefab);
            newPage.transform.SetParent(pagesParent, false);
            pages.Add(newPage);
        }
        else
        {            
            pages[currentPageIndex].SetActive(true);
        }
    }

    public void FlipPageBackward()
    {
        if (currentPageIndex == 0)
        {
            return;
        }

        if (currentPageIndex > 0)
        {
            pages[currentPageIndex].SetActive(false);
            currentPageIndex--;
            pages[currentPageIndex].SetActive(true);
        }
    }
}
