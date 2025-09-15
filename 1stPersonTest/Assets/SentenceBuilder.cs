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

    private void Update()
    {
        currentSentenceAsString = GetSentenceAsString();
        if (currentSentenceAsString != null)
        {
            Debug.Log(currentSentenceAsString);
        }
    }

    public string GetSentenceAsString()
    {
        StringBuilder result = new StringBuilder();

        foreach (RectTransform rect in wordList)
        {
            TMP_Text tmpText = rect.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                result.AppendLine(tmpText.text);
            }
        }
        string playerInputFormatted = Regex.Replace(result.ToString(), @"\s+$", "");
        return Regex.Replace(playerInputFormatted.ToString(), @"\s+", " ");
    }
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

    public void ClearSentence()
    {
        wordList.Clear();
        for (int i = this.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(this.transform.GetChild(i).gameObject);
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


