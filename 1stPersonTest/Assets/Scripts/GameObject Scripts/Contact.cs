using System.Collections.Generic;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;  // Must be assigned in Inspector
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public List<Word> SentenceWords = new List<Word>();
    public string ContactNumber;
    public string ContactName;
    public string ContactAddress;
    private bool nameKnown;
    private bool numberKnown;
    private bool addressKnown;
    [HideInInspector] public string ContactID = System.Guid.NewGuid().ToString();
    public string OpeningLine = string.Empty;
    public string PlayerInput = string.Empty;
    public string ContactResponse = string.Empty;

    [SerializeField] protected WordBank wordBank;

    private void Awake()
    {
        if (addressBook == null)
        {
            Debug.LogWarning($"Contact '{ContactName}' has no AddressBook assigned in Inspector.");
        }
    }

    public void DiscoverName(string name)
    {
        ContactName = name;
        nameKnown = true;
        NotifyAddressBook();
    }

    public void DiscoverNumber(PhoneNumberManager phoneNumberManager)
    {
        if (!numberKnown)
        {
            ContactNumber = phoneNumberManager.GetOrGenerateNumber(this);
            numberKnown = true;
            Debug.Log($"{ContactName} number discovered: {ContactNumber}");
        }

        NotifyAddressBook();
    }

    public void DiscoverAddress(string address)
    {
        ContactAddress = address;
        addressKnown = true;
        NotifyAddressBook();
    }

    private void NotifyAddressBook()
    {
        if (addressBook != null)
        {
            var data = new ContactData(
                ContactID,
                nameKnown ? ContactName : "",
                numberKnown ? ContactNumber : "",
                addressKnown ? ContactAddress : ""
            );

            Debug.Log($"Notifying AddressBook with: {data.Name}, {data.PhoneNumber}");
            addressBook.UpdateContact(data);
        }
    }

    public abstract void SpeakFirstLine();
    public abstract void GenerateResponse();

    protected void UpdateWordBankFromSentence()
    {
        if (wordBank != null)
        {
            wordBank.ClearWordBank();
            wordBank.AddWordsToWordBank(SentenceWords);
        }
    }

    protected void AddWordToSentence(string key)
    {
        var word = WordDataBase.Instance.GetWord(key);
        if (word != null && !SentenceWords.Contains(word))
            SentenceWords.Add(word);
    }
}





