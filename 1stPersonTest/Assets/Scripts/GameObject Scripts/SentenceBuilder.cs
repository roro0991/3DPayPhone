using UnityEngine;
using System.Collections.Generic;

public class SentenceBuilder : MonoBehaviour
{
    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>();
    public List<Word> wordDataList = new List<Word>(); // Track semantic words
    public string currentSentenceAsString;

    private RectTransform placeholderWord;

    // ---------------- Sentence management ----------------
    public void InsertWordAt(RectTransform rect, Word wordData, int index)
    {
        if (wordList.Contains(rect))
        {
            wordList.Remove(rect);
            wordDataList.Remove(wordData);
        }

        index = Mathf.Clamp(index, 0, wordList.Count);
        wordList.Insert(index, rect);
        wordDataList.Insert(index, wordData);

        UpdateWordPositions();
        UpdateSentenceString();
    }

    public void RemoveWord(RectTransform rect)
    {
        int idx = wordList.IndexOf(rect);
        if (idx >= 0)
        {
            wordList.RemoveAt(idx);
            wordDataList.RemoveAt(idx);
        }
        UpdateWordPositions();
        UpdateSentenceString();
    }

    public void UpdateWordPositions()
    {
        float currentX = startPosition.x;
        foreach (var rect in wordList)
        {
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(currentX, startPosition.y);
            currentX += rect.rect.width * rect.localScale.x + spacing;
        }
    }

    public void UpdateSentenceString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var w in wordDataList) sb.Append(w.Text + " ");
        currentSentenceAsString = sb.ToString().Trim();
    }

    public string GetSentenceAsString() => currentSentenceAsString;

    public void ClearSentence()
    {
        wordList.Clear();
        wordDataList.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        currentSentenceAsString = string.Empty;
    }

    // ---------------- Placeholder logic ----------------
    public void ShowPlaceholderAt(int index, RectTransform placeholder)
    {
        if (placeholderWord != null) RemovePlaceholder();
        placeholderWord = placeholder;
        index = Mathf.Clamp(index, 0, wordList.Count);
        wordList.Insert(index, placeholderWord);
        UpdateWordPositions();
    }

    public void RemovePlaceholder()
    {
        if (placeholderWord != null && wordList.Contains(placeholderWord))
        {
            wordList.Remove(placeholderWord);
            UpdateWordPositions();
            placeholderWord = null;
        }
    }

    public int GetInsertionIndex(float localX)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            float wordCenterX = wordList[i].anchoredPosition.x + wordList[i].rect.width * 0.5f;
            if (localX < wordCenterX) return i;
        }
        return wordList.Count;
    }
}


