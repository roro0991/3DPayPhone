
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Ink.Parsed;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using Random = UnityEngine.Random;

public class PuzzleManager : MonoBehaviour
{
    public Puzzle puzzle;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PhoneDisplayController phoneDisplayController;

    [SerializeField] private GameObject[] inputChars = new GameObject[7]; // relevent phone display objects for displaying input
    private char[] answerSequence = new char[7]; // correct solution
    int[] answerSequenceAsInt = new int[7]; // converted to int to compare to inputsequence
    private int[] inputSequence = new int[7]; // storing input sequence
    private int currentInputIndex; // tracking where int the input sequence we are       

    // relevent display objects for randomizing number segments
    [SerializeField] private GameObject[] puzzleColumnOne = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnTwo = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnThree = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnFour = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnFive = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnSix = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnSeven = new GameObject[4];

    private List<GameObject[]> puzzleColumns = new List<GameObject[]>(); // list of display column arrays above

    private bool inNumbers = true;
    private bool isPuzzleSolved;

    private void Start()
    {
        answerSequence = puzzle.answerSequence;

        puzzleColumns.AddRange(new List<GameObject[]>()
        {   puzzleColumnOne,
            puzzleColumnTwo,
            puzzleColumnThree,
            puzzleColumnFour,
            puzzleColumnFive,
            puzzleColumnSix,
            puzzleColumnSeven}
        );

        
        int index = 0;
        foreach (char character in answerSequence)
        {
            int characterAsInt = Dictionary.GetInstance().charIntPairs[character];
            answerSequenceAsInt[index] = characterAsInt;
            FillPuzzleColumn(puzzleColumns[index], characterAsInt);
            index++;
        }
    }


    private void Update()
    {

        // changing char color to indicate which one you have selected & when you've submitted correct sequence.
        foreach (GameObject inputChar in inputChars)
        {
            if (isPuzzleSolved)
            {
                return;
            }

            if (inputChar == inputChars[currentInputIndex])
            {
                inputChar.gameObject.GetComponent<CharController>().ChangeCharColor();
            }
            else
            {
                inputChar.gameObject.GetComponent<CharController>().ChangeCharColorWhite();
            }
        }
    }

    private void FillPuzzleColumn(GameObject[] column, int correctNumber)
    {
        // generating randomized column char list       
        int count = column.Length;
        int[] randomizedColumnSegments = new int[count];
        
        for (int i = 0; i < count; i++)
        {
            int j = Random.Range(0, i + 1);
            randomizedColumnSegments[i] = randomizedColumnSegments[j];
            randomizedColumnSegments[j] = 0 + i;
        }
        //

        int randomizedColumnSegmentsIndex = 0;
        int numberOfColumnIterations = 0;
        for (int i = 0; i < Dictionary.GetInstance().charSegments[correctNumber].Length; i++)
        {
            if (numberOfColumnIterations < column.Length)
            {
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(Dictionary.GetInstance().charSegments[correctNumber][i]);
                numberOfColumnIterations++;
                randomizedColumnSegmentsIndex++; 
            }
            else
            {
                numberOfColumnIterations = 0;
                randomizedColumnSegmentsIndex = 0;
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(Dictionary.GetInstance().charSegments[correctNumber][i]);                
                randomizedColumnSegmentsIndex++;
            }
        }

        isPuzzleSolved = false;
        currentInputIndex = 0;
        foreach (GameObject digit in inputChars)
        {
            digit.GetComponent<CharController>().DisplayChar(0);
        }
    }


    public void MoveToRightChar()
    {
        if (currentInputIndex < inputChars.Length - 1)
        {
            currentInputIndex = currentInputIndex += 1;
        }
        else
        {
            currentInputIndex = 0;
        }        

        if (inputSequence[currentInputIndex] > 9)
        {
            inNumbers = false;
        }
        else
        {
            inNumbers = true;
        }
    }

    public void MoveToLeftChar()
    {
        if (currentInputIndex > 0)
        {
            currentInputIndex = currentInputIndex -= 1;
        }
        else
        {
            currentInputIndex = 6;
        }

        if (inputSequence[currentInputIndex] > 9)
        {
            inNumbers = false;
        }
        else
        {
            inNumbers = true;
        }
    }

    public void IncreaseNumber()
    {               
        if (inNumbers)
        {
            if (inputSequence[currentInputIndex] < 9)
            {
                inputSequence[currentInputIndex]++;
            }
            else
            {
                inputSequence[currentInputIndex] = 0;
            }
        }
        else if (!inNumbers)
        {
            if (inputSequence[currentInputIndex] < 35)
            {
                inputSequence[currentInputIndex]++;
            }
            else
            {
                inputSequence[currentInputIndex] = 10;
            }
        }
        inputChars[currentInputIndex].GetComponent<CharController>().DisplayChar(inputSequence[currentInputIndex]);
        
    }

    public void DecreaseNumber()
    {
        if (inNumbers)
        {
            if (inputSequence[currentInputIndex] > 0)
            {
                inputSequence[currentInputIndex]--;
            }
            else
            {
                inputSequence[currentInputIndex] = 9;
            }
        }
        else if (!inNumbers)
        {
            if (inputSequence[currentInputIndex] > 10)
            {
                inputSequence[currentInputIndex]--;
            }
            else
            {
                inputSequence[currentInputIndex] = 35;
            }
        }
        inputChars[currentInputIndex].GetComponent<CharController>().DisplayChar(inputSequence[currentInputIndex]);        
    }

    public void SwitchToNumbersOrLetters()
    {
        if (!inNumbers)
        {
            inNumbers = true;
            inputSequence[currentInputIndex] = 0;
        }
        else
        {
            inNumbers = false;
            inputSequence[currentInputIndex] = 10;
        }
        inputChars[currentInputIndex].GetComponent<CharController>().DisplayChar(inputSequence[currentInputIndex]);
    }

    public void SubmitInputSequence()
    {

        if (inputSequence.SequenceEqual(answerSequenceAsInt) != true)
        {
            return;
        }


        isPuzzleSolved = true;
        foreach (GameObject inputChar in inputChars)
        {
            inputChar.gameObject.GetComponent<CharController>().ChangeCharColor();
        } 
    }

    private bool EqualityOperator(char[] firstArray, char[] secondArray) //for checking if inputSequence matches answerSequence.
    {
        return firstArray == secondArray;
    }
}
