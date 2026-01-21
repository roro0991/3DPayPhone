using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary<string, string> inputResponses = new Dictionary<string, string>()
    {
        { "hello.", "who is this?" }
    };
    private Dictionary<string, List<string>> wordsForBank = new Dictionary<string, List<string>>()
    {
        { "who is this?", new List<string>
            { "who", "are", "you" } }
    };
        

    private void Start()
    {
        ContactName = "John Smith";
        OpeningLine = "Hello?";

        AddWordToSentence("Who");
        AddWordToSentence("Anna");
    }

    public override void SpeakFirstLine()
    {
        ContactResponse = OpeningLine;        
    }

    public override string GenerateResponse(SentenceBreakdown sb)
    {
        ContactResponse = string.Empty;

        switch (sb.Intent)
        {
            case Intent.ASK_ABOUT_IDENTITY:
                switch (sb.Topic)
                {
                    case "anna":
                        ContactResponse = "Anna is my very important friend";
                        break;
                    default:
                        ContactResponse = "I've never heard of this person.";
                        break;
                }
                break;
            default:
                ContactResponse = "I don't understand.";
                break;
        }

        return ContactResponse;
    }
}








