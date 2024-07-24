using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Puzzle", menuName = "Puzzle")]
public class Puzzle : ScriptableObject
{

    public char[] answerSequence = new char[7]; 
}
