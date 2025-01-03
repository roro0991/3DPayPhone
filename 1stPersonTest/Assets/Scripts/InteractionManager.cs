using Ink.Parsed;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

interface IInteractable
{
    public void Interact();
}
public class InteractionManager : MonoBehaviour
{
    [SerializeField] Animator cameraAnimator;
    [SerializeField] Animator doorAnimator;
    [SerializeField] Camera equiprenderCam;

    public LayerMask defaultlayerMask;
    public LayerMask equiplayerMask;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;                            
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Ray ray2 = equiprenderCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
            
            if (Physics.Raycast(ray, out hit, defaultlayerMask) && hit.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                interactObj.Interact();
            }
            
                        
            if (Physics.Raycast(ray2, out hit2, equiplayerMask) && hit2.collider.gameObject.TryGetComponent(out IInteractable interactObj2))
            {
                interactObj2.Interact();
            }
            
        }
    }

    public void CameraTurnRight()
    {
        if (cameraAnimator.GetInteger("CameraPosition") < 3)
        {
            cameraAnimator.SetInteger("CameraPosition", cameraAnimator.GetInteger("CameraPosition") + 1); 
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
            cameraAnimator.SetInteger("CameraPosition", cameraAnimator.GetInteger("CameraPosition") - 1);
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
                doorAnimator.SetBool("isClosed", false);
            }
            else if (doorAnimator.GetBool("isClosed") == false)
            {
                doorAnimator.SetBool("isClosed", true);
            }
        }
    }      
}
