using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary<string, string> inputResponses = new Dictionary<string, string>();
    private Dictionary<string, List<SentenceWordEntry>> wordsForBank = new Dictionary<string, List<SentenceWordEntry>>();

    private void Start()
    {
        ContactName = "John Smith";
        ContactAddress = "123 Olympic Blvd";
        OpeningLine = "Hello?";

        // Add words using singleton
        AddWordToSentence("dog");
        AddWordToSentence("dogs");
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;
        UpdateWordBankFromSentence(); // load initial SentenceWords into bank
    }

    public override void GenerateResponse()
    {
        ContactResponse = string.Empty;

        if (inputResponses.ContainsKey(PlayerInput))
        {
            ContactResponse = inputResponses[PlayerInput];
        }
        else
        {
            ContactResponse = "I don't understand.";
            SentenceWords.Clear();
            UpdateWordBankFromSentence();
            return;
        }

        if (wordsForBank.ContainsKey(ContactResponse))
        {
            SentenceWords.Clear();
            SentenceWords.AddRange(wordsForBank[ContactResponse]);
        }
        else
        {
            SentenceWords.Clear();
        }

        UpdateWordBankFromSentence();
    }
}








