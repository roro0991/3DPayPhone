using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAudioManager : MonoBehaviour
{
    public AudioSource dialogueaudioSource;
    public AudioClip[] firstCall = new AudioClip[1];

    public void PlayDialogueClip(int audioLine, bool loop)
    {
        dialogueaudioSource.clip = firstCall[audioLine];
        dialogueaudioSource.volume = 1;
        dialogueaudioSource.loop = loop;
        dialogueaudioSource.Play();
    }
}
