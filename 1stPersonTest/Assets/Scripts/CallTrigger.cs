using JetBrains.Annotations;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] CallManager callManager;
    [SerializeField] SFXManager sfxManager;

    [Header("Ink JSON Files")]
    [SerializeField] private TextAsset firstNumber; // 225-5446

    int numberToCall;

    float ringTime;

    bool callIsInProgress;

    bool isDailing;

    bool isRinging;

    private void Start()
    {
        callIsInProgress = false;
        isDailing = false;
    }

    private void Update()
    {
        string numberToCall = phoneManager.GetPhoneNumber();

        if (!callIsInProgress && isRinging && phoneManager.GetReceiverStatus() == true)
        {
            callIsInProgress = true;
            sfxManager.dialSource.Stop();
            callManager.EnterCallMode(firstNumber);
        }

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
    
    public void ReceiveCall()
    {
        isRinging = true;
        sfxManager.CallRing();
    }    

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

    public void SetIsDailingStatus(bool status)
    {
        isDailing = status;
    }

}
