using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onHoverEnter = new UnityEvent();
    public UnityEvent onHoverExit = new UnityEvent();
    
    public void Interact()
    {
        onClick.Invoke();         
    }

    public void OnHoverEnter()
    {
        onHoverEnter.Invoke();
    }

    public void OnHoverExit()
    {
        onHoverExit.Invoke();
    }
}
