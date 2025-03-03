
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    //public Puzzle puzzle;

    [SerializeField] private CallManager callManager;
    public Animator callPanelAnimator;
    [SerializeField] PhoneManager phoneManager;

    [SerializeField] private GameObject[] inputChars = new GameObject[7]; // relevent phone display objects for displaying input
    private char[] answerSequence = new char[7]; // correct solution
    private string answerSequenceAsString = string.Empty;
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
    private bool isInPuzzleMode = false;

    private void Start()
    {
        //answerSequence = puzzle.answerSequence;

        //answerSequence = puzzle.answerSequence;
        int index = 0;
        foreach (char letter in answerSequence)
        {
            if (index == 3)
            {
                answerSequenceAsString += "-";                
            }
            answerSequenceAsString += letter;
            index++;
        }

        puzzleColumns.AddRange(new List<GameObject[]>()
        {   puzzleColumnOne,
            puzzleColumnTwo,
            puzzleColumnThree,
            puzzleColumnFour,
            puzzleColumnFive,
            puzzleColumnSix,
            puzzleColumnSeven}
        );
    }

    private void Update()
    {
        if (phoneManager.GetReceiverStatus() == PhoneManager.State.RECEIVER_DOWN)
        {
            ExitPuzzleMode();
        }
    }

    public void EnterPuzzleMode(int puzzleType, char[] answerSequence)
    {
        callPanelAnimator.SetBool("inCall", false);
        phoneManager.ClearDisplay();
        if (!isInPuzzleMode)
        {
            isInPuzzleMode = true;
            switch (puzzleType)
            {
                case 1:
                    PuzzleTypeOne(answerSequence);
                    break;
                case 2:
                    PuzzleTypeTwo(answerSequence);
                    break;
            }
        }
    }

    public void ExitPuzzleMode()
    {
        if (!isInPuzzleMode)
        {
            return;
        }
        isInPuzzleMode = false;
        phoneManager.ClearDisplay();
    }
    
    private void PuzzleTypeOne(char[] answerSequence)
    {
        int index = 0;
        foreach (char character in answerSequence)
        {
            int characterAsInt = Dictionary.GetInstance().charIntPairs[character];
            answerSequenceAsInt[index] = characterAsInt;
            FillPuzzleColumn(puzzleColumns[index], characterAsInt);
            index++;
        }

        currentInputIndex = 0;
        foreach (GameObject digit in inputChars)
        {
            digit.GetComponent<CharController>().DisplayChar(0);
        }

        inputChars[currentInputIndex].GetComponent<CharController>().ChangeCharColorGreen();
    }
    

    private void FillPuzzleColumn(GameObject[] column, int correctCharacterAsInt)
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
        for (int i = 0; i < Dictionary.GetInstance().charSegments[correctCharacterAsInt].Length; i++)
        {
            if (numberOfColumnIterations < column.Length)
            {
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(Dictionary.GetInstance().charSegments[correctCharacterAsInt][i]);
                numberOfColumnIterations++;
                randomizedColumnSegmentsIndex++; 
            }
            else
            {
                numberOfColumnIterations = 0;
                randomizedColumnSegmentsIndex = 0;
                column[randomizedColumnSegments[randomizedColumnSegmentsIndex]].
                GetComponent<CharController>().
                DisplaySegment(Dictionary.GetInstance().charSegments[correctCharacterAsInt][i]);                
                randomizedColumnSegmentsIndex++;
            }
        }        
    }

    private void PuzzleTypeTwo(char[] answerSequence)
    {
        int index = 0;
        int answerSequenceIndex = 0;
        foreach (char character in answerSequence)
        {
            int characterAsInt = Dictionary.GetInstance().charIntPairs[character];
            answerSequenceAsInt[answerSequenceIndex] = characterAsInt;
            answerSequenceIndex++;
            for (int i = 0; i < Dictionary.GetInstance().charSegments[characterAsInt].Length; i++)
            {
                phoneManager.displayCharArray[index].GetComponent<CharController>().
                DisplayChar(Dictionary.GetInstance().charSegmentLetters
                [Dictionary.GetInstance().charSegments[characterAsInt][i]]);
                index++; 
            }
            phoneManager.displayCharArray[index].GetComponent<CharController>().DisplayDash();

            index++;
        }
       
        currentInputIndex = 0;
        foreach (GameObject digit in inputChars)
        {
            digit.GetComponent<CharController>().DisplayChar(0);
        }

        inputChars[currentInputIndex].GetComponent<CharController>().ChangeCharColorGreen();
    }


    public void MoveToRightChar()
    {
        if (!isInPuzzleMode)
        {
            return;
        }

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

        foreach (GameObject inputChar in inputChars)
        {
            if (inputChar == inputChars[currentInputIndex])
            {
                inputChar.GetComponent<CharController>().ChangeCharColorGreen();
            }
            else
            {
                inputChar.GetComponent<CharController>().ChangeCharColorWhite();
            }
        }
    }

    public void MoveToLeftChar()
    {
        if (!isInPuzzleMode)
        {
            return;
        }

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

        foreach (GameObject inputChar in inputChars)
        {
            if (inputChar == inputChars[currentInputIndex])
            {
                inputChar.GetComponent<CharController>().ChangeCharColorGreen();
            }
            else
            {
                inputChar.GetComponent<CharController>().ChangeCharColorWhite();                
            }
        }
    }

    public void IncreaseNumber()
    {
        if (!isInPuzzleMode)
        {
            return;
        }

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
        if (!isInPuzzleMode)
        {
            return;
        }

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
        if (!isInPuzzleMode)
        {
            return;
        }

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
        if (!isInPuzzleMode)
        {
            return;
        }

        if (isInPuzzleMode && inputSequence.SequenceEqual(answerSequenceAsInt) != true)
        {
            StartCoroutine(WrongSequence());
            return;
        }
        else
        {
            foreach (GameObject inputChar in inputChars)
            {
                inputChar.gameObject.GetComponent<CharController>().ChangeCharColorGreen();
            }        
        }        
    }

    IEnumerator WrongSequence()
    {
        foreach (GameObject inputChar in inputChars)
        {
            inputChar.gameObject.GetComponent<CharController>().ChangeCharColorRed();
        }
        yield return new WaitForSeconds(1.5f);
        foreach (GameObject inputChar in inputChars)
        {
            inputChar.gameObject.GetComponent<CharController>().ChangeCharColorWhite();
        }
        inputChars[currentInputIndex].GetComponent<CharController>().ChangeCharColorGreen();
    }

    private bool EqualityOperator(char[] firstArray, char[] secondArray) //for checking if inputSequence matches answerSequence.
    {
        return firstArray == secondArray;
    }

    // Getter methods

    public bool GetPuzzleStatus()
    {
        return isInPuzzleMode;
    }
}
