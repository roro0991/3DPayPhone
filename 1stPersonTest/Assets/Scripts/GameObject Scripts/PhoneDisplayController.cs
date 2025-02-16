using Ink;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PhoneDisplayController : MonoBehaviour
{
    [SerializeField] private PhoneManager phoneManager;    
    [SerializeField] public GameObject[] displayCharArray = new GameObject[85]; 
    [SerializeField] private GameObject[] messageLineArray = new GameObject[18]; //34-51
    private string _pickUpReceiverString = "lift receiver";    

    private void Start()
    {        
        ClearAllDisplayChars();
        StartCoroutine(AnimateMessage());
    }

    //Methods
    public void ClearAllDisplayChars()
    {
        foreach (GameObject character in displayCharArray)
        {
            character.GetComponent<CharController>().ClearChar();
        }
    }

    public void PickUpReceiverMessage()
    {
        int index = 0;
        ClearAllDisplayChars();
        foreach (char letter in _pickUpReceiverString.ToCharArray())
        {
            int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
            messageLineArray[index].GetComponent<CharController>().DisplayChar(letterAsInt);
            index++;
        }
    }

    //Coroutine Methods
    IEnumerator AnimateMessage()
    {
        while (true)
        {
            if (phoneManager.GetReceiverStatus() == false)
            {
                for (int i = messageLineArray.Length - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        messageLineArray[i] = messageLineArray[i - 1];
                    }
                    else
                    {
                        messageLineArray[i] = messageLineArray[messageLineArray.Length - 1];
                    }
                }
                PickUpReceiverMessage();
                yield return new WaitForSeconds(.30f);                
            }
            yield return null;
        }
    }
}
