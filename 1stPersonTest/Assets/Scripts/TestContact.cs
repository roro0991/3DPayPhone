using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestContact : Contact
{
    private Dictionary <string, string> inputResponses = new Dictionary<string, string>();
    private Dictionary <string, string[]> wordsForBank = new Dictionary<string, string[]>();

    private void Awake()
    {
        inputResponses.Add("Hello.", "Hi there.");
        inputResponses.Add("How are you?", "I'm fine, thanks.");
        inputResponses.Add("Is Sarah there?", "I'm afraid not.");

        wordsForBank.Add("Hi there.", new string[] { "How", "are", "you?" });
        wordsForBank.Add("I'm fine, thanks.", new string[] { "Is", "Sarah", "there?" });
    }
    
    public override void GenerateResponse()
    {
        ContactResponse = string.Empty; 
        if (inputResponses.ContainsKey(PlayerInput))
        {
            ContactResponse = inputResponses[PlayerInput];
        }
        else
        {
            Debug.Log("I don't understand.");            
            return;
        }
        Debug.Log(ContactResponse);
        SentenceWords.Clear();
        if (wordsForBank.ContainsKey(ContactResponse))
        {
            foreach (string word in wordsForBank[ContactResponse])
            {
                SentenceWords.Add(word);
            }
        }
    }   
}
