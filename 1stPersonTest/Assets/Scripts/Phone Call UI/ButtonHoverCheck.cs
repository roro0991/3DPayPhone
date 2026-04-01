using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        this.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable);

        interactable.Interact();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable);

        interactable.Interact();
    }
}
