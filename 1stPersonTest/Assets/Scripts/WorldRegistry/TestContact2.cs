using UnityEngine;

public class TestContact2 : Contact
{
    private void Start()
    {
        ContactID = "sarah";
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
        wordBank.AddWordToSentence("live");
    }

    public override string GenerateResponse(InterpretedQuery interpretedQuery)
    {
        ContactResponse = HandleInterrogativeQuery(interpretedQuery);

        return ContactResponse;
    }
}
