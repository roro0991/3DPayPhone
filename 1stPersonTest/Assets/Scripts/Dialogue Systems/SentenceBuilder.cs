using Dialogue.Core;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceBuilder : MonoBehaviour
{
    // Other scripts
    public WordBank wordBank;

    // Panels & Graphics
    public GameObject InvalidDropIndicator;
    public GameObject verbMenu;
    public GameObject nounMenu;
    public GameObject interrogativeMenu;

    // RectTransforms
    public RectTransform sentencePanelRect;

    // Prefabs
    public GameObject draggableWordPrefab;

    // Lists & Dictionaries
    public List<RectTransform> wordList = new List<RectTransform>(); // sentence word gameobjects
    public List<SentenceWordEntry> sentenceModel = new List<SentenceWordEntry>(); // sentence word data
    public List<SentenceWordEntry> storedWordList = new List<SentenceWordEntry>(); // for wordbank repopulation
    private Dictionary<SentenceWordEntry, RectTransform> ModelRects = new Dictionary<SentenceWordEntry, RectTransform>();

    // Entries & Draggables
    public DraggableWord currentDraggable;
    public SentenceWordEntry currentPreviewEntry;
    public SentenceWordEntry deactivatedVerb = null;

    // Question Data    
    public PlayerQuestionData currentQuestionData;

    // Floats | Ints | Vectors
    public Vector2 startPosition = Vector2.zero;
    public float spacing = 10f;
    public int deactivatedVerbIndex = -1;
    private int currentPreviewIndex = -1;

    // Strings
    public string currentSentenceAsString;

    // Bools
    private bool sentenceHasPreviews;
    private bool sentenceMutated;

    // Enums    
    public enum InterrogativeRole
    {
        Unknown,
        Subject,
        Object,
        DeterminerOfSubject,
        DeterminerOfObject
    }
    public enum SubjectAgreement
    {
        Unknown,
        FirstPersonSingular, // I
        ThirdPersonSingular, // He, She, It [including Character names]
        Plural, // We, They, You, You [plural]
    }
    private void Start()
    {
        deactivatedVerb = null;
        InvalidDropIndicator.SetActive(false);
    }


    // ---------------- Hover & Drop Management ------------

    private void PositionInvalidDropIndicator(int insertIndex)
    {
        Vector3 position;

        if (insertIndex >= sentenceModel.Count)
        {
            position = wordList[wordList.Count - 1].transform.position;
        }
        else
        {
            position = wordList[insertIndex].transform.position;
        }

        InvalidDropIndicator.transform.position = position;
    }
    public void HandleHoveringWord(DraggableWord word, PointerEventData eventData) // Called from DraggableWord.cs
    {
        if (deactivatedVerb != null)
        {
            Debug.Log("swap state still active.");
        }

        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.transform.IsChildOf(sentencePanelRect))
        {
            // Edge case: dragging the very first word into an empty sentence panel.
            if (sentenceModel.Count == 1 && sentenceModel[0].isPreview)
            {
                return;
            }

            // Remove any existing preview words
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

            // --- INVALID DROP INDICATOR LOGIC (runs every frame) ---
            SentenceWordEntry previewEntryCheck = new SentenceWordEntry
            {
                Word = word.sentenceWordEntry.Word,
                Surface = word.sentenceWordEntry.Surface,
                isPreview = true
            };

            if (previewEntryCheck.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            {
                previewEntryCheck.Word = WordDataBase.Instance.GetWord("what");
                previewEntryCheck.Surface = previewEntryCheck.Word.Text;
            }

            bool canInsert = CanInsertAt(sentenceModel, insertIndex, previewEntryCheck);

            if (!canInsert)
            {
                InvalidDropIndicator.SetActive(true);
                PositionInvalidDropIndicator(insertIndex);
            }
            else
            {
                InvalidDropIndicator.SetActive(false);
            }

            // Prevent rebuild spam (ONLY affects preview, not indicator)
            if (insertIndex == currentPreviewIndex)
            {
                return;
            }

            currentPreviewIndex = insertIndex;

            // Create actual preview entry
            SentenceWordEntry previewEntry = new SentenceWordEntry
            {
                Word = word.sentenceWordEntry.Word,
                Surface = word.sentenceWordEntry.Surface,
                isPreview = true
            };

            if (word.sentenceWordEntry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            {
                previewEntry.Word = WordDataBase.Instance.GetWord("what");
                previewEntry.Surface = previewEntry.Word.Text;
            }

            currentPreviewEntry = previewEntry;

            if (canInsert)
            {
                // verb swap logic
                if (previewEntry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    sentenceModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary))
                {
                    Debug.Log("verb swap logic triggered!");
                    var existingVerb = sentenceModel.Find(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary);

                    deactivatedVerb = existingVerb;
                    Debug.Log("deactivedVerb no longer null.");

                    var deactivatedVerbAuxiliary = existingVerb.auxiliary != null ? existingVerb.auxiliary : null;
                    deactivatedVerbIndex = sentenceModel.IndexOf(existingVerb);

                    if (deactivatedVerbAuxiliary != null)
                    {
                        deactivatedVerb.auxiliary = null;
                        sentenceModel.Remove(deactivatedVerbAuxiliary);
                    }

                    sentenceModel.Remove(deactivatedVerb);

                    sentenceModel.Insert(deactivatedVerbIndex, previewEntry);
                    ApplyNormalizedPreview(sentenceModel, true);
                    sentenceHasPreviews = true;
                    return;
                }

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
                    if (deactivatedVerb != null)
                    {
                        sentenceModel.Insert(deactivatedVerbIndex, deactivatedVerb);
                    }
                    ApplyNormalizedPreview(sentenceModel, false);
                }
            }
        }
        else
        {
            if (sentenceHasPreviews)
            {
                ClearPreview();
                if (deactivatedVerb != null)
                {
                    sentenceModel.Insert(deactivatedVerbIndex, deactivatedVerb);
                }
                ApplyNormalizedPreview(sentenceModel, false);
            }

            // Ensure indicator is hidden when not hovering panel
            InvalidDropIndicator.SetActive(false);
        }
    }
    public void HandleWordDropped(DraggableWord word, PointerEventData eventData) // Called from DraggableWord.cs
    {
        if (word == null)
            return;

        currentDraggable = word;

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

            if (entryData.Word.HasPartOfSpeech(PartsOfSpeech.Verb))
            {
                draggableWord.gameObject.SetActive(false);
                verbMenu.SetActive(true);
                return;
            }

            if (entryData.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
            {
                draggableWord.gameObject.SetActive(false);
                nounMenu.SetActive(true);
                return;
            }

            if (entryData.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            {
                draggableWord.gameObject.SetActive(false);
                interrogativeMenu.SetActive(true);
                return;
            }

            ModelRects[entryData] = draggableWord;

            InsertWordEntryAt(entryData, currentPreviewIndex);
            ClearPreview();

            // Clear deactivated verb and return to wordbank if swapped
            if (entryData.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                deactivatedVerb != null)
            {
                wordBank.CreateWordUI(deactivatedVerb);
                deactivatedVerb = null;
                deactivatedVerbIndex = -1;
            }

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

    // Noun form menu button methods
    public void CommitNounForm(string nounForm)
    {
        SentenceWordEntry currentEntry = currentDraggable.sentenceWordEntry;
        RectTransform draggableRect = currentDraggable.GetComponent<RectTransform>();
        currentDraggable.gameObject.SetActive(true);
        Word.NounForms nounForms = new();
        nounForms = currentEntry.Word.GetNounForm();

        if (nounForms == null)
            Debug.Log("no noun forms found.");

        if (nounForm == "plural")
            currentEntry.Surface = nounForms.Plural;

        if (nounForm == "singular")
            currentEntry.Surface = nounForms.Singular;

        ModelRects[currentEntry] = draggableRect;

        InsertWordEntryAt(currentEntry, currentPreviewIndex);
        ClearPreview();

        currentDraggable.isInSentencePanel = true;

        sentenceMutated = true;

        CommitModelChange();

        nounMenu.SetActive(false);
    }
    public void HoverNounButtonEnter(string nounForm)
    {
        var nounForms = currentPreviewEntry.Word.GetNounForm();

        if (nounForm == "plural")
            RebuildPreview(nounForms.Plural);

        if (nounForm == "singular")
            RebuildPreview(nounForms.Singular);
    }

    // Verb form menu button methods
    public void CommitVerbForm(string verbForm)
    {
        SentenceWordEntry currentEntry = currentDraggable.sentenceWordEntry;
        RectTransform draggableRect = currentDraggable.GetComponent<RectTransform>();
        currentDraggable.gameObject.SetActive(true);
        Word.VerbForms verbForms = new();
        verbForms = currentEntry.Word.GetVerbForm();

        if (verbForms == null)
            Debug.Log("no verb forms found.");

        string cachedForm = string.Empty;

        switch (verbForm)
        {
            case "past":
                cachedForm = verbForms.Past;
                break;
            case "present":
                cachedForm = verbForms.ThirdPerson;
                break;
            default:
                break;
        }

        currentEntry.Surface = cachedForm;

        ModelRects[currentEntry] = draggableRect;

        InsertWordEntryAt(currentEntry, currentPreviewIndex);
        ClearPreview();

        // Clear deactivated verb and return to wordbank if swapped
        if (currentEntry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
            deactivatedVerb != null)
        {
            wordBank.CreateWordUI(deactivatedVerb);
            deactivatedVerb = null;
            deactivatedVerbIndex = -1;
        }

        currentDraggable.isInSentencePanel = true;

        sentenceMutated = true;

        CommitModelChange();

        verbMenu.SetActive(false);
    }
    public void HoverVerbButtonEnter(string form)
    {
        var verbForms = currentPreviewEntry.Word.GetVerbForm();

        switch (form)
        {
            case "past":
                RebuildPreview(verbForms.Past);
                break;
            case "present":
                RebuildPreview(verbForms.ThirdPerson);
                break;
            default:
                break;
        }
    }

    // Interrogative menu button methods
    public void CommitInterrogative(string interrogativeForm)
    {

        switch (interrogativeForm)
        {
            case "what":
                ChangeEntryWord(currentDraggable, "what");
                break;
            case "who":
                ChangeEntryWord(currentDraggable, "who");
                break;
            default:
                break;
        }

        SentenceWordEntry currentEntry = currentDraggable.sentenceWordEntry;
        RectTransform draggableRect = currentDraggable.GetComponent<RectTransform>();
        currentDraggable.gameObject.SetActive(true);

        ModelRects[currentEntry] = draggableRect;

        InsertWordEntryAt(currentEntry, currentPreviewIndex);
        ClearPreview();

        currentDraggable.isInSentencePanel = true;

        sentenceMutated = true;

        CommitModelChange();

        interrogativeMenu.SetActive(false);
    }
    public void HoverInterrogativeButton(string interrogativeForm)
    {
        switch (interrogativeForm)
        {
            case "what":
                RebuildPreview("what");
                break;
            case "who":
                RebuildPreview("who");
                break;
            default:
                break;
        }
    }

    // Return to bank button
    public void ReturnToBankButton()
    {
        currentDraggable.gameObject.SetActive(true);
        var draggableRect = currentDraggable.GetComponent<RectTransform>();
        ReturnWordToBank(draggableRect, currentDraggable, false);
        sentenceModel.RemoveAll(entry => entry.isPreview);
        sentenceModel = Normalize(sentenceModel);
        ApplyNormalizationResults(sentenceModel, false);
        verbMenu.SetActive(false);
        nounMenu.SetActive(false);
        interrogativeMenu.SetActive(false);
    }

    // Utility methods
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
    public void RemoveDraggableFromSentence(SentenceWordEntry draggableEntry, PointerEventData eventData) // Called from DraggableWord.cs
    {
        int idx = sentenceModel.IndexOf(draggableEntry);
        if (idx >= 0)
            sentenceModel.RemoveAt(idx);


        // sever grammatical connections
        if (draggableEntry.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
        {
            if (draggableEntry.isSubject)
                draggableEntry.isSubject = false;


            if (draggableEntry.isObject)
                draggableEntry.isObject = false;

            // Remove relationships references
            if (draggableEntry.article != null)
            {
                var articleEntry = draggableEntry.article;

                articleEntry.owningNoun = null;
                draggableEntry.article = null;

                // Remove article from sentenceModel if it exists
                sentenceModel.Remove(articleEntry);
            }

        }

        if (draggableEntry.verb != null &&
            draggableEntry.verb.owningSubject == draggableEntry)
        {
            Debug.Log("noun: " + draggableEntry.Surface + " removed from " + draggableEntry.verb.Surface + "'s owningSubjects List.");
            draggableEntry.verb.owningSubject = null;
            draggableEntry.verb = null;
        }

        if (draggableEntry.Word.HasPartOfSpeech(PartsOfSpeech.Verb))
        {
            // Remove relationship references
            if (draggableEntry.auxiliary != null)
            {
                var auxiliaryEntry = draggableEntry.auxiliary;

                auxiliaryEntry.owningVerb = null;
                draggableEntry.auxiliary = null;

                // Remove auxiliary from sentenceModel if it exists
                sentenceModel.Remove(auxiliaryEntry);
            }
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
    private void ReturnWordToBank(RectTransform draggableWord, DraggableWord word, bool droppedInWB, PointerEventData eventData = null)
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
    private void ChangeEntryWord(DraggableWord originalDraggable, string newWord)
    {
        originalDraggable.sentenceWordEntry.Word = WordDataBase.Instance.GetWord(newWord);
        originalDraggable.sentenceWordEntry.Surface = newWord;
    }
    private void RebuildPreview(string surface)
    {
        var entry = new SentenceWordEntry()
        {
            Word = currentPreviewEntry.Word,
            Surface = surface,
            isPreview = true
        };

        sentenceModel.RemoveAll(entry => entry.isPreview);
        sentenceModel.Insert(currentPreviewIndex, entry);

        currentPreviewEntry = entry;

        ApplyNormalizedPreview(sentenceModel, true);

    }
    private void InsertWordEntryAt(SentenceWordEntry entry, int index)
    {
        sentenceModel.Insert(index, entry);
        storedWordList.Add(entry); // Add to backup list
        sentenceMutated = true;
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

    // ---------------- Sentence management ----------------    
    // Preview Methods
    private void ClearPreview()
    {
        if (!sentenceHasPreviews)
            return;

        Debug.Log("previews cleared");

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
                //Debug.Log("Left word: " + leftWord.Surface);
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
                //Debug.Log("Right word: " + rightWord.Surface);
                break;
            }
        }

        // Cache existing object/subject role capable entries
        List<SentenceWordEntry> objectSubjectCapable = new();

        foreach (var capableEntry in model)
        {
            if (capableEntry.Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
                capableEntry.Word.HasPartOfSpeech(PartsOfSpeech.Character))
                objectSubjectCapable.Add(capableEntry);
        }

        // Prevent initial preview from interfering with checks
        if (model.Count == 1
            && model[0].isPreview)
        {
            leftWord = null;
            rightWord = null;
        }

        // ----- OBJECT & SUBJECT ROLE LIMIT -----
        // Limit sentences to a maximum of 2 subject/object role capable entires
        int coreCount = model.Count(entry => IsCoreEntity(entry) && !entry.isPreview);

        if (IsCoreEntity(entry) && coreCount >= 2)
        {
            Debug.Log("too many object & subject capable entries");
            return false;
        }

        bool hasSubjectPronoun = 
            model.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) && !entry.isPreview);
        bool hasObjectPronoun = 
            model.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun) && !entry.isPreview);

        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) 
            && hasSubjectPronoun)
        {
            Debug.Log("too many object & subject capable entries");
            return false;
        }

        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun)
            && hasObjectPronoun)
        {
            Debug.Log("too many object & subject capable entries");
            return false;
        }

        // ----- NOUN PHRASE RULES -----

        // Rule #1: Prevent article & owning noun split [Exception: adjectives]
        if (!entry.Word.HasPartOfSpeech(PartsOfSpeech.Adjective)
            && leftWord != null
            && rightWord != null
            && leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Article)
            && leftWord.owningNoun == rightWord)
        {
            //Debug.Log("ArticleNounSplit");
            return false;
        }

        // Rule #2: Prevent adjective placement after noun
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Adjective)
            && leftWord != null
            && leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
        {
            //Debug.Log("AdjectiveAfterNoun");
            return false;
        }

        // ----- INTERROGATIVE PLACEMENT RULES -----

        // Rule #1: Prevent multiple interrogatives
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
            && sentenceModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative) && !entry.isPreview))
        {
            //Debug.Log("MultipleInterrogatives");
            return false;
        }
        // Rule #1.2: Allow insertion of sole interrogative anywhere
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
            return true;
        // Rule #1.3: Prevent insertion before interrogative 
        if (rightWord != null && rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative) && !rightWord.isPreview)
        {
            //Debug.Log("InsertionBeforeInterrogative");
            return false;
        }

        // ----- VERB PHRASE RULES -----

        // Rule #1: Only allow 1 verb at a time
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
            (model.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
            entry.isPreview && entry.activePOS != PartsOfSpeech.Auxiliary)
            ))
            return false;

        // Rule #2: Allow swapping verb | limit sentence to 1 verb token
        if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
            (model.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb)) &&
            entry.activePOS != PartsOfSpeech.Auxiliary &&
            !entry.isPreview))
        {
            if (rightWord != null && rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Verb) ||
                leftWord != null && leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Verb))
                return true;
            else
            {
                Debug.Log("failed verb rule #1");
                return false;
            }
        }


        // Rule #3: Verbs only after nouns, characters, pronouns and interrogatives
        if ((!model.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
            && entry.activePOS != PartsOfSpeech.Auxiliary)
            && entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
            && leftWord != null
            && !(
                leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
                leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                leftWord.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) ||
                leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
                )))
        {
            //Debug.Log("VerbNotAfterNounOrInt");
            return false;
        }

        // Rule #4: Prevent non-interrogatives/characters/pronouns/nouns placed left of verbs
        if (rightWord != null
            && rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
            && !(
            entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
            entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative) ||
            entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
            entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) ||
            entry.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun)
            ))
        {
            //Debug.Log("VerbNotAfterNounOrInt");
            return false;
        }

        return true;
    }

    private bool IsCoreEntity(SentenceWordEntry entry)
    {
        return entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun)
            || entry.Word.HasPartOfSpeech(PartsOfSpeech.Character)
            || entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun)
            || entry.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun);
    }
    private List<SentenceWordEntry> Normalize(List<SentenceWordEntry> rawModel) // If insertion gate passed, fixes remaining grammar
    {
        ConsoleClearer.ClearConsole();

        var workingModel = new List<SentenceWordEntry>(rawModel);        

        ClearPreviewRelationships(workingModel);

        ClearLooseAuxiliaries(workingModel);

        ClearLooseArticles(workingModel);

        PairAdjectivesToNouns(workingModel);

        var articleEntriesToInsert = CollectArticlesToInsert(workingModel);
        UpdateAndInsertArticles(workingModel, articleEntriesToInsert);

        NormalizeInterrogative(workingModel); // just force interrogative to start of sentence

        NormalizeIfQuestion(workingModel);

        NormalizeConjunctions(workingModel);

        NormalizeTrailingPunctuation(workingModel);         

            return workingModel;
    }
    private void ClearPreviewRelationships(List<SentenceWordEntry> workingModel)
    {
        if (workingModel.Count == 0)
            return;

        if (workingModel.Any(entry => entry.isPreview))
            return;

        for (int i = 0; i < workingModel.Count; i++)
        {
            var entry = workingModel[i];

            if (entry.owningSubject != null &&
                entry.owningSubject.isPreview)
            {
                entry.owningSubject = null;
                Debug.Log("preview owningSubjects removed");
            }

            if (entry.owningNoun != null && entry.owningNoun.isPreview)
                entry.owningNoun = null;

            if (entry.owningVerb != null && entry.owningVerb.isPreview)
                entry.owningVerb = null;

            if (entry.owningAdjective != null && entry.owningAdjective.isPreview)
                entry.owningAdjective = null;

            if (entry.article != null && entry.article.isPreview)
                entry.article = null;

            if (entry.verb != null && entry.verb.isPreview)
                entry.verb = null;

            if (entry.auxiliary != null && entry.auxiliary.isPreview)
                entry.auxiliary = null;

            if (entry.adjectives.Count > 0 && entry.adjectives.Any(e => e.isPreview))
            {
                int count = entry.adjectives.Count;

                for (int j = 0; j < count; j++)
                {
                    var adjEntry = entry.adjectives.Dequeue();

                    if (!adjEntry.isPreview)
                        entry.adjectives.Enqueue(adjEntry);
                }
            }
        }
    }
    private void ClearLooseAuxiliaries(List<SentenceWordEntry> workingModel)
    {
        // Auxiliaries already get cleared if their owningVerb is removed
        // This is to remove auxiliaries if their ownVerb's owningSubjects are removed.

        if (!workingModel.Any(entry => entry.activePOS == PartsOfSpeech.Auxiliary))
            return;

        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            if (workingModel[i].activePOS == PartsOfSpeech.Auxiliary && workingModel[i].owningVerb != null)
            {
                var owningVerb = workingModel[i].owningVerb;

                if (owningVerb.auxiliary == workingModel[i]
                    && owningVerb.owningSubject == null)
                {
                    owningVerb.auxiliary = null;
                    workingModel.RemoveAt(i);
                }
            }
        }
    }
    private void ClearLooseArticles(List<SentenceWordEntry> workingModel)
    {
        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            if (!workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Article))
                continue;

            bool precededByInterrogative = false;

            var articleEntry = workingModel[i];

            if (i - 1 >= 0)
            {
                if (workingModel[i - 1].Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
                    precededByInterrogative = true;
            }

            // Remove if owningNoun exists, but article is preceded by an interrogative
            if (articleEntry.owningNoun != null && precededByInterrogative)
            {
                articleEntry.owningNoun.article = null;
                articleEntry.owningNoun = null;
                workingModel.RemoveAt(i);
                continue;
            }

            // Skip if article has an owningNoun && not preceded by an interrogative
            if (articleEntry.owningNoun != null && !precededByInterrogative)
                continue;

            workingModel.RemoveAt(i);
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
            //Debug.Log(wordData.Surface + " has " + tempAdjList.Count + " adjectives.");

            tempAdjList.Reverse();

            foreach (var adjective in tempAdjList)
            {
                adjective.owningNoun = wordData;
                wordData.adjectives.Enqueue(adjective);
            }
        }
    }
    private List<PendingArticleInsertion> CollectArticlesToInsert(List<SentenceWordEntry> workingModel)
    {
        List<PendingArticleInsertion> articleEntriesToInsert = new List<PendingArticleInsertion>();

        for (int i = workingModel.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry wordData = workingModel[i];
            bool precededByInterrogative = false;

            if (!wordData.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                continue;
            if (!wordData.Word.IsSingular(wordData.Surface))
                continue;
            if (wordData.article != null)
                continue;

            int nounIndex = workingModel.IndexOf(wordData);

            for (int j = nounIndex - 1; j >= 0; j--)
            {
                if (workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Punctuation))
                    continue;

                if (!workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Interrogative))
                    break;

                precededByInterrogative = true;
            }

            if (precededByInterrogative)
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
    private void UpdateAndInsertArticles(List<SentenceWordEntry> workingModel, List<PendingArticleInsertion> articlesToInsert)
    {
        // Update existing articles if necessary
        if (workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Article)))
        {
            List<SentenceWordEntry> existingArticles = new();

            for (int i = 0; i < workingModel.Count; i++)
            {
                if (workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Article))
                    existingArticles.Add(workingModel[i]);
            }
            //Debug.Log("existing articles: " + existingArticles.Count);

            for (int i = 0; i < existingArticles.Count; i++)
            {
                // Check article anchor
                SentenceWordEntry articleAnchor = null;
                var owningNoun = existingArticles[i].owningNoun;

                if (owningNoun == null)
                {
                    //Debug.LogError("Article without owning noun detected");
                    continue;
                }

                if (owningNoun.adjectives.Count == 0)
                {
                    articleAnchor = owningNoun;
                    //Debug.Log("article anchor: " + articleAnchor.Surface);
                }
                else
                {
                    articleAnchor = existingArticles[i].owningNoun.adjectives.Peek();
                    //Debug.Log("article anchor: " + articleAnchor.Surface);
                }

                char firstLetterOfAnchor = char.ToLower(articleAnchor.Surface[0]);
                bool startsWithVowel = "aeiou".Contains(firstLetterOfAnchor);
                string article = startsWithVowel ? "an" : "a";
                //Debug.Log("updated article: " + article);

                if (existingArticles[i].Surface != article)
                {
                    //Debug.Log("article updated from " + existingArticles[i].Surface + " to " + article);
                    existingArticles[i].Word = WordDataBase.Instance.GetWord(article);
                    existingArticles[i].Surface = article;
                }
            }
        }

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
        //Debug.Log("aritcleAnchor for " + pending.nounData.Surface + " is " + pending.articleAnchor.Surface);

        return articleEntry;
    }
    private void NormalizeInterrogative(List<SentenceWordEntry> rawModel)
    {
        var foundInterrogative = rawModel.FirstOrDefault(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
        && !entry.isPreview);

        if (foundInterrogative != null &&
            rawModel[0] != foundInterrogative)
            MoveWord(rawModel, rawModel.IndexOf(foundInterrogative), 0);
    }
    // Question Normalization Methods
    private void NormalizeIfQuestion(List<SentenceWordEntry> workingModel)
    {
        // Defensive checks
        if (workingModel == null)
        {
            //Debug.Log("Verb normalization cancelled!");
            return;
        }

        // Check for interrogative
        SentenceWordEntry interrogative = null;
        int interrogativeIndex = -1;
        if (workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)))
        {
            interrogativeIndex = workingModel.FindIndex(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative));
        }

        if (interrogativeIndex == -1)
        {
            //Debug.Log("No interrogative found!");
            return;
        }

        interrogative = workingModel[interrogativeIndex];

        switch (interrogative.Word.Text)
        {
            case "what":
                //Debug.Log("reached what normalization");
                NormalizeWhatInterrogative(workingModel);
                break;
            default:
                break;
        }
    }
    private void NormalizeWhatInterrogative(List<SentenceWordEntry> workingModel)
    {
        // Defensive checks
        if (workingModel == null || workingModel.Count == 0)
        {
            //Debug.Log("What interrogative normalization cancelled!");
            return;
        }

        if (!workingModel.Any(entry => entry.Word.HasPartOfSpeech(PartsOfSpeech.Interrogative)
            && entry.Word.Text == "what"))
        {
            //Debug.Log("What interrogative normalization cancelled!");
            return;
        }

        // Cache interrogative index for iteration
        int interrogativeIndex = workingModel.FindIndex(entry => entry.Word.Text == "what");

        InterrogativeRole interRole = InterrogativeRole.Unknown;

        if (interrogativeIndex == -1)
        {
            //Debug.Log("interrogative index not found!");
            return;
        }

        // Cache interrogative word data
        var whatInterrogative = workingModel[interrogativeIndex];

        // Confirm subject/object/determiner status of [what]        
        bool isNounAfterInterrogative = false;

        // Question variables
        SentenceWordEntry nounAfterInterrogative = new();
        SentenceWordEntry subjectEntry = new();
        SentenceWordEntry objectEntry = new();
        SentenceWordEntry verbEntry = null;

        // Determine Interrogative Role
        for (int i = interrogativeIndex + 1; i < workingModel.Count; i++)
        {
            var entry = workingModel[i];

            // skip to reach first noun or verb + skip trailing punctuation
            if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Adjective) ||
                entry.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation) ||
                entry.Word.HasPartOfSpeech(PartsOfSpeech.Adverb) ||
                entry.Word.HasPartOfSpeech(PartsOfSpeech.Article))
                continue;

            if (!isNounAfterInterrogative)
            {
                if (interRole == InterrogativeRole.Unknown &&
                (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb)
                && entry.activePOS != PartsOfSpeech.Auxiliary))
                {
                    if (subjectEntry != whatInterrogative)
                        subjectEntry = whatInterrogative;

                    interRole = InterrogativeRole.Subject;
                    Debug.Log("interRole is Subject");
                    break;
                }

                if (interRole == InterrogativeRole.Unknown &&
                    (entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun)))
                {
                    if (objectEntry != whatInterrogative)
                        objectEntry = whatInterrogative;

                    interRole = InterrogativeRole.Object;
                    Debug.Log("interRole is Object");
                    break;
                }

                if (interRole == InterrogativeRole.Unknown &&
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                {
                    Debug.Log("noun found after interrogative");
                    isNounAfterInterrogative = true;
                    continue;
                }
            }

            if (isNounAfterInterrogative)
            {
                if (interRole == InterrogativeRole.Unknown &&
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary)
                {
                    interRole = InterrogativeRole.DeterminerOfSubject;
                    Debug.Log("interRole is DeterminerOfSubject");
                    break;
                }

                if (interRole == InterrogativeRole.Unknown &&
                    (entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun)))
                {
                    if (subjectEntry != entry)
                        subjectEntry = entry;

                    interRole = InterrogativeRole.DeterminerOfObject;
                    Debug.Log("interRole is DeterminerOfObject");
                    break;
                }
            }
        }

        if (interRole == InterrogativeRole.Unknown)
        {
            Debug.Log("Failed to identify interrogative role.");
            return;
        }

        if (interRole == InterrogativeRole.Subject)
        {
            if (subjectEntry != whatInterrogative)
                subjectEntry = whatInterrogative;

            for (int i = interrogativeIndex + 1; i < workingModel.Count; i++)
            {
                var entry = workingModel[i];

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary)
                {
                    verbEntry = entry;
                    continue;
                }
            }
        }

        if (interRole == InterrogativeRole.Object)
        {
            if (objectEntry != whatInterrogative)
                objectEntry = whatInterrogative;

            for (int i = interrogativeIndex + 1; i < workingModel.Count; i++)
            {
                var entry = workingModel[i];

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun))
                {
                    if (subjectEntry != entry)
                        subjectEntry = entry;

                    continue;
                }

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary)
                {
                    if (verbEntry != entry)
                        verbEntry = entry;

                    continue;
                }
            }
        }

        if (interRole == InterrogativeRole.DeterminerOfSubject)
        {
            for (int i = interrogativeIndex + 1; i < workingModel.Count; i++)
            {
                var entry = workingModel[i];

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                {
                    if (subjectEntry != entry)
                        subjectEntry = entry;

                    continue;
                }

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary)
                {
                    if (verbEntry != entry)
                        verbEntry = entry;

                    continue;
                }

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                {
                    if (objectEntry != entry)
                        objectEntry = entry;

                    continue;
                }
            }
        }

        if (interRole == InterrogativeRole.DeterminerOfObject)
        {
            for (int i = interrogativeIndex + 1; i < workingModel.Count; i++)
            {
                var entry = workingModel[i];

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Noun))
                {
                    if (objectEntry != entry)
                        objectEntry = entry;

                    continue;
                }

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                    entry.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun))
                {
                    if (subjectEntry != entry)
                        subjectEntry = entry;

                    continue;
                }

                if (entry.Word.HasPartOfSpeech(PartsOfSpeech.Verb) &&
                    entry.activePOS != PartsOfSpeech.Auxiliary)
                {
                    if (verbEntry != entry)
                        verbEntry = entry;

                    continue;
                }
            }
        }

        // Determine subject agreement
        if (subjectEntry == null)
            return;

        if (verbEntry == null)
            return;

        subjectEntry.isSubject = true;
        objectEntry.isObject = true;

        // Cache relationship ref between verb and subjects        
        if (verbEntry.owningSubject != subjectEntry)
        {
            verbEntry.owningSubject = subjectEntry;
        }

        if (subjectEntry.verb != verbEntry)
        {
            subjectEntry.verb = verbEntry;
            Debug.Log("verb: " + verbEntry.Surface + " has been added as " + subjectEntry.Surface + "'s verb.");
        }                

        SubjectAgreement subjectAgreement = SubjectAgreement.Unknown;

        List<string> ThirdPersonSingular = new List<string> { "he", "she", "it" };
        List<string> Plural = new List<string> { "you", "we", "they" };

        string subjectEntryAsString = subjectEntry.Word.Text;
            
        if (subjectEntryAsString != string.Empty)
        {
            if (subjectEntryAsString == "i")
            {
                subjectAgreement = SubjectAgreement.FirstPersonSingular;
            }

            if (ThirdPersonSingular.Contains(subjectEntryAsString) ||
                subjectEntry.Word.HasPartOfSpeech(PartsOfSpeech.Character))
            {
                subjectAgreement = SubjectAgreement.ThirdPersonSingular;
            }

            bool hasAny = new();

            hasAny = Plural.Contains(subjectEntryAsString);

            if (hasAny)
            {
                subjectAgreement = SubjectAgreement.Plural;
            }
        }

        // Determine auxiliary insertion index
        int auxiliaryInsertionIndex = -1;

        switch (interRole)
        {
            case (InterrogativeRole.Subject):
                auxiliaryInsertionIndex = -1;
                break;
            case (InterrogativeRole.Object):
                auxiliaryInsertionIndex = interrogativeIndex + 1;
                break;
            case (InterrogativeRole.DeterminerOfSubject):
                auxiliaryInsertionIndex = -1;
                break;
            case (InterrogativeRole.DeterminerOfObject):
                auxiliaryInsertionIndex = workingModel.IndexOf(objectEntry) + 1;
                break;
            default:
                auxiliaryInsertionIndex = -1;
                break;
        }

        // Cache verb form
        if (verbEntry == null)
            return;
        Word.VerbForms verbForms = new();
        verbForms = verbEntry.Word.GetVerbForm();
        if (verbForms == null)
            return;

        Word.VerbForms.VerbForm cachedForm;
        bool found = verbForms.TryGetForm(verbEntry.Surface, out cachedForm);

        // Reset cachedForm to Base for interrogative question
        if (cachedForm == Word.VerbForms.VerbForm.ThirdPersonSingular)
        {
            verbEntry.Surface = verbForms.Base;
            found = verbForms.TryGetForm(verbEntry.Surface, out cachedForm);
        }

        var pendingData = new PendingAuxiliaryInsertion
        {
            isQuestion = true,
            auxInsertionIndex = auxiliaryInsertionIndex,
            subjAgreement = subjectAgreement,
            verb = verbEntry,
            verbForm = cachedForm,
        };

        if (verbEntry.auxiliary != null)
        {
            Debug.Log("existing auxiliary for verb " + verbEntry.Surface + " is " + verbEntry.auxiliary.Surface);
        }

        currentQuestionData = new PlayerQuestionData
        {
            Interrogative = InterrogativeType.What,
            isCopular = verbEntry.Word.Text == "be" ? true : false,
            Subject = subjectEntry != null ? subjectEntry : null,
            Object = objectEntry != null ? objectEntry : null,
            Verb = verbEntry != null ? verbEntry : null
        };

        CreateAuxiliaryVerb(workingModel, pendingData);
    }
    private void CreateAuxiliaryVerb(List<SentenceWordEntry> workingModel, PendingAuxiliaryInsertion pendingAuxiliaryData)
    {
        if (workingModel == null || workingModel.Count == 0)
            return;

        if (pendingAuxiliaryData == null)
            return;

        if (pendingAuxiliaryData.auxInsertionIndex == -1)
            return;

        Word.VerbForms verbForms = new();
        verbForms = pendingAuxiliaryData.verb.Word.GetVerbForm();

        SentenceWordEntry auxiliaryVerb;
        bool updatedExistingAuxiliary = false;

        // Remove existing auxiliary ref if it's a preview
        if (pendingAuxiliaryData.verb.auxiliary != null &&
            pendingAuxiliaryData.verb.auxiliary.isPreview)
        {
            pendingAuxiliaryData.verb.auxiliary = null;
        }

        // Update existing auxiliary entry
        if (pendingAuxiliaryData.verb.auxiliary != null)
        {
            var verb = pendingAuxiliaryData.verb;
            Debug.Log("Existing auxiliary for verb [" + verb.Surface + "] has been updated");
            auxiliaryVerb = verb.auxiliary;
            updatedExistingAuxiliary = true;
        }
        else
        {
            Debug.Log("New auxiliary created");
            // Create auxiliary entry
            auxiliaryVerb = new SentenceWordEntry();
            auxiliaryVerb.activePOS = PartsOfSpeech.Auxiliary;
            auxiliaryVerb.owningVerb = pendingAuxiliaryData.verb;
            auxiliaryVerb.owningVerb.auxiliary = auxiliaryVerb;

        }

        SubjectAgreement subjectAgreement = pendingAuxiliaryData.subjAgreement;

        // Determine auxiliary entry data
        switch (pendingAuxiliaryData.verbForm)
        {
            case Word.VerbForms.VerbForm.Base: // eat
                if (subjectAgreement == SubjectAgreement.Plural ||
                    subjectAgreement == SubjectAgreement.FirstPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("do");
                    auxiliaryVerb.Surface = auxiliaryVerb.Word.Text;
                }
                else if (subjectAgreement == SubjectAgreement.ThirdPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("do");
                    auxiliaryVerb.Surface = "does";
                }
                break;
            case Word.VerbForms.VerbForm.PresentParticiple: // eating
                if (subjectAgreement == SubjectAgreement.Plural)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("be");
                    auxiliaryVerb.Surface = "are";
                }
                else if (subjectAgreement == SubjectAgreement.FirstPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("be");
                    auxiliaryVerb.Surface = "am";
                }
                else if (subjectAgreement == SubjectAgreement.ThirdPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("be");
                    auxiliaryVerb.Surface = "is";
                }
                break;
            case Word.VerbForms.VerbForm.Past:
                auxiliaryVerb.Word = WordDataBase.Instance.GetWord("do");
                auxiliaryVerb.Surface = "did";
                pendingAuxiliaryData.verb.Surface = verbForms.Base;
                break;
            case Word.VerbForms.VerbForm.PastParticiple:
                if (subjectAgreement == SubjectAgreement.Plural ||
                    subjectAgreement == SubjectAgreement.FirstPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("have");
                    auxiliaryVerb.Surface = auxiliaryVerb.Word.Text;
                }
                else if (subjectAgreement == SubjectAgreement.ThirdPersonSingular)
                {
                    auxiliaryVerb.Word = WordDataBase.Instance.GetWord("have");
                    auxiliaryVerb.Surface = "has";
                }
                break;
            default:
                break;
        }

        if (auxiliaryVerb.Word == null)
            return;

        int auxiliaryInsertionIndex = pendingAuxiliaryData.auxInsertionIndex;

        if (updatedExistingAuxiliary)
        {
            // Update auxiliary position
            if (workingModel.IndexOf(auxiliaryVerb) != auxiliaryInsertionIndex)
                MoveWord(workingModel, workingModel.IndexOf(auxiliaryVerb), auxiliaryInsertionIndex);

            return;
        }

        workingModel.Insert(auxiliaryInsertionIndex, auxiliaryVerb);
    }
    private void NormalizeConjunctions(List<SentenceWordEntry> workingModel)
    {
        if (workingModel == null || workingModel.Count == 0) //Defensive check
            return;

        // Remove only loose conjunctions
        List<int> conjunctionIndices = new();

        for (int i = 0; i < workingModel.Count; i++)
        {
            if (workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Conjunction))
                conjunctionIndices.Add(i);
        }

        for (int i = conjunctionIndices.Count - 1; i >= 0; i--)
        {
            int conjunctionIndex = conjunctionIndices[i];

            var rightWord = (conjunctionIndex + 1) < workingModel.Count
                ? workingModel[conjunctionIndex + 1]
                : null;

            var leftWord = (conjunctionIndex - 1) >= 0
                ? workingModel[conjunctionIndex - 1]
                : null;

            if (rightWord == null ||
                rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Conjunction) ||
                rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Punctuation) ||
                !rightWord.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) ||
                !rightWord.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun) ||
                !rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
                !rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Adjective) ||
                (!rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Adverb) && rightWord.owningAdjective == null))
            {
                workingModel.RemoveAt(conjunctionIndex);
                continue;
            }

            if (leftWord == null ||
                !leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                !leftWord.Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) ||
                !leftWord.Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun) ||
                !leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
                !leftWord.Word.HasPartOfSpeech(PartsOfSpeech.Adjective) ||
                (!rightWord.Word.HasPartOfSpeech(PartsOfSpeech.Adverb) && rightWord.owningAdjective == null))
            {
                workingModel.RemoveAt(conjunctionIndex);
                continue;
            }
        }

        List<int> nounIndices = new();
        List<int> conjunctionInsertionIndices = new();

        for (int i = 0; i < workingModel.Count; i++)
        {
            if (workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Noun) ||
                workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.Character) ||
                workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.SubjectPronoun) ||
                workingModel[i].Word.HasPartOfSpeech(PartsOfSpeech.ObjectPronoun))
                nounIndices.Add(i);
        }

        if (nounIndices.Count < 2) // conj unnecessary if < 2 nouns present in model
            return;

        for (int i = 0; i < nounIndices.Count - 1; i++)
        {
            int firstNounIndex = nounIndices[i];
            int secondNounindex = nounIndices[i + 1];

            int start = firstNounIndex + 1;
            int end = secondNounindex;

            // Check for verbs or existing conjuntions between nouns
            bool verbOrConjFound = false;

            for (int j = start; j < end; j++)
            {
                if (workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Verb) ||
                    workingModel[j].Word.HasPartOfSpeech(PartsOfSpeech.Conjunction))
                {
                    verbOrConjFound = true;
                    break;
                }
            }

            if (verbOrConjFound)
                continue;

            // Prevent conjunctions between nouns and (characters + pronouns)
            if ((workingModel[firstNounIndex].Word.HasPartOfSpeech(PartsOfSpeech.Noun) &&
                !workingModel[secondNounindex].Word.HasPartOfSpeech(PartsOfSpeech.Noun)) ||
                (!workingModel[firstNounIndex].Word.HasPartOfSpeech(PartsOfSpeech.Noun) &&
                workingModel[secondNounindex].Word.HasPartOfSpeech(PartsOfSpeech.Noun)))
                continue;

            conjunctionInsertionIndices.Add(firstNounIndex + 1);
        }

        if (conjunctionInsertionIndices.Count == 0)
            return;

        var andWord = WordDataBase.Instance.GetWord("and");

        for (int i = conjunctionInsertionIndices.Count - 1; i >= 0; i--)
        {
            SentenceWordEntry conjunction = new();
            conjunction.Word = andWord;
            conjunction.Surface = andWord.Text;

            workingModel.Insert(conjunctionInsertionIndices[i], conjunction);
        }
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

    // Data classes
    private class PendingAuxiliaryInsertion
    {
        public bool isQuestion = false;
        public int auxInsertionIndex;
        public SubjectAgreement subjAgreement;
        public SentenceWordEntry verb;
        public Word.VerbForms.VerbForm verbForm;
    }
    private class PendingArticleInsertion // Helper class for article insertion
    {
        public SentenceWordEntry nounData;
        public int nounIndex;
        public SentenceWordEntry articleAnchor;
    }

    // Model dictionary methods
    private void UpdateModelDictionary() // Middleman between model entries and rects
    {
        ModelRects.Clear();

        foreach (SentenceWordEntry entry in sentenceModel)
        {

            RectTransform rect = FindRectForEntry(entry);
            if (rect != null && !ModelRects.ContainsKey(entry))
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

    // UI Methods
    private void CommitModelChange() // Commit sentence mutations for UI update
    {
        if (!sentenceMutated)
            return;
        sentenceModel = Normalize(sentenceModel);
        ApplyNormalizationResults(sentenceModel);
        sentenceMutated = false;
        UpdateSentenceString();
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
            Debug.Log("rect destroyed");
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

        // Ensure UI text matches entries' interntal data
        for (int i = 0; i < wordList.Count; i++)
        {
            var committed = wordList[i];
            var committedText = committed.GetComponent<TMP_Text>();
            var committedDraggable = committed.GetComponent<DraggableWord>();

            if (committedText.text != committedDraggable.sentenceWordEntry.Surface)
            {
                committedText.text = committedDraggable.sentenceWordEntry.Surface;

                committedText.ForceMeshUpdate();
                LayoutRebuilder.ForceRebuildLayoutImmediate(committed.GetComponent<RectTransform>());
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
        sentenceModel.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        { 
            if (transform.GetChild(i).gameObject != InvalidDropIndicator)
                Destroy(transform.GetChild(i).gameObject);                
        }

        currentSentenceAsString = string.Empty;
    }
    public void ClearStoredWords()
    {
        storedWordList.Clear();
    }
}










