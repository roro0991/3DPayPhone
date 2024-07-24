
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
    int[] answerSequenceAsInt = new int[7];
    private int[] inputSequence = new int[7]; // storing input sequence
    private int currentInputIndex; // tracking where int the input sequence we are    

    // jagged array for storing relevant display char segments for each possible number
    int[][] numbersArray = new int[][]
        { new int[] { 0, 1, 2, 3, 4, 5, 10, 11 }, //0
          new int[] { 1, 2, 10 }, //1
          new int[] { 0, 1, 3, 4, 6, 7 }, //2
          new int[] { 0, 1, 2, 3, 7 }, //3
          new int[] { 1, 2, 5, 6, 7 }, // 4
          new int[] { 0, 2, 3, 5, 6, 7 }, // 5
          new int[] { 0, 2, 3, 4, 5, 6, 7 }, //6
          new int[] { 0, 1, 2 }, //7
          new int[] { 0, 1, 2, 3, 4, 5, 6, 7}, //8
          new int[] { 0, 1, 2, 3, 5, 6, 7 }, //9
          new int[] { 0, 1, 2, 4, 5, 6, 7}, //a
          new int[] { 0, 1, 2, 3, 7, 9, 12}, //b
          new int[] { 0, 3, 4, 5}, //c
          new int[] { 0, 1, 2, 3, 9, 12}, //d
          new int[] { 0, 3, 4, 5, 6}, //e
          new int[] { 0, 4, 5, 6}, //f
          new int[] { 0, 2, 3, 4, 5, 7}, //g
          new int[] { 1, 2, 4, 5, 6, 7}, //h
          new int[] { 0, 3, 9, 12}, //i
          new int[] { 1, 2, 3, 4}, //j
          new int[] { 4, 5, 6, 10, 13}, //k
          new int[] { 3, 4, 5}, //l
          new int[] { 1, 2, 4, 5, 8, 10}, //m
          new int[] { 1, 2, 4, 5, 8, 13}, //n
          new int[] { 0, 1, 2, 3, 4, 5,}, //o
          new int[] { 0, 1, 4, 5, 6, 7}, //p
          new int[] { 0, 1, 2, 3, 4, 5, 13}, //q
          new int[] { 0, 1, 4, 5, 6, 7, 13}, //r
          new int[] { 0, 2, 3, 5, 6, 7}, //s
          new int[] { 0, 9, 12}, //t
          new int[] { 1, 2, 3, 4}, //u
          new int[] { 4, 5, 10, 11}, //v
          new int[] { 1, 2, 4, 5, 11, 13}, //w
          new int[] { 8, 10, 11, 13}, //x
          new int[] { 8, 10, 12}, //y
          new int[] { 0, 3, 10, 11} //z

        };

    // relevent display objects for randomizing number segments
    [SerializeField] private GameObject[] puzzleColumnOne = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnTwo = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnThree = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnFour = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnFive = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnSix = new GameObject[4];
    [SerializeField] private GameObject[] puzzleColumnSeven = new GameObject[4];

    private List<GameObject[]> puzzleColumns = new List<GameObject[]>(); // list of display column arrays above

    bool inNumbers = true;
    private bool isPuzzleSolved;

    private void Start()
    {
        Dictionary<char, int> keyValuePairs = new Dictionary<char, int>();
        keyValuePairs.Add('0', 0);
        keyValuePairs.Add('1', 1);
        keyValuePairs.Add('2', 2);
        keyValuePairs.Add('3', 3);
        keyValuePairs.Add('4', 4);
        keyValuePairs.Add('5', 5);
        keyValuePairs.Add('6', 6);
        keyValuePairs.Add('7', 7);
        keyValuePairs.Add('8', 8);
        keyValuePairs.Add('9', 9);
        keyValuePairs.Add('a', 10);
        keyValuePairs.Add('b', 11);
        keyValuePairs.Add('c', 12);
        keyValuePairs.Add('d', 13);
        keyValuePairs.Add('e', 14);
        keyValuePairs.Add('f', 15);
        keyValuePairs.Add('g', 16);
        keyValuePairs.Add('h', 17);
        keyValuePairs.Add('i', 18);
        keyValuePairs.Add('j', 19);
        keyValuePairs.Add('k', 20);
        keyValuePairs.Add('l', 21);
        keyValuePairs.Add('m', 22);
        keyValuePairs.Add('n', 23);
        keyValuePairs.Add('o', 24);
        keyValuePairs.Add('p', 25);
        keyValuePairs.Add('q', 26);
        keyValuePairs.Add('r', 27);
        keyValuePairs.Add('s', 28);
        keyValuePairs.Add('t', 29);
        keyValuePairs.Add('u', 30);
        keyValuePairs.Add('v', 31);
        keyValuePairs.Add('w', 32);
        keyValuePairs.Add('x', 33);
        keyValuePairs.Add('y', 34);
        keyValuePairs.Add('z', 35);


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
            int characterAsInt = keyValuePairs[character];
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
        for (int i = 0; i < numbersArray[correctNumber].Length; i++)
        {
            if (numberOfColumnIterations < column.Length)
            {
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(numbersArray[correctNumber][i]);
                numberOfColumnIterations++;
                randomizedColumnSegmentsIndex++; 
            }
            else
            {
                numberOfColumnIterations = 0;
                randomizedColumnSegmentsIndex = 0;
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(numbersArray[correctNumber][i]);                
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
