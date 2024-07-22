
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Ink.Parsed;
using System.Collections.Generic;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PhoneDisplayController phoneDisplayController;

    [SerializeField] private GameObject[] inputChars = new GameObject[7]; // relevent phone display objects for displaying input
    [SerializeField] private int[] answerSequence = new int[7]; // correct solution
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
          new int[] { 0, 1, 2, 3, 5, 6, 7 } //9
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

    private bool isPuzzleSolved;

    private void Start()
    {        
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
        foreach (int number in answerSequence)
        {
            FillPuzzleColumn(puzzleColumns[index], number);
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
    }

    public void IncreaseNumber()
    {
        if (inputSequence[currentInputIndex] < 9)
        {
            inputSequence[currentInputIndex]++;
        }
        else
        {
            inputSequence[currentInputIndex] = 0;
        }
        inputChars[currentInputIndex].GetComponent<CharController>().DisplayChar(inputSequence[currentInputIndex]);
    }

    public void DecreaseNumber()
    {
        if (inputSequence[currentInputIndex] > 0)
        {
            inputSequence[currentInputIndex]--;
        }
        else
        {
            inputSequence[currentInputIndex] = 9;
        }
        inputChars[currentInputIndex].GetComponent<CharController>().DisplayChar(inputSequence[currentInputIndex]);
    }

    public void SubmitInputSequence()
    {
        if (inputSequence.SequenceEqual(answerSequence) != true)
        {
            return;
        }

         isPuzzleSolved = true;
        foreach (GameObject inputChar in inputChars)
        {
            inputChar.gameObject.GetComponent<CharController>().ChangeCharColor();
        }           
    }

    private bool EqualityOperator(int[] firstArray, int[] secondArray) //for checking if inputSequence matches answerSequence.
    {
        return firstArray == secondArray;
    }
}
