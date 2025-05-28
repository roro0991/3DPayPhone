
using UnityEngine;
using UnityEngine.EventSystems;

interface IInteractable
{
    public void Interact();
}
public class InteractionManager : MonoBehaviour
{
    [SerializeField] Animator cameraAnimator;
    [SerializeField] Animator doorAnimator;
    [SerializeField] Camera equiprenderCam;
    [SerializeField] StoryManager storyManager;

    private int _layerNumber = 6;
    private int _layerMask;
    private bool _isDoorOpenedFirstTime = false;

    private void Start()
    {
        _layerMask = 1 << _layerNumber;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))//Check if player left clicked on mouse
        {
            if (EventSystem.current.IsPointerOverGameObject())//Check if player clicked on canvas UI element
            {
                return;
            }

            //Raycast originating from main camera.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            //Raycast originating equiped item render camera (upReceiver & notebook)
            Ray ray2 = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
                     
            //check if raycast hit equiprenderObj
            if (Physics.Raycast(ray2, out hit2, Mathf.Infinity, _layerMask))
            {
                if (hit2.collider.gameObject.TryGetComponent(out IInteractable interactObj2))
                {                    
                    interactObj2.Interact();
                }
                Debug.Log("you hit a rendercam obj");
            }            
            //check if raycast hit from maincam
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~_layerMask)) 
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.Interact();
                }
                Debug.Log("you hit world obj");
            }
            else
            {
                return;
            }            
        }
    }

    public void CameraLeanLeft()
    {
        if (cameraAnimator.GetBool("isLeaningLeft") == false)
        {
            cameraAnimator.SetBool("isLeaningLeft", true);
        }
        else
        {
            cameraAnimator.SetBool("isLeaningLeft", false);
        }
    }

    public void CameraLeanRight()
    {
        if (cameraAnimator.GetBool("isLeaningRight") == false)
        {
            cameraAnimator.SetBool("isLeaningRight", true);
        }
        else
        {
            cameraAnimator.SetBool("isLeaningRight", false);
        }
    }

    public void CameraTurnRight()
    {
        if (cameraAnimator.GetInteger("CameraPosition") < 3)
        {
            cameraAnimator.SetInteger("CameraPosition", 
            cameraAnimator.GetInteger("CameraPosition") + 1); 
        }
        else
        {
            cameraAnimator.SetInteger("CameraPosition", 0);
        }        
    }

    public void CameraTurnLeft()
    {
        if (cameraAnimator.GetInteger("CameraPosition") > 0)
        {
            cameraAnimator.SetInteger("CameraPosition", 
            cameraAnimator.GetInteger("CameraPosition") - 1);
        }
        else
        {
            cameraAnimator.SetInteger("CameraPosition", 3);
        }
    }

    public void OpenCloseDoor()
    {
        if (cameraAnimator.GetInteger("CameraPosition") != 2)
        {
            return;
        }
        else
        {
            if (doorAnimator.GetBool("isClosed") == true)
            {
                if (_isDoorOpenedFirstTime == false)
                {
                    _isDoorOpenedFirstTime = true;
                    storyManager.SetFirstDoorOpen(true); 
                }
                doorAnimator.SetBool("isClosed", false);
            }
            else if (doorAnimator.GetBool("isClosed") == false)
            {
                doorAnimator.SetBool("isClosed", true);
            }
        }
    }      
}
