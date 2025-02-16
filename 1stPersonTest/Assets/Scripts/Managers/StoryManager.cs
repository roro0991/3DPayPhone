
using System.Collections;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [SerializeField] CallManager callManager;
    [SerializeField] DialogueAudioManager dialogueAudioManager;
    [SerializeField] CallTrigger callTrigger;
    [SerializeField] PhoneManager phoneManager;

    //Bools for triggering first event: getting first number to call.
    //Once all three are true, the player gets the first call with the first number.
    bool firstdoorOpen = false;
    bool firstlineDrawn = false;
    bool firstnoteWritten = false;

    bool firstcallPlayed = false;
    bool firstcallSent = false;
    private void Update()
    {
        if (firstdoorOpen == true && firstlineDrawn == true && firstnoteWritten == true
            && firstcallSent == false)
        {
            callTrigger.ReceiveCall();
            firstcallSent = true;
        }
    }

    //Setter Methods
    public void SetFirstDoorOpen(bool status)
    {
        Debug.Log("you have opened the door for the first time!");
        firstdoorOpen = status;
    }

    public void SetFirstLineDrawn(bool status)
    {
        Debug.Log("you have drawn a line for the first time!");
        firstlineDrawn = status; 
    }

    public void SetFirstNoteWritten(bool status)
    {
        Debug.Log("you have written a note for the first time!");
        firstnoteWritten = status;
    }

    public void SetFirstCallStatus(bool status)
    {
        firstcallPlayed = status;
    }

    //Getter Methods
    public bool GetFirstCallStatus()
    {
        return firstcallPlayed;
    }    
}
