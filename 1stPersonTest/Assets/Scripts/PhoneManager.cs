using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{    
    [SerializeField] private PhoneDisplayController phoneDisplayController;
    [SerializeField] private CallManager callManager;
    [SerializeField] private CallTrigger callTrigger;
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private SFXManager sfxManager;
    [SerializeField] private DialogueAudioManager dialogueaudioManager;

    [SerializeField] private GameObject upReceiver;
    [SerializeField] private GameObject downReceiver;

    [SerializeField] private Animator[] buttonAnimators = new Animator[12];

    public Animator receiverAnimator;
    
    // phone number variables
    private int?[] phoneNumber = new int?[7]; // to store the player input
    private int currentNumberIndex = 0; // to track where in the phone number we are
    private string phoneNumberAsString; // for calltrigger
    private int currentDisplayCharIndex = 38; // to set where to display numbers on phone display    

    // extention variables
    private int?[] extentionNumber = new int?[3];
    private int currentExtentionNumberIndex = 0;
    private string extentionAsString;
    private int currentExtentionDisplayCharIndex = 46;
    
    private bool receiverIsPickedUp = false;

    
    private void Update()
    {
        phoneNumberAsString = string.Join(string.Empty, phoneNumber);

        extentionAsString = string.Join(string.Empty, extentionNumber);

    }
    public void NumberButton(int input)
    {
        if (receiverIsPickedUp)
        {
            if (callTrigger.GetCallStatus() == false)
            {
                if (currentNumberIndex < phoneNumber.Length && callManager.GetExtentionStatus() == false)
                {                
                    phoneNumber[currentNumberIndex] = input;
                    currentNumberIndex++;
                    if (currentDisplayCharIndex == 41)
                    {
                        phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayDash();
                        currentDisplayCharIndex++;
                    }
                    phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayChar(input);
                    currentDisplayCharIndex++;
                }
            }

            if (callManager.GetExtentionStatus() == true)
            {
                if (currentExtentionNumberIndex < extentionNumber.Length)
                {
                    extentionNumber[currentExtentionNumberIndex] = input;
                    currentExtentionNumberIndex++;
                    phoneDisplayController.chars[currentExtentionDisplayCharIndex].GetComponent<CharController>().DisplayChar(input);
                    currentExtentionDisplayCharIndex++;
                }
            }
        }
        if (input == 0)
        {
            buttonAnimators[10].SetTrigger("isPressed");
        }
        else
        {
            buttonAnimators[input - 1].SetTrigger("isPressed");            
        }
        sfxManager.ButtonPress();
    }

    public void SymbolButton (int input)
    {
        if (input == 97) // # symbol
        {
            buttonAnimators[11].SetTrigger("isPressed");
        }
        else if (input == 98) // * symbol
        {
            buttonAnimators[9].SetTrigger("isPressed");
        }
        sfxManager.ButtonPress();
    }

    public void PickUpReceiver()
    {
        if (!receiverIsPickedUp)
        {
            receiverIsPickedUp = true;
            downReceiver.SetActive(false);
            upReceiver.SetActive(true);
            //receiverAnimator.SetBool("isPickedUp", true);
            phoneDisplayController.ClearAllChars();
            sfxManager.ReceiverUP();
        }
        else if (receiverIsPickedUp)
        {
            if (callManager.GetInDialogueStatus() == true)
            {
                return;
            }
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

            //receiverAnimator.SetBool("isPickedUp", false);
            callManager.SetAutomatedSystemStatus(false);
            downReceiver.SetActive(true);
            upReceiver.SetActive(false);
            receiverIsPickedUp = false;
            phoneDisplayController.ClearAllChars();
            currentDisplayCharIndex = 38;
            Array.Clear(phoneNumber, 0, phoneNumber.Length);
            currentNumberIndex = 0;
            currentExtentionDisplayCharIndex = 46;
            Array.Clear(extentionNumber, 0, extentionNumber.Length);
            currentExtentionNumberIndex = 0;
            phoneDisplayController.PickUpReceiverMessage();
            sfxManager.ReceiverDown();
            dialogueaudioManager.dialogueaudioSource.Stop();
        }                   
    }

    public void ResetExtention()
    {
        phoneDisplayController.chars[46].GetComponent<CharController>().ClearChar();
        phoneDisplayController.chars[47].GetComponent<CharController>().ClearChar();
        phoneDisplayController.chars[48].GetComponent<CharController>().ClearChar();
        currentExtentionDisplayCharIndex = 46;
        Array.Clear(extentionNumber, 0, extentionNumber.Length);
        currentExtentionNumberIndex = 0;
    }

    // Getter methods

    public string GetExtentionNumber()
    {
        return extentionAsString;
    }
    public string GetPhoneNumber()
    {
        return phoneNumberAsString;
    }

    public bool GetReceiverStatus()
    {
        return receiverIsPickedUp;
    }
}

