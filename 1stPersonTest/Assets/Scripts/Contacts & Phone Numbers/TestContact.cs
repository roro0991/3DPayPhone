using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Dialogue.Core;

public class TestContact : Contact
{
    private void Start()
    {
        ContactName = "John Smith";
        OpeningLine = "Hello?";

        ResponsesByIntent[new ResponseKey(Intent.ASK_ABOUT_IDENTITY, WordID.Anna)] =
            "Anna is my friend.";
        ResponsesByIntent[new ResponseKey(Intent.ASK_ABOUT_LOCATION, WordID.Anna)] =
            "Anna is at the office.";
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;        
    }

    public override void PopulateWordBank()
    {
        wordBank.AddWordToSentence("Who");
        wordBank.AddWordToSentence("Anna");
        wordBank.AddWordToSentence("Where");
    }

    public override string GenerateResponse(ResponseKey responsekey)
    {
        ContactResponse = string.Empty;

        ContactResponse = ResponsesByIntent[responsekey];

        return ContactResponse;
    }
}








