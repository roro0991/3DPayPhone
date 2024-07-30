using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.UnityIntegration;
using System;

public class DialogueManager : MonoBehaviour
{
    [Header("Globals Ink File")]

    [SerializeField] private InkFile globalsInkFile;

    [Header("Call UI")]

    [SerializeField] private TextMeshProUGUI callText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject[] callChoices;
    [SerializeField] private TMP_InputField inputField;
    private TextMeshProUGUI[] callChoicesText;
    public Animator callPanelAnimator;

    [SerializeField] PhoneDisplayController phoneDisplayController;

    //ink story related elements
    private Story currentStory;
    private DialogueVariables dialogueVariables;
    private string playerInputCity;
    private string playerInputName;
    private string directoryFinalInput;

    private float textSpeed = 0.05f;
    private float displayTextSpeed = 0.1f;
    private bool isInDialogue = false;
    private bool isInDirectory = false;
    private bool isInputingCity = true;
    private bool areBothInputsFilled = false;
    

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

    private void Update()
    {
        Debug.Log(playerInputCity);
        Debug.Log(playerInputName);
    }
    public void EnterCallMode(TextAsset inkJSON)
    {
        isInDialogue = true;
        currentStory = new Story(inkJSON.text); // load in relevant dialogue information
        callPanelAnimator.SetBool("inCall", true); // bring up dialogue panel
        dialogueVariables.StartListening(currentStory);  // listen to story variables

        ContinueCall(); // begin dialogue 
    }

    public void ExitCallMode()
    {
        isInDialogue = false;
        isInDirectory = false;
        StopAllCoroutines();
        phoneDisplayController.ClearAllChars();
        callText.text = string.Empty; // clear dialogue panel
        callPanelAnimator.SetBool("inCall", false); // put away dialogue panel
        dialogueVariables.StopListening(currentStory); // stop listening to story variables
        currentStory = null;
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
        bool isPrintingToDisplay = false; // to know when to print secret messages to display
        int index = 0;
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
            else if (letter == '@')
            {
                phoneDisplayController.ClearAllChars();
                isPrintingToDisplay = true;
                continue;
            }
            else if (isPrintingToDisplay)
            {
                if (letter == '^' && isPrintingToDisplay)
                {
                    isPrintingToDisplay = false;
                    continue;
                }
                int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
                phoneDisplayController.chars[index].GetComponent<CharController>().DisplayChar(letterAsInt);
                index++;
                yield return new WaitForSeconds(displayTextSpeed);
            }                
            else
            {
                callText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
        }
     if (!isInDirectory)
        {
            EnableChoices();
            DisplayCallChoices();
        }
    }

    private void DisplayCallChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0 && !isInDirectory);
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

    public void ReadPlayerInput(string s)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isInputingCity)
            {
                StopAllCoroutines();
                isInputingCity = false;
                playerInputCity = s;
                string line = "Please provide the first and last name of the person you're attempting to reach";
                StartCoroutine(TypeLine(line));
            }
            else if (!isInputingCity && !areBothInputsFilled)
            {                
                playerInputName = s;
                areBothInputsFilled = true;
                directoryFinalInput = playerInputCity + playerInputName;
                if (!Dictionary.GetInstance().directoryNumbers.ContainsKey(directoryFinalInput))
                {
                    StopAllCoroutines();
                    string line = "The number for " + playerInputName + " in " + playerInputCity + " is not listed.";
                    StartCoroutine(TypeLine(line));
                }
                else
                {
                    StopAllCoroutines();
                    string output = Dictionary.GetInstance().directoryNumbers[directoryFinalInput];
                    //currentStory.variablesState["directoryReturn"] = Dictionary.GetInstance().directoryNumbers[directoryFinalInput];
                    string line = "The number for " + playerInputName + " in " + playerInputCity + " is " + output + ".";
                    StartCoroutine(TypeLine(line));
                }
                
            }
            else if (areBothInputsFilled)
            {
                areBothInputsFilled = false;
                playerInputCity = string.Empty;
                playerInputName = string.Empty;
                directoryFinalInput = string.Empty;
                isInputingCity = true;
                ExitDirectoryMode();
            }
            inputField.text = string.Empty;
        }
    }

    public void EnterDirectoryMode()
    {
        isInDirectory = true;
        inputField.gameObject.SetActive(true);
        callPanelAnimator.SetBool("inCall", true);
        string line = "You've reached the directory.\n Please provide the name of the city you are trying to reach.";
        StartCoroutine(TypeLine(line)); 
    }

    public void ExitDirectoryMode()
    {
        callPanelAnimator.SetBool("inCall", false);
        inputField.gameObject.SetActive(false);
        playerInputCity = string.Empty;
        playerInputName = string.Empty;
        StopAllCoroutines();
        isInDirectory = false;
    }

    public bool GetInDialogueStatus()
    {
        return isInDialogue;
    }

}