using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordBank : MonoBehaviour
{
    private Queue<Word> wordsInQueue = new Queue<Word>();    // store Word objects now
    public GameObject draggableWordPrefab;

    private List<Coroutine> runningFades = new List<Coroutine>();

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
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generate new words
        foreach (Word word in wordsInQueue)
        {
            CreateWordUI(word);
        }

        Debug.Log("GenerateWords called. Words in queue: " + wordsInQueue.Count);
    }

    public void AddWordsToWordBank(List<Word> words)
    {
        // Queue up the words
        wordsInQueue = new Queue<Word>(words);

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

    private void CreateWordUI(Word word)
    {
        GameObject newWord = Instantiate(draggableWordPrefab, transform);

        TMP_Text textComponent = newWord.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = word.Text; // Use Word.Text instead of string
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

    public void UpdateWordBank(List<Word> words)
    {
        foreach (Word word in words)
        {
            wordsInQueue.Enqueue(word);
        }
        GenerateWords();
    }
}


