using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    public PlayerInputParser inputParser;
    public string contactNumber;
    public int contactID;    
    public string playerInput = string.Empty;
    public string contactResponse = string.Empty;
    public bool isAskingFollowUp;
    public int questionIndex;
    public string firstKey;
    public string secondKey;
    public string questionTarget;

    public enum Dialogue_State
    {
        ASKING_QUESTION,
        ASKING_FOLLOW_UP_QUESTION
    }
    public Dialogue_State CurrentDialogueState;

    public abstract void GenerateResponse();

    public void ParseTopQuestion()
    {
        inputParser.ParsePlayerinput(playerInput);
        firstKey = inputParser.firstKey;
        secondKey = inputParser.secondKey;
        questionTarget = inputParser.questionTarget;

        //Debug.Log("The first question key is: " + firstKey);
        //Debug.Log("The second question key is: " + secondKey);
        //Debug.Log("The question target is: " + questionTarget);
    }
    
    public void TestValues()
    {
        Debug.Log(contactNumber);
        Debug.Log(contactID);
    }
}
