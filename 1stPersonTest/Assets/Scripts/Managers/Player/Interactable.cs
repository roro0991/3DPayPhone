using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    public UnityEvent unityEvent = new UnityEvent();
    
    public void Interact()
    {
        unityEvent.Invoke(); 
    }
}
