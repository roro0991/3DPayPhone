using Ink;
using Ink.Parsed;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private void Start()
    {                
        inputParser = GetComponent<PlayerInputParser>();
        ContactNumber = "5555555";
    }
    public override void GenerateResponse()
    {
        if (PlayerInput == string.Empty)
        {
            return;
        }
        inputParser.ParsePlayerinput(PlayerInput);
        string firstKey = inputParser.FirstKey;
        string secondKey = inputParser.SecondKey;
        string questionTarget = inputParser.QuestionTarget;

        Debug.Log("The first question key is: " + firstKey);
        Debug.Log("The second question key is: " + secondKey);
        Debug.Log("The question target is: " + questionTarget);

        if (firstKey != null)
        {
            switch (firstKey)
            {
                case "who":
                    switch (questionTarget)
                    {
                        case "john brown":
                            ContactResponse = "John Brown is a my friend.";
                            break;
                        default:
                            ContactResponse = "I don't know who that is.";
                                break;
                    }
                    break;
                default:
                    ContactResponse = "I don't understand the question.";
                    break;
            }
        }
        Debug.Log(ContactResponse);
    }
}
