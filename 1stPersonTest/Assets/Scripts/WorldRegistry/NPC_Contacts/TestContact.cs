using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Dialogue.Core;
using NUnit.Framework.Constraints;

public class TestContact : Contact
{
    private void Awake()
    {
        ContactID = "john";
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
        wordBank.AddWordToSentence("work");
        wordBank.AddWordToSentence("anna");
    }

    public override string GenerateResponse(InterpretedInputData interpretedQuery)
    {
        ContactResponse = HandleInterrogativeQuery(interpretedQuery);
        
        return ContactResponse;
    }
}








