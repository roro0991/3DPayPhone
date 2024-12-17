using System.Collections;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] CallManager callManager;
    [SerializeField] SFXManager sfxManager;
    
    [Header("Ink JSON Files")]
    [SerializeField] private TextAsset handler; //555-3473
    [SerializeField] private TextAsset informant; //554-8923
    [SerializeField] private TextAsset falseNumber; //639-9675
    [SerializeField] private TextAsset johnMccoy; //442-4542

    int numberToCall;

    float ringTime;

    bool callIsInProgress;

    bool isDailing;

    private void Start()
    {
        callIsInProgress = false;
        isDailing = false;
    }

    private void Update()
    {
        string numberToCall = phoneManager.GetPhoneNumber();

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
                case "5553473":
                    callIsInProgress = true;
                    StartCoroutine(Call(handler));
                    break;
                case "5548923":
                    callIsInProgress = true;
                    StartCoroutine(Call(informant));
                    break;
                case "6399675":
                    callIsInProgress = true;
                    StartCoroutine(Call(falseNumber));                    
                    return;
                case "4424542":
                    callIsInProgress = true;
                    StartCoroutine(Call(johnMccoy));
                    break;
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

    // getter methods

    public bool GetCallStatus()
    {
        return callIsInProgress;
    }
    public bool GetIsDailingStatus()
    {
        return isDailing;
    }

    public void SetIsDailingStatus(bool status)
    {
        isDailing = status;
    }

}
