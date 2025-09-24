using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    public List<string> SentenceWords = new List<string>();
    public string ContactNumber;
    public string ContactName;
    public string OpeningLine = string.Empty;
    public string PlayerInput = string.Empty;
    public string ContactResponse = string.Empty;

    // Abstract methods to be implemented by subclasses
    public abstract void SpeakFirstLine();
    public abstract void GenerateResponse();
}

