using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContactsManager : MonoBehaviour
{
    [SerializeField] GameObject[] Contacts = new GameObject[0];

    private void Start()
    {
        foreach (GameObject contact in Contacts)
        {
            contact.GetComponent<Contact>().ClearAllInfo();
        }
    }
}
