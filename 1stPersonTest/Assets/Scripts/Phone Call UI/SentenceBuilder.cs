using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class SentenceBuilder : MonoBehaviour 
{
    public WordBank wordBank;

    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>(); // sentence word gameobjects    
    public List<SentenceWordEntry> committedModel = new List<SentenceWordEntry>();
    public List<SentenceWordEntry> storedWordList = new List<SentenceWordEntry>(); // for wordbank repopulation
    public string currentSentenceAsString;
    public GameObject draggableWordPrefab;

    private int currentPreviewIndex = -1;

    private void Start()
    {
        wordBank = FindAnyObjectByType<WordBank>();
    }

    public void HandleHoveringWord(DraggableWord word, PointerEventData eventData)
    {
        currentPreviewIndex = -1;
        var previewModel = new List<SentenceWordEntry>(committedModel);

        // Convert pointer to localX
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Calculate insertion index
        int insertIndex = GetInsertionIndex(localPoint.x);

        // Clamp to valid range
        insertIndex = Mathf.Clamp(insertIndex, 0, wordList.Count);
        Debug.Log("insert index is: " + insertIndex);

        // Prevent rebuild spam
        if (insertIndex == currentPreviewIndex)
            return;

        currentPreviewIndex = insertIndex;

        // Create a preview entry
        SentenceWordEntry previewEntry = new SentenceWordEntry
        {
            Word = word.sentenceWordEntry.Word,
            Surface = word.sentenceWordEntry.Surface
        };        

        if (CanInsertAt(previewModel, insertIndex, previewEntry))
        {
            // Insert preview into model
            previewModel.Insert(insertIndex, previewEntry);

            // Normalize the previewModel
            previewModel = Normalize(previewModel);

            // Apply normalization/UI updates        
            ApplyNormalizationResults(previewModel, true);
            Debug.Log("preview generated");
        }
    }

    public void HandleWordDropped(DraggableWord word, PointerEventData eventData)
    {
        if (word == null)
            return;

        RectTransform draggableWord = word.GetComponent<RectTransform>();
        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.CompareTag("SentencePanel"))
        {
            ClearSentence();

            // Create a commit entry
            SentenceWordEntry commitEntry = new SentenceWordEntry
            {
                Word = word.sentenceWordEntry.Word,
                Surface = word.sentenceWordEntry.Surface
            };

            if (CanInsertAt(committedModel, currentPreviewIndex, commitEntry))
            {
                Destroy(word.gameObject);
                committedModel.Insert(currentPreviewIndex, commitEntry);

                committedModel = Normalize(committedModel);
                
                ApplyNormalizationResults(committedModel, false);
                Debug.Log("HANDLE WORD DROPPED EXECUTING");
            }            
            else
            {
                ReturnWordToBank(draggableWord, word);
                word.isInSentencePanel = false;

            }
        }
        else
        {
            ReturnWordToBank(draggableWord, word);
            word.isInSentencePanel = false;
        }

        currentPreviewIndex = -1;
    }

    private void ReturnWordToBank(RectTransform draggableWord, DraggableWord word)
    {
        WordBank wb = wordBank;

        if (wb == null)
            return;

        draggableWord.transform.SetParent(wb.transform, false);
        draggableWord.anchoredPosition = Vector2.zero;
        draggableWord.pivot = new Vector2(0.5f, 0.5f);
    }


    // ---------------- Sentence management ----------------    

    private bool CanInsertAt(List<SentenceWordEntry> model, int insertIndex, SentenceWordEntry entry)
    {
        if (model == null)
            return false;

        if (insertIndex < 0 || insertIndex > model.Count)
            return false;

        if (model.Count == 0)
            return true;

        bool isArticle = entry.Word.HasPartOfSpeech(PartsOfSpeech.Article);

        if (!isArticle)
        {
            if (insertIndex > 0)
            {
                var left = model[insertIndex - 1];

                if (left.Word.HasPartOfSpeech(PartsOfSpeech.Article))
                {
                    if (insertIndex < model.Count &&
                        model[insertIndex] == left.owningNoun)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void CommitModelChange()
    {
        committedModel = Normalize(committedModel);
        ApplyNormalizationResults(committedModel, false);
    }

    public void ClearPreviewOnly()
    {
        bool hasPreview = wordList.Any(w => w.GetComponent<DraggableWord>().isPreview);

        if (!hasPreview)
            return;
        
        ApplyNormalizationResults(committedModel, false);

        currentPreviewIndex = -1;
    }

    // Helper class for article insertion.
    private class PendingArticleInsertion
    {
        public SentenceWordEntry nounData;
        public int nounIndex;
    }

    private List<SentenceWordEntry> Normalize(List<SentenceWordEntry> rawModel)
    {
        var workingModel = new List<SentenceWordEntry>(rawModel);
        
        // Collect loose articles for removal.
        List<SentenceWordEntry> articleEntriesToRemove = new List<SentenceWordEntry>();

        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry wordData = workingModel[i];

            if (!wordData.Word.HasPartOfSpeech(PartsOfSpeech.Article))
                continue;

            bool hasNextWord = i + 1 < workingModel.Count;

            if (!hasNextWord)
            {
                articleEntriesToRemove.Add(workingModel[i]);
            }
            else
            {
                
                if (wordData.owningNoun == null)
                {
                    articleEntriesToRemove.Add(workingModel[i]);
                }
            }
        }

        Debug.Log("articles to remove: " + articleEntriesToRemove.Count);
        RemoveArticles(workingModel, articleEntriesToRemove);

        // Collect nouns that need articles.
        List<PendingArticleInsertion> articleEntriesToInsert = new List<PendingArticleInsertion>(); 

        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry wordData = workingModel[i];

            if (!wordData.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                continue;
            if (!wordData.Word.IsSingular(wordData.Surface))
                continue;
            if (wordData.article != null)
                continue;
            
            articleEntriesToInsert.Add(new PendingArticleInsertion
            {
                nounIndex = i,
                nounData = wordData
            });            
        }

        Debug.Log("missing articles: " + articleEntriesToInsert.Count);
        InsertArticles(workingModel,articleEntriesToInsert);

        NormalizeTrailingPunctuation(workingModel);

        return workingModel;
    }

    private void ApplyNormalizationResults(List<SentenceWordEntry> workingModel, bool isPreviewMode = false)
    {
        ClearSentence();

        foreach (var entry in workingModel)
        {
            // Instantiate prefabs for workingModel entries
            RectTransform word = Instantiate(draggableWordPrefab, transform).GetComponent<RectTransform>();

            // Set word data to model data
            var draggable = word.GetComponent<DraggableWord>();
            draggable.isInSentencePanel = true;
            draggable.sentenceWordEntry = entry;
            var text = word.GetComponent<TMP_Text>();

            if (isPreviewMode)
            {
                draggable.isPreview = true;
                text.color = Color.gray;
                // Make sure preview does not block pointer events
                CanvasGroup cg = word.GetComponent<CanvasGroup>();
                if (cg == null) cg = word.gameObject.AddComponent<CanvasGroup>();
                cg.blocksRaycasts = false;

                Debug.Log("normalization applied in preview");
                
            }
            else
            {
                draggable.isPreview = false;
                Debug.Log("normalization applied NOT in preview");
            }

            TMP_Text wordText = word.GetComponent<TMP_Text>();
            wordText.text = entry.Surface;

            wordText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(word.GetComponent<RectTransform>());

            // Add instantiated prefab to wordList
            wordList.Add(word);              
        }

        UpdateWordPositions();
    }

    private void RemoveArticles(List<SentenceWordEntry> workingModel, List<SentenceWordEntry> articlesToRemove)
    {
        foreach (SentenceWordEntry articleEntry in articlesToRemove)
        {
            workingModel.Remove(articleEntry);

            if (articleEntry.owningNoun != null)
            {
                articleEntry.owningNoun.article = null;
                articleEntry.owningNoun = null;
            }
        }

    }

    private void InsertArticles(List<SentenceWordEntry> workingModel, List<PendingArticleInsertion> articlesToInsert)
    {

        foreach (var pending in articlesToInsert)
        {
            int nounIndex = pending.nounIndex;

            if (nounIndex < 0)
                continue; // noun no longer exists - safety check

            SentenceWordEntry articleEntry =
                CreateArticleForNoun(pending.nounData);

            workingModel.Insert(nounIndex, articleEntry);
        }
    }

    public void InsertWordEntryAt(SentenceWordEntry entry, int index)
    {
        committedModel.Insert(index, entry);
        storedWordList.Add(entry); // Add to backup list

        CommitModelChange();
    }

    
    public void RemoveDraggableFromSentence(SentenceWordEntry draggableEntry)
    {
        int idx = committedModel.IndexOf(draggableEntry);
        if (idx >= 0)
            committedModel.RemoveAt(idx);


        // sever grammatical connections
        if (draggableEntry.article != null)
        {
            var articleEntry = draggableEntry.article;

            articleEntry.owningNoun = null;
            draggableEntry.article = null;

            // Remove article from sentenceModel if it exists
            committedModel.Remove(articleEntry);
        }

        committedModel.Remove(draggableEntry);

    }

    private SentenceWordEntry CreateArticleForNoun(SentenceWordEntry wordData)
    {
        SentenceWordEntry articleEntry = new SentenceWordEntry();

        // Determine article
        string firstLetter = wordData.Surface.ToLower();
        bool startsWithVowel = "aeiou".Contains(firstLetter[0]);
        string article = startsWithVowel ? "an" : "a";

        // Set word data
        articleEntry.Word = WordDataBase.Instance.GetWord(article);
        articleEntry.Surface = article;
        articleEntry.owningNoun = wordData;
        wordData.article = articleEntry;
       

        Debug.Log("inserted article: " + article);
        Debug.Log("article's owning noun: " + wordData.Surface);

        return articleEntry;
    }

    private void NormalizeTrailingPunctuation(List<SentenceWordEntry> rawModel)
    {
        // Remove any existing punctuation entries
        rawModel.RemoveAll(entry =>
        entry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation));

        // if no content words, do nothing
        if (!rawModel.Any(entry =>
            !entry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation)))
            return;

        var firstWord = rawModel[0];

        string punctuation =
            firstWord.Word.PartOfSpeech == PartsOfSpeech.Interrogative
            ? "?"
            : ".";

        var punctuationEntry = new SentenceWordEntry
        {
            Word = WordDataBase.Instance.GetWord(punctuation),
            Surface = punctuation
        };

        rawModel.Add(punctuationEntry);
    }
    

    private void UpdateWordPositions()
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

    private void UpdateSentenceString()
    {
        List<SentenceWordEntry> wordDataList = new List<SentenceWordEntry>();

        foreach (RectTransform rect in wordList)
        {
            wordDataList.Add(rect.GetComponent<DraggableWord>().
                sentenceWordEntry);
        }


        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var w in wordDataList)
        {
            if (w.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
                sb.Append(w.Surface);
            else
                sb.Append(w.Surface + " ");
        }

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

    public void ClearStoredWords()
    {
        storedWordList.Clear();        
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

    private int GetInsertionIndex(float draggableMidX, float margin = 10f)
    {
        var committedRects = GetCommittedRects();
        int modelIndex = committedRects.Count; // default to end        

        for (int i = 0; i < committedRects.Count; i++)
        {            
            var rect = committedRects[i];           

            float wordLeft = rect.localPosition.x - margin;
            float wordRight = rect.localPosition.x + rect.rect.width + margin;
            float wordMidX = rect.localPosition.x + rect.rect.width / 2f;

            // If draggableMidX is within the "margin zone" to the left of the midpoint, insert here
            if (draggableMidX < wordMidX + margin)
                return i;
        }

        return modelIndex;
    }

    private List<RectTransform> GetCommittedRects()
    {
        return wordList
            .Where(w => !w.GetComponent<DraggableWord>().isPreview)
            .Select(w => w.GetComponent<RectTransform>())
            .ToList();
    }
}



