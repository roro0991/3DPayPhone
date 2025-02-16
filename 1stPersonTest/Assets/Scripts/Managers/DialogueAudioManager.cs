using UnityEngine;

public class DialogueAudioManager : MonoBehaviour
{
    public AudioSource dialogueaudioSource;
    public AudioClip[] directory = new AudioClip[1];
    public AudioClip[] testCall = new AudioClip[1];
   
    private AudioClip[][] dialogue = new AudioClip[20][];

    private void Start()
    {
        dialogue[0] = directory;
        dialogue[1] = testCall;
    }

    public void PlayDialogueClip(int contact, int audioLine)
    {
        dialogueaudioSource.clip = dialogue[contact][audioLine];
        dialogueaudioSource.volume = 1;
        dialogueaudioSource.Play();
    }
}
