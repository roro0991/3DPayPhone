using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings.SplashScreen;

public class SentenceBuilder : MonoBehaviour
{
    public WordBank wordBank;
    public RectTransform sentencePanelRect;

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

    // ---------------- Hover & Drop Management ------------
    public void HandleHoveringWord(DraggableWord word, PointerEventData eventData) // Called from DraggableWord.cs
    {
        GameObject dropTarget = eventData.pointerEnter;
        
        if (dropTarget != null && dropTarget.transform.IsChildOf(sentencePanelRect))
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

            // Check for interrogative
            if (word.sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
                insertIndex = 0;

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
                Debug.Log("***PREVIEW GENERATED***");                
            }
            else
            {
                if (sentenceHasPreviews)
                {
                    ClearPreview();
                    ApplyNormalizedPreview(sentenceModel, false);
                }
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
    public void HandleWordDropped(DraggableWord word, PointerEventData eventData) // Called from DraggableWord.cs
    {
        if (word == null)
            return;        

        RectTransform draggableWord = word.GetComponent<RectTransform>();
        var entryData = word.sentenceWordEntry;

        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.transform.IsChildOf(sentencePanelRect))
        {
            draggableWord.transform.SetParent(transform, false);            
            
            // ? CRITICAL: Validate drop commit
            if (!CanInsertAt(sentenceModel, currentPreviewIndex, entryData))
            {
                Debug.Log("Drop rejected by grammar validation");

                ReturnWordToBank(draggableWord, word, false, eventData);
                ClearPreview();
                ApplyNormalizationResults(sentenceModel);
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
    public void InsertWordEntryAt(SentenceWordEntry entry, int index)
    {
        sentenceModel.Insert(index, entry);
        storedWordList.Add(entry); // Add to backup list
        sentenceMutated = true;        
    }    
    public void RemoveDraggableFromSentence(SentenceWordEntry draggableEntry, PointerEventData eventData) // Called from DraggableWord.cs
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

    // ---------------- Sentence management ----------------    
        // Preview Methods
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
    private void ApplyNormalizedPreview(List<SentenceWordEntry> model, bool isPreview)
    {
        List<SentenceWordEntry> normalizedModel = Normalize(model);
        ApplyNormalizationResults(normalizedModel, isPreview);
    }
        // Drop Methods
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

        word.isInSentencePanel = false;
    }         
        // Normalization Methods
    private bool CanInsertAt(List<SentenceWordEntry> model, int insertIndex, SentenceWordEntry entry) // Initial grammar gate for insertion
    {
        // Initial defensive checks
        if (model == null)
            return false;
        if (insertIndex < 0 || insertIndex > model.Count)
            return false;
        if (model.Count == 0)
            return true;
        // Cache words left and right of insertion for checks
        var leftWord = insertIndex > 0 ? model[insertIndex - 1] : null;
        for (int i = insertIndex - 1; i >= 0; i--)
        {
            var candidate = model[i];
            if (!candidate.isPreview
                && !candidate.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation)
                && candidate != entry)
            {
                leftWord = candidate;
                Debug.Log("Left word: " + leftWord.Surface);
                break;
            }
        }

        var rightWord = insertIndex < model.Count ? model[insertIndex] : null;
        for (int i = insertIndex; i < model.Count; i++)
        {
            var candidate = model[i];

            // Skip previews, punctuation, and the word being inserted
            if (!candidate.isPreview
                && !candidate.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation)
                && candidate != entry)
            {
                rightWord = candidate;
                Debug.Log("Right word: " + rightWord.Surface);
                break;
            }
        }
        
        // Prevent initial preview from interfering with checks
        if (model.Count == 1 
            && model[0].isPreview)
        {
            leftWord = null;
            rightWord = null;
        }                

        // ----- NOUN PHRASE RULES -----
        
        // Rule #1: Prevent article & owning noun split [Exception: adjectives]
        if (!entry.Word.HasPartOfSpeech(PartsOfSpeech.Adjective)
            &&leftWord != null
            && rightWord != null
            && leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Article)
            && leftWord.owningNoun == rightWord)
            {
                Debug.Log("ArticleNounSplit");
                return false;
            }

        // Rule #2: Prevent adjective placement after noun
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Adjective) 
            && leftWord != null 
            && leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
            {
                Debug.Log("AdjectiveAfterNoun");
                return false;
            }

        // ----- INTERROGATIVE PHRASE RULES -----

