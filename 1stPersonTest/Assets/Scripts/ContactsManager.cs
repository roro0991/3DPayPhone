using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContactsManager : MonoBehaviour
{
    [SerializeField] public GameObject[] Contacts = new GameObject[0];
    [SerializeField] private GameObject environmentContactSheet;
    [SerializeField] private GameObject uiContactSheet;

    private bool contactSheetOpen = false;
    private void Start()
    {

        foreach (GameObject contact in Contacts)
        {
            contact.GetComponent<Contact>().ClearAllInfo();
        }        
    }

    public void OpenContacts()
    {
        if (!contactSheetOpen)
        {
            contactSheetOpen = true;
            environmentContactSheet.gameObject.SetActive(false);
            uiContactSheet.gameObject.SetActive(true);
        }
        else
        {
            contactSheetOpen = false;
            environmentContactSheet.gameObject.SetActive(true);
            uiContactSheet.gameObject.SetActive(false);
        }
    }
}
