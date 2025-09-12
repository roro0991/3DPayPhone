using System.Collections.Generic;
using UnityEngine;

public class SentenceBuilder : MonoBehaviour
{
    public Vector2 startPosition = Vector2.zero; // Adjust in Inspector for sentence start offset
    public float spacing = 10f; // Space between words

    [HideInInspector]
    public List<RectTransform> wordList = new List<RectTransform>();

    public void AddWord(RectTransform word)
    {
        if (!wordList.Contains(word))
        {
            wordList.Add(word);
            UpdateWordPositions();
        }
    }

    public void RemoveWord(RectTransform word)
    {
        if (wordList.Contains(word))
        {
            wordList.Remove(word);
            UpdateWordPositions();
        }
    }

    public void InsertWordAt(RectTransform word, int index)
    {
        if (wordList.Contains(word))
        {
            wordList.Remove(word);
        }

        index = Mathf.Clamp(index, 0, wordList.Count);
        wordList.Insert(index, word);
        UpdateWordPositions();
    }

    public void UpdateWordPositions()
    {
        float currentX = startPosition.x;

        foreach (RectTransform word in wordList)
        {
            // Ensure pivot is left-middle for consistent positioning
            word.pivot = new Vector2(0, 0.5f);

            word.anchoredPosition = new Vector2(currentX, startPosition.y);

            float width = word.rect.width * word.localScale.x;
            currentX += width + spacing;
        }
    }

}


