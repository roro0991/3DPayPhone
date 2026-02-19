using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SentenceWordEntry
{
    public Word Word; // semantic object
    public string Surface; // the actual form used (ie. dog vs dogs)
    public SentenceWordEntry owningNoun; // corresponding noun to this article
    public SentenceWordEntry article;
    public bool isPreview; // for placeholder preview
}

public class WordBank : MonoBehaviour
{

    public List<SentenceWordEntry> wordsInQueue = new List<SentenceWordEntry>(); // store Word objects now        
    public GameObject draggableWordPrefab;

    private List<Coroutine> runningFades = new List<Coroutine>();

    // Methods for adding words to wordbank based on current contact called
    // These methods will possibly be replaced

    public void AddWordToSentence(string key)
    {
        key = key.ToLower();

        // First try direct match
        var word = WordDataBase.Instance.GetWord(key);
        if (word != null)
        {
            AddEntry(word, key);
            return;
        }

        // Check known influections       
        foreach (var w in WordDataBase.Instance.Words.Values)
        {
            // Check noun forms
            foreach (var nf in w.NounFormsList)
            {
                if (nf.Plural == key)
                {
                    AddEntry(w, key);
                    return;
                }
            }
        }

        Debug.LogWarning($"Couldn't find base word for '{key}'");
    }

    private void AddEntry(Word word, string surface)
    {
        wordsInQueue.Add(new SentenceWordEntry
        {
            Word = word,
            Surface = surface // ? THIS is "dogs"
        });
    }

    public void Refresh()
    {
        GenerateWords();
    }

    private void GenerateWords()
    {
        // Stop all running fade coroutines
        foreach (var fade in runningFades)
        {
            if (fade != null)
                StopCoroutine(fade);
        }
        runningFades.Clear();

        // Clear existing children
        /*
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        */

        // Generate new words
        foreach (SentenceWordEntry word in wordsInQueue)
        {
            CreateWordUI(word);
        }

        Debug.Log("GenerateWords called. Words in queue: " + wordsInQueue.Count);
    }

    public void AddWordsToWordBank(List<SentenceWordEntry> words)
    {
        // Queue up the words
        wordsInQueue = new List<SentenceWordEntry>(words);

        // If panel is active, generate immediately
        if (gameObject.activeInHierarchy)
        {
            GenerateWords();
        }
        else
        {
            // Wait until the panel is active
            StartCoroutine(WaitForActivationAndGenerate());
        }       
    }

    private IEnumerator WaitForActivationAndGenerate()
    {
        yield return new WaitUntil(() => gameObject.activeInHierarchy);
        GenerateWords();
    }

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
        wordsInQueue.Clear();
        GenerateWords();
    }

}






