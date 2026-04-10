using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Dialogue.Core;
using NUnit.Framework.Constraints;

public class TestContact : Contact
{
    private void Start()
    {
        ContactName = "John Smith";
        OpeningLine = "Hello?";
        
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;        
    }

    public override void PopulateWordBank()
    {
        wordBank.AddWordToSentence("[?]");
        wordBank.AddWordToSentence("john");
        wordBank.AddWordToSentence("car");
        wordBank.AddWordToSentence("drive");
    }

    public override string GenerateResponse(InterpretedQuery interpretedQuery)
    {
        switch (interpretedQuery.Interrogative)
        {
            case InterrogativeType.What:
                ContactResponse = HandleWhat(interpretedQuery);
                break;
            default:
                break;
        }
        
        return ContactResponse;
    }
}








