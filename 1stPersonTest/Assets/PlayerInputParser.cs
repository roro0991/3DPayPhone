using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using UnityEngine;
using static CallManager;

public class PlayerInputParser : MonoBehaviour
{
    public int? CurrentDialogueStateAsInt;

    private string _playerInput;

    public string FirstKey = string.Empty;
    public string SecondKey = string.Empty;
    public string QuestionTarget = string.Empty;

    private string OBSCENITY_PATTERN = @"(fuck|shit|cock)";
    private string GREETINGS_PATTERN = @"(^hello|^hi|^greetings)";
    private string QUESTION_FIRST_KEY_PATTERN = @"(?<firstKey>wh(o|at|ere|en|y))";

    private string[] WHO_QUESTION_PATTERN_ARRAY = new[]
    {
        @"^(?<firstKey>who)\s(((\w+)?\s)+)?are\s(?<fullName>you)\?$",
        @"^(?<firstKey>who)\s(((\w+)?\s)+)?is\s(?<fullName>[a-z]{1,}(\s[a-z]{1,})?)\?$",
        @"^do\syou\s([\w+\s]+)?know\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\?$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,})\sis\.$",
        @"^([\w+\s]+)?tell\sme\s(?<firstKey>who)\s(((\w+)?\s)+)?is\s(((\w+)?\s)+)?(?<fullName>[a-z]{1,}\s[a-z]{1,}).$",
        @"^(?<firstKey>who)\s(?<secondKey>killed)\s(?<fullName>[a-z]{1,}\s[a-z]{1,})\?$"
    };

    private string[] WHAT_QUESTION_PATTERN_ARRAY = new[]
    {
        @"^(?<firstKey>what)\s(((\w+)?\s)+)?is\syour\s(((\w+)?\s)+)?(?<fullName>name)\?$"
    };

    public void ParsePlayerinput(string playerInput)
    {
        FirstKey = string.Empty;
        SecondKey = string.Empty;
        QuestionTarget = string.Empty;

        if (Regex.IsMatch(playerInput, OBSCENITY_PATTERN))
        {
            FirstKey = "obscene";
            return;
        }

        if (Regex.IsMatch(playerInput, GREETINGS_PATTERN))
        {
            CurrentDialogueStateAsInt = 1; //greeting
        }
        else if (Regex.IsMatch(playerInput, @"\?$"))
        {
            CurrentDialogueStateAsInt = 2; //asking question
        }

        switch (CurrentDialogueStateAsInt)
        {
            case 1:
                FirstKey = playerInput;
                break;
            case 2:
                var inputMatch = Regex.Match(playerInput, QUESTION_FIRST_KEY_PATTERN);
                FirstKey = inputMatch.Groups["firstKey"].ToString();                               
                if (FirstKey != string.Empty)
                {
                    switch (FirstKey)
                    {
                        case "who":
                            foreach (string whoQuestionPattern in WHO_QUESTION_PATTERN_ARRAY)
                            {
                                if (Regex.IsMatch(playerInput, whoQuestionPattern))
                                {
                                    inputMatch = Regex.Match(playerInput, whoQuestionPattern);
                                    SecondKey = inputMatch.Groups["secondKey"].ToString();
                                    QuestionTarget = inputMatch.Groups["fullName"].ToString();
                                    break;
                                }
                            }
                            break;
                        case "what":
                            foreach (string whatQuestionPattern in WHAT_QUESTION_PATTERN_ARRAY)
                            {
                                if (Regex.IsMatch(playerInput, whatQuestionPattern))
                                {
                                    inputMatch = Regex.Match(playerInput, whatQuestionPattern);
                                    SecondKey = inputMatch.Groups["secondKey"].ToString();
                                    QuestionTarget = inputMatch.Groups["fullName"].ToString();
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
