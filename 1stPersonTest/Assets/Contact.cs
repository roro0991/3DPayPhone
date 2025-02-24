using Ink;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contact : MonoBehaviour
{
    private PlayerInputParser inputParser;
    public string playerInput = string.Empty;    
    [SerializeField] string contactName;
    [SerializeField] string contactNumber;

    private void Start()
    {
        inputParser = GetComponent<PlayerInputParser>();
    }
    public void GenerateResponse()
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
                        case "john brown":
                            Debug.Log("John Brown is a my friend.");
                            break;
                        default:
                            Debug.Log("I don't know who that is.");
                                break;
                    }
                    break;
                default:
                    Debug.Log("I don't understand the question.");
                    break;
            }
        }
    }
}
