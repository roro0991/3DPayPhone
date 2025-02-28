using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Contact : MonoBehaviour
{
    public PlayerInputParser inputParser;
    public string contactNumber;
    public int contactID;    
    public string playerInput = string.Empty;
    public string contactResponse = string.Empty;

    public abstract void GenerateResponse();
    
    public void TestValues()
    {
        Debug.Log(contactNumber);
        Debug.Log(contactID);
    }
}
