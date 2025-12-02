using System.Collections.Generic;
using UnityEngine;

public class LinePool : MonoBehaviour
{
    public Line prefab;
    public int initialSize = 5;

    private Queue<Line> pool = new Queue<Line>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public Line Get()
    {
        Line obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }

        obj.ResetLine();
        return obj;
    }

    public void Return(Line obj)
    {
        obj.ResetLine();
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
