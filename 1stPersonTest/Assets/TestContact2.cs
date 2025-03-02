using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class TestContact2 : Contact
{
    private string[] QUESTION_INDEX_ONE_FOLLOWUPS = new[]
    {
        @"^why\?$",
        @"^why\s([\w+\s]+)is\sshe\syour\s([\w+\s]+)friend\?$"
    };

    private void Start()
    {
        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
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

        switch (CurrentDialogueState)
        {
            case Dialogue_State.ASKING_QUESTION:
                ParseTopQuestion();
                if (firstKey != null)
                {
                    switch (firstKey)
                    {
                        case "who":
                            switch (questionTarget)
                            {
                                case "sally jones":
                                    questionIndex = 1;
                                    contactResponse = "Sally Jones is my enemy.";
                                    break;
                                default:
                                    questionIndex = 0;
                                    contactResponse = "I don't know who that is.";
                                    break;
                            }
                            break;
                        default:
                            questionIndex = 0;
                            contactResponse = "I don't understand the question.";
                            break;
                    }                    
                    CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                }
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_QUESTION:
                string playerInputSingleSpaceLowerCase = Regex.Replace(playerInput, @"\s+", " ").ToLower();
                switch (questionIndex)
                {
                    case 1:
                        foreach (string questionPattern in QUESTION_INDEX_ONE_FOLLOWUPS)
                        {
                            if (Regex.IsMatch(playerInputSingleSpaceLowerCase, questionPattern))
                            {
                                contactResponse = "Because she stole my candy!";
                                break;
                            }
                            else
                            {
                                CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                                GenerateResponse();
                                return;
                            }
                        }
                        break;
                    default:
                        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                        GenerateResponse();
                        return;
                }
                break;
            default:
                break;
        }
        Debug.Log("response is: " + contactResponse);
    }
}
