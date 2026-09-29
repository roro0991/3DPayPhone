using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Dialogue.Core;
using System.Collections.Specialized;

public class WordBank : MonoBehaviour
{
    public SentenceBuilder SentenceBuilder;
    public List<string> CurrentWordBankWords = new List<string>();    
    public List<SentenceWordEntry> WordBankEntries = new List<SentenceWordEntry>(); // store Word objects now        
    public List<RectTransform> DraggableRects = new List<RectTransform>();
    public GameObject draggableWordPrefab;

    private List<Coroutine> runningFades = new List<Coroutine>();

    private void Awake()
    {
        CurrentWordBankWords = new List<string>
        {
            "you", "work", "accountant", "know", "do"
        };
    }

    private void OnEnable()
    {
        PopulateWordBank();
        CallManager.OnInputSubmitted += HandleInputSubmitted;
    }

    private void OnDisable()
    {
        ClearWordBank();
        CallManager.OnInputSubmitted -= HandleInputSubmitted;
    }

    private void HandleInputSubmitted()
    {
        List<SentenceWordEntry> wordsToAdd = SentenceBuilder.GetStoredWords();

        AddWordsToWordBank(wordsToAdd);
    }

    private void AddEntry(string surface)
    {
        Word word = WordDataBase.Instance.GetWord(surface);
        
        if (word != null)
        {
            Debug.Log("word found in worddatabase");
            WordBankEntries.Add(new SentenceWordEntry
            {
                Word = word,
                Surface = surface // ? THIS is "dogs"
            });
        }
        else
        {
            Debug.Log("word not found in worddatabase.");
        }
    }

    private void PopulateWordBank()
    {
        foreach (string word in CurrentWordBankWords)
        {
            AddEntry(word);
        }

        // Stop all running fade coroutines
        foreach (var fade in runningFades)
        {
            if (fade != null)
                StopCoroutine(fade);
        }
        runningFades.Clear();

        // Generate new words
        foreach (SentenceWordEntry word in WordBankEntries)
        {
            CreateWordUI(word);
        }
    }

    private void AddWordsToWordBank(List<SentenceWordEntry> words)
    {
        // If panel is active, generate immediately
        if (gameObject.activeInHierarchy)
        {
            foreach (SentenceWordEntry word in words)
            {
                CreateWordUI(word);
            }
        }
    }

    // UI Generation Methods
    public void CreateWordUI(SentenceWordEntry word)
    {
        GameObject newWord = Instantiate(draggableWordPrefab, transform);

        TMP_Text textComponent = newWord.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = word.Surface; // Use SentenceWordEntry surface instead of string
        }
        else
        {
            Debug.LogWarning("The instantiated prefab does not have a TMP_Text component");
        }

        RectTransform newRect = newWord.GetComponent<RectTransform>();
        DraggableRects.Add(newRect);

        if (newRect != null)
        {
            newRect.anchoredPosition = GetRandomPositionWithinParent();
        }

        // Start fade-in and track coroutine
        Coroutine fadeCoroutine = StartCoroutine(FadeInWord(newWord));
        runningFades.Add(fadeCoroutine);
    }

    private IEnumerator FadeInWord(GameObject wordObject)
    {
        if (wordObject == null) yield break;

        CanvasGroup canvasGroup = wordObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = wordObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (canvasGroup != null) // null-safe check
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);

            yield return null;

            if (wordObject == null) yield break; // stop if object destroyed
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private Vector2 GetRandomPositionWithinParent()
    {
        Vector2 size = this.GetComponent<RectTransform>().rect.size;
        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float y = Random.Range(-size.y / 2f, size.y / 2f);

        return new Vector2(x, y);
    }

    public void ClearWordBank()
    {
        WordBankEntries.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        DraggableRects.Clear();
        //GenerateWords();
    }
}






