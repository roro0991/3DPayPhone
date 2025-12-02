using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Notepad : MonoBehaviour
{
    [Header("Notepad Setup")]
    [SerializeField] private GameObject environNotepad;
    [SerializeField] private List<GameObject> pages = new List<GameObject>();    
    [SerializeField] private Transform pagesParent;
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private TextMeshPro pageNumber;
    [SerializeField] private GameObject contacts;
    [SerializeField] private GameObject notes;

    [Header("Camera & Input")]
    [SerializeField] private Camera equiprenderCam;
    [SerializeField] private TMP_InputField inputField;

    [Header("Object Pools")]
    [SerializeField] private NotePool notePool;
    [SerializeField] private LinePool linePool;

    private int currentPageIndex = 0;

    private Line currentLine;
    private TextMeshPro currentNote;

    private Dictionary<int, List<TextMeshPro>> notesPerPage = new Dictionary<int, List<TextMeshPro>>();
    private Dictionary<int, List<Line>> linesPerPage = new Dictionary<int, List<Line>>();

    private void Update()
    {
        HandleRealtimeNoteUpdate();
        HandleMouseInput();
    }

    private void HandleRealtimeNoteUpdate()
    {
        if (currentNote != null)
        {
            currentNote.text = inputField.text;
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClickStart();
        if (Input.GetMouseButtonUp(1)) FinishCurrentLine();
        if (currentLine != null) UpdateCurrentLine();
    }

    #region Notes

    private void HandleLeftClick()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Physics.Raycast(equiprenderCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            if (hit.collider.CompareTag("buttons")) return;
            if (hit.collider.CompareTag("pages"))
            {
                float[] deltas = CalculateTextDelta(hit);
                CreateNewNote(hit, deltas[0], deltas[1]);
            }
        }
    }

    private void CreateNewNote(RaycastHit hit, float horizontalDelta, float verticalDelta)
    {
        // Clear previous note from editing
        if (currentNote != null)
        {
            StoreNoteOnPage(currentPageIndex, currentNote);
            currentNote = null;
        }

        // Reset input field so new note is blank
        inputField.text = "";

        currentNote = notePool.Get();
        currentNote.transform.SetParent(hit.transform, false);

        // Position in local space to avoid world offsets
        Vector3 localPos = hit.transform.InverseTransformPoint(hit.point);
        currentNote.transform.localPosition = new Vector3(localPos.x, localPos.y + 0.050f, localPos.z);

        currentNote.rectTransform.sizeDelta = new Vector2(horizontalDelta - 0.05f, verticalDelta);
        currentNote.text = ""; // ensure blank
        inputField.ActivateInputField();
    }

    #endregion

    #region Lines

    private void HandleRightClickStart()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Physics.Raycast(equiprenderCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            if (hit.collider.CompareTag("buttons")) return;
            if (hit.collider.CompareTag("pages"))
            {
                Vector3 localStart = hit.transform.InverseTransformPoint(hit.point);
                currentLine = linePool.Get();
                currentLine.transform.SetParent(hit.transform, false);
                currentLine.Initialize(localStart); // local space
            }
        }
    }

    private void UpdateCurrentLine()
    {
        if (Physics.Raycast(equiprenderCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            if (hit.collider.CompareTag("buttons") || !hit.collider.CompareTag("pages"))
            {
                currentLine = null;
                return;
            }

            Vector3 localPos = hit.transform.InverseTransformPoint(hit.point);
            currentLine.UpdateLine(localPos);
        }
    }

    private void FinishCurrentLine()
    {
        if (currentLine != null)
        {
            StoreLineOnPage(currentPageIndex, currentLine);
            currentLine = null;
        }
    }

    #endregion

    #region Page Flipping

    public void FlipPageForward()
    {
        if (notes.gameObject.activeSelf)
        {
            pages[currentPageIndex].SetActive(false);
            currentPageIndex++;
            //pageNumberAsInt++;
            int page = currentPageIndex + 1;
            pageNumber.text = page.ToString();

            if (currentPageIndex >= pages.Count)
            {
                GameObject newPage = Instantiate(pagePrefab, pagesParent, false);
                pages.Add(newPage);
            }

            pages[currentPageIndex].SetActive(true);
            RestorePageContent(currentPageIndex);
        }
    }

    public void FlipPageBackward()
    {
        if (notes.gameObject.activeSelf)
        {
            if (currentPageIndex == 0) return;
            pages[currentPageIndex].SetActive(false);
            currentPageIndex--;
            //pageNumberAsInt--;
            int page = currentPageIndex + 1;
            pageNumber.text = page.ToString();
            pages[currentPageIndex].SetActive(true);
            RestorePageContent(currentPageIndex);
        }
    }

    #endregion

    #region Notepad Visibility

    public void OpenNotepad()
    {
        gameObject.SetActive(true);
        environNotepad.SetActive(false);
    }

    public void CloseNotepad()
    {
        gameObject.SetActive(false);
        environNotepad.SetActive(true);
    }

    public void OpenNotes()
    {
        contacts.gameObject.SetActive(false);
        notes.gameObject.SetActive(true);
        int page = currentPageIndex + 1;
        pageNumber.text = page.ToString();
    }

    public void OpenContacts()
    {
        notes.gameObject.SetActive(false);
        contacts.gameObject.SetActive(true);
    }

    #endregion

    #region Page Content Management

    private void StoreNoteOnPage(int pageIndex, TextMeshPro note)
    {
        if (!notesPerPage.ContainsKey(pageIndex))
            notesPerPage[pageIndex] = new List<TextMeshPro>();
        notesPerPage[pageIndex].Add(note);
    }

    private void StoreLineOnPage(int pageIndex, Line line)
    {
        if (!linesPerPage.ContainsKey(pageIndex))
            linesPerPage[pageIndex] = new List<Line>();
        linesPerPage[pageIndex].Add(line);
    }

    private void RestorePageContent(int pageIndex)
    {
        if (notesPerPage.TryGetValue(pageIndex, out var notes))
            foreach (var n in notes) n.gameObject.SetActive(true);

        if (linesPerPage.TryGetValue(pageIndex, out var lines))
            foreach (var l in lines) l.gameObject.SetActive(true);
    }

    #endregion

    #region Helpers

    private float[] CalculateTextDelta(RaycastHit hit)
    {
        Mesh mesh = hit.transform.GetComponent<MeshFilter>().mesh;
        Vector3 localHit = transform.InverseTransformPoint(hit.point);
        Vector3[] v = mesh.vertices;

        float horizontal = CalculateTriangleDelta(localHit, v[0], v[2], v[1], true);
        float vertical = CalculateTriangleDelta(localHit, v[0], v[1], v[2], false);

        return new float[] { horizontal, vertical };
    }

    private float CalculateTriangleDelta(Vector3 p, Vector3 a, Vector3 b, Vector3 c, bool horizontal)
    {
        float sideA = Vector3.Distance(p, a);
        float sideB = Vector3.Distance(p, b);
        float sideC = Vector3.Distance(a, b);
        float S = (sideA + sideB + sideC) / 2f;
        float A = Mathf.Sqrt(S * (S - sideA) * (S - sideB) * (S - sideC));
        return 2f * (A / sideC);
    }

    #endregion
}










