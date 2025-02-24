using System.Collections;
using UnityEngine;

public class CallTrigger : MonoBehaviour
{
    //Caching other managers to access functions
    [SerializeField] CallManager callManager;
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] SFXManager sfxManager;
    [SerializeField] StoryManager storyManager;

    //JSON for story/display dialogue (other dialogue handled via audio)
    [Header("Ink JSON Files")]
    [SerializeField] private TextAsset firstNumber;
    [SerializeField] private TextAsset secondNumber; // 225-5446

    //making&receiving call variables
    private string _numberToCall;
    private float _ringTime;    
    private float INCOMING_CALL_COUNTDOWN = 5f;

    //state bools
    private bool _isCallCountDown;
    private bool _isRinging;
    private bool _isDailing;
    private bool _isCallInProgress;

    private void Start()
    {
        // initializing bools in start
        _isCallCountDown = false;
        _isRinging = false;
        _isDailing = false;
        _isCallInProgress = false;
    }

    private void Update()
    {
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN && _isDailing)
        {
                _isDailing = false;
        }

        //updating number being dialed at runtime
        _numberToCall = phoneManager.GetPhoneNumber();

        //receiving a call
        if (_isCallCountDown)
        {
            switch (phoneManager.GetReceiverStatus())
            {
                case PhoneManager.State.RECEIVER_DOWN:
                    switch (INCOMING_CALL_COUNTDOWN)
                    {
                        case >= 0:
                            INCOMING_CALL_COUNTDOWN -= Time.deltaTime;
                            Debug.Log(INCOMING_CALL_COUNTDOWN);
                            break;
                        case <= 0:
                            _isCallCountDown = false;
                            INCOMING_CALL_COUNTDOWN = 5f;
                            _isRinging = true;
                            sfxManager.CallRing();
                            break;
                        default:
                            break;
                    }
                    break;
                case PhoneManager.State.RECEIVER_UP:
                    switch (INCOMING_CALL_COUNTDOWN)
                    {
                        case >= 0:
                            INCOMING_CALL_COUNTDOWN = 5f;
                            Debug.Log(INCOMING_CALL_COUNTDOWN);
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        //answering a call
        if (!_isCallInProgress 
            && _isRinging && phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_UP)
        {
            if (storyManager.GetFirstCallStatus() == false)
            {
                storyManager.SetFirstCallStatus(true);
                _isCallInProgress = true;
                _isRinging = false;
                sfxManager.dialSource.Stop();
                //callManager.EnterCallMode(1, 0);
                //callManager.SetLoopCallStatus(true);
            }
            else
            {
                return;
            }
        }

        //making a call
        if (!_isCallInProgress && _numberToCall.Length > 0)
        {
            switch (_numberToCall.Length)
            {
                case 3:
                    switch (_numberToCall)
                    {
                        case "411":
                            _isCallInProgress = true;
                            StartCoroutine(EnterDirectoryMode());
                            break;
                    }
                    break;                
                case 7:
                    switch (_numberToCall)
                    {
                        case "5555555":
                            callManager.EnterCallMode(0);
                            break;
                        default:
                            _isCallInProgress = true;
                            StartCoroutine(NumberNotInService());
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        //cancelling any methods if receiver is hung up
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            StopAllCoroutines();
            _isCallInProgress = false;
        }
    }

    //Methods
    public void ReceiveCall()
    {
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            _isCallCountDown = true;            
        }
    } 

    IEnumerator NumberNotInService()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        _isDailing = true;
        yield return new WaitForSeconds(2.5f);
        _isDailing = false;
        sfxManager.dialSource.Stop();
        callManager.NotInService();
    }

    IEnumerator EnterDirectoryMode()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        _isDailing = true;
        yield return new WaitForSeconds(2.5f);
        _isDailing = false;
        sfxManager.dialSource.Stop();
        callManager.EnterDirectoryMode();
    }
    
    //getter methods
    public bool GetCallStatus()
    {
        return _isCallInProgress;
    }
}
