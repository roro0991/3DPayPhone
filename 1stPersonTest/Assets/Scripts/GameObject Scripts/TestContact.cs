using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary<string, string> inputResponses = new Dictionary<string, string>();
    private Dictionary<string, List<string>> wordsForBank = new Dictionary<string, List<string>>();

    private void Awake()
    {
        ContactName = "Them";
        OpeningLine = "Hello?";
        SentenceWords.AddRange(new string[] { "Hi", "there." });

        string testsentence = "I have met this dog before";
        SmartSentenceParser.PrintParsedSentence(testsentence);

        
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





