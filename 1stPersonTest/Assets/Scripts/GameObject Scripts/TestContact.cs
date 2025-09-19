using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary <string, string> inputResponses = new Dictionary<string, string>();
    private Dictionary <string, List<string>> wordsForBank = new Dictionary<string, List<string>>();
    
    private void Awake()
    {
        ContactName = "Them";
        OpeningLine = "Hello?";
        SentenceWords.AddRange(new string[] { "Hi", "there." });
        inputResponses.Add("Hi there.", "Who is this?");
        inputResponses.Add("I don't know.", "What do you mean, \"You don't know\"?");
        inputResponses.Add("Is Sarah there?", "I'm afraid not.");

        wordsForBank.Add("Who is this?", new List<string> { "I", "don't", "know." });
        wordsForBank.Add("What do you mean, \"You don't know\"?", new List<string> { "I", "can't", "remember" });
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
                SentenceWords.Add(word);
            }
        }
        else
        {
            SentenceWords.Clear();
        }
    }   
}
