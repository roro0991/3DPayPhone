using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource dialSource;
    public AudioClip receiverUp, receiverDown, dialRinging, callRinging;
    public List<AudioClip> buttonPresses = new List<AudioClip>();
    public List<AudioClip> coinInserts = new List<AudioClip>();

    public void coinInsert()
    {
        int index = Random.Range(0, coinInserts.Count);
        AudioClip coinInsertClip = coinInserts[index];
        audioSource.PlayOneShot(coinInsertClip);
    }
    public void ButtonPress()
    {
        int index = Random.Range(0, buttonPresses.Count);
        AudioClip buttonPressClip = buttonPresses[index];
        audioSource.PlayOneShot(buttonPressClip, 1f);
    }

    public void DialRing()
    {
        dialSource.clip = dialRinging;
        dialSource.loop = true;
        dialSource.volume = 0.4f;
        dialSource.Play();
    }

    public void CallRing()
    {
        Debug.Log("the phone is ringing!");
        dialSource.clip = callRinging;
        dialSource.loop = true;
        dialSource.volume = .2f;
        dialSource.Play();
    }
    public void ReceiverUP()
    {
        audioSource.PlayOneShot(receiverUp, 1f);
    }

    public void ReceiverDown()
    {
        audioSource.PlayOneShot(receiverDown, 1f);
    }

}