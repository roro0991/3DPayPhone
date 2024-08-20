using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Android;

public class CallManager : MonoBehaviour
{
    [Header("Load Globals JSON")]

    [SerializeField] private TextAsset loadGlobalsJSON;

    [Header("Call UI")]

    [SerializeField] private TextMeshProUGUI callText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject[] callChoices;
    [SerializeField] private TMP_InputField inputField;
    private TextMeshProUGUI[] callChoicesText;
    public Animator callPanelAnimator;
    bool secretMessage = false;

    [SerializeField] PhoneDisplayController phoneDisplayController;
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] PuzzleManager puzzleManager;
    [SerializeField] SFXManager sfxManager;

    //ink story related elements
    private Story currentStory;
    private DialogueVariables dialogueVariables;
    private string playerInputCity; // store city name for directory
    private string playerInputName; // story person name for directory
    private string directoryFinalInput; // concatenate above values for dictionary key

    private float textSpeed = 0.03f;
    
    //state bools
    private bool isInDialogue = false; // check if in dialogue
    private bool isInDirectory = false; // check if in directory
    private bool isInputingCity = true; // check if inputing city into directory
    private bool isInputingName = false; // check if inputing name into directory
    private bool isInAutomatedSystem = false; // check if in automated system
    private bool isInExtentionSystem = false; // check if inputing extention number
    

    private void Awake()
    {
        dialogueVariables = new DialogueVariables(loadGlobalsJSON);
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
        isInDialogue = true;
        currentStory = new Story(inkJSON.text); // load in relevant dialogue information
        callPanelAnimator.SetBool("inCall", true); // bring up dialogue panel
        dialogueVariables.StartListening(currentStory);  // listen to story variables
        // allow access to C# functions/methods from ink
        currentStory.BindExternalFunction("EnterPuzzleMode", (int puzzleType, string answerSequence) =>
        {
            puzzleManager.EnterPuzzleMode(puzzleType, answerSequence.ToCharArray());
        });
        currentStory.BindExternalFunction("SetAutomatedSystem", (bool status) =>
        {
            SetAutomatedSystemStatus(status);
        });
        currentStory.BindExternalFunction("SetExtentionSystem", (bool status) =>
        {
            SetExtentionStatus(status);
        });
        currentStory.BindExternalFunction("ResetExtention", () =>
        {
            phoneManager.ResetExtention();
        });

        ContinueCall(); // begin dialogue 
    }

    public void NotInService()
    {
        StopAllCoroutines();
        callPanelAnimator.SetBool("inCall", true);
        isInDialogue = true;
        string line = "The number you have dialed is not in service.";
        StartCoroutine(TypeLine(line));
        EnableContinueCallButton();
    }

    public void ExitCallMode()
    {
        StopAllCoroutines();
        isInDialogue = false;
        phoneDisplayController.ClearAllChars();
        callText.text = string.Empty; // clear dialogue panel
        callPanelAnimator.SetBool("inCall", false); // put away dialogue panel
        if (currentStory != null)
        {                 
            currentStory.variablesState["extention"] = "";
            SetExtentionStatus(false);
            dialogueVariables.StopListening(currentStory); // stop listening to story variables
            currentStory.UnbindExternalFunction("EnterPuzzleMode");
            currentStory.UnbindExternalFunction("SetAutomatedSystem");
            currentStory.UnbindExternalFunction("SetExtentionSystem");
            currentStory.UnbindExternalFunction("ResetExtention");
        }
        currentStory = null;
    }
    public void ContinueCall()
    {        
        if (secretMessage == true)
        {
            phoneDisplayController.ClearAllChars();
            secretMessage = false;
        }
        if (currentStory != null && currentStory.canContinue)
        {
            StopAllCoroutines(); // so we don't have multiple coroutines running at the same time
            string line = currentStory.Continue();
            if (line.Equals("") && currentStory.canContinue)
            {
                line = currentStory.Continue();
            }
            else if (line.Equals("") && !currentStory.canContinue)
            {
                ExitCallMode();
            }
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
        DisableContinueButton();
        /*callText.text = "";
        bool isAddingRichTextTag = false; // so we don't print the richtext code from ink into the dialogue
        bool isPrintingToDisplay = false; // to know when to print secret messages to display
        int index = 0; // for when we print to phone display instead of call panel
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
                yield return new WaitForSeconds(textSpeed);
            }                
            else
            {
                callText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
            sfxManager.DialogueBlip();
        }*/
        string lineForCallPanel = "";
        foreach (char letter in line.ToCharArray())
        {
            if (letter != '@')
            {
                lineForCallPanel += letter;
            }
            else
            {
                secretMessage = true;
                break;
            }
        }

        callText.text = lineForCallPanel;
        int totalVisibleCharacters = line.Length;
        int counter = 0;

        bool isPrintingToDisplay = false; // to know when to print secret messages to display
        int index = 0;
        foreach (char letter in line.ToCharArray())
        {
            if (letter == '@')
            {                
                isPrintingToDisplay = true;
                phoneDisplayController.ClearAllChars();
                continue;
            }
            else if (isPrintingToDisplay)
            {
                if (letter == '^' && isPrintingToDisplay)
                {
                    isPrintingToDisplay = false;
                    break;
                }
                int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
                phoneDisplayController.chars[index].GetComponent<CharController>().DisplayChar(letterAsInt);
                index++;
                yield return new WaitForSeconds(textSpeed);
            }

            int visibleCount = counter % (totalVisibleCharacters + 1);
            callText.maxVisibleCharacters = visibleCount;
            counter++;
            if (!isPrintingToDisplay)
            {
                sfxManager.DialogueBlip();
            }
            yield return new WaitForSeconds(textSpeed);

        }

        if (!isInDirectory && currentStory != null)
        {
            DisplayCallChoices();
        }
    }

    private void DisplayCallChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count == 0 && !isInDirectory &&!isInExtentionSystem)
        {
            EnableContinueCallButton();
        }

        if (!isInAutomatedSystem) // disable call panel choice buttons when using phone buttons instead
        {
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
    }

    public void MakeCallChoice(int callChoiceIndex) // for when choices use call panel buttons
    {
        if (isInDirectory)
        {
            return;
        }
        currentStory.ChooseChoiceIndex(callChoiceIndex);
        ContinueCall();
    }

    public void MakeAutomatedCallChoice(int callChoiceIndex) // for when choices use phone buttons
    {
        if (!isInAutomatedSystem)
        {
            return;
        }
        if (callChoiceIndex < currentStory.currentChoices.Count)
        {
            currentStory.ChooseChoiceIndex(callChoiceIndex);
            ContinueCall();
        }
    }

    public void EnterExtention()
    {
        if (!isInExtentionSystem)
        {
            return;
        }
        currentStory.variablesState["extention"] = phoneManager.GetExtentionNumber();
        StartCoroutine(ConnectExtention());
    }

    IEnumerator ConnectExtention()
    {
        yield return new WaitForSeconds(1.5f);
        sfxManager.DialRing();
        yield return new WaitForSeconds(2.5f);
        sfxManager.dialSource.Stop();
        ContinueCall();
    }

    private void DisableCallChoices()
    {
        foreach (GameObject choice in callChoices)
        {
            choice.SetActive(false);
        }
    }

    private void EnableCallChoices()
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

    private void DisableContinueButton()
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

    public void ReadPlayerInput(string s) // reading player input for directory (expand for other functions?)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isInputingCity)
            {
                // inputting city into directory
                StopAllCoroutines();
                playerInputCity = s;
                Debug.Log(playerInputCity);
                isInputingCity = false;
                isInputingName = true;
                string line = "Please provide the first and last name of the person you're attempting to reach.";
                StartCoroutine(TypeLine(line));
                inputField.ActivateInputField();
            }
            else if (!isInputingCity && isInputingName)
            {
                // inputing name into directory
                playerInputName = s;
                Debug.Log(playerInputName);
                isInputingName = false;
                directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() + playerInputName.ToLower());
                if (!Dictionary.GetInstance().directoryNumbers.ContainsKey(directoryFinalInput))
                {
                    // if key is not in dictionary
                    StopAllCoroutines();
                    inputField.gameObject.SetActive(false);
                    string line = "The number for " + playerInputName.Trim() + " in " + playerInputCity.Trim() + " is not listed.\n" +
                    "Do you need further assistance?";
                    StartCoroutine(TypeLine(line));
                }
                else
                {
                    // if key is in dictionary
                    StopAllCoroutines();
                    inputField.gameObject.SetActive(false);
                    string directoryOutput = Dictionary.GetInstance().directoryNumbers[directoryFinalInput];                    
                    string line = "The number for " + playerInputName.Trim() + " in " + playerInputCity.Trim() + " is " + directoryOutput + ".\n" +
                    "Do you need further assistance?";
                    StartCoroutine(TypeLine(line));
                }
                Invoke("ContinueExitDirectoryOptions", 2.9f);
            }
            inputField.text = string.Empty;
        }
    }
    public void EnterDirectoryMode()
    {
        DisableCallChoices();
        DisableContinueButton();
        isInDirectory = true;
        callPanelAnimator.SetBool("inCall", true);
        inputField.gameObject.SetActive(true);
        string line = "You've reached the directory.\nPlease provide the name of the city you are trying to reach.";
        StartCoroutine(TypeLine(line));
        inputField.ActivateInputField();
    }
    public void ExitDirectoryMode()
    {
        if (!isInDirectory)
        {
            return;
        }
        StopAllCoroutines();
        isInDirectory = false;
        callPanelAnimator.SetBool("inCall", false);
        inputField.text = "";
        inputField.gameObject.SetActive(false);
        playerInputCity = string.Empty;
        playerInputName = string.Empty;
        directoryFinalInput = string.Empty;
        isInputingCity = true;
        isInputingName = false;
    }

    public void FurtherAssistace()
    {
        if (!isInDirectory)
        {
            return;
        }
        playerInputCity = string.Empty;
        playerInputName = string.Empty;
        directoryFinalInput = string.Empty;
        isInputingCity = true;
        isInputingName = false;
        inputField.gameObject.SetActive(true);
        string line = "Please provide the name of the city you are trying to reach.";
        StartCoroutine(TypeLine(line));
        inputField.ActivateInputField();
    }
    private string ReplaceSpacesInString(string s)
    {
        string result = s.Replace(" ", "");
        return result; 
    }

    private void ContinueExitDirectoryOptions() // exit directory or search again options
    {
        foreach (GameObject choice in callChoices)
        {
            if (choice == callChoices[0])
            {
                choice.SetActive(false);
            }
            if (choice == callChoices[1])
            {
                choice.SetActive(true);
                callChoicesText[1].GetComponent<TextMeshProUGUI>().text = "Yes";
            }
            if (choice == callChoices[2])
            {
                choice.SetActive(true);
                callChoicesText[2].GetComponent<TextMeshProUGUI>().text = "No";
            }
        }
    }

    public void SetAutomatedSystemStatus(bool status)
    {
        isInAutomatedSystem = status;
    }

    public void SetExtentionStatus(bool status)
    {
        isInExtentionSystem = status;
    }

    // Getter Methods

    public bool GetExtentionStatus()
    {
        return isInExtentionSystem;
    }

    public bool GetInDialogueStatus()
    {
        return isInDialogue;
    }

    public bool GetInDirectoryStatus()
    {
        return isInDirectory;
    }
}