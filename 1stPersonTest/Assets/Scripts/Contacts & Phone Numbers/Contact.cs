using System.Collections.Generic;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;  // Must be assigned in Inspector
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public List<SentenceWordEntry> SentenceWords = new List<SentenceWordEntry>();
    public string ContactNumber;
    public string ContactName;
    private bool nameKnown;
    private bool numberKnown;
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

    private void NotifyAddressBook()
    {
        if (addressBook != null)
        {
            var data = new ContactData(
                ContactID,
                nameKnown ? ContactName : "",
                numberKnown ? ContactNumber : ""
            );

            Debug.Log($"Notifying AddressBook with: {data.Name}, {data.PhoneNumber}");
            addressBook.UpdateContact(data);
        }
    }

    public abstract void SpeakFirstLine();
    public abstract string GenerateResponse(SentenceBreakdown sb);

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
        key = key.ToLower();

        // First try direct match
        var word = WordDataBase.Instance.GetWord(key);
        if (word != null)
        {
            AddEntry(word, key);
            return;
        }   
        
        // Check known influections       
        foreach (var w in WordDataBase.Instance.Words.Values)
        {
            // Check noun forms
            foreach (var nf in w.NounFormsList)
            {
                if (nf.Plural == key)
                {
                    AddEntry(w, key);
                    return;
                }
            }
        }

        Debug.LogWarning($"Couldn't find base word for '{key}'");
    }

    private void AddEntry(Word word, string surface)
    {
        SentenceWords.Add(new SentenceWordEntry
        {
            Word = word,
            Surface = surface // ? THIS is "dogs"
        });
    }
}





