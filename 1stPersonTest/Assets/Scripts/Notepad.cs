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
    [SerializeField] StoryManager storyManager;

    [SerializeField] private GameObject environNotepad;
    [SerializeField] private List<GameObject> pages = new List<GameObject>();
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] Transform pagesParent;
    [SerializeField] private TextMeshPro pageNumber;

    [SerializeField] private Camera equiprenderCam;

    [SerializeField] GameObject linePrefab;
    Line activeLine;

    [SerializeField] TextMeshPro notePrefab;
    TextMeshPro newNote;
    [SerializeField] TMP_InputField inputField;

    int currentPageIndex;
    int pageNumberAsInt;

    bool isWriting = false;

    bool firstlineDrawn = false;
    bool firstnoteWritten = false;
    private void Start()
    {
        currentPageIndex = 0;
        pageNumberAsInt = 1;
    }

    private void Update()
    {
        //updating note text in realtime from offscreen inputfield
        if (newNote != null)
        {
            if (firstnoteWritten == false && newNote.text != "")
            {
                firstnoteWritten = true; 
                storyManager.SetFirstNoteWritten(true);
            }
            newNote.text = inputField.text;
        }

        //code for instantiating notes
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);            
            RaycastHit hit;                   

            if (Physics.Raycast(ray, out hit)) 
            {
                if (hit.collider.gameObject.tag == "buttons")
                {
                    return;
                }
                else if (hit.collider.gameObject.tag == "pages")
                {
                    Mesh mesh = hit.transform.gameObject.GetComponent<MeshFilter>().mesh;
                    Vector3[] vertices = mesh.vertices;

                    float[] textDeltas = CalculateTextDelta(hit);
                

                    if (!isWriting)
                    {
                        WriteNote(hit, textDeltas[0], textDeltas[1]);
                        return;
                    }
                    else if (isWriting && newNote.text == "")
                    {
                        Destroy(newNote.gameObject);
                        WriteNote(hit, textDeltas[0], textDeltas[1]);
                    }
                    else
                    {
                        inputField.text = null;
                        WriteNote(hit, textDeltas[0], textDeltas[1]); 
                    }
                }
            }              
        }

        //code for drawing 
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "buttons")
                {
                    return;
                }
                else if (hit.collider.gameObject.tag == "pages")
                {
                    Vector3 mousePos = hit.point;
                    GameObject newLine = Instantiate(linePrefab);
                    newLine.transform.SetParent(hit.transform);
                    activeLine = newLine.GetComponent<Line>();
                    activeLine.UpdateLine(mousePos);
                }
            }                
        }

        if (Input.GetMouseButtonUp(1) && activeLine !=null)
        {            
            activeLine = null;               
        }

        if (activeLine != null)
        {
            Ray ray = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "buttons")
                {
                    activeLine = null;
                }
                else if (hit.collider.gameObject.tag == "pages")
                {
                    if (firstlineDrawn == false)
                    {
                        firstlineDrawn = true;
                        storyManager.SetFirstLineDrawn(true);
                    }
                    Vector3 mousePos = hit.point;
                    activeLine.UpdateLine(mousePos);
                }
                else
                {
                    activeLine = null;
                }
            }
        }
        
    }

    //calculating textdelta for instantiated notes for wrapping
    private float[] CalculateTextDelta(RaycastHit hit)
    {
        Mesh mesh = hit.transform.gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        float[] textDeltas = new float[2];

        //for transforming quad vertice vectors into worldspace
        //transform.TransformPoints(vertices);

        Vector3 localHit = transform.InverseTransformPoint(hit.point);

        //measuring horizontal text delta
        float sideA = Vector3.Distance(localHit, vertices[2]);
        float sideB = Vector3.Distance(localHit, vertices[0]);
        float sideC = Vector3.Distance(vertices[2], vertices[0]);

        float S_h = (sideA + sideB + sideC) / 2;

        float A_h = Mathf.Sqrt(S_h * (S_h - sideA) * (S_h - sideB) * (S_h - sideC));

        float horizontaltextDelta = 2 * (A_h / sideC);
        textDeltas[0] = horizontaltextDelta;

        //measuring vertical text delta
        float sideD = Vector3.Distance(localHit, vertices[1]);
        float sideE = Vector3.Distance(vertices[0], vertices[1]);

        float S_v = (sideB + sideD + sideE) / 2;

        float A_v = Mathf.Sqrt(S_v * (S_v - sideB) * (S_v - sideD) * (S_v - sideE));

        float verticaltextDelta = 2 * (A_v / sideE);
        textDeltas[1] = verticaltextDelta;

        /* for debugging and checking vectors in localspace & worldspace
        Debug.Log(localHit.x);
        Debug.Log(vertices[1].x);
        Debug.Log(vertices[3].x);

        Debug.Log(vertices[0].x);
        Debug.Log(vertices[2].x);
        */

        return textDeltas;
    }

        private void WriteNote(RaycastHit hit, float horizontaltextDelta, float verticaltextDelta)
    {
        isWriting = true;
        Vector3 mousePos = hit.point;       
        newNote = Instantiate(notePrefab);
        newNote.transform.SetParent(hit.transform, false);
        newNote.transform.position = mousePos;
        newNote.rectTransform.sizeDelta = new Vector2(horizontaltextDelta-.05f, verticaltextDelta); 
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
