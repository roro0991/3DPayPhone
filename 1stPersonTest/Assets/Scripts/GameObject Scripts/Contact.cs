using System.Collections.Generic;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    [SerializeField] private AddressBook addressBook;
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    public List<Word> SentenceWords = new List<Word>();
    public string ContactNumber;
    public string ContactName;
    public string ContactAddress;
    public bool Discovered;
    [HideInInspector] public string ContactID = System.Guid.NewGuid().ToString();
    public string OpeningLine = string.Empty;
    public string PlayerInput = string.Empty;
    public string ContactResponse = string.Empty;

    [SerializeField] protected WordBank wordBank; // reference to the player’s word bank

    public void DiscoverContact()
    {
        if (Discovered) return;

        Discovered = true;

        if (addressBook == null)
        {
            addressBook = FindObjectOfType<AddressBook>();
        }

        if (phoneNumberManager == null)
        {
            phoneNumberManager = FindObjectOfType<PhoneNumberManager>();
        }

        if (addressBook != null)
        {
            addressBook.OnContactDiscovered(this);
        }
        else
        {
            Debug.LogWarning($"No AddressBook found for {ContactName}");
        }
    }

    // Abstract methods to be implemented by subclasses
    public abstract void SpeakFirstLine();
    public abstract void GenerateResponse();

    /// <summary>
    /// Pushes current SentenceWords to the WordBank.
    /// </summary>
    protected void UpdateWordBankFromSentence()
    {
        if (wordBank != null)
        {
            wordBank.ClearWordBank();
            wordBank.AddWordsToWordBank(SentenceWords);
        }
        else
        {
            Debug.LogWarning($"{ContactName} has no WordBank reference assigned.");
        }
    }

    /// <summary>
    /// Utility: Add words to SentenceWords using WordDatabase singleton
    /// </summary>
    protected void AddWordToSentence(string key)
    {
        var word = WordDataBase.Instance.GetWord(key);
        if (word != null && !SentenceWords.Contains(word))
        {
            SentenceWords.Add(word);
        }
        else if (word == null)
        {
            Debug.LogWarning($"Word '{key}' not found in WordDatabase!");
        }
    }
}


