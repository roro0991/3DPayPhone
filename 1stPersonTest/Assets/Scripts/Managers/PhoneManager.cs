using System;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{   
    [Header("Managers")]
    [SerializeField] private CallManager callManager;
    [SerializeField] private CallTrigger callTrigger;
    [SerializeField] private DialogueAudioManager dialogueaudioManager;
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private SFXManager sfxManager;

    [Header("Display")]
    [SerializeField] public GameObject[] displayCharArray = new GameObject[85];
    [SerializeField] private GameObject[] messageLineArray = new GameObject[18]; // 34-51
    private string _pickUpReceiverString = "lift receiver";

    [Header("Receiver")]
    [SerializeField] private GameObject _upReceiver;
    [SerializeField] private GameObject _downReceiver;

    [Header("Button Animators")]
    [SerializeField] private Animator[] _buttonAnimatorsArray = new Animator[12];
    
    //Phone State
    public enum State
    {
        RECEIVER_DOWN,
        RECEIVER_UP
    };
    public State _currentState;    

    //Phone Number Variables
    private int?[] _phoneNumberArray = new int?[7]; //to store player input (number being dialed)
    private int _currentPhoneNumberArrayIndex = 0; //to track where in the _phoneNumberArray we are
    private string _phoneNumberAsString; //converted phone number as a string to send to callTrigger
    private int _currentPhoneNumberDisplayCharIndex = 38; //to set where to display numbers on phone display   

    //Extention Variables
    private int?[] _extentionNumberArray = new int?[3]; //to store player input (extention being dialed)
    private int _currentExtentionNumberArrayIndex = 0; //to track where in the _extentionNumberArray we are
    private string _extentionNumberAsString; //converted extention number as a string to send to callManager
                                            //currenlty handled by Ink JSON External Function.
    private int _currentExtentionDisplayCharIndex = 46; //to set where to display numbers on phone display
    

    private void Start()
    {
        _currentState = State.RECEIVER_DOWN;
        ClearDisplay();
        StartCoroutine(AnimateMessage());
    }

    private void Update()
    {
        //Updating phone number and extention number at runtime
        _phoneNumberAsString = string.Join(string.Empty, _phoneNumberArray);
        _extentionNumberAsString = string.Join(string.Empty, _extentionNumberArray);
    }

    //Display Methods
    public void ClearDisplay()
    {
        foreach (GameObject character in displayCharArray)
        {
            character.GetComponent<CharController>().ClearChar();
        }
    }

    private void PickUpReceiverMessage()
    {
        ClearDisplay();
        int index = 0;
        foreach (char letter in _pickUpReceiverString.ToCharArray())
        {
            int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
            messageLineArray[index].GetComponent<CharController>().DisplayChar(letterAsInt);
            index++;
        }
    }

    IEnumerator AnimateMessage()
    {
        while (true)
        {
            if (_currentState == State.RECEIVER_DOWN)
            {
                for (int i = messageLineArray.Length - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        messageLineArray[i] = messageLineArray[i - 1];
                    }
                    else
                    {
                        messageLineArray[i] = messageLineArray[messageLineArray.Length - 1];
                    }
                }
                PickUpReceiverMessage();
                yield return new WaitForSeconds(.3f);
            }
            yield return null;
        }
    }

    //Button Methods
    public void NumberButton(int input)
    {
        switch (_currentState)
        {
            case State.RECEIVER_DOWN:
                break;
            case State.RECEIVER_UP:
                if (callTrigger.GetCallStatus() == true 
                    && callManager.GetExtentionStatus() == false)//Checks if player is already in a call
                {
                    break;
                }
                
                if (_currentPhoneNumberArrayIndex < _phoneNumberArray.Length
                    && callManager.GetExtentionStatus() == false) //Checks that number dialed fits array ( > 7 digits)
                {
                    //Add number dialed in phone number array
                    _phoneNumberArray[_currentPhoneNumberArrayIndex] = input;
                    _currentPhoneNumberArrayIndex++;

                    if (_currentPhoneNumberDisplayCharIndex == 41)
                    {
                        //Add "-" after first 3 digits of phone number
                        displayCharArray[_currentPhoneNumberDisplayCharIndex]
                        .GetComponent<CharController>().DisplayDash();
                        _currentPhoneNumberDisplayCharIndex++;
                    }
                    //Display number dialed on phone display
                    displayCharArray[_currentPhoneNumberDisplayCharIndex]
                    .GetComponent<CharController>().DisplayChar(input);
                    _currentPhoneNumberDisplayCharIndex++;
                    break;
                }
                
                if (_currentExtentionNumberArrayIndex < _extentionNumberArray.Length)//Check that extention dialed fits array ( > 3 digits)
                {
                    //Add extention dialed in extention array
                    _extentionNumberArray[_currentExtentionNumberArrayIndex] = input;
                    _currentExtentionNumberArrayIndex++;
                    //Display extention dialed on phone display
                    displayCharArray[_currentExtentionDisplayCharIndex]
                    .GetComponent<CharController>().DisplayChar(input);
                    _currentExtentionDisplayCharIndex++;
                }                
                break;
        }

        //Animating Buttons Pressed
        if (input == 0)
        {
            _buttonAnimatorsArray[10].SetTrigger("isPressed");
        }
        else
        {
            _buttonAnimatorsArray[input - 1].SetTrigger("isPressed");
        }
        sfxManager.ButtonPress();
    }

    //Animating # and * Buttons
    public void SymbolButton (int input)
    {
        switch (input)
        {
            case 97: // # symbol
                _buttonAnimatorsArray[11].SetTrigger("isPressed");
                break;
            case 98: // * symbol                
                _buttonAnimatorsArray[9].SetTrigger("isPressed");
                break;
            default:
                break;
        }
        sfxManager.ButtonPress();        
    }

    //Receiver Methods
    public void PickUpReceiver()
    {
        switch (_currentState)
        {
            case State.RECEIVER_DOWN:

                _currentState = State.RECEIVER_UP;
                _downReceiver.SetActive(false);
                _upReceiver.SetActive(true);
                ClearDisplay();
                sfxManager.ReceiverUP();
                break;

            case State.RECEIVER_UP:   
                
                //Receiver Status
                _currentState = State.RECEIVER_DOWN;
                _downReceiver.SetActive(true);
                _upReceiver.SetActive(false);

                //Reset Phone Display
                PickUpReceiverMessage();
                _currentPhoneNumberDisplayCharIndex = 38;
                _currentExtentionDisplayCharIndex = 46;

                //Reset Phone Number and Extention
                Array.Clear(_phoneNumberArray, 0, _phoneNumberArray.Length);
                _currentPhoneNumberArrayIndex = 0;
                Array.Clear(_extentionNumberArray, 0, _extentionNumberArray.Length);
                _currentExtentionNumberArrayIndex = 0;

                //SFX
                sfxManager.ReceiverDown();
                dialogueaudioManager.dialogueaudioSource.Stop();
                break;

            default:
                break;
        }
    }

    public void ResetExtention()//External Function for Ink JSON
    {
        displayCharArray[46].GetComponent<CharController>().ClearChar();
        displayCharArray[47].GetComponent<CharController>().ClearChar();
        displayCharArray[48].GetComponent<CharController>().ClearChar();
        _currentExtentionDisplayCharIndex = 46;
        Array.Clear(_extentionNumberArray, 0, _extentionNumberArray.Length);
        _currentExtentionNumberArrayIndex = 0;
    }

    // Getter methods
    public string GetExtentionNumber()
    {
        return _extentionNumberAsString;
    }
    public string GetPhoneNumber()
    {
        return _phoneNumberAsString;
    }
    public State GetReceiverStatus()
    {
        return _currentState;
    }
}

