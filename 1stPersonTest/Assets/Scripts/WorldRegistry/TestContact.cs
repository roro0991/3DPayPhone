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
        ContactName = "john";
        OpeningLine = "Hello?";
        
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;        
    }

    public override void PopulateWordBank()
    {
        wordBank.AddWordToSentence("you");
        wordBank.AddWordToSentence("do");
        wordBank.AddWordToSentence("live");
    }

    public override string GenerateResponse(InterpretedQuery interpretedQuery)
    {
        switch (interpretedQuery.Interrogative)
        {
            case InterrogativeType.What:
                ContactResponse = HandleInterrogativeQuery(interpretedQuery);
                break;
            case InterrogativeType.Where:
                ContactResponse = HandleInterrogativeQuery(interpretedQuery);
                break;
            default:
                break;
        }
        
        return ContactResponse;
    }
}








