using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;
using NUnit.Framework.Constraints;

public class SentenceBuilder : MonoBehaviour
{
    public WordBank wordBank;

    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>();
    public string currentSentenceAsString;
    public GameObject draggableWordPrefab;

    private RectTransform trailingPunctuation;

    private RectTransform placeholderWord;
    private RectTransform placeholderArticle;
    private RectTransform placeholderTrailingPunctuation;

    private void Start()
    {
        wordBank = FindAnyObjectByType<WordBank>();       
    }

    public void HandleHoveringWord(DraggableWord word, PointerEventData eventData)
    {
        // Convert pointer to localX
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        if (placeholderWord != null)
        {
            float delta = Mathf.Abs(localPoint.x - placeholderWord.anchoredPosition.x);
            if (delta < 2f) return; // tiny dead zone
        }        

        int insertIndex = GetInsertionIndex(localPoint.x);

        if (placeholderWord == null)
            placeholderWord = CreatePlaceHolder(word.GetComponent<RectTransform>());
        
        if (placeholderArticle != null)
        {
            ShowPlaceholderAt(insertIndex, placeholderArticle);
            ShowPlaceholderAt(insertIndex + 1, placeholderWord);
        }
        else
        {
            ShowPlaceholderAt(insertIndex, placeholderWord);
        }

        if (placeholderTrailingPunctuation == null)
        {
            TemporarilyRemoveTrailingPunctuation();
            placeholderTrailingPunctuation = CreatePlaceholderTrailingPunctuation();
        }
        UpdatePlaceholderTrailingPunctuation();
    }

