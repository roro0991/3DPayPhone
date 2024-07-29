using Ink;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PhoneDisplayController : MonoBehaviour
{
    [SerializeField] public GameObject[] chars = new GameObject[85];
    [SerializeField] private GameObject[] messageLine = new GameObject[17];
    [SerializeField] private PhoneManager phoneManager;
    private string pickUpReceiver = "pick up receiver";
    

    private void Start()
    {        
        ClearAllChars();
        PickUpReceiverMessage();
    }


    public void ClearAllChars()
    {
        foreach (GameObject character in chars)
        {
            character.GetComponent<CharController>().ClearChar();
        }
    }

    public void PickUpReceiverMessage()
    {
        int index = 0;
        ClearAllChars();
        foreach (char letter in pickUpReceiver.ToCharArray())
        {
            messageLine[index].GetComponent<CharController>().DisplayChar(letter);
            index++;
        }
    }

    private GameObject[] Shift(GameObject[] array, int k)
    {
        var result = new GameObject[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            if (i == array.Length)
            {
                i = 0;
            }
            result[(i + k) % array.Length] = array[i];
        }
        return result;    
    }
}
