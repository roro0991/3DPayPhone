using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using UnityEngine;
using static CallManager;

public class PlayerInputParser : MonoBehaviour
{
    public enum Dialogue_State
    {
        ASKING_QUESTION,
        ASKING_FOLLOW_UP_QUESTION
    }
    public Dialogue_State CurrentDialogueState;

    private string _playerInput;

    public string firstKey = string.Empty;
    public string secondKey = string.Empty;
    public string questionTarget = string.Empty;
    
    private string QUESTION_FIRST_KEY_PATTERN = @"(?<firstKey>wh(o|at|ere|en|y))";

    private string[] WHO_QUESTION_PATTERN_ARRAY = new[]
    {
        @"^(?<firstKey>who)\s(((\w+)?\s)+)?is\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$",
        @"^do\syou\s([\w+\s]+)?know\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\?$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\.$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?is\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,}).$",
        @"^(?<firstKey>who)\s(?<secondKey>killed)\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$"
    };

    public void ParsePlayerinput(string playerInput)
    {                       
        if (Regex.IsMatch(playerInput, @"\?$"))
        {
            CurrentDialogueState = Dialogue_State.ASKING_QUESTION;
        }

        switch (CurrentDialogueState)
        {
            case Dialogue_State.ASKING_QUESTION:
                var inputMatch = Regex.Match(playerInput, QUESTION_FIRST_KEY_PATTERN);
                firstKey = inputMatch.Groups["firstKey"].ToString();                               
                if (firstKey != string.Empty)
                {
                    switch (firstKey)
                    {
                        case "who":
                            foreach (string whoQuestionPattern in WHO_QUESTION_PATTERN_ARRAY)
                            {
                                if (Regex.IsMatch(playerInput, whoQuestionPattern))
                                {
                                    inputMatch = Regex.Match(playerInput, whoQuestionPattern);
                                    secondKey = inputMatch.Groups["secondKey"].ToString();
                                    questionTarget = inputMatch.Groups["fullName"].ToString();
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }
}
