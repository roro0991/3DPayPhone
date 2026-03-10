using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceBuilder : MonoBehaviour
{
    public WordBank wordBank;

    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;

    public List<RectTransform> wordList = new List<RectTransform>(); // sentence word gameobjects
    public List<SentenceWordEntry> sentenceModel = new List<SentenceWordEntry>(); // sentence word data
    public List<SentenceWordEntry> storedWordList = new List<SentenceWordEntry>(); // for wordbank repopulation
    public string currentSentenceAsString;
    public GameObject draggableWordPrefab;

    private bool sentenceHasPreviews;
    private bool sentenceMutated;
    private int currentPreviewIndex = -1;

    private Dictionary<SentenceWordEntry, RectTransform> ModelRects = new Dictionary<SentenceWordEntry, RectTransform>();

    private void Start()
    {
        wordBank = FindAnyObjectByType<WordBank>();
    }

    public void HandleHoveringWord(DraggableWord word, PointerEventData eventData)
    {
        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.CompareTag("SentencePanel"))
        {
            // Edge case: dragging the very first word into an empty sentence panel.
            // No need to process hover because there's no insertion point.
            if (sentenceModel.Count == 1 && sentenceModel[0].isPreview)
            {
                Debug.Log("***HOVER LOGIC CANCELLED!***");
                return;
            }

            //Remove any existing preview words
            sentenceModel.RemoveAll(entry => entry.isPreview);

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
            insertIndex = Mathf.Clamp(insertIndex, 0, sentenceModel.Count);

            // Prevent rebuild spam
            if (insertIndex == currentPreviewIndex)
            {
                Debug.Log("***REBUILD SPAM PREVENTED***");
                return;
            }            

            currentPreviewIndex = insertIndex;            
        
            // Create a preview entry
            SentenceWordEntry previewEntry = new SentenceWordEntry
            {
                Word = word.sentenceWordEntry.Word,
                Surface = word.sentenceWordEntry.Surface,
                isPreview = true
            };

            if (CanInsertAt(sentenceModel, insertIndex, previewEntry))
            {
                // Insert preview into model
                sentenceModel.Insert(insertIndex, previewEntry);
                ApplyNormalizedPreview(sentenceModel, true);
                sentenceHasPreviews = true;
                Debug.Log("preview generated");                
            }
            else
            {
                Debug.Log("***CAN INSERT CHECK FAILED***");
            }
        }
        else
        {            
            if (sentenceHasPreviews)
            {
                ClearPreview();
                ApplyNormalizedPreview(sentenceModel, false);
            }            
        }
    }

    // Helper method to normalize & apply UI results
    private void ApplyNormalizedPreview(List<SentenceWordEntry> model, bool isPreview)
    {
        List<SentenceWordEntry> normalizedModel = Normalize(model);
        ApplyNormalizationResults(normalizedModel, isPreview);
    }

    public void HandleWordDropped(DraggableWord word, PointerEventData eventData)
    {
        if (word == null)
            return;        

        RectTransform draggableWord = word.GetComponent<RectTransform>();
        var entryData = word.sentenceWordEntry;

        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.CompareTag("SentencePanel"))
        {
            draggableWord.transform.SetParent(transform, false);            
            
            // ? CRITICAL: Validate drop commit
            if (!CanInsertAt(sentenceModel, currentPreviewIndex, entryData))
            {
                Debug.Log("Drop rejected by grammar validation");

                ReturnWordToBank(draggableWord, word, false, eventData);
                ClearPreview();
                return;
            }

            ModelRects[entryData] = draggableWord;

            InsertWordEntryAt(entryData, currentPreviewIndex);
            ClearPreview();

            word.isInSentencePanel = true;

            sentenceMutated = true;
        }
        else if (dropTarget != null && dropTarget.CompareTag("WordBankPanel"))
        {
            ReturnWordToBank(draggableWord, word, true, eventData);
            word.isInSentencePanel = false;
        }
        else
        {
            ReturnWordToBank(draggableWord, word, false, eventData);

            sentenceModel.Remove(entryData);
            word.isInSentencePanel = false;
        }

        CommitModelChange();
    }

    private void ReturnWordToBank(RectTransform draggableWord, DraggableWord word, bool droppedInWB, PointerEventData eventData)
    {
        WordBank wb = wordBank;

        if (wb == null)
            return;

        draggableWord.transform.SetParent(wb.transform, false);
        draggableWord.pivot = new Vector2(0.5f, 0.5f);

        if (droppedInWB == false)
        {
            draggableWord.anchoredPosition = Vector2.zero;
        }

        if (droppedInWB == true)
        {
            RectTransform bankRect = wb.GetComponent<RectTransform>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                bankRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            draggableWord.anchoredPosition = localPoint;
        }
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
        if (!sentenceMutated)
            return;
        sentenceModel = Normalize(sentenceModel);
        ApplyNormalizationResults(sentenceModel);
        sentenceMutated = false;
    }

    public void ClearPreview()
    {
        if (!sentenceHasPreviews)
            return;

        sentenceModel.RemoveAll(entry => entry.isPreview);
        sentenceHasPreviews = false;

        // Remove preview RectTransforms from the UI and dictionary
        foreach (var kvp in ModelRects.Where(kvp => kvp.Key.isPreview).ToList())
        {
            RectTransform rect = kvp.Value;

            // Remove from wordList to prevent it from being rebuilt
            if (wordList.Contains(rect))
                wordList.Remove(rect);

            // Destroy the UI element
            Destroy(rect.gameObject);

            // Remove from ModelRects 
            ModelRects.Remove(kvp.Key);
        }

        // Reset preview tracking
        currentPreviewIndex = -1;
        Debug.Log("Previews removed");
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
        InsertArticles(workingModel, articleEntriesToInsert);
        
        NormalizeTrailingPunctuation(workingModel);

        return workingModel;
    }

    private void MoveWord(List<SentenceWordEntry> list, int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && oldIndex < list.Count &&
            newIndex >= 0 && newIndex <= list.Count)
        {
            SentenceWordEntry entry = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, entry);
        }
    }

    private void UpdateModelDictionary()
    {
        ModelRects.Clear();

        foreach (SentenceWordEntry entry in sentenceModel)
        {
            RectTransform rect = FindRectForEntry(entry);
            if (rect != null)
                ModelRects.Add(entry, rect);
        }
    }

    private RectTransform FindRectForEntry(SentenceWordEntry entry)
    {
        foreach (RectTransform rect in wordList)
        {
            var draggable = rect.GetComponent<DraggableWord>();
            if (draggable != null && draggable.sentenceWordEntry == entry)
                return rect;
        }

        return null;
    }

    private void ApplyNormalizationResults(List<SentenceWordEntry> workingModel, bool isPreviewMode = false)
    {
        // Collect UI rects to remove
        List<RectTransform> uiRectsToRemove = new List<RectTransform>();
        if (wordList.Count > 0)
        {
            foreach (RectTransform uiRect in wordList)
            {
                SentenceWordEntry uiEntry = uiRect.GetComponent<DraggableWord>().sentenceWordEntry;
                if (uiEntry != null)
                {
                    if (!workingModel.Contains(uiEntry))
                    {
                        uiRectsToRemove.Add(uiRect);
                    }
                }
            }
        }

        foreach (RectTransform uiRect in uiRectsToRemove)
        {
            wordList.Remove(uiRect);
            Destroy(uiRect.gameObject);
        }


        foreach (SentenceWordEntry entry in workingModel)
        {
            // Add sentenceModel rects not present in wordList.
            if (ModelRects.TryGetValue(entry, out RectTransform existingRect))
            {
                if (!wordList.Contains(existingRect))
                    wordList.Add(existingRect);
                continue;
            }

            // Instantiate prefabs for sentenceModel elements that require them
            RectTransform word = Instantiate(draggableWordPrefab, transform).GetComponent<RectTransform>();

            // Set word data to model data
            var draggable = word.GetComponent<DraggableWord>();
            draggable.isInSentencePanel = true;
            draggable.isDraggable = false;
            draggable.sentenceWordEntry = entry;

            if (isPreviewMode)
            {
                draggable.sentenceWordEntry.isPreview = true;

                // Make sure preview does not block pointer events
                CanvasGroup cg = word.GetComponent<CanvasGroup>();
                if (cg == null) cg = word.gameObject.AddComponent<CanvasGroup>();
                cg.blocksRaycasts = false;
            }


            TMP_Text wordText = word.GetComponent<TMP_Text>();
            wordText.text = entry.Surface;

            wordText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(word.GetComponent<RectTransform>());

            // Add instantiated prefab to wordList
            wordList.Add(word);
            ModelRects[entry] = word;
        }

        // Reorder wordList rects to match sentenceModel

        List<RectTransform> reorderedRects = new List<RectTransform>();

        foreach (SentenceWordEntry entry in workingModel)
        {
            if (ModelRects.ContainsKey(entry))
            {
                reorderedRects.Add(ModelRects[entry]);
            }
        }

        foreach (SentenceWordEntry entry in workingModel)
        {
            if (ModelRects.TryGetValue(entry, out RectTransform rect))
            {
                var text = rect.GetComponent<TMP_Text>();
                if (entry.isPreview)
                    text.color = Color.gray;
            }
        }

        wordList = reorderedRects;

        UpdateModelDictionary();
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
        sentenceModel.Insert(index, entry);
        storedWordList.Add(entry); // Add to backup list

        CommitModelChange();
    }


    public void RemoveDraggableFromSentence(SentenceWordEntry draggableEntry, PointerEventData eventData)
    {
        int idx = sentenceModel.IndexOf(draggableEntry);
        if (idx >= 0)
            sentenceModel.RemoveAt(idx);


        // sever grammatical connections
        if (draggableEntry.article != null)
        {
            var articleEntry = draggableEntry.article;

            articleEntry.owningNoun = null;
            draggableEntry.article = null;

            // Remove article from sentenceModel if it exists
            sentenceModel.Remove(articleEntry);
        }

        sentenceModel.Remove(draggableEntry);

        // remove rect to prevent destruction by normalization

        if (ModelRects.TryGetValue(draggableEntry, out RectTransform draggableRect)
            && wordList.Contains(draggableRect))
        {
            wordList.Remove(draggableRect);
        }

        draggableRect.pivot = new Vector2(0.5f, 0.5f);
        draggableRect.position = eventData.position;
        sentenceMutated = true;
        CommitModelChange();
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

    private int GetInsertionIndex(float draggableMidX, float margin = 10f)
    {
        int modelIndex = sentenceModel.Count; // default to end

        for (int i = 0; i < sentenceModel.Count; i++)
        {
            var entry = sentenceModel[i];
            
            if (!ModelRects.TryGetValue(entry, out RectTransform rect))
                continue; // skip entries without rect

            float wordMidX = rect.localPosition.x + rect.rect.width / 2f;

            // If draggableMidX is within the "margin zone" to the left of the midpoint, insert here
            if (draggableMidX < wordMidX + margin)
                return i;
        }

        return modelIndex;
    }
}



