using System;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{   
    //Managers
    [SerializeField] private CallManager callManager;
    [SerializeField] private CallTrigger callTrigger;
    [SerializeField] private DialogueAudioManager dialogueaudioManager;
    [SerializeField] private PhoneDisplayController phoneDisplayController;
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private SFXManager sfxManager;

    //Receivers
    [SerializeField] private GameObject _upReceiver;
    [SerializeField] private GameObject _downReceiver;

    //Button Animator Array
    [SerializeField] private Animator[] _buttonAnimatorsArray = new Animator[12];
    
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
    
    //Receiver Variables
    private bool _isReceiverPickedUp = false; //keep track of receiver status.

    
    private void Update()
    {
        //Updating phone number and extention number at runtime
        _phoneNumberAsString = string.Join(string.Empty, _phoneNumberArray);
        _extentionNumberAsString = string.Join(string.Empty, _extentionNumberArray);
    }

    //Methods
    public void NumberButton(int input)
    {
        if (_isReceiverPickedUp)    
        {
            if (callTrigger.GetCallStatus() == false)//Checks if player is already in a call
            {
                if (_currentPhoneNumberArrayIndex < _phoneNumberArray.Length 
                    && callManager.GetExtentionStatus() == false) //Checks that number dialed fits array ( > 7 digits)
                {
                    //Add number dialed in phone number array
                    _phoneNumberArray[_currentPhoneNumberArrayIndex] = input;
                    _currentPhoneNumberArrayIndex++;

                    if (_currentPhoneNumberDisplayCharIndex == 41) 
                    {
                        //Add "-" after first 3 digits of phone number
                        phoneDisplayController.displayCharArray[_currentPhoneNumberDisplayCharIndex]
                        .GetComponent<CharController>().DisplayDash();
                        _currentPhoneNumberDisplayCharIndex++;
                    }
                    //Display number dialed on phone display
                    phoneDisplayController.displayCharArray[_currentPhoneNumberDisplayCharIndex]
                    .GetComponent<CharController>().DisplayChar(input);
                    _currentPhoneNumberDisplayCharIndex++;
                }
            }
            else if (callManager.GetExtentionStatus() == true)//Check if player is entering phone extention
            {
                if (_currentExtentionNumberArrayIndex < _extentionNumberArray.Length)//Check that extention dialed fits array ( > 3 digits)
                {
                    //Add extention dialed in extention array
                    _extentionNumberArray[_currentExtentionNumberArrayIndex] = input;
                    _currentExtentionNumberArrayIndex++;
                    //Display extention dialed on phone display
                    phoneDisplayController.displayCharArray[_currentExtentionDisplayCharIndex]
                    .GetComponent<CharController>().DisplayChar(input);
                    _currentExtentionDisplayCharIndex++;
                }
            }
            
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
                {
                    _buttonAnimatorsArray[11].SetTrigger("isPressed");
                    break;
                }
            case 98: // * symbol
                {
                    _buttonAnimatorsArray[9].SetTrigger("isPressed");
                    break;
                }

        }
        sfxManager.ButtonPress();        
    }

    public void PickUpReceiver()
    {
        if (!_isReceiverPickedUp)
        {
            _isReceiverPickedUp = true;
            _downReceiver.SetActive(false);
            _upReceiver.SetActive(true);
            phoneDisplayController.ClearAllDisplayChars();
            sfxManager.ReceiverUP();
        }
        else if (_isReceiverPickedUp)
        {
            /*
            if (callManager.GetCanHangUpStatus() == false)
            {
                return;
            }
            */
            if (callTrigger.GetIsDailingStatus() == true)
            {
                callTrigger.SetIsDailingStatus(false);
                sfxManager.dialSource.Stop();
            }
            if (callManager.GetInDirectoryStatus() == true)
            {
                callManager.ExitDirectoryMode();
            }
            if (puzzleManager.GetPuzzleStatus() == true)
            {
                puzzleManager.ExitPuzzleMode();
            }

            //callManager Methods
            callManager.ExitCallMode();
            callManager.SetAutomatedSystemStatus(false);

            //Receiver Status
            _downReceiver.SetActive(true);
            _upReceiver.SetActive(false);
            _isReceiverPickedUp = false;

            //Phone Display
            phoneDisplayController.ClearAllDisplayChars();
            _currentPhoneNumberDisplayCharIndex = 38;
            _currentExtentionDisplayCharIndex = 46;
            phoneDisplayController.PickUpReceiverMessage();

            //Reset Phone Number and Extention
            Array.Clear(_phoneNumberArray, 0, _phoneNumberArray.Length);
            _currentPhoneNumberArrayIndex = 0;
            Array.Clear(_extentionNumberArray, 0, _extentionNumberArray.Length);
            _currentExtentionNumberArrayIndex = 0;

            //SFX
            sfxManager.ReceiverDown();
            dialogueaudioManager.dialogueaudioSource.Stop();
        }                   
    }

    public void ResetExtention()//External Function for Ink JSON
    {        
        phoneDisplayController.displayCharArray[46].GetComponent<CharController>().ClearChar();
        phoneDisplayController.displayCharArray[47].GetComponent<CharController>().ClearChar();
        phoneDisplayController.displayCharArray[48].GetComponent<CharController>().ClearChar();
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
    public bool GetReceiverStatus()
    {
        return _isReceiverPickedUp;
    }
}

