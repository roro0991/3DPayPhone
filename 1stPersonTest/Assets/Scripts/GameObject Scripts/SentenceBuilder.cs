using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class SentenceBuilder : MonoBehaviour
{
    public Vector2 startPosition = Vector2.zero; // Adjust in Inspector for sentence start offset
    public float spacing = 10f; // Space between words

    [HideInInspector]
    public List<RectTransform> wordList = new List<RectTransform>();
    public string currentSentenceAsString;

    private RectTransform placeholderWord;

    // Update currentSentenceAsString only when sentence changes, so no Update() needed

    private void UpdateCurrentSentenceString()
    {
        currentSentenceAsString = GetSentenceAsString();
    }

    public string GetSentenceAsString()
    {
        StringBuilder result = new StringBuilder();

        foreach (RectTransform rect in wordList)
        {
            TMP_Text tmpText = rect.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                result.Append(tmpText.text + " ");
            }
        }

        return result.ToString().Trim();
    }

    public void AddWord(RectTransform word)
    {
        if (!wordList.Contains(word))
        {
            wordList.Add(word);
            UpdateWordPositions();
            UpdateCurrentSentenceString();
        }
    }

    public void RemoveWord(RectTransform word)
    {
        if (wordList.Contains(word))
        {
            wordList.Remove(word);
            UpdateWordPositions();
            UpdateCurrentSentenceString();
        }
    }

    public void ClearSentence()
    {
        wordList.Clear();
        for (int i = this.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        UpdateCurrentSentenceString();
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
        UpdateCurrentSentenceString();
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

    // -------- Placeholder logic ---------

    /// <summary>
    /// Shows the placeholder word at the specified index in the sentence, shifting other words accordingly.
    /// </summary>
    /// <param name="index">The index where to show the placeholder.</param>
    /// <param name="placeholder">The RectTransform representing the placeholder word.</param>
    public void ShowPlaceholderAt(int index, RectTransform placeholder)
    {
        if (placeholderWord != null)
        {
            RemovePlaceholder();
        }

        placeholderWord = placeholder;

        index = Mathf.Clamp(index, 0, wordList.Count);
        wordList.Insert(index, placeholderWord);
        UpdateWordPositions();
    }

    /// <summary>
    /// Removes the placeholder word from the sentence.
    /// </summary>
    public void RemovePlaceholder()
    {
        if (placeholderWord != null && wordList.Contains(placeholderWord))
        {
            wordList.Remove(placeholderWord);
            UpdateWordPositions();
            placeholderWord = null;
        }
    }

    /// <summary>
    /// Calculates the insertion index based on the local X position of the pointer.
    /// Assumes horizontal layout from left to right.
    /// </summary>
    /// <param name="localX">The local X position relative to the container.</param>
    /// <returns>The index where a word should be inserted.</returns>
    public int GetInsertionIndex(float localX)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            float wordCenterX = wordList[i].anchoredPosition.x + wordList[i].rect.width * 0.5f;
            if (localX < wordCenterX)
                return i;
        }
        return wordList.Count;
    }
}



