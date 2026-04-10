using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    public TextMeshPro notePrefab;
    public int initialSize = 10;

    private Queue<TextMeshPro> pool = new Queue<TextMeshPro>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(notePrefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public TextMeshPro Get()
    {
        TextMeshPro obj;
        if (pool.Count == 0)
        {
            obj = Instantiate(notePrefab);
        }
        else
        {
            obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
        }

        // Reset text and any other state
        obj.text = "";
        obj.rectTransform.localPosition = Vector3.zero;
        obj.rectTransform.localRotation = Quaternion.identity;
        obj.rectTransform.localScale = Vector3.one;

        return obj;
    }

    public void Return(TextMeshPro obj)
    {
        obj.gameObject.SetActive(false);
        obj.text = "";
        pool.Enqueue(obj);
    }
}

