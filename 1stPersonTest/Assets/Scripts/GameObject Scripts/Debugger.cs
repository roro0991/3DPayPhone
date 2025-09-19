using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    GraphicRaycaster raycaster;

    private void Awake()
    {
        raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            raycaster.Raycast(pointerData, results);

            foreach (var result in results)
            {
                Debug.Log("Hit: " + result.gameObject.name);
            }
        }
    }
}
