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
using System.Xml;
using JetBrains.Annotations;

public class Notepad : MonoBehaviour
{
    [SerializeField] private GameObject environNotepad;
    [SerializeField] private List<GameObject> pages = new List<GameObject>();
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] Transform pagesParent;
    [SerializeField] private TextMeshProUGUI pageNumber;

    [SerializeField] private Camera equiprenderCam;

    [SerializeField] GameObject linePrefab;
    Line activeLine;

    [SerializeField] TextMeshPro notePrefab;
    TextMeshPro newNote;
    [SerializeField] TMP_InputField inputField;    

    int currentPageIndex;
    int pageNumberAsInt;

    bool isWriting = false;

    private void Start()
    {
        currentPageIndex = 0;
        pageNumberAsInt = 1;
    }

    private void Update()
    {
        if (newNote != null)
        {
            newNote.text = inputField.text;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "pages")
            {
                if (!isWriting)
                {
                    WriteNote(hit);
                    return;
                }
                else if (isWriting && newNote.text == "")
                {
                    Destroy(newNote.gameObject);
                    WriteNote(hit);
                }
                else
                {
                    inputField.text = null;
                    WriteNote(hit); 
                }
            }              
        }


        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "pages")
            {
                Vector3 mousePos = hit.point;
                GameObject newLine = Instantiate(linePrefab);
                newLine.transform.SetParent(hit.transform);
                activeLine = newLine.GetComponent<Line>();
                activeLine.UpdateLine(mousePos);
            }                
        }

        if (Input.GetMouseButtonUp(1))
        {            
            activeLine = null;               
        }

        if (activeLine != null)
        {
            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "pages")
            {
                Vector3 mousePos = hit.point;
                activeLine.UpdateLine(mousePos);
            }
        }
        
    }

        private void WriteNote(RaycastHit hit)
    {
        isWriting = true;
        Vector3 mousePos = hit.point;       
        newNote = Instantiate(notePrefab);
        newNote.transform.SetParent(hit.transform, false);
        newNote.transform.position = mousePos;
        inputField.ActivateInputField();
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
