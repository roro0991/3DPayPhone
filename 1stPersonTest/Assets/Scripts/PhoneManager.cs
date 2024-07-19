using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class PhoneManager : MonoBehaviour
{
    private int?[] phoneNumber = new int?[7];

    private int currentNumberIndex = 0;
    private int currentDisplayCharIndex = 0;


    public Animator receiverAnimator;

    [SerializeField] private PhoneDisplayController phoneDisplayController;

    private bool receiverIsPickedUp = false;

    private void Update()
    {
        string phoneNumberAsString = string.Join(string.Empty, phoneNumber);
    }
    public void NumberButton(int number)
    {
        if (receiverIsPickedUp)
        {
            if (currentNumberIndex < phoneNumber.Length)
            {
                phoneNumber[currentNumberIndex] = number;
                currentNumberIndex++;
                if (currentDisplayCharIndex == 3)
                {
                    phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayDash();
                    currentDisplayCharIndex++;
                }
                phoneDisplayController.chars[currentDisplayCharIndex].GetComponent<CharController>().DisplayChar(number);
                currentDisplayCharIndex++;
            }
        Debug.Log(number);
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
            receiverAnimator.SetBool("isPickedUp", false);
            receiverIsPickedUp = false;
            Array.Clear(phoneNumber, 0, phoneNumber.Length);
            currentNumberIndex = 0;
            phoneDisplayController.ClearAllChars();
            currentDisplayCharIndex = 0;
        }
    }

}

