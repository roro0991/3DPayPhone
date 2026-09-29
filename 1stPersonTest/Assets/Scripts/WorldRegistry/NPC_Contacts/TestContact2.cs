using UnityEngine;
using Dialogue.Core;

public class TestContact2 : Contact
{
    private void Awake()
    {
        ContactID = "anna";
        OpeningLine = "Who is it?";

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
