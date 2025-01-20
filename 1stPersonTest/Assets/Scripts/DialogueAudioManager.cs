using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueAudioManager : MonoBehaviour
{
    public AudioSource dialogueaudioSource;
    public AudioClip[] firstCall = new AudioClip[1];
    public AudioClip[] directory = new AudioClip[1];
   
    private AudioClip[][] dialogue = new AudioClip[20][];

    private void Start()
    {
        dialogue[0] = firstCall;
        dialogue[1] = directory;
    }

    public void PlayDialogueClip(int contact, int audioLine)
    {
        dialogueaudioSource.clip = dialogue[contact][audioLine];
        dialogueaudioSource.volume = 1;
        dialogueaudioSource.Play();
    }
}
