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

    public override string GenerateResponse(InterpretedInputData interpretedQuery)
    {
        if (interpretedQuery.InputMode == InputMode.Query)
        {
            ContactResponse = HandleInterrogativeQuery(interpretedQuery);
        }
        else if (interpretedQuery.InputMode == InputMode.Statement)
        {
            ContactResponse = HandleDeclarative(interpretedQuery);
        }


            return ContactResponse;
    }
}








