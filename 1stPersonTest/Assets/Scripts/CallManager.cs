using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CallManager : MonoBehaviour
{
    [Header("Load Globals JSON")]
    [SerializeField] private TextAsset loadGlobalsJSON;


    [Header("Call UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject[] callChoices;
    private TextMeshProUGUI[] callChoicesText;
    [SerializeField] private TMP_InputField inputField;
    public Animator callPanelAnimator;


    [Header("Managers")]
    [SerializeField] PhoneDisplayController phoneDisplayController;
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] PuzzleManager puzzleManager;
    [SerializeField] SFXManager sfxManager;
    [SerializeField] DialogueAudioManager dialogueaudioManager;

    
    //ink story related elements
    private Story currentStory;
    private DialogueVariables dialogueVariables;
    private string playerInputCity; // store city name for directory
    private string playerInputBusiness; // store business name for directory
    private string playerInputName; // store person name for directory
    private string directoryFinalInput; // concatenate above values for dictionary key

    private float textSpeed = 0.03f;
    
    //state bools
    private bool isInDialogue = false; // check if in dialogue    
    private bool isInDirectory = false; // check if in directory    
    private bool isInputingCity = true; // check if inputing city into directory
    private bool isChoosingBetweenResidentialOrBusinessListing = false;
    private bool isInputingName = false; // check if inputing name into directory
    private bool isInputingBusiness = false; // check if inputing business into directory
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
        //callPanelAnimator.SetBool("inCall", true); // bring up dialogue panel
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
        currentStory.BindExternalFunction("PlayAudioClip", (int contact, int audioLine) =>
        {
            dialogueaudioManager.PlayDialogueClip(contact, audioLine);
        });
        currentStory.BindExternalFunction("EnterDirectoryMode", () =>
        {
            EnterDirectoryMode();
        });
        currentStory.BindExternalFunction("ExitDirectoryMode", () =>
        {
            ExitDirectoryMode();
        });
        currentStory.BindExternalFunction("ResidentialListing", () =>
        {
            ResidentialListing();
        });
        currentStory.BindExternalFunction("BusinessListing", () =>
        {
            BusinessListing();
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
            currentStory.UnbindExternalFunction("PlayAudioClip");
            currentStory.UnbindExternalFunction("EnterDirectoryMode");
            currentStory.UnbindExternalFunction("ExitDirectoryMode");
            currentStory.UnbindExternalFunction("ResidentialListing");
            currentStory.UnbindExternalFunction("BusinessListing");
        }
        currentStory = null;
    }
    public void ContinueCall()
    { 
        if (currentStory != null && currentStory.canContinue)
        {
            StopAllCoroutines(); // so we don't have multiple coroutines running at the same time
            string line = currentStory.Continue();
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
        
        if (line != null)
        {
            phoneDisplayController.ClearAllChars();
            int index = 0;
            foreach (char letter in line.ToCharArray())
            {
                if (letter == '.')
                {
                    break; 
                }
                    int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
                    phoneDisplayController.chars[index].GetComponent<CharController>().DisplayChar(letterAsInt);
                    index++;
                    yield return new WaitForSeconds(textSpeed);                            
            }
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
                callChoicesText[index].color = Color.white;
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
    public void EnterDirectoryMode()
    {
        DisableCallChoices();
        DisableContinueButton();
        isInDirectory = true;
        dialogueaudioManager.PlayDialogueClip(1, 0);
        callPanelAnimator.SetBool("inCall", true);
        inputField.gameObject.SetActive(true);
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
        playerInputBusiness = string.Empty;
        directoryFinalInput = string.Empty;
        isInputingCity = true;
        isInputingName = false;
        isInputingBusiness = false;
        isChoosingBetweenResidentialOrBusinessListing = false;
    }

    public void ReadPlayerInput(string s) // reading player input for directory (expand for other functions?)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isInputingCity)
            {
                // inputting city into directory
                StopAllCoroutines();
                inputField.GameObject().SetActive(false);
                playerInputCity = s.ToUpper();
                Debug.Log(playerInputCity);
                isInputingCity = false;
                dialogueaudioManager.dialogueaudioSource.Stop();
                dialogueaudioManager.PlayDialogueClip(1, 1);
                isChoosingBetweenResidentialOrBusinessListing = true;
            }
            else if (!isInputingCity)
            {
                if (isInputingName)
                {
                    // inputing name into directory
                    dialogueaudioManager.dialogueaudioSource.Stop();
                    playerInputName = s.ToUpper();
                    Debug.Log(playerInputName);
                    isInputingName = false;
                    directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() + playerInputName.ToLower());
                    if (!Dictionary.GetInstance().directoryResidentialNumbers.ContainsKey(directoryFinalInput))
                    {
                        // if key is not in dictionary
                        StopAllCoroutines();
                        StartCoroutine(DirectoryAnswer(false, 0));
                    }
                    else
                    {
                        // if key is in dictionary
                        StopAllCoroutines();
                        StartCoroutine(DirectoryAnswer(true, 0));
                    }
                }
                else if (isInputingBusiness)
                {
                    //inputing business into directory
                    dialogueaudioManager.dialogueaudioSource.Stop();
                    playerInputBusiness = s.ToUpper();
                    Debug.Log(playerInputBusiness);
                    isInputingBusiness = false;
                    directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() + playerInputBusiness.ToLower());
                    if (!Dictionary.GetInstance().directoryBusinessNumbers.ContainsKey(directoryFinalInput))
                    {
                        // if key is not in dictionary
                        StopAllCoroutines();
                        StartCoroutine(DirectoryAnswer(false, 1));
                    }
                    else
                    {
                        StopAllCoroutines();
                        StartCoroutine(DirectoryAnswer(true, 1));
                    }
                }
            }
            inputField.text = string.Empty;
        }
    }

    public void ResidentialListing()
    {
        if (!isInDirectory | !isChoosingBetweenResidentialOrBusinessListing)        
        {
            return;
        }
        isChoosingBetweenResidentialOrBusinessListing = false;
        isInputingName = true;
        inputField.GameObject().SetActive(true);
        inputField.ActivateInputField();
        dialogueaudioManager.dialogueaudioSource.Stop();
        dialogueaudioManager.PlayDialogueClip(1, 2);
    }

    public void BusinessListing()
    {
        if (!isInDirectory | !isChoosingBetweenResidentialOrBusinessListing)
        {
            return;
        }
        isChoosingBetweenResidentialOrBusinessListing = false;
        isInputingBusiness = true;
        inputField.GameObject().SetActive(true);
        inputField.ActivateInputField();
        dialogueaudioManager.dialogueaudioSource.Stop();
        dialogueaudioManager.PlayDialogueClip(1, 3);
    }

    IEnumerator DirectoryAnswer(bool listed, int residentialOrBusiness)
    {
        inputField.gameObject.SetActive(false);
        dialogueaudioManager.dialogueaudioSource.Stop();
        if (residentialOrBusiness == 0)
        {
            if (!listed)
            {
                yield return new WaitForSeconds(2f);
                dialogueaudioManager.PlayDialogueClip(1, 4);
            }
            else
            {
                string line = "Please hold...";
                StartCoroutine(TypeLine(line));
                yield return new WaitForSeconds(2f);
                string directoryOutput = Dictionary.GetInstance().directoryResidentialNumbers[directoryFinalInput];
                string line2 = "The residential number for...\n" + playerInputName.Trim() + " in " + playerInputCity.Trim() + " is...\n" + directoryOutput + ".\n" +
                "Do you need further assistance?";
                StartCoroutine(TypeLine(line2));
            }
        }
        else if (residentialOrBusiness == 1)
        {
            if (!listed)
            {
                yield return new WaitForSeconds(2f);
                dialogueaudioManager.PlayDialogueClip(1, 5);
                
            }
            else
            {
                string line = "Please hold...";
                StartCoroutine(TypeLine(line));
                yield return new WaitForSeconds(2f);
                string directoryOutput = Dictionary.GetInstance().directoryBusinessNumbers[directoryFinalInput];
                string line2 = "The business number for...\n" + playerInputBusiness.Trim() + " in " + playerInputCity.Trim() + " is...\n" + directoryOutput + ".\n" +
                "Do you need further assistance?";
                StartCoroutine(TypeLine(line2));
            }
        }
    }

    
    public void FurtherAssistace()
    {
        if (!isInDirectory | isInputingCity)
        {
            return;
        }
        playerInputCity = string.Empty;
        playerInputName = string.Empty;
        directoryFinalInput = string.Empty;
        isInputingCity = true;
        isInputingName = false;
        isInputingBusiness = false;
        isChoosingBetweenResidentialOrBusinessListing = false;
        inputField.gameObject.SetActive(true);
        inputField.text = null;
        inputField.ActivateInputField();
        dialogueaudioManager.PlayDialogueClip(1, 0);
    }
    

    private string ReplaceSpacesInString(string s)
    {
        string result = s.Replace(" ", "");
        return result; 
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