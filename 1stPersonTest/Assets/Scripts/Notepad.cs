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
using UnityEngine.Assertions.Must;

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
    [SerializeField] GameObject textBarrier;

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
            {;
                Mesh mesh = hit.transform.gameObject.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;

                float sizeDelta = CalculateTextDelta(hit);

                Debug.Log(sizeDelta);

                if (!isWriting)
                {
                    WriteNote(hit, sizeDelta);
                    return;
                }
                else if (isWriting && newNote.text == "")
                {
                    Destroy(newNote.gameObject);
                    WriteNote(hit, sizeDelta);
                }
                else
                {
                    inputField.text = null;
                    WriteNote(hit, sizeDelta); 
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

    private float CalculateTextDelta(RaycastHit hit)
    {
        Mesh mesh = hit.transform.gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //transform.TransformPoints(vertices);

        Vector3 localHit = transform.InverseTransformPoint(hit.point);

        float sideA = Vector3.Distance(localHit, vertices[2]);
        float sideB = Vector3.Distance(localHit, vertices[0]);
        float sideC = Vector3.Distance(vertices[2], vertices[0]);

        float S = (sideA + sideB + sideC) / 2;

        float A = Mathf.Sqrt(S * (S - sideA) * (S - sideB) * (S - sideC));

        float sizeDelta = 2 * (A / sideC);
        
        /*
        Debug.Log(localHit.x);
        Debug.Log(vertices[1].x);
        Debug.Log(vertices[3].x);

        Debug.Log(vertices[0].x);
        Debug.Log(vertices[2].x);
        */

        return sizeDelta; 
    }

        private void WriteNote(RaycastHit hit, float sizeDelta)
    {
        isWriting = true;
        Vector3 mousePos = hit.point;       
        newNote = Instantiate(notePrefab);
        newNote.transform.SetParent(hit.transform, false);
        newNote.transform.position = mousePos;
        newNote.rectTransform.sizeDelta = new Vector2(sizeDelta, 1); 
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
