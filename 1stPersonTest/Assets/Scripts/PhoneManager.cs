using System;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{    
    [SerializeField] private PhoneDisplayController phoneDisplayController;
    [SerializeField] private DialogueManager dialogueManager;
        
    public Animator receiverAnimator;
    public Animator cameraAnimator;
        
    private int[] phoneNumber = new int[7]; // to store the player input
    private int currentNumberIndex = 0; // to track where in the phone number we are
    private int currentDisplayCharIndex = 28; // to set where to display numbers on phone display
    private int numberAsInt; // to send to call trigger

    private bool receiverIsPickedUp = false;
    private bool cameraIsZoomedIn = false;

    private void Update()
    {
        string phoneNumberAsString = string.Join(string.Empty, phoneNumber);
        int.TryParse(phoneNumberAsString, out numberAsInt);
    }
    public void NumberButton(int input)
    {
        if (receiverIsPickedUp)
        {
            if (currentNumberIndex < phoneNumber.Length)
            {
                phoneNumber[currentNumberIndex] = input;
                currentNumberIndex++;
                if (currentDisplayCharIndex == 31)
                {
                    phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayDash();
                    currentDisplayCharIndex++;
                }
                phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayChar(input);
                currentDisplayCharIndex++;
            }        
        }
    }

    public void PickUpReceiver()
    {
        if (!receiverIsPickedUp)
        {
            receiverAnimator.SetBool("isPickedUp", true);
            receiverIsPickedUp = true;
        }
        else
        {
            // end call
            dialogueManager.ExitCallMode();
            // update receiver status
            receiverAnimator.SetBool("isPickedUp", false);
            receiverIsPickedUp = false;
            // clear phone number array            
            Array.Clear(phoneNumber, 0, phoneNumber.Length);
            currentNumberIndex = 0;
            // clear phone display
            phoneDisplayController.ClearAllChars();
            currentDisplayCharIndex = 28;
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

    public int GetPhoneNumber()
    {
        return numberAsInt;
    }

    public bool GetReceiverStatus()
    {
        return receiverIsPickedUp;
    }
}

