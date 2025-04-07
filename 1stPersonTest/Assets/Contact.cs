using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    public PlayerInputParser inputParser;
    public string contactNumber; 
    public string playerInput = string.Empty;
    public string contactResponse = string.Empty;
    public int? questionIndex;
    public int followUpQuestionIndex;
    public string firstKey;
    public string secondKey;
    public string questionTarget;
    public string playerInputFormated;

    public enum Dialogue_State
    {
        ASKING_QUESTION,
        ASKING_FOLLOW_UP_QUESTION,
        ASKING_FOLLOW_UP_FOLLOW_UP_QUESTION
    }
    public Dialogue_State CurrentDialogueState;

    public abstract void GenerateResponse();
    
    public void TestValues()
    {
        Debug.Log(contactNumber);
    }
}
