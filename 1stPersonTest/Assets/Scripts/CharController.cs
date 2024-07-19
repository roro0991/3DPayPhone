using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CharController : MonoBehaviour
{
    [SerializeField] private GameObject[] segments = new GameObject[14];
   
    public void DisplayChar(int number)
    {
        switch (number)
        {
            case 0:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(true);
                segments[5].SetActive(true);
                segments[6].SetActive(false);
                segments[7].SetActive(false);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(true);
                segments[11].SetActive(true);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 1:
                segments[0].SetActive(false);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(false);
                segments[4].SetActive(false);
                segments[5].SetActive(false);
                segments[6].SetActive(false);
                segments[7].SetActive(false);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(true);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 2:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(false);
                segments[3].SetActive(true);
                segments[4].SetActive(true);
                segments[5].SetActive(false);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 3:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(false);
                segments[5].SetActive(false);
                segments[6].SetActive(false);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 4:
                segments[0].SetActive(false);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(false);
                segments[4].SetActive(false);
                segments[5].SetActive(true);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 5:
                segments[0].SetActive(true);
                segments[1].SetActive(false);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(false);
                segments[5].SetActive(true);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 6:
                segments[0].SetActive(true);
                segments[1].SetActive(false);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(true);
                segments[5].SetActive(true);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 7:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(false);
                segments[4].SetActive(false);
                segments[5].SetActive(false);
                segments[6].SetActive(false);
                segments[7].SetActive(false);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 8:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(true);
                segments[5].SetActive(true);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;
            case 9:
                segments[0].SetActive(true);
                segments[1].SetActive(true);
                segments[2].SetActive(true);
                segments[3].SetActive(true);
                segments[4].SetActive(false);
                segments[5].SetActive(true);
                segments[6].SetActive(true);
                segments[7].SetActive(true);
                segments[8].SetActive(false);
                segments[9].SetActive(false);
                segments[10].SetActive(false);
                segments[11].SetActive(false);
                segments[12].SetActive(false);
                segments[13].SetActive(false);
                break;            
        }
    }

    public void DisplayDash()
    {
        segments[0].SetActive(false);
        segments[1].SetActive(false);
        segments[2].SetActive(false);
        segments[3].SetActive(false);
        segments[4].SetActive(false);
        segments[5].SetActive(false);
        segments[6].SetActive(true);
        segments[7].SetActive(true);
        segments[8].SetActive(false);
        segments[9].SetActive(false);
        segments[10].SetActive(false);
        segments[11].SetActive(false);
        segments[12].SetActive(false);
        segments[13].SetActive(false);
    }

    public void ClearChar()
    {
        segments[0].SetActive(false);
        segments[1].SetActive(false);
        segments[2].SetActive(false);
        segments[3].SetActive(false);
        segments[4].SetActive(false);
        segments[5].SetActive(false);
        segments[6].SetActive(false);
        segments[7].SetActive(false);
        segments[8].SetActive(false);
        segments[9].SetActive(false);
        segments[10].SetActive(false);
        segments[11].SetActive(false);
        segments[12].SetActive(false);
        segments[13].SetActive(false);
    }
}