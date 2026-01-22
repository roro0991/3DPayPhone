using Ink.Parsed;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CallManager : MonoBehaviour
{
    [Header("Call UI")]
    [SerializeField] TMP_InputField _playerInputField;
    [SerializeField] GameObject wordBank;
    [SerializeField] MessagePanel messagePanel;
    public Animator CallPanelAnimator;

    [Header("Managers")]
    [SerializeField] SentenceBuilder sentenceBuilder;
    [SerializeField] InputInterpreter inputInterpreter;
    [SerializeField] PhoneManager PhoneManager;
    [SerializeField] SFXManager SfxManager;
    [SerializeField] DialogueAudioManager DialogueAudioManager;

    [SerializeField] public Contact[] Contacts = new Contact[0];

    private Contact currentContact;

    public enum Call_State { ON_STANDBY, IN_CALL }
    public Call_State CurrentState;

    private void Start()
    {
        CurrentState = Call_State.ON_STANDBY;
    }

    private void Update()
    {
        if (PhoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            if (CurrentState == Call_State.IN_CALL)
            {
                ExitCallMode();
            }
        }
    }

    public void NotInService()
    {
        StopAllCoroutines();
        CurrentState = Call_State.IN_CALL;
        DialogueAudioManager.PlayDialogueClip(1, 0);
    }

    public void Call911()
    {
        StopAllCoroutines();
        CurrentState = Call_State.IN_CALL;
        DialogueAudioManager.PlayDialogueClip(2, 0);
    }

    // --- ENTER CALL ---
    public void EnterCallMode(int contactIndex)
    {
        if (CurrentState == Call_State.IN_CALL) return;

        if (contactIndex < 0 || contactIndex >= Contacts.Length)
        {
            Debug.LogWarning("Invalid contact index passed to EnterCallMode.");
            return;
        }

        CurrentState = Call_State.IN_CALL;
        currentContact = Contacts[contactIndex];

        wordBank.gameObject.SetActive(true);

        // NPC opens with their first line + initial SentenceWords
        currentContact.SpeakFirstLine();
        messagePanel.AddMessage(currentContact.ContactName + ": " + currentContact.ContactResponse);

        WordBank wordBankComponent = wordBank.GetComponentInChildren<WordBank>();
        wordBankComponent.ClearWordBank();
        currentContact.PopulateWordBank();
        wordBankComponent.Refresh();
    }


    // --- EXIT CALL ---
    public void ExitCallMode()
    {
        if (CurrentState != Call_State.IN_CALL) return;

        CurrentState = Call_State.ON_STANDBY;
        StopAllCoroutines();

        wordBank.gameObject.SetActive(false);
        PhoneManager.ClearDisplay();
        messagePanel.ClearMessagePanel();
        sentenceBuilder.ClearSentence();
        wordBank.GetComponentInChildren<WordBank>().ClearWordBank();
    }

    // --- PLAYER INPUT ---
    public void ReadPlayerInput(string s)
    {
        if (CurrentState != Call_State.IN_CALL) return;        

        SentenceBreakdown sb = inputInterpreter.InterpretPlayerInput(sentenceBuilder.wordList);

        currentContact.GenerateResponse(sb);
        StartCoroutine(ReadPlayerInputSequence());
    }

    private IEnumerator ReadPlayerInputSequence()
    {
        var wdb = WordDataBase.Instance;
        float delay = sentenceBuilder.GetSentenceAsString().Length * 0.15f;

        if (sentenceBuilder.wordList.Count > 0)
        {
            messagePanel.AddMessage("You: " + sentenceBuilder.GetSentenceAsString());
            sentenceBuilder.ClearSentence();
        }

        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrWhiteSpace(currentContact.ContactResponse))
        {
            messagePanel.AddMessage(currentContact.ContactName + ": " + currentContact.ContactResponse);
            yield return new WaitForSeconds(2f);

            WordBank wordBankComponent = wordBank.GetComponentInChildren<WordBank>();
            
            if (currentContact.ContactResponse == "I don't understand.")
            {
                // ?? Add previous words back into the bank (convert from string to Word)                                
                wordBankComponent.AddWordsToWordBank(sentenceBuilder.storedWordList);
                sentenceBuilder.ClearStoredWords();
            }
            else
            {
                // Normal flow: replace with NPC's new SentenceWords
                wordBankComponent.ClearWordBank();
                wordBankComponent.AddWordsToWordBank(currentContact.SentenceWords);
                sentenceBuilder.ClearStoredWords();
            }
            
        }
    }

    // --- TYPE OUT A LINE TO PHONE DISPLAY ---
    private IEnumerator TypeLine(string line)
    {
        float _textSpeed = .3f;
        if (line != null)
        {
            int index = 0;
            foreach (char letter in line.ToCharArray())
            {
                if (letter == '.') break;

                int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
                PhoneManager.displayCharArray[index].GetComponent<CharController>()
                    .DisplayChar(letterAsInt);

                index++;
                yield return new WaitForSeconds(_textSpeed);
            }
        }
    }

    // --- GETTERS ---
    public Call_State GetInDialogueStatus() => CurrentState;
}
