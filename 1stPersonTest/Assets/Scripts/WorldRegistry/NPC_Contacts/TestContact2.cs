using UnityEngine;

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

    public override void PopulateWordBank()
    {
        wordBank.AddWordToSentence("you");
        wordBank.AddWordToSentence("do");
        wordBank.AddWordToSentence("john");
    }

    public override string GenerateResponse(InterpretedInputData interpretedQuery)
    {
        ContactResponse = HandleInterrogativeQuery(interpretedQuery);

        return ContactResponse;
    }
}
