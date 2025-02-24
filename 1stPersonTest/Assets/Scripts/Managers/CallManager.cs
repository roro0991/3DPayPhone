using System.Collections;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using System.Text.RegularExpressions;

public class CallManager : MonoBehaviour
{
    [Header("Call UI")]
    [SerializeField] private TMP_InputField _playerInputField;
    public Animator CallPanelAnimator;


    [Header("Managers")]
    [SerializeField] PhoneManager PhoneManager;
    [SerializeField] PuzzleManager PuzzleManager;
    [SerializeField] SFXManager SfxManager;
    [SerializeField] DialogueAudioManager DialogueAudioManager;
    [SerializeField] StoryManager StoryManager;

    //dialogue parsing related variables
    private string _playerInput;
    private bool _isPlayerInputQuestion = false;
    private bool _isPlayerInputStatement = false;
    //regex patterns for parsing _playerResponse;
    private string QUESTION_PATTERN = @"\?$";
    private string[] QUESTION_FIRST_KEY_PATTERN_ARRAY = new[]
    {
        @"(?<firstKey>wh(o|at|ere|en|y))"
    };
    private string[] WHO_QUESTION_PATTERN_ARRAY = new[]
    {
        @"^(?<firstKey>who)\s(((\w+)?\s)+)?is\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$",
        @"^do\syou\s([\w+\s]+)?know\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\?$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\.$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?is\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,}).$",
        @"^(?<firstKey>who)\s(?<secondKey>killed)\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$"
    };

    [SerializeField] Contact[] Contacts = new Contact[0];

    private Contact currentContact;
    
    public enum Call_State
    {
        ON_STANDBY,
        IN_CALL,
        IN_DIRECTORY
    };
    public Call_State CurrentState;

    private enum Directory_State
    {
        INPUTTING_CITY,
        IS_CHOOSING_RES_OR_BIS,
        INPUTTING_NAME,
        INPUTTING_BUSINESS        
    };
    private Directory_State _currentDirectoryState;


    //state bools
    private bool _isInExtentionSystem = false; // check if inputing extention number

    private void Start()
    {
        CurrentState = Call_State.ON_STANDBY;
    }

    private void Update()
    {
        if (PhoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            switch (CurrentState)
            {
                case Call_State.IN_CALL:
                    ExitCallMode();
                    break;
                case Call_State.IN_DIRECTORY:
                    ExitDirectoryMode();
                    break;
                default:
                    break;
            }            
        }       
    }

    public void NotInService()
    {
        StopAllCoroutines();
        CurrentState = Call_State.IN_CALL;
        DialogueAudioManager.PlayDialogueClip(0, 7);
    }

    public void EnterCallMode(int contact)
    {
        if (CurrentState == Call_State.IN_CALL)
        {
            return;
        }
        CurrentState = Call_State.IN_CALL;
        currentContact = Contacts[contact];
        CallPanelAnimator.SetBool("inCall", true);
        _playerInputField.gameObject.SetActive(true);
        _playerInputField.ActivateInputField();
    }

    public void ExitCallMode()
    {
        if (CurrentState != Call_State.IN_CALL)
        {
            return;
        }        
        CurrentState = Call_State.ON_STANDBY;
        StopAllCoroutines();
        PhoneManager.ClearDisplay();
        _playerInputField.DeactivateInputField();
        _playerInputField.gameObject.SetActive(false);
        CallPanelAnimator.SetBool("inCall", false);
    }

    public void ReadPlayerInput(string s)
    {
        if (CurrentState == Call_State.IN_CALL 
            && Input.GetKeyDown(KeyCode.Return))
        {
            currentContact.playerInput = s;
            currentContact.GenerateResponse();
        }
    }     

