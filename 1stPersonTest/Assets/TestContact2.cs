using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestContact2 : Contact
{
    private void Start()
    {
        inputParser = GetComponent<PlayerInputParser>();
        contactNumber = "6666666";
        contactID = 1;
    }
    public override void GenerateResponse()
    {
        if (playerInput == string.Empty)
        {
            return;
        }
        inputParser.ParsePlayerinput(playerInput);
        string firstKey = inputParser.firstKey;
        string secondKey = inputParser.secondKey;
        string questionTarget = inputParser.questionTarget;

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
                        case "sally jones":
                            contactResponse = "Sally Jones is my enemy.";
                            break;
                        default:
                            contactResponse = "I don't know who that is.";
                            break;
                    }
                    break;
                default:
                    contactResponse = "I don't understand the question.";
                    break;
            }
        }
        Debug.Log(contactResponse);
    }
}
