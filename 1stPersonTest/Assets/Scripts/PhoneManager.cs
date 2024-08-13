using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{    
    [SerializeField] private PhoneDisplayController phoneDisplayController;
    [SerializeField] private CallManager callManager;
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private SFXManager sfxManager;

    [SerializeField] private Transform receiver;
    [SerializeField] private Transform mainCamera;

    [SerializeField] private Animator[] buttonAnimators = new Animator[12];

    public Animator receiverAnimator;
    public Animator cameraAnimator;
    
    // phone number variables
    private int?[] phoneNumber = new int?[7]; // to store the player input
    private int currentNumberIndex = 0; // to track where in the phone number we are
    private int numberAsInt; // to send to call trigger
    private int currentDisplayCharIndex = 38; // to set where to display numbers on phone display    

    // extention variables
    private int?[] extentionNumber = new int?[3];
    private int currentExtentionNumberIndex = 0;
    private int extentionAsInt;
    private int currentExtentionDisplayCharIndex = 46;
    
    private bool receiverIsPickedUp = false;
    private bool cameraIsZoomedIn = false;

    
    private void Update()
    {
        string phoneNumberAsString = string.Join(string.Empty, phoneNumber);
        int.TryParse(phoneNumberAsString, out numberAsInt);

        string extentionAsString = string.Join(string.Empty, extentionNumber);
        int.TryParse(extentionAsString, out extentionAsInt);
    }
    public void NumberButton(int input)
    {
        if (receiverIsPickedUp)
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
            buttonAnimators[9].SetTrigger("isPressed");
    }

    public void PickUpReceiver()
    {
        if (!receiverIsPickedUp)
        {
            receiverIsPickedUp = true;
            receiver.transform.SetParent(mainCamera);
            receiverAnimator.SetBool("isPickedUp", true);
            phoneDisplayController.ClearAllChars();
            sfxManager.ReceiverUP();
        }
        else if (receiverIsPickedUp)
        {
            if (callManager.GetInDialogueStatus() == true)
            {
                callManager.ExitCallMode();
            }
            if (callManager.GetInDirectoryStatus() == true)
            {
                callManager.ExitDirectoryMode();
            }
            if (puzzleManager.GetPuzzleStatus() == true)
            {
                puzzleManager.ExitPuzzleMode();
            }

            receiverAnimator.SetBool("isPickedUp", false);
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
        }                   
    }

    public void ZoomInOnDisplay()
    {
        if (!cameraIsZoomedIn)
        {
            cameraAnimator.SetBool("isZoomedInOnDisplay", true);
            cameraIsZoomedIn = true;
        }
        else
        {
            cameraAnimator.SetBool("isZoomedInOnDisplay", false);
            cameraIsZoomedIn = false;
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

    private void SetParent(Transform child, Transform newParent)
    {
        child.transform.SetParent(newParent);
    }

    IEnumerator AnimateReceiver()
    {
        receiverIsPickedUp = true;
        receiverAnimator.SetBool("isPickedUp", true);
        yield return new WaitForSeconds(1f);
        SetParent(receiver, mainCamera);
    }

    // Getter methods

    public int GetExtentionNumber()
    {
        return extentionAsInt;
    }
    public int GetPhoneNumber()
    {
        return numberAsInt;
    }

    public bool GetReceiverStatus()
    {
        return receiverIsPickedUp;
    }
}