    IEnumerator TypeLine(string line)
    {
        float _textSpeed = .3f;
        if (line != null)
        {
            int index = 0;
            foreach (char letter in line.ToCharArray())
            {
                if (letter == '.')
                {
                    break; 
                }
                    int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];

                    PhoneManager.displayCharArray[index].GetComponent<CharController>()
                    .DisplayChar(letterAsInt);

                    index++;
                    yield return new WaitForSeconds(_textSpeed);                            
            }
        }
    }

    public void EnterDirectoryMode()
    {
        if (CurrentState == Call_State.IN_DIRECTORY)
        {
            return;
        }
        CurrentState = Call_State.IN_DIRECTORY;
        _currentDirectoryState = Directory_State.INPUTTING_CITY;
        DialogueAudioManager.PlayDialogueClip(0, 0);
        CallPanelAnimator.SetBool("inCall", true);
        _playerInputField.gameObject.SetActive(true);
        _playerInputField.ActivateInputField();
    }
    public void ExitDirectoryMode()
    {
        if (CurrentState != Call_State.IN_DIRECTORY)
        {
            return;
        }
        StopAllCoroutines();
        CurrentState = Call_State.ON_STANDBY;                
        _playerInputField.gameObject.SetActive(false);
        _playerInputField.text = string.Empty;
        CallPanelAnimator.SetBool("inCall", false);
    }

    // reading player input for directory (expand for other functions?)
    public void ReadDirectoryInput(string s)
    {
        if (CurrentState == Call_State.IN_DIRECTORY && Input.GetKeyDown(KeyCode.Return))
        {
            string playerInputCity = string.Empty;
            string directoryFinalInput = string.Empty;

            switch (_currentDirectoryState)
            {
                case Directory_State.INPUTTING_CITY:
                    _currentDirectoryState = Directory_State.IS_CHOOSING_RES_OR_BIS;                    
                    playerInputCity = s.ToUpper();
                    _playerInputField.text = "";
                    _playerInputField.GameObject().SetActive(false);
                    DialogueAudioManager.PlayDialogueClip(0, 1);
                    break;
                case Directory_State.INPUTTING_NAME:
                    string playerInputName = s.ToUpper();
                    directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower() 
                        + playerInputName.ToLower());
                    if (!Dictionary.GetInstance().directoryResidentialNumbers
                            .ContainsKey(directoryFinalInput))
                    {
                        StartCoroutine(DirectoryAnswer("unlisted", "residential"));
                    }
                    else
                    {
                        StartCoroutine(DirectoryAnswer("listed", "residential"));
                    }
                    break;
                case Directory_State.INPUTTING_BUSINESS:
                    string playerInputBusiness = s.ToUpper();
                    directoryFinalInput = ReplaceSpacesInString(playerInputCity.ToLower()
                        + playerInputBusiness.ToLower());
                    if (!Dictionary.GetInstance().directoryResidentialNumbers
                            .ContainsKey(directoryFinalInput))
                    {
                        StartCoroutine(DirectoryAnswer("unlisted", "business"));
                    }
                    else
                    {
                        StartCoroutine(DirectoryAnswer("listed", "business"));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void ResidentialListing()
    {
        if (_currentDirectoryState != Directory_State.IS_CHOOSING_RES_OR_BIS)        
        {
            return;
        }
        _currentDirectoryState = Directory_State.INPUTTING_NAME;
        _playerInputField.GameObject().SetActive(true);
        _playerInputField.ActivateInputField();
        DialogueAudioManager.dialogueaudioSource.Stop();
        DialogueAudioManager.PlayDialogueClip(0, 2);
    }

    public void BusinessListing()
    {
        if (_currentDirectoryState != Directory_State.IS_CHOOSING_RES_OR_BIS)
        {
            return;
        }
        _currentDirectoryState = Directory_State.INPUTTING_BUSINESS;
        _playerInputField.GameObject().SetActive(true);
        _playerInputField.ActivateInputField();
        DialogueAudioManager.dialogueaudioSource.Stop();
        DialogueAudioManager.PlayDialogueClip(0, 3);
    }

    IEnumerator DirectoryAnswer(string listedOrUnlisted, string residentialOrBusiness)
    {
        DialogueAudioManager.dialogueaudioSource.Stop();
        _playerInputField.gameObject.SetActive(false);
        if (residentialOrBusiness == "residential")
        {
            if (listedOrUnlisted == "unlisted")
            {
                yield return new WaitForSeconds(1f);
                DialogueAudioManager.PlayDialogueClip(0, 4);
                yield return new WaitForSeconds(3f);
                DialogueAudioManager.PlayDialogueClip(0, 5);
            }
            else
            {
                yield return new WaitForSeconds(1f);
                DialogueAudioManager.PlayDialogueClip(0, 4);
                //insert code for returning found number
            }
        }
        else if (residentialOrBusiness == "business")
        {
            if (listedOrUnlisted == "unlisted")
            {
                yield return new WaitForSeconds(1f);
                DialogueAudioManager.PlayDialogueClip(0, 4);
                yield return new WaitForSeconds(3f);
                DialogueAudioManager.PlayDialogueClip(0, 6);
                
            }
            else
            {
                yield return new WaitForSeconds(1f);
                DialogueAudioManager.PlayDialogueClip(0, 4);
                //insert code for returning found number
            }
        }
    }

    public void ReturnToDirectoryMainMenu()
    {
        if (CurrentState != Call_State.IN_DIRECTORY | 
            _currentDirectoryState == Directory_State.INPUTTING_CITY)
        {
            return;
        }
        _currentDirectoryState = Directory_State.INPUTTING_CITY;
        _playerInputField.gameObject.SetActive(true);
        _playerInputField.text = string.Empty;
        _playerInputField.ActivateInputField();
        DialogueAudioManager.PlayDialogueClip(0, 0);
    }


    private string ReplaceSpacesInString(string s)
    {
        string result = s.Replace(" ", "");
        return result; 
    }    
        
    // Getter Methods

    public bool GetExtentionStatus()
    {
        return _isInExtentionSystem;
    }

    public Call_State GetInDialogueStatus()
    {
        return CurrentState;
    }
}