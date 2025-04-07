using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using System.Reflection;
using UnityEditor;


public class TestContact2 : Contact
{

    int? followUpIndex;

    private string[] QUESTION_INDEX_ONE_FOLLOWUPS = new[]
    {
        @"^why\?$",
        @"^why\sis\sshe\syour\sfriend\?$",
        @"^how\sdid\syou\sfirst\smeet\?$",
        @"^how\slong\shave\syou\sbeen\sfriends\?$"
    };

    private string[] QUESTION_INDEX_TWO_FOLLOWUPS = new[]
    {
        @"^why\?$",
        @"^why\is\she\syour\senemy\?$"
    };

    private string[] FOLLOW_UP_ONE_FOLLOWUPS = new[]
    {
        @"^where\sdid\syou\sgrow\sup\?$"
    };

    private string[] FOLLOW_UP_TWO_FOLLOWUPS = new[]
    {
        @"^how\?$",
        @"^how\sdid\she\sdo\sthat\?$"
    };

    private void Start()
    {
        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
        inputParser = GetComponent<PlayerInputParser>();
        contactNumber = "6666666";
        questionIndex = 0;
    }
    public override void GenerateResponse()
    {
        followUpIndex = null;
        if (playerInput == string.Empty)
        {
            return;
        }
        
        //Formatting input to remove spaces at beginning, end, and extra spaces between words.
        playerInputFormated =
            Regex.Replace(playerInput, @"\s+", " ").ToLower();
        playerInputFormated =
            Regex.Replace(playerInputFormated, @"^\s+", "");
        playerInputFormated =
            Regex.Replace(playerInputFormated, @"\s+$", "");

        inputParser.ParsePlayerinput(playerInputFormated);
        firstKey = inputParser.firstKey;
        questionTarget = inputParser.questionTarget;

        switch (CurrentDialogueState)
        {
            case Dialogue_State.ASKING_QUESTION:
                contactResponse = GenerateRootResponse(firstKey, secondKey, questionTarget);
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_QUESTION:
                contactResponse = GenerateFollowUpResponse(questionIndex);
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION:
                contactResponse = GenerateFollowUpFollowUp(followUpQuestionIndex);
                break;                
        }
        ClearLog();
        Debug.Log("Followup Index: " + followUpIndex);
        Debug.Log(contactResponse);
        //Debug.Log("first key is: "+firstKey);
        //Debug.Log("question target is: "+questionTarget);
        Debug.Log(CurrentDialogueState);
    }

    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    private string GenerateRootResponse(string firstKey, string secondKey, string questionTarget)
    {
        switch (firstKey)
        {
            case "who":
                switch (questionTarget)
                {
                    case "sally jones":
                        questionIndex = 1;
                        contactResponse = "Sally Jones is my friend";
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        break;
                    case "billy brown":
                        questionIndex = 2;
                        contactResponse = "Billy Brown is my enemy";
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        break;
                    default:
                        questionIndex = 0;
                        contactResponse = "I don't know who that is.";
                        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                        break;
                }
                break;
            default:
                questionIndex = 0;
                contactResponse = "I don't understand the question.";
                CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                break;
        }
        return contactResponse;
    }

    private string GenerateFollowUpResponse(int? questionIndex)
    {
        switch (questionIndex)
        {
            case 1:
                for (int i = 0; i < QUESTION_INDEX_ONE_FOLLOWUPS.Length; i++)
                {
                    if (Regex.IsMatch(playerInputFormated, QUESTION_INDEX_ONE_FOLLOWUPS[i]))
                    {
                        followUpIndex = Array.IndexOf(QUESTION_INDEX_ONE_FOLLOWUPS, QUESTION_INDEX_ONE_FOLLOWUPS[i]);
                        switch (followUpIndex)
                        {
                            case 0:
                                followUpQuestionIndex = 1;
                                contactResponse = "Because we grew up together.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                                break;
                            case 1:
                                followUpQuestionIndex = 1;
                                contactResponse = "Because we grew up together.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                                break;
                            case 2:
                                followUpQuestionIndex = 2;
                                contactResponse = "We met in middle school.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                                break;
                            case 3:
                                followUpQuestionIndex = 3;
                                contactResponse = "We've been friends since middle school.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                    else
                    {
                        contactResponse = GenerateRootResponse(firstKey, secondKey, questionTarget);
                        if (contactResponse == "I don't understand the question.")
                        {
                            CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        }
                        //break;
                    }
                }
                break;
            case 2:
                foreach (string questionPattern in QUESTION_INDEX_TWO_FOLLOWUPS)
                {
                    if (Regex.IsMatch(playerInputFormated, questionPattern))
                    {
                        followUpQuestionIndex = 2;
                        contactResponse = "Because he stole my girlfriend.";
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                        break;
                    }
                    else
                    {
                        contactResponse = GenerateRootResponse(firstKey, secondKey, questionTarget);
                        if (contactResponse == "I don't understand the question.")
                        {
                            CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        }
                        break;
                    }
                }
                break;
            default:
                contactResponse = GenerateRootResponse(firstKey, secondKey, questionTarget);
                break;
        }
        return contactResponse;
    }

    private string GenerateFollowUpFollowUp(int followUpQuestionindex)
    {
        switch (followUpQuestionIndex)
        {
            case 1:
                foreach (string questionPattern in FOLLOW_UP_ONE_FOLLOWUPS)
                {
                    if (Regex.IsMatch(playerInputFormated, questionPattern))
                    {
                        contactResponse = "We grew up in Dallas.";
                        break;
                    }
                    else
                    {
                        contactResponse = GenerateFollowUpResponse(questionIndex);
                        if (contactResponse == "I don't understand the question.")
                        {
                            CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                        }
                        break;
                    }
                }
                break;
            case 2:
                foreach (string questionPattern in FOLLOW_UP_TWO_FOLLOWUPS)
                {
                    if (Regex.IsMatch(playerInputFormated, questionPattern))
                    {
                        contactResponse = "None of your business. That's how.";
                        break;
                    }
                    else
                    {
                        contactResponse = GenerateFollowUpResponse(questionIndex);
                        if (contactResponse == "I don't understand the question.")
                        {
                            CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                        }
                        break;
                    }
                }
                break;
            default:
                contactResponse = GenerateFollowUpResponse(questionIndex);
                break;
        }
        return contactResponse;
    }
}
