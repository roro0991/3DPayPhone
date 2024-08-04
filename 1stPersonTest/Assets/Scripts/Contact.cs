using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Contact : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI contactName;
    [SerializeField] public TextMeshProUGUI city;
    [SerializeField] public TextMeshProUGUI number;

    
    public void ClearAllInfo()
    {
        contactName.text = string.Empty;
        city.text = string.Empty;
        number.text = string.Empty;
    }
}
