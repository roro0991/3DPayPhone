using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using System.Reflection;
using UnityEditor;
using Unity.VisualScripting;


public class TestContact2 : Contact
{
    private string WHAT_ABOUT_PATTERN = @"what\sabout\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$";
    private string[] QUESTION_INDEX_ONE_FOLLOWUPS = new[]
    {
        @"(^why\?([?!]+)?$)|(^why\s([\w+\s]+)?are\syou\s([\w+\s]+)?friends\?([?!]+)?$)|(^why\s([\w+\s]+)?is\sshe\syour\s([\w+\s]+)?friend\?([?!]+)?$)",
        @"^how\s([\w+\s]+)?did\syou\s([\w+\s]+)?meet\?([?!]+)?$",
        @"^how\s([\w+\s]+)?long\shave\syou\s([\w+\s]+)?been\s([\w+\s]+)?friends(\sfor)?\?([?!]+)?$"
    };
    private void Start()
    {
        inputParser = GetComponent<PlayerInputParser>();
        ContactNumber = "6666666";
        QuestionIndex = 0;
    }
    public override void GenerateResponse()
    {
        //followUpIndex = null;
        if (PlayerInput == string.Empty)
        {
            return;
        }
        
        //Formatting input to remove spaces at beginning, end, and extra spaces between words.
        PlayerInputFormated =
            Regex.Replace(PlayerInput, @"\s+", " ").ToLower();
        PlayerInputFormated =
            Regex.Replace(PlayerInputFormated, @"^\s+", "");
        PlayerInputFormated =
            Regex.Replace(PlayerInputFormated, @"\s+$", "");

        inputParser.ParsePlayerinput(PlayerInputFormated);        
        FirstKey = inputParser.FirstKey;
        SecondKey = inputParser.SecondKey;
        QuestionTarget = inputParser.QuestionTarget;

        if (FirstKey == "obscene")
        {
            Debug.Log("I don't appreciate that kind of language.");
            return;
        }

        if (CurrentDialogueState != Dialogue_State.ASKING_FOLLOW_UP_QUESTION)        
        {
            switch (inputParser.CurrentDialogueStateAsInt)
            {
                case 1:
                    CurrentDialogueState = Dialogue_State.GREETING;
                    break;
                case 2:
                    CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                    break;
                default:
                    break;
            }
        }

        switch (CurrentDialogueState)
        {
            case Dialogue_State.GREETING:
                ContactResponse = "hello there.";
                break;
            case Dialogue_State.ASKING_QUESTION:
                ContactResponse = GenerateRootResponse(FirstKey, SecondKey, QuestionTarget);
                break;
            case Dialogue_State.ASKING_FOLLOW_UP_QUESTION:
                ContactResponse = GenerateFollowUpResponse();
                break;
            default:
                break;
        }
        ClearLog();
        Debug.Log(ContactResponse);
        Debug.Log("FirstKey is: "+FirstKey);
        Debug.Log("SecondKey is: " + SecondKey);
        Debug.Log("QuestionTarget is: "+QuestionTarget);
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
            case "do":
                switch (SecondKey)
                {
                    case "know":
                        switch (questionTarget)
                        {
                            case "sally jones":
                                QuestionIndex = 1;
                                ContactResponse = "Sally Jones is my friend.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                                break;
                            default:
                                QuestionIndex = 0;
                                ContactResponse = "I don't know who that is.";
                                CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                                break;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case "who":
                switch (questionTarget)
                {
                    case "you":
                        QuestionIndex = 0;
                        ContactResponse = "The name's Billy Bob, but my friends call me Bill.";
                        break;
                    case "sally jones":
                        QuestionIndex = 1;
                        ContactResponse = "Sally Jones is my friend.";
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        break;
                    default:
                        QuestionIndex = 0;
                        ContactResponse = "I don't know who you're talking about.";
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;
                        break;
                }
                break;
            case "what":
                switch (questionTarget)
                {
                    case "name":
                        QuestionIndex = 0;
                        ContactResponse = "The name's Billy Bob, but my friends call me Bill.";
                        break;
                    default:
                        QuestionIndex = 0;
                        ContactResponse = "I don't know what you're asking me.";
                        CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                        break;

                }
                break;
            default:
                QuestionIndex = 0;
                ContactResponse = "I don't understand the question.";
                CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
                break;
        }
        return ContactResponse;
    }

    private string GenerateFollowUpResponse()
    {
        string targetQuestion = string.Empty;
        switch (QuestionIndex)
        {
            case 0:                
                if (Regex.IsMatch(PlayerInputFormated, WHAT_ABOUT_PATTERN))
                {
                    var inputMatch = Regex.Match(PlayerInputFormated, WHAT_ABOUT_PATTERN);
                    QuestionTarget = inputMatch.Groups["fullName"].ToString();
                    FirstKey = "who";
                    SecondKey = "";
                    GenerateRootResponse(FirstKey, SecondKey, QuestionTarget);
                    break;
                }
                else
                {
                    GenerateRootResponse(FirstKey, SecondKey, QuestionTarget);
                    break;
                }
            case 1:
                if (inputParser.CurrentDialogueStateAsInt == 1)
                {
                    CurrentDialogueState = Dialogue_State.GREETING;
                    ContactResponse = "hello there.";
                    break;
                }

                foreach (string questionPattern in QUESTION_INDEX_ONE_FOLLOWUPS)
                {
                    if (Regex.IsMatch(PlayerInputFormated, questionPattern))
                    {
                        targetQuestion = questionPattern;
                        break;
                    }                    
                }

                if (targetQuestion != "")
                {
                    switch (targetQuestion)
                    {
                        case @"(^why\?([?!]+)?$)|(^why\s([\w+\s]+)?are\syou\s([\w+\s]+)?friends\?([?!]+)?$)|(^why\s([\w+\s]+)?is\sshe\syour\s([\w+\s]+)?friend\?([?!]+)?$)":
                            ContactResponse = "Because we grew up together.";
                            break;
                        case @"^how\s([\w+\s]+)?did\syou\s([\w+\s]+)?meet\?([?!]+)?$":
                            ContactResponse = "We met in school.";
                            break;
                        case @"^how\s([\w+\s]+)?long\shave\syou\s([\w+\s]+)?been\s([\w+\s]+)?friends(\sfor)?\?([?!]+)?$":
                            ContactResponse = "We've been friends for about ten years.";
                            break;
                    }
                    return ContactResponse;
                }
                else
                {
                    ContactResponse = GenerateRootResponse(FirstKey, SecondKey, QuestionTarget);
                    if (ContactResponse == "I don't understand the question.")
                    {
                        QuestionIndex = 1;
                        CurrentDialogueState = Dialogue_State.ASKING_FOLLOW_UP_QUESTION;                        
                    }
                }
                break;
            default:
                ContactResponse = GenerateRootResponse(FirstKey, SecondKey, QuestionTarget);
                break;
        }
        return ContactResponse;
    }
}
