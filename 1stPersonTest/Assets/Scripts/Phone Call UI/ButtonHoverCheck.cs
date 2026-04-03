using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private IInteractable interactable;

    private void Awake()
    {
        TryGetComponent(out interactable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        interactable?.OnHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        interactable?.OnHoverExit();
    }
}