        // Rule #1: Prevent multiple interrogatives
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
            && sentenceModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative) && !entry.isPreview))
        {
            Debug.Log("MultipleInterrogatives");
            return false;
        }
        // Rule #1.2: Allow insertion of sole interrogative anywhere
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            return true;
        // Rule #1.3: Prevent insertion before interrogative 
        if (rightWord != null && rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative) && !rightWord.isPreview)
        {
            Debug.Log("InsertionBeforeInterrogative");
            return false;
        }

        // ----- VERB PHRASE RULES -----

        // Rule #1: Verbs only after nouns and interrogatives
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
            && leftWord != null
            && !(
                leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun)
                || leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
                ))
        {
            Debug.Log("VerbNotAfterNounOrInt");
            return false;
        }

        // Rule #1.1: Prevent non-verbs and non-interrogatives placed left of verbs
        if (rightWord != null
            && rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
            && !(
            entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun)
            || entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
            ))
        {
            Debug.Log("right word: " + rightWord.Surface);
            Debug.Log("VerbNotAfterNounOrInt");
            return false;
        }        
        return true;
    }        
    private List<SentenceWordEntry> Normalize(List<SentenceWordEntry> rawModel) // If insertion gate passed, fixes remaining grammar
    {
        var workingModel = new List<SentenceWordEntry>(rawModel);

        ClearAllArticles(workingModel);

        PairAdjectivesToNouns(workingModel);

        var articleEntriesToInsert = CollectArticlesToInsert(workingModel);
        InsertArticles(workingModel, articleEntriesToInsert);

        NormalizeConjunctions(workingModel);
                
        NormalizeInterrogative(workingModel);

        NormalizeVerb(workingModel);
        
        NormalizeTrailingPunctuation(workingModel);

        return workingModel;
    }

    // Early conjunction normalizer meant to add 'and' between adjacent noun phrases
    private void NormalizeConjunctions(List<SentenceWordEntry> workingModel)
    {
        if (workingModel == null)
            return;

        if (!workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun)))
            return;

        if (workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Conjunction)))
            workingModel.RemoveAll(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Conjunction));

        List<int> nounIndices = new List<int>();

        for (int i = 0; i < workingModel.Count; i++)
        {
            if (workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                nounIndices.Add(i);
        }

        if (nounIndices.Count < 2)
            return;

        for (int i = nounIndices.Count - 2; i >= 0; i--)
        {
            int firstNounIndex = nounIndices[i];
            int secondNounIndex = nounIndices[i + 1];

            int start = firstNounIndex + 1; // start after first noun
            int end = secondNounIndex; // up to the next noun

            bool verbFound = false;

            for (int j = start; j < end; j++)
            {
                if (workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Verb))
                {
                    verbFound = true;
                    break;
                }
            }

            if (verbFound)
                continue;

            // Generate conjunction between nounphrases
            SentenceWordEntry conjunction = new SentenceWordEntry();
            conjunction.Word = WordDataBase.Instance.GetWord("and");
            conjunction.Surface = conjunction.Word.Text;

            workingModel.Insert(firstNounIndex +1, conjunction);
        }
    }
    // Early work in progress method of verb normalization
    // designed only to autogenerate verb 'to be'
    private void NormalizeVerb(List<SentenceWordEntry> workingModel)
    {
        // Remove existing instances of verb 'to be'
        if (workingModel.Any(entry => entry.Word.Text == "be"))
            workingModel.RemoveAll(entry => entry.Word.Text == "be");

        // Defensive checks
        if (workingModel == null) 
            return;

        if (workingModel.Count <= 1) 
            return;

        if (!workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun)))
            return;

        // Create verb 'to be' 
        SentenceWordEntry verbToBe = new();
        verbToBe.Word = WordDataBase.Instance.GetWord("be");
        Word.VerbForms beForms = verbToBe.Word.GetVerbForm();

        // Check for interrogative
        SentenceWordEntry interrogative = null;
        if (workingModel[0].Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            interrogative = workingModel[0];

        if (interrogative == null)
            return;

        // Check for nounAnchor
        SentenceWordEntry anchorNoun = null;
        for (int i = workingModel.IndexOf(interrogative); i < workingModel.Count; i++)
        {
            if (!workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                continue;

            anchorNoun = workingModel[i];
            break;
        }

        if (anchorNoun == null)
        {
            Debug.Log("No anchorNoun found for verb");
            return;
        }

        switch (interrogative.Surface)
        {
            case "what":
                if (anchorNoun.Word.IsPlural(anchorNoun.Surface))
                    verbToBe.Surface = beForms.ThirdPersonPlural;
                else
                    verbToBe.Surface = beForms.ThirdPersonSingular;
                    break;
            default:
                break;
        }

        workingModel.Insert(1, verbToBe);
    }
    private void ClearAllArticles(List<SentenceWordEntry> workingModel)
    {
        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            if (!workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Article))
                continue;

            var articleToRemove = workingModel[i];

            workingModel.RemoveAt(i);

            if (articleToRemove.owningNoun != null)
            {
                articleToRemove.owningNoun.article = null;
                articleToRemove.owningNoun = null;
            }
        }
    }    
    private void PairAdjectivesToNouns(List<SentenceWordEntry> workingModel)
    {
        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry wordData = workingModel[i];

            if (!wordData.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                continue;

            foreach (var adjective in wordData.adjectives)
            {
                adjective.owningNoun = null;
            }
            wordData.adjectives.Clear();
            
            List<SentenceWordEntry> tempAdjList = new();
            
            for (int j = i - 1; j >= 0; j--)
            {
                if (workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
                    continue;

                if (!workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Adjective))
                    break;

                tempAdjList.Add(workingModel[j]);
            }
            Debug.Log(wordData.Surface + " has " + tempAdjList.Count + " adjectives.");

            tempAdjList.Reverse();

            foreach (var adjective in tempAdjList)
            {
                adjective.owningNoun = wordData;
                wordData.adjectives.Enqueue(adjective);
            }
        }
    }    
    private class PendingArticleInsertion // Helper class for article insertion
    {
        public SentenceWordEntry nounData;
        public int nounIndex;
        public SentenceWordEntry articleAnchor;
    }
    private List<PendingArticleInsertion> CollectArticlesToInsert(List<SentenceWordEntry> workingModel)
    {
        List<PendingArticleInsertion> articleEntriesToInsert = new List<PendingArticleInsertion>();

        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry wordData = workingModel[i];

            if (!wordData.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                continue;
            if (!wordData.Word.IsSingular(wordData.Surface))
                continue;
            
            // Set articleAnchor to noun if no adjectives present
            SentenceWordEntry articleAnchor = wordData;

            if (wordData.adjectives.Count > 0)
            {
                articleAnchor = wordData.adjectives.Peek();
            }                

            articleEntriesToInsert.Add(new PendingArticleInsertion
            {
                nounIndex = i,
                nounData = wordData,
                articleAnchor = articleAnchor
            });
        }

        return articleEntriesToInsert;
    }   
    private void InsertArticles(List<SentenceWordEntry> workingModel, List<PendingArticleInsertion> articlesToInsert)
    {

        foreach (var pending in articlesToInsert)
        {
            int anchorIndex = workingModel.IndexOf(pending.articleAnchor);

            if (anchorIndex < 0)
                continue; // noun no longer exists - safety check

            SentenceWordEntry articleEntry =
                CreateArticleForNoun(pending);

            workingModel.Insert(anchorIndex, articleEntry);
        }
    }
    private SentenceWordEntry CreateArticleForNoun(PendingArticleInsertion pending)
    {
        SentenceWordEntry articleEntry = new SentenceWordEntry();

        // Determine article
        char firstLetterOfAnchor = char.ToLower(pending.articleAnchor.Surface[0]);
        bool startsWithVowel = "aeiou".Contains(firstLetterOfAnchor);
        string article = startsWithVowel ? "an" : "a";

        // Set word data
        articleEntry.Word = WordDataBase.Instance.GetWord(article);
        articleEntry.Surface = article;
        articleEntry.owningNoun = pending.nounData;
        pending.nounData.article = articleEntry;

        //Debug.Log("inserted article: " + article);
        //Debug.Log("article's owning noun: " + wordData.Surface);
        Debug.Log("aritcleAnchor for " + pending.nounData.Surface + " is " + pending.articleAnchor.Surface);

        return articleEntry;
    }    
    private void UpdateModelDictionary() // Middleman between model entries and rects
    {
        ModelRects.Clear();

        foreach (SentenceWordEntry entry in sentenceModel)
        {
            RectTransform rect = FindRectForEntry(entry);
            if (rect != null)
                ModelRects.Add(entry, rect);
        }
    }
    private RectTransform FindRectForEntry(SentenceWordEntry entry) // Helper method for ModelDictionary
    {
        foreach (RectTransform rect in wordList)
        {
            var draggable = rect.GetComponent<DraggableWord>();
            if (draggable != null && draggable.sentenceWordEntry == entry)
                return rect;
        }

        return null;
    }
    private void NormalizeInterrogative(List<SentenceWordEntry> rawModel)
    {
        var foundInterrogative = rawModel.FirstOrDefault(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
        && !entry.isPreview);

        if (foundInterrogative != null &&
            rawModel[0] != foundInterrogative)
            MoveWord(rawModel, rawModel.IndexOf(foundInterrogative), 0);
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
    
        // UI Methods
    public void CommitModelChange() // Commit sentence mutations for UI update
    {
        if (!sentenceMutated)
            return;
        sentenceModel = Normalize(sentenceModel);
        ApplyNormalizationResults(sentenceModel);
        sentenceMutated = false;
    } 
    private void ApplyNormalizationResults(List<SentenceWordEntry> workingModel, bool isPreviewMode = false) // Update UI from normalized model
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
}



