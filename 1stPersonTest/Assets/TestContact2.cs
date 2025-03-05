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
        @"^why\sis\sshe\syour\sfriend\?$"
    };

    private string[] FOLLOW_UP_ONE_FOLLOWUPS = new[]
    {
        @"^how\sdid\syou\sfirst\smeet\?$"
    };

    private void Start()
    {
        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
        inputParser = GetComponent<PlayerInputParser>();
        contactNumber = "6666666";
        contactID = 1;
        questionIndex = 0;
    }
    public override void GenerateResponse()
    {
        if (playerInput == string.Empty)
        {
            return;
        }
        
        playerInputSingleSpaceLowerCase =
            Regex.Replace(playerInput, @"\s+", " ").ToLower();
        playerInputSingleSpaceLowerCase =
            Regex.Replace(playerInputSingleSpaceLowerCase, @"^\s+", "");

        inputParser.ParsePlayerinput(playerInputSingleSpaceLowerCase);
        firstKey = inputParser.firstKey;
        questionTarget = inputParser.questionTarget;

        switch (CurrentDialogueState)
        {
            case Dialogue_State.ASKING_QUESTION:
                //CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                contactResponse = GenerateRootResponse(firstKey, secondKey, questionTarget);
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_QUESTION:
                //CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION;
                contactResponse = GenerateFollowUpResponse(questionIndex);
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION:
                contactResponse = GenerateFollowUpFollowUp(followUpQuestionIndex);
                break;                
        }
        Debug.Log(contactResponse);
        //Debug.Log("first key is: "+firstKey);
        //Debug.Log("question target is: "+questionTarget);
        Debug.Log(CurrentDialogueState);
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

    private string GenerateFollowUpResponse(int questionIndex)
    {
        switch (questionIndex)
        {
            case 1:
                foreach (string questionPattern in QUESTION_INDEX_ONE_FOLLOWUPS)
                {
                    if (Regex.IsMatch(playerInputSingleSpaceLowerCase, questionPattern))
                    {
                        followUpQuestionIndex = 1;
                        contactResponse = "Because we grew up together.";
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
                    if (Regex.IsMatch(playerInputSingleSpaceLowerCase, questionPattern))
                    {
                        contactResponse = "We went to the same school";
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
