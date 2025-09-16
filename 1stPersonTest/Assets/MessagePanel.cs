using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Import TMP namespace

public class MessagePanel : MonoBehaviour
{
    public GameObject messagePrefab;          // Your TMP prefab
    public Transform contentTransform;        // Scroll View Content
    public ScrollRect scrollRect;

    public void AddMessage(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, contentTransform);
        TextMeshProUGUI tmpText = newMessage.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = message;
        }

        // Update layout and scroll to bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }
}
