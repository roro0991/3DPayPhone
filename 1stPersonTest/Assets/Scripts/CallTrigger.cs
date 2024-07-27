using System.Collections;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] DialogueManager dialogueManager;
    
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

        if (!callIsInProgress && numberToCall.ToString().Length == 7)
        {
            switch (numberToCall)
            {
                case 5555555:
                    callIsInProgress = true;
                    StartCoroutine(Call(testCall));
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
        dialogueManager.EnterCallMode(Number);
    }
}
