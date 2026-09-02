using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MessagePanel : MonoBehaviour
{
    [Header("References")]         // Your TMP prefab
    public Transform contentTransform;        // Scroll View Content
    public TextMeshProUGUI playerInput;
    public TextMeshProUGUI NPCResponse;    

    /// <summary>
    /// Adds a message to the panel, reusing pooled objects if available.
    /// </summary>
    public void AddMessage(string message, float typingDelay = 0.05f)
    {
        if (playerInput.text != "")        
            playerInput.text = "";
       

        if (NPCResponse.text != "")
            NPCResponse.text = "";

        if (message != "You: ")                   
            StartCoroutine(TypeText(playerInput, message, typingDelay));
              
    }

    public void AddResponse(string response, float typingDelay = 0.05f)
    {
        if (NPCResponse.text != "")        
            NPCResponse.text = "";

        if (response != ": ")    
            StartCoroutine(TypeText(NPCResponse, response, typingDelay));       
    }

    private IEnumerator TypeText(TextMeshProUGUI tmpText, string message, float delay = 0.05f)
    {
        tmpText.text = "";

        for (int i = 0; i <= message.Length; i++)
        {
            tmpText.text = message.Substring(0, i);

            yield return null;  // Wait a frame for layout updates

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform as RectTransform);            

            yield return new WaitForSeconds(delay);
        }
    }

    /// <summary>
    /// Clears all messages by disabling and pooling them for reuse.
    /// </summary>
    public void ClearMessagePanel()
    {
        if (playerInput.text != "")
            playerInput.text = "";

        if (NPCResponse.text != "")
            NPCResponse.text = "";
    }

}

