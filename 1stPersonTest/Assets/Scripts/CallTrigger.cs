using JetBrains.Annotations;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] CallManager callManager;
    [SerializeField] SFXManager sfxManager;
    [SerializeField] StoryManager storyManager;

    [Header("Ink JSON Files")]
    [SerializeField] private TextAsset firstNumber; // 225-5446

    int numberToCall;

    float ringTime;

    bool callIsInProgress;

    bool isDailing;

    bool isRinging;

    bool iscallcountDown;

    float callcountDown = 5f;

    private void Start()
    {
        callIsInProgress = false;
        isDailing = false;
        isRinging = false;
        iscallcountDown = false;
    }

    private void Update()
    {
        string numberToCall = phoneManager.GetPhoneNumber();
        //countdown for receiving a call
        if (iscallcountDown == true && callcountDown >= 0 
            && phoneManager.GetReceiverStatus() == false)
        {
            callcountDown -= Time.deltaTime;
            Debug.Log(callcountDown);
        }
        else if (iscallcountDown == true && callcountDown >= 0 
            && phoneManager.GetReceiverStatus() == true)
        {
            callcountDown = 5f;
            Debug.Log(callcountDown);
        }
        else if (iscallcountDown == true && callcountDown <= 0 
            && phoneManager.GetReceiverStatus() == false)
        {
            iscallcountDown = false;
            callcountDown = 5f;
            isRinging = true;
            sfxManager.CallRing();
        }


        //code for handling receiving calls
        if (!callIsInProgress && isRinging && phoneManager.GetReceiverStatus() == true)
        {
            if (storyManager.GetFirstCallStatus() == false)
            {
                storyManager.SetFirstCallStatus(true);
                callIsInProgress = true;
                isRinging = false;
                sfxManager.dialSource.Stop();
                callManager.EnterCallMode(firstNumber);
                callManager.SetLoopCallStatus(true);
            }
            else
            {
                return;
            }
        }

        //code for handling making calls
        if (!callIsInProgress && numberToCall.Length == 3)
        {
            switch (numberToCall)
            {
                case "411":
                    callIsInProgress = true;
                    StartCoroutine(EnterDirectoryMode());
                    break;
                default:
                    break;
            }
        }

        if (!callIsInProgress && numberToCall.ToString().Length == 7)
        {
            switch (numberToCall)
            {
                default:
                    callIsInProgress = true;
                    StartCoroutine(NumberNotInService());
                    break;
            }
        }


        if (phoneManager.GetReceiverStatus() == false)
        {
            StopAllCoroutines();
            callIsInProgress = false;
        }
    }

    //methods
    public void ReceiveCall()
    {
        if (phoneManager.GetReceiverStatus() == false)
        {
            iscallcountDown = true;            
        }
    }    

    IEnumerator Call(TextAsset Number)
    {
        ringTime = Random.Range(5.5f, 10.5f);
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        isDailing = true;
        yield return new WaitForSeconds(ringTime);
        sfxManager.dialSource.Stop();
        callManager.EnterCallMode(Number);
    }

    IEnumerator NumberNotInService()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        isDailing = true;
        yield return new WaitForSeconds(2.5f);
        isDailing = false;
        sfxManager.dialSource.Stop();
        callManager.NotInService();
    }

    IEnumerator EnterDirectoryMode()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        isDailing = true;
        yield return new WaitForSeconds(2.5f);
        isDailing = false;
        sfxManager.dialSource.Stop();
        callManager.EnterDirectoryMode();
    }
    

    //getter methods
    public bool GetCallStatus()
    {
        return callIsInProgress;
    }
    public bool GetIsDailingStatus()
    {
        return isDailing;
    }

    public bool GetIsRingingStatus()
    {
        return isRinging;
    }

    //setter methods
    public void SetIsDailingStatus(bool status)
    {
        isDailing = status;
    }

    public void SetIsRingingStatus(bool status)
    {
        isRinging = status;
    }

}
