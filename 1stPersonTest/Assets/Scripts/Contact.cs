using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    public PlayerInputParser inputParser;
    public string ContactNumber; 
    public string PlayerInput = string.Empty;
    public string ContactResponse = string.Empty;
    public float? QuestionIndex;
    public int FollowUpQuestionIndex;
    public string FirstKey;
    public string SecondKey;
    public string QuestionTarget;
    public string PlayerInputFormated;

    public enum Dialogue_State
    {
        GREETING,
        ASKING_QUESTION,
        ASKING_FOLLOW_UP_QUESTION
    }
    public Dialogue_State CurrentDialogueState;

    public abstract void GenerateResponse();
    
    public void TestValues()
    {
        Debug.Log(ContactNumber);
    }
}
