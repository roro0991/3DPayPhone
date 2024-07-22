using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.UnityIntegration; 

public class DialogueManager : MonoBehaviour
{
    [Header("Globals Ink File")]

    [SerializeField] private InkFile globalsInkFile;

    [Header("Call UI")]

    [SerializeField] private TextMeshProUGUI callText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject[] callChoices;
    private TextMeshProUGUI[] callChoicesText;
    public Animator callPanelAnimator;

    //ink story related elements
    private Story currentStory;
    private DialogueVariables dialogueVariables;

    private float textSpeed = 0.05f;

    private void Awake()
    {
        dialogueVariables = new DialogueVariables(globalsInkFile.filePath);
    }

    private void Start()
    {
        callChoicesText = new TextMeshProUGUI[callChoices.Length];
        int index = 0;
        foreach (GameObject choice in callChoices)
        {
            callChoicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    public void EnterCallMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text); // load in relevant dialogue information
        callPanelAnimator.SetBool("inCall", true); // bring up dialogue panel
        dialogueVariables.StartListening(currentStory);  // listen to story variables

        ContinueCall(); // begin dialogue 
    }

    public void ExitCallMode()
    {
        callText.text = string.Empty; // clear dialogue panel
        callPanelAnimator.SetBool("inCall", false); // put away dialogue panel
        dialogueVariables.StopListening(currentStory); // stop listening to story variables
    }
    public void ContinueCall()
    {
        if (currentStory == null)
        {
            ExitCallMode();
            return;
        }

        if (currentStory.canContinue)
        {
            string line = currentStory.Continue();
            StopAllCoroutines(); // so we don't have multiple coroutines running at the same time
            StartCoroutine(TypeLine(line));            
        }
        else
        {
            ExitCallMode();
        }
    }

    IEnumerator TypeLine(string line)
    {
        DisableCallChoices();
        DisableConitnueCallButton();
        callText.text = "";
        bool isAddingRichTextTag = false; // so we don't print the richtext code from ink into the dialogue
        foreach (char letter in line.ToCharArray())
        {
            if (letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                callText.text += letter;
                if (letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            else
            {
                callText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
        }
        EnableChoices();
        DisplayCallChoices();
    }

    private void DisplayCallChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0)
        {
            EnableContinueCallButton();
        }

        if (currentChoices.Count > callChoices.Length)
        {
            Debug.LogError("There are too many choices to fit the UI");
        }

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            callChoices[index].gameObject.SetActive(true);
            callChoicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < callChoices.Length; i++)
        {
            callChoices[i].gameObject.SetActive(false);
        }
    }

    public void MakeCallChoice(int callChoiceIndex)
    {
        currentStory.ChooseChoiceIndex(callChoiceIndex);
        ContinueCall();
    }

    private void DisableCallChoices()
    {
        foreach (GameObject choice in callChoices)
        {
            choice.SetActive(false);
        }
    }

    private void EnableChoices()
    {
        foreach (GameObject choice in callChoices)
        {
            choice.SetActive(true);
        }
    }

    private void EnableContinueCallButton()
    {
        continueButton.gameObject.SetActive(true);
    }

    private void DisableConitnueCallButton()
    {
        continueButton.gameObject.SetActive(false);
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue == null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }
}