    public void HandleWordDropped(DraggableWord word, PointerEventData eventData)
    {
        RemovePlaceholderTrailingPunctuation();

        if (placeholderWord != null || placeholderArticle != null)
            RemovePlaceholder();

        GameObject dropTarget = eventData.pointerEnter;
        RectTransform draggableWord = word.GetComponent<RectTransform>();

        if (dropTarget == null)
            return;

        if (dropTarget.CompareTag("SentencePanel"))
        {
            draggableWord.transform.SetParent(transform, false);
            // Convert pointer to localX
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            float pointerX = localPoint.x;

            int insertIndex = GetInsertionIndex(pointerX);

            InsertWordAt(draggableWord, insertIndex);
            word.isInSentencePanel = true;
        }
        else
        {
            WordBank wb = dropTarget != null
                ? dropTarget.GetComponent<WordBank>()
                : wordBank;

            if (wb != null)
            {
                // Convert screen position ? WordBank local space
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    wb.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint
                );

                // Reparent wihout changing world position
                draggableWord.transform.SetParent(wb.transform, false);

                // Place word exactly where it was dropped
                draggableWord.anchoredPosition = localPoint;

                // Ensure pivot consistency
                draggableWord.pivot = new Vector2(0.5f, 0.5f);
            }

            RemoveWord(draggableWord);
            word.isInSentencePanel = false;
        }
    }

    // ---------------- Sentence management ----------------

    public void InsertWordAt(RectTransform rect, int index)
    {
        SentenceWordEntry wordData = rect.GetComponent<DraggableWord>().sentenceWordEntry;      

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

        // Handle trailing punctuation
        RemovePlaceholderTrailingPunctuation();
        EnsureTrailingPunctuationExists();
        UpdateTrailingPunctuation();

        // Refresh positions and sentence string
        UpdateWordPositions();
        UpdateSentenceString();
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

        return articleWord.GetComponent<RectTransform>();
    }

    private void EnsureTrailingPunctuationExists()
    {
        if (!HasContentWords())
            return;

        if (trailingPunctuation != null)
            return;

        GameObject go = Instantiate(draggableWordPrefab, transform);
        trailingPunctuation = go.GetComponent<RectTransform>();

        var dw = go.GetComponent<DraggableWord>();
        dw.isDraggable = false;
        dw.isInSentencePanel = true;


        wordList.Add(trailingPunctuation);
    }

    private void TemporarilyRemoveTrailingPunctuation()
    {
        if (trailingPunctuation == null)
            return;

        wordList.Remove(trailingPunctuation);
        trailingPunctuation.gameObject.SetActive(false);
    }

    private void RestoreTrailingPunctuation()
    {
        if (trailingPunctuation == null)
            return;

        trailingPunctuation.gameObject.SetActive(true);

        if (!wordList.Contains(trailingPunctuation))
            wordList.Add(trailingPunctuation);
    }



    private void RemoveTrailingPunctuationIfEmpty()
    {
        if (HasContentWords())
            return;

        if (trailingPunctuation == null)
            return;

        wordList.Remove(trailingPunctuation);
        Destroy(trailingPunctuation.gameObject);
        trailingPunctuation = null;       
    }

    private void UpdateTrailingPunctuation()
    {
        if (!HasContentWords())
        {
            RemoveTrailingPunctuationIfEmpty();
            return;
        }

        // Make sure trailing punctuation exists
        if (trailingPunctuation == null)
            EnsureTrailingPunctuationExists();

        // Move it to the end of the list
        if (wordList.Contains(trailingPunctuation))
        {
            wordList.Remove(trailingPunctuation);
        }
        wordList.Add(trailingPunctuation);

        // Determine Punctuation
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

        // Set Punctuation Data
        var draggableScript = trailingPunctuation.GetComponent<DraggableWord>();
        draggableScript.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(punctuation);
        draggableScript.sentenceWordEntry.Surface = punctuation;

        // Update TMP mesh immediately
        TMP_Text text = trailingPunctuation.GetComponent<TMP_Text>();
        text.raycastTarget = false;
        text.text = punctuation;
        LayoutRebuilder.ForceRebuildLayoutImmediate(trailingPunctuation.GetComponent<RectTransform>());
    }

    private bool HasContentWords()
    {
        foreach (var rect in wordList)
        {
            var dw = rect.GetComponent<DraggableWord>();
            if (dw == null) continue;

            var entry = dw.sentenceWordEntry;

            if (entry == null || entry.Word == null)
                continue;

            if (!entry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
                return true;
        }
        return false;
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

        TMP_Text text = rect.GetComponent<TMP_Text>();

        EnsureTrailingPunctuationExists();
        UpdateTrailingPunctuation();
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

    public int GetInsertionIndex(float localX, RectTransform ignoreRect = null)
    {
        int rawIndex = wordList.Count; // default to end

        for (int i = 0; i < wordList.Count; i++)
        {
            RectTransform rect = wordList[i];
            if (rect == null)
                continue;

            var dw = rect.GetComponent<DraggableWord>();
            if (dw == null || dw.sentenceWordEntry?.Word == null || rect == trailingPunctuation || rect == placeholderTrailingPunctuation)
                continue;

            // Include placeholder in the position calculation, but treat it as zero-width if you want
            float leftEdge = rect.anchoredPosition.x;
            float rightEdge = rect.anchoredPosition.x + rect.rect.width * rect.localScale.x;

            float mid = (leftEdge + rightEdge) / 2f;

            // margin proportional to word width (e.g., 10% of width)
            float margin = rect.rect.width * rect.localScale.x * 0.1f;

            // Insert before this word if pointer is left of mid + margin
            if (localX < mid + margin)
            {
                rawIndex = i;
                break;
            }               
        }

        // Ensure the insertion respects article-noun boundaries (and any future rules)
        return NormalizeInsertionIndex(rawIndex);
    }

    // Pass insertion index here to make sure placeholders and drops follow basic grammar
    private int NormalizeInsertionIndex(int index)
    {
        if (index <= 0 || index >= wordList.Count)
            return index;

        RectTransform next = wordList[index];
        var nextDW = next?.GetComponent<DraggableWord>();

        if (nextDW != null &&
            nextDW.sentenceWordEntry != null &&
            nextDW.sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Noun) &&
            nextDW.sentenceWordEntry.hasArticle &&
            wordList[index - 1] == nextDW.sentenceWordEntry.article)
        {
            // Redirect insertion to BEFORE the article
            return index - 1;
        }

        return index;
    }

    // ---------------- Placeholder Methods ----------------
        
    public RectTransform CreatePlaceHolder(RectTransform originalRect)
    {
        if (originalRect == null) return null;

        var originalScript = originalRect.GetComponent<DraggableWord>();

        // Instantiate a new placeholder from the prefab
        placeholderWord = Instantiate(draggableWordPrefab, transform).GetComponent<RectTransform>();
        placeholderWord.name = "PlaceholderWord";

        // --- Only modify the placeholder's components ---
        CanvasGroup cg = placeholderWord.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = placeholderWord.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;   // placeholder doesn't block raycasts

        TMP_Text tmp = placeholderWord.GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.raycastTarget = false;

        Image img = placeholderWord.GetComponent<Image>();
        if (img != null)
            img.raycastTarget = false;

        // Copy word data from original
        var placeholderWordDraggable = placeholderWord.GetComponent<DraggableWord>();
        placeholderWordDraggable.sentenceWordEntry = new SentenceWordEntry
        {
            Surface = originalScript.sentenceWordEntry.Surface,
            Word = originalScript.sentenceWordEntry.Word,
            hasArticle = originalScript.sentenceWordEntry.hasArticle,
            article = originalScript.sentenceWordEntry.article
        };

        placeholderWordDraggable.isPlaceholder = true;
        placeholderWordDraggable.isDraggable = false;
        placeholderWordDraggable.isInSentencePanel = true;

        // Article logic for singular nouns
        SentenceWordEntry swe = originalScript.sentenceWordEntry;
        if (swe.Word.HasPartOfSpeech(PartsOfSpeech.Noun) &&
            swe.Word.IsSingular(swe.Surface))
        {
            placeholderArticle = Instantiate(draggableWordPrefab, transform).GetComponent<RectTransform>();
            placeholderArticle.name = "PlaceholderArticle";

            CanvasGroup articleCG = placeholderArticle.GetComponent<CanvasGroup>();
            if (articleCG == null)            
                articleCG = placeholderArticle.gameObject.AddComponent<CanvasGroup>();
            articleCG.blocksRaycasts = false; // placeholder doesn't block raycasts

            TMP_Text articleText = placeholderArticle.GetComponent<TMP_Text>();
            if (articleText != null)
                articleText.raycastTarget = false;

            Image articleIMG = placeholderArticle.GetComponent<Image>();
            if (articleIMG != null)
                articleIMG.raycastTarget = false;

            // Determine article
            string firstLetter = swe.Surface.ToLower();
            bool startswithVowel = "aeiou".Contains(firstLetter[0]);
            string article = startswithVowel ? "an" : "a";

            // Set article word data
            var draggableScript = placeholderArticle.GetComponent<DraggableWord>();
            draggableScript.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(article);
            draggableScript.sentenceWordEntry.Surface = article;
            draggableScript.isDraggable = false;
            draggableScript.isInSentencePanel = true;
                        
            // Set visual text
            articleText.text = article;
            articleText.color = Color.gray;

            // Update TMP mesh immediately
            articleText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(placeholderArticle.GetComponent<RectTransform>());
        }

        // Set visual text
        TMP_Text text = placeholderWord.GetComponent<TMP_Text>();
        text.color = Color.gray;

        text.text = placeholderWordDraggable.sentenceWordEntry.Surface;
        text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(placeholderWord);

        Debug.Log("Placeholder has been created.");
        Debug.Log("Placeholder name is: " + placeholderWord.name);
        Debug.Log("Placeholder word is: " + placeholderWordDraggable.sentenceWordEntry.Surface);

        return placeholderWord;
    }
    
    public void ShowPlaceholderAt(int index, RectTransform placeholder)
    {
        if (placeholder == null)
            return;

        // If placeholder is already in the list, just move it
        int existingIndex = wordList.IndexOf(placeholder);
        if (existingIndex != -1)
        {
            wordList.RemoveAt(existingIndex);
        }

        index = Mathf.Clamp(index, 0, wordList.Count);

        // Called again to ensure placeholder doesn't preview in places it shouldn't
        index = NormalizeInsertionIndex(index);

        if (index == wordList.Count && trailingPunctuation != null)
        {
            index = wordList.Count - 1;
        }

        wordList.Insert(index, placeholder);

        UpdateWordPositions();
    }


    public void RemovePlaceholder()
    {
        if (placeholderWord != null && wordList.Contains(placeholderWord))
        {
            wordList.Remove(placeholderWord);
            Destroy(placeholderWord.gameObject);
            UpdateWordPositions();
            placeholderWord = null;
        }

        if (placeholderArticle != null && wordList.Contains(placeholderArticle))
        {
            wordList.Remove(placeholderArticle);
            Destroy(placeholderArticle.gameObject);
            UpdateWordPositions();
            placeholderArticle = null;
        }
    }

    // ---------------- Placeholder Trailing Punctuation ----------------    

    // NOTE: Placeholder punctuation is intentionally separate from real punctuation.
    // Do not unify unless preview + commit lifecycles truly converge.

    // Creates the placeholder punctuation object
    public RectTransform CreatePlaceholderTrailingPunctuation()
    {
        if (!HasContentWords() && placeholderWord == null)
            return null;

        RectTransform rt = Instantiate(draggableWordPrefab, transform).GetComponent<RectTransform>();
        rt.name = "PlaceholderTrailingPunctuation";

        CanvasGroup cg = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        TMP_Text tmp = rt.GetComponent<TMP_Text>();
        tmp.raycastTarget = false;
        tmp.color = Color.gray;

        Image img = rt.GetComponent<Image>();
        if (img != null)
            img.raycastTarget = false;

        var dw = rt.GetComponent<DraggableWord>();
        dw.isPlaceholder = true;
        dw.isDraggable = false;
        dw.isInSentencePanel = true;

        return rt;
    }

    private string DetermineTrailingPunctuationForPreview()
    {
        // Determine the preview "first word"
        RectTransform first = null;

        if (wordList.Count > 0)
            first = wordList[0];
        else if (placeholderWord != null)
            first = placeholderWord;

        if (first == null)
            return ".";

        var entry = first.GetComponent<DraggableWord>()?.sentenceWordEntry;
        if (entry?.Word == null)
            return ".";

        return entry.Word.PartOfSpeech == PartsOfSpeech.Interrogative ? "?" : ".";
    }

    // Inserts or updates the trailing punctuation at the end of the sentence
    public void UpdatePlaceholderTrailingPunctuation()
    {
        if (placeholderTrailingPunctuation == null)
        {
            placeholderTrailingPunctuation = CreatePlaceholderTrailingPunctuation();
            if (placeholderTrailingPunctuation == null) return;
        }

        if (wordList.Contains(placeholderTrailingPunctuation))
            wordList.Remove(placeholderTrailingPunctuation);

        // Always recompute punctuation based on preview state
        string punctuation = DetermineTrailingPunctuationForPreview();

        var draggable = placeholderTrailingPunctuation.GetComponent<DraggableWord>();
        draggable.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(punctuation);
        draggable.sentenceWordEntry.Surface = punctuation;

        TMP_Text text = placeholderTrailingPunctuation.GetComponent<TMP_Text>();
        text.text = punctuation;
        text.ForceMeshUpdate();

        wordList.Add(placeholderTrailingPunctuation);
        UpdateWordPositions();
    }


    // Removes the placeholder trailing punctuation
    public void RemovePlaceholderTrailingPunctuation()
    {

        if (placeholderTrailingPunctuation != null && wordList.Contains(placeholderTrailingPunctuation))
        {
            wordList.Remove(placeholderTrailingPunctuation);
            Destroy(placeholderTrailingPunctuation.gameObject);
            placeholderTrailingPunctuation = null;
            UpdateWordPositions();
        }
        
        RestoreTrailingPunctuation();
        UpdateWordPositions();
    }
}



