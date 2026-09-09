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
        wordBank.AddWordToSentence("live");
        wordBank.AddWordToSentence("chicago");
    }

    public override string GenerateResponse(InterpretedInputData interpretedQuery)
    {
        ContactResponse = HandleInterrogativeQuery(interpretedQuery);
        
        return ContactResponse;
    }
}








