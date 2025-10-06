using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AddressBook : MonoBehaviour
{
    [SerializeField] private PhoneNumberManager phoneNumberManager;
    [SerializeField] private List<GameObject> contactSlots; // Each slot has Name/Number TMPs
    [SerializeField] private int contactsPerPage = 4;

    private readonly List<ContactData> allContacts = new List<ContactData>();
    private int currentPage = 0;

    public void AddContact(ContactData data)
    {
        // Avoid duplicates
        if (!allContacts.Exists(c => c.ID == data.ID))
            allContacts.Add(data);

        UpdatePage();
    }

    public void NextPage()
    {
        if ((currentPage + 1) * contactsPerPage < allContacts.Count)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        int start = currentPage * contactsPerPage;

        for (int i = 0; i < contactSlots.Count; i++)
        {
            TMP_Text nameText = contactSlots[i].transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text numberText = contactSlots[i].transform.Find("NumberText").GetComponent<TMP_Text>();
            TMP_Text addressText = contactSlots[i].transform.Find("AddressText")?.GetComponent<TMP_Text>(); // safe optional

            int index = start + i;

            if (index < allContacts.Count)
            {
                ContactData data = allContacts[index];

                nameText.text = $"Name: {data.Name.Trim()}";
                numberText.text = $"Phone: {data.PhoneNumber.Trim()}";

                if (addressText != null)
                    addressText.text = $"Address: {data.Address.Trim()}";
            }
            else
            {
                // Keep the labels, just clear the data
                nameText.text = "Name:";
                numberText.text = "Phone:";

                if (addressText != null)
                    addressText.text = "Address:";
            }

            contactSlots[i].SetActive(true); // always active now
        }
    }



    public void OnContactDiscovered(Contact contact)
    {
        if (phoneNumberManager == null)
            phoneNumberManager = FindObjectOfType<PhoneNumberManager>();

        if (phoneNumberManager == null)
        {
            Debug.LogError("AddressBook: PhoneNumberManager not found in scene!");
            return;
        }

        string number = phoneNumberManager.GetOrGenerateNumber(contact);
        contact.ContactNumber = number;

        ContactData newContact = new ContactData(
            contact.ContactID,
            contact.ContactName,
            contact.ContactNumber,
            contact.ContactAddress // new line
        );

        AddContact(newContact);
    }

}

[System.Serializable]
public class ContactData
{
    public string ID;
    public string Name;
    public string PhoneNumber;
    public string Address;

    public ContactData(string id, string name, string number, string address)
    {
        ID = id;
        Name = name;
        PhoneNumber = number;
        Address = address;
    }
}



