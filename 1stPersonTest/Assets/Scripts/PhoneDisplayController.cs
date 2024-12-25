using Ink;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PhoneDisplayController : MonoBehaviour
{
    [SerializeField] public GameObject[] chars = new GameObject[85];
    [SerializeField] private GameObject[] messageLine = new GameObject[18];
    [SerializeField] private PhoneManager phoneManager;
    private GameObject[] shiftedLine = new GameObject[17];
    private string pickUpReceiver = "lift receiver";
    private string insterCoins = "insert money";

    private void Start()
    {        
        ClearAllChars();
        StartCoroutine(AnimateMessage());
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
            int letterAsInt = Dictionary.GetInstance().charIntPairs[letter];
            messageLine[index].GetComponent<CharController>().DisplayChar(letterAsInt);
            index++;
        }
    }


    IEnumerator AnimateMessage()
    {
        while (true)
        {
            while (phoneManager.GetReceiverStatus() == false)
            {
                for (int i = messageLine.Length - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        messageLine[i] = messageLine[i - 1];
                    }
                    else
                    {
                        messageLine[i] = messageLine[messageLine.Length - 1];
                    }
                }
                PickUpReceiverMessage();
                yield return new WaitForSeconds(.30f);                

            }
            yield return null;
        }
    }
}
