using System.Collections;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] CallManager callManager;
    
    [Header("Ink JSON Files")]
    [SerializeField] private TextAsset testCall; //555-5555

    int numberToCall;

    bool callIsInProgress;

    private void Start()
    {
        callIsInProgress = false;
    }

    private void Update()
    {
        int numberToCall = phoneManager.GetPhoneNumber();

        if (!callIsInProgress && numberToCall.ToString().Length == 3)
        {
            switch (numberToCall)
            {
                case 411:
                    callIsInProgress = true;
                    callManager.EnterDirectoryMode();
                    break;
                default:
                    break;
            }
        }

        if (!callIsInProgress && numberToCall.ToString().Length == 7)
        {
            switch (numberToCall)
            {
                case 5555555:
                    callIsInProgress = true;
                    StartCoroutine(Call(testCall));
                    break;
                default:
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
        yield return new WaitForSeconds(1.5f);
        callManager.EnterCallMode(Number);
    }
}
