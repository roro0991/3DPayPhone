using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;

public enum Intent
{
    None,
    ASK_ABOUT_IDENTITY,
}
public class SentenceBreakdown
    {
        public Intent Intent; // Is it a question. If so, what kind of question?
        public string Topic; // What is the question about?
    }

public class InputInterpreter : MonoBehaviour
{        
    public string ContactResponse;

    public SentenceBreakdown InterpretPlayerInput(List<RectTransform> playerInput)
    {
        List<RectTransform> sentence = new List<RectTransform>();

        sentence = playerInput;

        SentenceBreakdown sb = new SentenceBreakdown();

        string firstWord = sentence[0].GetComponent<DraggableWord>().sentenceWordEntry.Surface; 

        for (int i = 0; i < sentence.Count; i++)
        {
            if (sentence[i].GetComponent<DraggableWord>().sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.ProperNoun))
            {
                sb.Topic = sentence[i].GetComponent<DraggableWord>().sentenceWordEntry.Surface;
                break;
            }
        }

        switch (firstWord)
        {
            case "Who":
                sb.Intent = Intent.ASK_ABOUT_IDENTITY;
                break;
            default:
                sb.Intent = Intent.None;
                break;
        }

        Debug.Log("The Topic is: "+sb.Topic);
        return sb;
    }
}
