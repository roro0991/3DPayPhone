using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary<string, string> inputResponses = new Dictionary<string, string>()
    {
        { "hello.", "who is this?" }
    };
    private Dictionary<string, List<string>> wordsForBank = new Dictionary<string, List<string>>()
    {
        { "who is this?", new List<string>
            { "who", "are", "you" } }
    };
        

    private void Start()
    {
        ContactName = "John Smith";
        ContactAddress = "123 Olympic Blvd";
        OpeningLine = "Hello?";

        // Add words using singleton
        AddWordToSentence("hello");
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;        
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
            return;
        }

        if (wordsForBank.ContainsKey(ContactResponse))
        {
            SentenceWords.Clear();
            foreach (string word in wordsForBank[ContactResponse])
            {
                AddWordToSentence(word);
            }
            //SentenceWords.AddRange(wordsForBank[ContactResponse]);
        }
        else
        {
            SentenceWords.Clear();
        }
    }
}








