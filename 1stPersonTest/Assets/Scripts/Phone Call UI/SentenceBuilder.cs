using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class SentenceBuilder : MonoBehaviour
{
    public WordBank wordBank;

    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>();
    //public List<SentenceWordEntry> wordDataList = new List<SentenceWordEntry>();
    public string currentSentenceAsString;
    public GameObject draggableWordPrefab;

    private void Start()
    {
        wordBank = FindAnyObjectByType<WordBank>();
    }

    // ---------------- Sentence management ----------------

    private void InsertTrailingPunctuation()
    {
        if (wordList.Count == 0) return; // if no words in sentencepanel        

        // Create punctuation object
        GameObject punctuationWord = Instantiate(draggableWordPrefab, transform);
        TMP_Text text = punctuationWord.GetComponent<TMP_Text>();

        // Determine punctuation character
        string punctuation;
        var firstWord = wordList[0].GetComponent<DraggableWord>().sentenceWordEntry;                        
        switch (firstWord.Word.PartOfSpeech)
        {
            case PartsOfSpeech.Interrogative:
                punctuation = "?";
                break;
            default:
                punctuation = ".";
                break;
        }

        // Set punctuation data
        var draggable = punctuationWord.GetComponent<DraggableWord>();
        draggable.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(punctuation);
        draggable.sentenceWordEntry.Surface = punctuation;
        draggable.isInSentencePanel = true;
        draggable.isDraggable = false;

        // Update TMP mesh immediately
        text.text = punctuation;
        text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(punctuationWord.GetComponent<RectTransform>());
        
        wordList.Add(punctuationWord.GetComponent<RectTransform>());
    }
    private RectTransform InsertArticleAt(RectTransform rect, int index, SentenceWordEntry wordData)
    {
        // Create article object
        GameObject articleWord = Instantiate(draggableWordPrefab, transform);
        TMP_Text text = articleWord.GetComponent<TMP_Text>();

        // Determine article
        string firstLetter = wordData.Surface.ToLower();
        bool startsWithVowel = "aeiou".Contains(firstLetter[0]);
        string article = startsWithVowel ? "an" : "a";

        // Set word data
        var draggable = articleWord.GetComponent<DraggableWord>();
        draggable.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(article);
        draggable.sentenceWordEntry.Surface = article;
        draggable.isInSentencePanel = true;
        draggable.isDraggable = false;
        
        // Update TMP mesh immediately
        text.text = article;
        text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(articleWord.GetComponent<RectTransform>());

        //InsertWordAt(articleWord.GetComponent<RectTransform>(), index);
        return articleWord.GetComponent<RectTransform>();
    }

    public void InsertWordAt(RectTransform rect, int index)
    {
        SentenceWordEntry wordData = rect.GetComponent<DraggableWord>().sentenceWordEntry;

        // Remove trailing punctuation
        if (wordList.Count > 0)
        {
            var lastWord = wordList[wordList.Count - 1].GetComponent<DraggableWord>();
            if (lastWord.sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
            {
                RectTransform punctRect = wordList[wordList.Count - 1];
                wordList.RemoveAt(wordList.Count - 1);
                Destroy(punctRect.gameObject);
            }
        }

        // Remove the word if it already exists in the list
        if (wordList.Contains(rect))
        {
            wordList.Remove(rect);
        }

        // Clamp index first to ensure it's within bounds
        index = Mathf.Clamp(index, 0, wordList.Count);

        // Insert article before singular nouns
        if (wordData.Word.HasPartOfSpeech(PartsOfSpeech.Noun) && wordData.Word.IsSingular(wordData.Surface))
        {
            RectTransform articleRect = InsertArticleAt(rect, index, wordData);
            wordList.Insert(index, articleRect);
            wordData.article = articleRect;
            wordData.hasArticle = true;

            index++; // make sure main word comes after article
            index = Mathf.Clamp(index, 0, wordList.Count); // clamp again
        }

        // Insert the main word
        wordList.Insert(index, rect);

        // Add punctuation at the end
        InsertTrailingPunctuation();

        // Refresh positions and sentence string
        UpdateWordPositions();
        UpdateSentenceString();
    }



    public void RemoveWord(RectTransform rect)
    {
        int idx = wordList.IndexOf(rect);

        if (idx >= 0)
        {
            wordList.RemoveAt(idx);
        }

        var draggable = rect.GetComponent<DraggableWord>();
        var wordData = draggable.sentenceWordEntry;

        if (wordData.hasArticle && wordData.article != null) // remove article
        {
            RectTransform articleRect = wordData.article;
            wordList.Remove(articleRect); // remove article from word List
            Destroy(articleRect.gameObject); // destroy the article
            wordData.hasArticle = false;
            wordData.article = null;
        }

        if (wordList.Count == 1 && wordList[0].GetComponent<DraggableWord>().
            sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
        {
            Destroy(wordList[0].gameObject);
            wordList.Clear();
            return;
        }

        UpdateWordPositions();
        UpdateSentenceString();
    }

    public void UpdateWordPositions()
    {
        if (wordList.Count == 0) return;

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
        List<SentenceWordEntry> wordDataList = new List<SentenceWordEntry>();

        foreach (RectTransform rect in wordList)
        {
            wordDataList.Add(rect.GetComponent<DraggableWord>().
                sentenceWordEntry);
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var w in wordDataList)
            sb.Append(w.Surface + " ");

        currentSentenceAsString = sb.ToString().Trim();
    }

    public string GetSentenceAsString() => currentSentenceAsString;

    public void ClearSentence()
    {
        wordList.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        currentSentenceAsString = string.Empty;
    }

    public void TestSingularOrPlural(SentenceWordEntry word)
    {
        if (word == null || word.Word == null) return;

        if (word.Word.PartOfSpeech == PartsOfSpeech.Noun)
        {
            if (word.Word.IsSingular(word.Surface))
                Debug.Log("This word is singular");
            else if (word.Word.IsPlural(word.Surface))
                Debug.Log("This word is plural");
        }
    }

    public int GetInsertionIndex(float localX)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            float centerX = wordList[i].anchoredPosition.x +
                            wordList[i].rect.width * 0.5f;

            if (localX < centerX)
                return i;
        }

        return wordList.Count;
    }

    /*
    public RectTransform CreatePlaceHolder(RectTransform rect)
    {
        var originalDraggable = rect.GetComponent<DraggableWord>(); // oriignal word

        // Create placeholder
        GameObject placeholder = Instantiate(draggableWordPrefab, transform);
        placeholder.name = "placeholder";

        // Set placeholder word data
        var draggable = placeholder.GetComponent<DraggableWord>();
        draggable.sentenceWordEntry.Word =
            WordDataBase.Instance.GetWord(originalDraggable.GetComponent<TMP_Text>().text);
        draggable.sentenceWordEntry.Surface = originalDraggable.sentenceWordEntry.Surface;        
        draggable.isDraggable = false;
        draggable.isInSentencePanel = true;

        return placeholder.GetComponent<RectTransform>();
    }
    */
}



