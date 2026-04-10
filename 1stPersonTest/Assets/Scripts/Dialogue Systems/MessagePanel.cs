using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MessagePanel : MonoBehaviour
{
    [Header("References")]
    public GameObject messagePrefab;          // Your TMP prefab
    public Transform contentTransform;        // Scroll View Content
    public ScrollRect scrollRect;

    // Pool to store reusable message GameObjects
    private readonly List<GameObject> messagePool = new List<GameObject>();

    /// <summary>
    /// Adds a message to the panel, reusing pooled objects if available.
    /// </summary>
    public void AddMessage(string message, float typingDelay = 0.05f)
    {
        GameObject newMessage;

        if (messagePool.Count > 0)
        {
            newMessage = messagePool[messagePool.Count - 1];
            messagePool.RemoveAt(messagePool.Count - 1);
            newMessage.SetActive(true);
        }
        else
        {
            newMessage = Instantiate(messagePrefab, contentTransform);
        }

        TextMeshProUGUI tmpText = newMessage.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            // Start typing coroutine
            StartCoroutine(TypeText(tmpText, message, typingDelay));
        }

        // Update layout and scroll to bottom (consider updating at the end of typing as well)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator TypeText(TextMeshProUGUI tmpText, string message, float delay = 0.05f)
    {
        tmpText.text = "";

        for (int i = 0; i <= message.Length; i++)
        {
            tmpText.text = message.Substring(0, i);

            yield return null;  // Wait a frame for layout updates

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform as RectTransform);
            scrollRect.verticalNormalizedPosition = 0f;

            yield return new WaitForSeconds(delay);
        }
    }

    /// <summary>
    /// Clears all messages by disabling and pooling them for reuse.
    /// </summary>
    public void ClearMessagePanel()
    {
        // Disable all message children and add them back to the pool
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            GameObject messageObj = contentTransform.GetChild(i).gameObject;
            messageObj.SetActive(false);
            messagePool.Add(messageObj);
        }
    }

    /// <summary>
    /// Optional: Clear the pool and destroy pooled objects (if you want to free memory).
    /// </summary>
    public void ClearPool()
    {
        foreach (var pooledObj in messagePool)
        {
            Destroy(pooledObj);
        }
        messagePool.Clear();
    }
}

