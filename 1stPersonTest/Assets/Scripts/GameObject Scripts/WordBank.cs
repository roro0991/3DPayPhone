using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordBank : MonoBehaviour
{    
    public List<string> wordsInQueue = new List<string>();
    public GameObject draggableWordPrefab;

    private void GenerateWords()
    {
        if (wordsInQueue.Count > 0)
        {
            foreach (string word in wordsInQueue)
            {
                GameObject newWord = Instantiate(draggableWordPrefab, transform);
                TMP_Text textComponent = newWord.GetComponent<TMP_Text>();
            
                if (textComponent != null)
                {
                    textComponent.text = word;
                }
                else
                {
                    Debug.LogWarning("The insantiated prefab does not have a text component");
                }

                RectTransform newRect = newWord.GetComponent<RectTransform>();
                if (newRect != null)
                {
                    newRect.anchoredPosition = GetRandomPositionWithinParent();
                }
            }
        }
        else
        {
            foreach (Transform child in this.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    Vector2 GetRandomPositionWithinParent()
    {
        // Get the size of the parent container
        Vector2 size = this.GetComponent<RectTransform>().rect.size;

        // Generate random coordinates within that area
        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float y = Random.Range(-size.y / 2f, size.y / 2f);

        return new Vector2(x, y);
    }

    public void ClearWordBank()
    {
        wordsInQueue.Clear();
        GenerateWords();
    }

    public void UpdateWordBank(List<string> words)
    {
        foreach (string word in words)
        {
            wordsInQueue.Add(word);
        }
        GenerateWords();
    }
}
