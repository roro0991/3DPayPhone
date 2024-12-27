using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;

public class Notepad : MonoBehaviour
{
    [SerializeField] private GameObject environNotepad;
    [SerializeField] private List<GameObject> pages = new List<GameObject>();
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] Transform pagesParent;
    [SerializeField] private TextMeshProUGUI pageNumber;

    [SerializeField] private GameObject linePrefab;
    GameObject newLine;

    Vector2 lastPosition;

    [SerializeField] TMP_InputField notePrefab;
    TMP_InputField newNote;   

    int currentPageIndex;
    int pageNumberAsInt;

    GraphicRaycaster rayCaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;


    bool isWriting = false;

    private void Start()
    {
        currentPageIndex = 0;
        pageNumberAsInt = 1;
        rayCaster = GetComponent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            rayCaster.Raycast(pointerEventData, results);

            if (results.Count == 0)
            {
                isWriting = false;
                return; 
            }
            else 
            {
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.tag == "pages" && !isWriting)
                    {
                        WriteNote(result);
                    }
                    else if (result.gameObject.tag == "pages" && isWriting)
                    {
                        if (newNote.text == "") // removes empty note instances
                        {
                            Destroy(newNote.gameObject);
                        }
                        else
                        {
                            newNote.readOnly = true;
                        }                           
                        WriteNote(result); 
                    }
                    else
                    {
                        if (newNote != null && newNote.text != "")
                        {
                            newNote.readOnly = true;
                        }
                        else if (newNote != null && newNote.text == "")
                        {
                            Destroy(newNote.gameObject);
                        }
                        isWriting = false;
                        return;
                    }
                }
            }  
        }

        if (Input.GetMouseButtonDown(1))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            rayCaster.Raycast(pointerEventData, results);

            if (results.Count == 0)
            {
                return;
            }
            else
            {
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.tag == "pages")
                    {
                        lastPosition = Input.mousePosition;
                    }
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            rayCaster.Raycast(pointerEventData, results);

            if (results.Count == 0)
            {
                return;
            }
            else
            {
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.tag == "pages")
                    {
                        if (Vector2.Distance(lastPosition, Input.mousePosition) > .5f)
                        {
                            DrawLine(result, Input.mousePosition);
                            lastPosition = Input.mousePosition;
                        }
                    }
                }
            }
        }
    }

    private void DrawLine(RaycastResult result, Vector3 position)
    {
        if (lastPosition != null)
        {
            newLine = Instantiate(linePrefab, position, Quaternion.identity);
            newLine.transform.LookAt(lastPosition);
            newLine.transform.SetParent(result.gameObject.transform);
            return;
        }
        newLine = Instantiate(linePrefab, position, Quaternion.identity);
        newLine.transform.SetParent(result.gameObject.transform);
    }


    public void OpenNotepad()
    {
        this.gameObject.SetActive(true);
        environNotepad.gameObject.SetActive(false);
    }

    public void CloseNotepad()
    {
        this.gameObject.SetActive(false);
        environNotepad.gameObject.SetActive(true);
    }

    private void WriteNote(RaycastResult result)
    {
        Vector3 instantiatePosition = 
            new Vector3(Input.mousePosition.x, Input.mousePosition.y + 25, Input.mousePosition.z);
        isWriting = true;
        Debug.Log("You clicked on a page!");
        newNote = Instantiate(notePrefab, instantiatePosition, Quaternion.identity);
        newNote.transform.SetParent(result.gameObject.transform);
        newNote.ActivateInputField();
    }

    public void FlipPageForward()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex++;
        pageNumberAsInt++;
        pageNumber.text = pageNumberAsInt.ToString();
        Debug.Log(currentPageIndex);
        Debug.Log(pages.Count);      
        if (pages.Count < currentPageIndex+1)
        {
            GameObject newPage = Instantiate(pagePrefab);
            newPage.transform.SetParent(pagesParent, false);
            pages.Add(newPage);
            pages[currentPageIndex].SetActive(true); 
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
            pageNumberAsInt--;
            pageNumber.text = pageNumberAsInt.ToString();
            pages[currentPageIndex].SetActive(true);
        }
    }
}
