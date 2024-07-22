using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneDisplayController : MonoBehaviour
{
    [SerializeField] public GameObject[] chars = new GameObject[65];

    private void Awake()
    {
        ClearAllChars();        
    }
    
    public void ClearAllChars()
    {
        foreach (GameObject character in chars)
        {
            character.GetComponent<CharController>().ClearChar();
        }
    }
}
