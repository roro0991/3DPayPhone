using System.Collections.Generic;
using UnityEngine;

public class Dictionary : MonoBehaviour
{
    public static Dictionary instance { get; private set; }
    public Dictionary<char, int> charIntPairs = new Dictionary<char, int>(); //to treat char inputs as ints for method arguments
    public Dictionary<int, int[]> charSegments = new Dictionary<int, int[]>(); //for accessing segment arrays of each char

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dictionary in the scene");
        }
        instance = this;

        charIntPairs.Add(' ', 99);
        charIntPairs.Add('0', 0);
        charIntPairs.Add('1', 1);
        charIntPairs.Add('2', 2);
        charIntPairs.Add('3', 3);
        charIntPairs.Add('4', 4);
        charIntPairs.Add('5', 5);
        charIntPairs.Add('6', 6);
        charIntPairs.Add('7', 7);
        charIntPairs.Add('8', 8);
        charIntPairs.Add('9', 9);
        charIntPairs.Add('a', 10);
        charIntPairs.Add('b', 11);
        charIntPairs.Add('c', 12);
        charIntPairs.Add('d', 13);
        charIntPairs.Add('e', 14);
        charIntPairs.Add('f', 15);
        charIntPairs.Add('g', 16);
        charIntPairs.Add('h', 17);
        charIntPairs.Add('i', 18);
        charIntPairs.Add('j', 19);
        charIntPairs.Add('k', 20);
        charIntPairs.Add('l', 21);
        charIntPairs.Add('m', 22);
        charIntPairs.Add('n', 23);
        charIntPairs.Add('o', 24);
        charIntPairs.Add('p', 25);
        charIntPairs.Add('q', 26);
        charIntPairs.Add('r', 27);
        charIntPairs.Add('s', 28);
        charIntPairs.Add('t', 29);
        charIntPairs.Add('u', 30);
        charIntPairs.Add('v', 31);
        charIntPairs.Add('w', 32);
        charIntPairs.Add('x', 33);
        charIntPairs.Add('y', 34);
        charIntPairs.Add('z', 35);

        charSegments.Add(0, new int[] { 0, 1, 2, 3, 4, 5, 10, 11 }); //0
        charSegments.Add(1, new int[] { 1, 2, 10 }); //1
        charSegments.Add(2, new int[] { 0, 1, 3, 4, 6, 7 }); //2
        charSegments.Add(3, new int[] { 0, 1, 2, 3, 7 }); //3
        charSegments.Add(4, new int[] { 1, 2, 5, 6, 7 }); //4
        charSegments.Add(5, new int[] { 0, 2, 3, 5, 6, 7 }); //5
        charSegments.Add(6, new int[] { 0, 2, 3, 4, 5, 6, 7 }); //6
        charSegments.Add(7, new int[] { 0, 1, 2 }); //7
        charSegments.Add(8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }); //8
        charSegments.Add(9, new int[] { 0, 1, 2, 3, 5, 6, 7 }); // 9
        charSegments.Add(10, new int[] { 0, 1, 2, 4, 5, 6, 7 }); //a
        charSegments.Add(11, new int[] { 0, 1, 2, 3, 7, 9, 12 }); //b
        charSegments.Add(12, new int[] { 0, 3, 4, 5 }); //c
        charSegments.Add(13, new int[] { 0, 1, 2, 3, 9, 12 }); //d
        charSegments.Add(14, new int[] { 0, 3, 4, 5, 6 }); //e
        charSegments.Add(15, new int[] { 0, 4, 5, 6 }); //f
        charSegments.Add(16, new int[] { 0, 2, 3, 4, 5, 7 }); //g
        charSegments.Add(17, new int[] { 1, 2, 4, 5, 6, 7 }); //h
        charSegments.Add(18, new int[] { 0, 3, 9, 12 }); //i
        charSegments.Add(19, new int[] { 1, 2, 3, 4 }); //j
        charSegments.Add(20, new int[] { 4, 5, 6, 10, 13 }); //k
        charSegments.Add(21, new int[] { 3, 4, 5 }); //l
        charSegments.Add(22, new int[] { 1, 2, 4, 5, 8, 10 }); //m
        charSegments.Add(23, new int[] { 1, 2, 4, 5, 8, 13 }); //n
        charSegments.Add(24, new int[] { 0, 1, 2, 3, 4, 5, }); //o
        charSegments.Add(25, new int[] { 0, 1, 4, 5, 6, 7 }); //p
        charSegments.Add(26, new int[] { 0, 1, 2, 3, 4, 5, 13 }); //q
        charSegments.Add(27, new int[] { 0, 1, 4, 5, 6, 7, 13 }); //r
        charSegments.Add(28, new int[] { 0, 2, 3, 5, 6, 7 }); //s
        charSegments.Add(29, new int[] { 0, 9, 12 }); // t
        charSegments.Add(30, new int[] { 1, 2, 3, 4, 5 }); //u
        charSegments.Add(31, new int[] { 4, 5, 10, 11 }); //v
        charSegments.Add(32, new int[] { 1, 2, 4, 5, 11, 13 }); //w
        charSegments.Add(33, new int[] { 8, 10, 11, 13 }); //x
        charSegments.Add(34, new int[] { 8, 10, 12 }); //y
        charSegments.Add(35, new int[] { 0, 3, 10, 11 }); //z
    }



    public static Dictionary GetInstance()
    {
        return instance;
    }
}
