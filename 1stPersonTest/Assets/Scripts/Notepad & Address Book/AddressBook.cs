using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AddressBook : MonoBehaviour
{
    [SerializeField] private GameObject addressBookRoot;
    [SerializeField] private PhoneNumberManager phoneNumberManager;
    [SerializeField] private List<GameObject> contactSlots;
    [SerializeField] private int contactsPerPage = 4;

    private readonly List<ContactData> allContacts = new List<ContactData>();
    private int currentPage = 0;

    public void OnAddressBookOpened()
    {
        RefreshCurrentPage();
    }

    public void AddContact(ContactData data)
    {
        if (!allContacts.Exists(c => c.ID == data.ID))
            allContacts.Add(data);

        RefreshCurrentPage();
    }

    public void NextPage()
    {
        if (!addressBookRoot.activeSelf) return;
        if ((currentPage + 1) * contactsPerPage < allContacts.Count)
        {
            currentPage++;
            RefreshCurrentPage();
        }
    }

    public void PrevPage()
    {
        if (!addressBookRoot.activeSelf) return;
        if (currentPage > 0)
        {
            currentPage--;
            RefreshCurrentPage();
        }
    }

    private int GetPageOfContact(string contactID)
    {
        for (int i = 0; i < allContacts.Count; i++)
        {
            if (allContacts[i].ID == contactID)
                return i / contactsPerPage;
        }
        return -1;
    }

    private void RefreshCurrentPage()
    {
        int start = currentPage * contactsPerPage;

        for (int i = 0; i < contactSlots.Count; i++)
        {
            TMP_Text nameText = contactSlots[i].transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text numberText = contactSlots[i].transform.Find("NumberText").GetComponent<TMP_Text>();
            TMP_Text addressText = contactSlots[i].transform.Find("AddressText")?.GetComponent<TMP_Text>();

            int index = start + i;

            if (index < allContacts.Count)
            {
                var data = allContacts[index];
                nameText.text = $"Name: {data.Name}";
                numberText.text = $"Phone: {data.PhoneNumber}";
                if (addressText != null)
                    addressText.text = $"Address: {data.Address}";
            }
            else
            {
                nameText.text = "Name:";
                numberText.text = "Phone:";
                if (addressText != null)
                    addressText.text = "Address:";
            }

            contactSlots[i].SetActive(true);
        }
    }

    public void UpdateContact(ContactData updatedContact)
    {
        var existing = allContacts.Find(c => c.ID == updatedContact.ID);

        if (existing != null)
        {
            existing.UpdateInfo(
                name: string.IsNullOrEmpty(updatedContact.Name) ? existing.Name : updatedContact.Name,
                number: string.IsNullOrEmpty(updatedContact.PhoneNumber) ? existing.PhoneNumber : updatedContact.PhoneNumber,
                address: string.IsNullOrEmpty(updatedContact.Address) ? existing.Address : updatedContact.Address
            );
        }
        else
        {
            allContacts.Add(updatedContact);
        }

        // Keep the current page on the updated contact
        int page = GetPageOfContact(updatedContact.ID);
        if (page >= 0)
            currentPage = page;

        // Always refresh the page, active or not—it won’t hurt
        RefreshCurrentPage();
    }

}

[System.Serializable]
public class ContactData
{
    public string ID;
    public string Name;
    public string PhoneNumber;
    public string Address;

    public ContactData(string id, string name = "", string number = "", string address = "")
    {
        ID = id;
        Name = name;
        PhoneNumber = number;
        Address = address;
    }

    public void UpdateInfo(string name = null, string number = null, string address = null)
    {
        if (name != null) Name = name;
        if (number != null) PhoneNumber = number;
        if (address != null) Address = address;
    }
}


