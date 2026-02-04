using Ink.Parsed;
using UnityEngine;
using System.Collections.Generic;
using Dialogue.Core;



public class InputInterpreter : MonoBehaviour
{
    Intent foundIntent = Intent.None;
    WordID foundTopic = WordID.None;

    public ResponseKey InterpretPlayerInput(List<RectTransform> playerInput)
    {
        List<RectTransform> sentence = new List<RectTransform>();
        sentence = playerInput;
        

        for (int i = 0; i < sentence.Count; i++)
        {
            if(sentence[i].GetComponent<DraggableWord>().sentenceWordEntry.Word.
                Intent != Intent.None)
               
            {
                foundIntent = sentence[i].GetComponent<DraggableWord>().
                    sentenceWordEntry.Word.Intent;
            }
        }

        for (int i = 0; i < sentence.Count; i++)
        {
            if (sentence[i].GetComponent<DraggableWord>().sentenceWordEntry.Word.
                HasPartOfSpeech(PartsOfSpeech.Character))
            {
                foundTopic = sentence[i].GetComponent<DraggableWord>().sentenceWordEntry.Word.
                WordID;
            }
        }

        ResponseKey responsekey = new ResponseKey(foundIntent, foundTopic);

        return responsekey;
    }
}
