using UnityEngine;

public class DialogueAudioManager : MonoBehaviour
{
    public AudioSource dialogueaudioSource;
    public AudioClip[] directory = new AudioClip[1];
    public AudioClip[] notInService = new AudioClip[1];
    public AudioClip[] nineOneOne = new AudioClip[1];
   
    private AudioClip[][] dialogue = new AudioClip[20][];

    private void Start()
    {
        dialogue[0] = directory;
        dialogue[1] = notInService;
        dialogue[2] = nineOneOne;
    }

    public void PlayDialogueClip(int contact, int audioLine)
    {
        dialogueaudioSource.clip = dialogue[contact][audioLine];
        dialogueaudioSource.volume = .5f;
        dialogueaudioSource.Play();
    }
}
