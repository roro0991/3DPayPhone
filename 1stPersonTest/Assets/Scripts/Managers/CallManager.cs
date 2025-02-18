using Ink.Runtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem.Layouts;
using UnityEditor.Rendering;

public class CallManager : MonoBehaviour
{
    [Header("Load Globals JSON")]
    [SerializeField] private TextAsset loadGlobalsJSON;


    [Header("Call UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_InputField inputField;
    public Animator callPanelAnimator;


    [Header("Managers")]
    [SerializeField] PhoneManager phoneManager;
    [SerializeField] PuzzleManager puzzleManager;
    [SerializeField] SFXManager sfxManager;
    [SerializeField] DialogueAudioManager dialogueaudioManager;
    [SerializeField] StoryManager storyManager;

    
    //ink story related variables
    private Story currentStory;
    private DialogueVariables dialogueVariables;
    private string playerInputCity; // store city name for directory
    private string playerInputBusiness; // store business name for directory
    private string playerInputName; // store person name for directory
    private string directoryFinalInput; // concatenate above values for dictionary key

    //dialogue parsing related variables
    private string _playerInput;
    private bool _isPlayerInputQuestion = false;
    private bool _isPlayerInputStatement = false;
    //regex patterns for parsing _playerResponse;
    private string question_Pattern = @"\?$";
    private string[] questionFirstKeyPatternArray = new[]
    {
        @"(?<firstKey>wh(o|at|ere|en|y))"
    };
    private string[] whoQuestionPatternArray = new[]
    {
        @"^(?<firstKey>who)\s(((\w+)?\s)+)?is\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$",
        @"^do\syou\s([\w+\s]+)?know\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\?$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\.$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?is\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,}).$",
        @"^(?<firstKey>who)\s(?<secondKey>killed)\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$"
    };
        
    private int loopCount = 0;
    private float textSpeed = 0.03f;
    
    //state bools
    private bool isInDialogue = false; // check if in dialogue
    private bool canhangUp = true; // check if you can hang up
    private bool loopCall = false; // check if audio should loop
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

    private void Update()
    {
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            ExitDirectoryMode();
            ExitCallMode();
        }

        if (loopCall == true)
        {
            if (currentStory != null)
            {
                if (dialogueaudioManager.dialogueaudioSource.isPlaying && loopCount == 0)
                {
                    loopCount++;
                    Debug.Log(loopCount);
                    return;
                }
                else if (dialogueaudioManager.dialogueaudioSource.isPlaying == false 
                        && loopCount > 0)
                {
                    canhangUp = true;
                    loopCount++;
                    Debug.Log(loopCount);
                    dialogueaudioManager.dialogueaudioSource.Play();
                }
            }
        }        
    }

    public void NotInService()
    {
        StopAllCoroutines();
        isInDialogue = true;
        dialogueaudioManager.PlayDialogueClip(0, 7);
    }

    public void EnterCallMode(TextAsset inkJSON)
    {
        isInDialogue = true;
        canhangUp = false;
        currentStory = new Story(inkJSON.text); // load in relevant dialogue information
        //callPanelAnimator.SetBool("inCall", true); // bring up dialogue panel
        dialogueVariables.StartListening(currentStory);  // listen to story variables
        // allow access to C# functions/methods from ink
        currentStory.BindExternalFunction("EnterPuzzleMode", 
        (int puzzleType, string answerSequence) =>
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
        ContinueCall(); // begin dialogue 
    }

    public void EnterCallModeV2(int caller, int line)
    {
        isInDialogue = true;
        DisableContinueButton();
        dialogueaudioManager.PlayDialogueClip(caller, line);
        callPanelAnimator.SetBool("inCall", true);
        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();
    }

    public void ExitCallMode()
    {
        if (!isInDialogue)
        {
            return;
        }
        StopAllCoroutines();
        isInDialogue = false;
        loopCount = 0;
        loopCall = false;
        canhangUp = true;
        phoneManager.ClearDisplay();
        SetAutomatedSystemStatus(false);
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
        }
        currentStory = null;
    }

    public void ReadPlayerInput(string s)
    {
        if (isInDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _playerInput = s;
                ParsePlayerInput(_playerInput);
            }
        }
    }

