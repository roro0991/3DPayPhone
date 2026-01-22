using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        wordBank.AddWordToSentence("Who");
        wordBank.AddWordToSentence("Anna");
        wordBank.AddWordToSentence("Dog");
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








