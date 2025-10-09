using System.Collections;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CallManager callManager;
    [SerializeField] private PhoneManager phoneManager;
    [SerializeField] private SFXManager sfxManager;
    [SerializeField] private PhoneNumberManager phoneNumberManager;

    // Optional test contact for debugging
    [SerializeField] private Contact testContact;

    // Call state
    private string numberToCall;
    public bool isCallInProgress;

    private void Start()
    {
        // Optional: assign number for a test contact
        if (testContact != null)
        {
            testContact.DiscoverName(testContact.ContactName); 
            testContact.DiscoverNumber(phoneNumberManager); 
            //string assignedNumber = phoneNumberManager.AssignNumber(testContact);
            //Debug.Log($"TestContact '{testContact.ContactName}' assigned number: {assignedNumber}");
        }
    }

    private void Update()
    {
        // Always update current dialed number
        numberToCall = phoneManager.GetPhoneNumber();

        // Emergency numbers are exceptions
        if (!isCallInProgress && numberToCall == "911")
        {
            isCallInProgress = true;
            StartCoroutine(Call911());
            return;
        }
        if (!isCallInProgress && numberToCall == "411")
        {
            // Optional: directory assistance
            return;
        }

        // Only try to call when the full number length is dialed
        if (!isCallInProgress && phoneManager.GetDigitCount() == phoneNumberManager.FullNumberLength)
        {
            TryCall(numberToCall);
        }

        // Cancel any call if receiver is hung up
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            StopAllCoroutines();
            isCallInProgress = false;

            // Stop ringing audio
            if (sfxManager.dialSource.isPlaying)
                sfxManager.dialSource.Stop();
        }
    }

    private void TryCall(string number)
    {
        // Emergency numbers handled above
        var contact = phoneNumberManager.GetContactByNumber(number);

        // If the number hasn't been discovered yet, no contact exists
        if (contact != null)
        {
            int index = System.Array.IndexOf(callManager.Contacts, contact);
            if (index >= 0)
            {
                callManager.EnterCallMode(index);
                isCallInProgress = true;
            }
        }
        else
        {
            // Number doesn't exist yet, or isn't in service
            isCallInProgress = true;
            StartCoroutine(NumberNotInService());
        }
    }

    private IEnumerator Call911()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        yield return new WaitForSeconds(2.5f);
        sfxManager.dialSource.Stop();
        callManager.Call911();
    }

    private IEnumerator NumberNotInService()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        yield return new WaitForSeconds(2.5f);
        sfxManager.dialSource.Stop();
        callManager.NotInService();
    }

    // This is called when a contact is discovered in the story
    public void DiscoverContact(Contact contact)
    {
        string assignedNumber = phoneNumberManager.GetOrGenerateNumber(contact);
        Debug.Log($"Contact '{contact.ContactName}' discovered! Number assigned: {assignedNumber}");
    }
}