    private void ParsePlayerInput(string playerInput)
    {
        string playerResponseSingleSpaceLowerCase =
            Regex.Replace(playerInput, @"\s+", " ").ToLower();        
        string firstKey = null;
        string secondKey = null;
        string questionTarget = null;

        if (Regex.IsMatch(playerResponseSingleSpaceLowerCase, question_Pattern))
        {
            _isPlayerInputQuestion = true;
            Debug.Log("You asked a question!");
        }
        else
        {
            _isPlayerInputStatement = true;
            Debug.Log("You made a statement!");
        }

        //code for parsing questions
        foreach (string pattern in questionFirstKeyPatternArray)
        {
            if (Regex.IsMatch(playerResponseSingleSpaceLowerCase, pattern))
            {
                var inputMatch = Regex.Match(playerResponseSingleSpaceLowerCase, pattern);
                firstKey = inputMatch.Groups["firstKey"].ToString();
                Debug.Log("the first question key is: " + firstKey);
            }
            else
            {
                Debug.Log("no key words were found.");
            }
            break;
        }

        if (firstKey != null)
        {
            switch (firstKey)
            {
                case "who":    
                    foreach (string whoQuestionPattern in whoQuestionPatternArray)
                    {
                        if (Regex.IsMatch(playerResponseSingleSpaceLowerCase, whoQuestionPattern))
                        {
                            var inputMatch = Regex.Match(playerResponseSingleSpaceLowerCase, whoQuestionPattern);
                            secondKey = inputMatch.Groups["secondKey"].ToString();
                            questionTarget = inputMatch.Groups["fullName"].ToString();
                            Debug.Log("the second question key is: " + secondKey);
                            Debug.Log("the question target is: " + questionTarget);
                            break;
                        }
                    }
                        
                    if (secondKey != "")
                    {
                        switch (secondKey)
                        {
                            case "killed":
                                switch (questionTarget)
                                {
                                    case "sam brown":
                                        Debug.Log("Sally Smith killed Sam Brown");
                                        break;
                                    case "billy bob":
                                        Debug.Log("Billy Bob is dead!?");
                                        break;
                                }
                                break;
                            default:
                                Debug.Log("I don't know.");
                                break;
                        }
                        inputField.text = "";
                        inputField.ActivateInputField();
                        return;
                    }

                    switch (questionTarget)
                    {
                        case "sam brown":
                            Debug.Log("Sam Brown is my friend.");
                            break;
                        case "sally smith":
                            Debug.Log("Sall Smith is my enemy.");
                            break;
                        default:
                            Debug.Log("I don't know who that is.");
                            break;
                    }
                    break;
                default:                    
                    break;

            }
        }
        inputField.text = "";
        inputField.ActivateInputField();
        
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
        DisableContinueButton();
        
        if (line != null)
        {
            //phoneDisplayController.ClearAllChars();
            int index = 0;
            foreach (char letter in line.ToCharArray())
            {
                if (letter == '.')
                {
                    break; 
                }
                    int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];

                    phoneManager.displayCharArray[index].GetComponent<CharController>()
                    .DisplayChar(letterAsInt);

                    index++;
                    yield return new WaitForSeconds(textSpeed);                            
            }
        }
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
        if (isInDirectory)
        {
            return;
        }
        Debug.Log(playerInputCity);
        DisableContinueButton();
        isInDirectory = true;
        dialogueaudioManager.PlayDialogueClip(0, 0);
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

    // reading player input for directory (expand for other functions?)
    public void ReadDirectoryInput(string s) 
    {
        if (isInDirectory)
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
                    dialogueaudioManager.PlayDialogueClip(0, 1);
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

                        directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() 
                        + playerInputName.ToLower());

                        if (!Dictionary.GetInstance().directoryResidentialNumbers
                            .ContainsKey(directoryFinalInput))
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

                        directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() 
                        + playerInputBusiness.ToLower());

                        if (!Dictionary.GetInstance().directoryBusinessNumbers
                            .ContainsKey(directoryFinalInput))
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
        dialogueaudioManager.PlayDialogueClip(0, 2);
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
        dialogueaudioManager.PlayDialogueClip(0, 3);
    }

    IEnumerator DirectoryAnswer(bool listed, int residentialOrBusiness)
    {
        inputField.gameObject.SetActive(false);
        dialogueaudioManager.dialogueaudioSource.Stop();
        if (residentialOrBusiness == 0)
        {
            if (!listed)
            {
                yield return new WaitForSeconds(1f);
                dialogueaudioManager.PlayDialogueClip(0, 4);
                yield return new WaitForSeconds(3f);
                dialogueaudioManager.PlayDialogueClip(0, 5);
            }
            else
            {
                yield return new WaitForSeconds(1f);
                dialogueaudioManager.PlayDialogueClip(0, 4);
                //insert code for returning found number
            }
        }
        else if (residentialOrBusiness == 1)
        {
            if (!listed)
            {
                yield return new WaitForSeconds(1f);
                dialogueaudioManager.PlayDialogueClip(0, 4);
                yield return new WaitForSeconds(3f);
                dialogueaudioManager.PlayDialogueClip(0, 6);
                
            }
            else
            {
                yield return new WaitForSeconds(1f);
                dialogueaudioManager.PlayDialogueClip(0, 4);
                //insert code for returning found number
            }
        }
    }

    public void ReturnToDirectoryMainMenu()
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
        dialogueaudioManager.PlayDialogueClip(0, 0);
    }


    private string ReplaceSpacesInString(string s)
    {
        string result = s.Replace(" ", "");
        return result; 
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

    public bool GetCanHangUpStatus()
    {
        return canhangUp;
    }

    // Setter Methods
    public void SetLoopCallStatus(bool status)
    {
        loopCall = status;
    }

    public void SetAutomatedSystemStatus(bool status)
    {
        isInAutomatedSystem = status;
    }

    public void SetExtentionStatus(bool status)
    {
        isInExtentionSystem = status;
    }
}