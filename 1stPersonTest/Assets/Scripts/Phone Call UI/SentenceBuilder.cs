using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SentenceBuilder : MonoBehaviour
{
    public WordBank wordBank;

    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>();
    public List<SentenceWordEntry> wordDataList = new List<SentenceWordEntry>(); // Track semantic words
    public string currentSentenceAsString;
    public GameObject draggableWordPrefab;

    private RectTransform placeholderWord;

    private void Start()
    {
        wordBank = FindAnyObjectByType<WordBank>();
    }

    // ---------------- Sentence management ----------------

    private void CheckForPunctuation(List<SentenceWordEntry> wordDataList)
    {
        if (wordDataList == null || wordDataList.Count == 0)
            return;

        int lastIndex = wordDataList.Count - 1;
        TMP_Text lastWord = wordList[lastIndex].GetComponent<TMP_Text>();

            for (int i = 0; i < wordDataList.Count; i++)
            {
                if (wordDataList[i].hasPunctuation)
                {
                    RemovePunctuation(wordList[i], wordDataList[i]);
                }

            }

        if (wordDataList[0].Word.PartOfSpeech == PartsOfSpeech.Interrogative)
        {
            wordDataList[lastIndex].Surface += "?";
        }
        else
        {
            wordDataList[lastIndex].Surface += ".";
        }

        lastWord.text = wordDataList[lastIndex].Surface;
        lastWord.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordList[lastIndex]);
        wordDataList[lastIndex].hasPunctuation = true;
    }    

    public void RemovePunctuation(RectTransform rect, SentenceWordEntry wordData)
    {
        TMP_Text text = rect.GetComponent<TMP_Text>();
        if (wordData.hasPunctuation == true)
        {
            wordData.Surface = wordData.Surface.Remove(wordData.Surface.Length - 1);
            text.text = wordData.Surface;
        }
        text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        wordData.hasPunctuation = false;
    }

    public void InsertWordAt(RectTransform rect, SentenceWordEntry wordData, int index)
    {
        if (wordList.Contains(rect))
        {
            wordList.Remove(rect);
            wordDataList.Remove(wordData);
        }

        index = Mathf.Clamp(index, 0, wordList.Count);
        wordList.Insert(index, rect);
        wordDataList.Insert(index, wordData);
        
        if (wordData.Word.PartOfSpeech == PartsOfSpeech.Noun &&
            wordData.Word.IsSingular(wordData.Surface))
        {
            InsertArticle(rect, wordData);
        }

        CheckForPunctuation(wordDataList);
        
        UpdateWordPositions();
        UpdateSentenceString();
    }

    public void InsertArticle(RectTransform rect, SentenceWordEntry wordData)
    {
        if (wordData.hasArticle)
        {
            return;
        }
        string firstLetter = wordData.Surface.ToLower();
        bool startsWithVowel = "aeiou".Contains(firstLetter[0]);
        string article = startsWithVowel ? "an" : "a";

        TMP_Text tmp = rect.GetComponent<TMP_Text>();
       
        tmp.text = article + " " + wordData.Surface;
        wordData.hasArticle = true;

        tmp.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    public void RemoveArticle(RectTransform rect, SentenceWordEntry wordData)
    {
        string[] words = wordData.Surface.Split(' ');
        string noun = words[words.Length - 1];

        TMP_Text tmp = rect.GetComponent<TMP_Text>();       
        tmp.text = noun;
        wordData.hasArticle = false;

        tmp.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }  

    public void RemoveWord(RectTransform rect)
    {
        int idx = wordList.IndexOf(rect);
        if (idx >= 0)
        {
            wordList.RemoveAt(idx);
            wordDataList.RemoveAt(idx);
        }

        CheckForPunctuation(wordDataList);

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
        foreach (var w in wordDataList) sb.Append(w.Surface + " ");
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

    public void TestSingularOrPlural(SentenceWordEntry word)
    {
        var placeholder = word; 
        if (placeholder != null && placeholder.Word.PartOfSpeech == PartsOfSpeech.Noun)
        {
            if (placeholder.Word.IsSingular(placeholder.Surface))
            {
                Debug.Log("This word is singular");
            }
            else if (placeholder.Word.IsPlural(placeholder.Surface))
            {
                Debug.Log("This word is plural");
            }
        }
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


