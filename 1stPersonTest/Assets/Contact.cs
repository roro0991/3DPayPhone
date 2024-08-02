using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Contact : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI city;
    [SerializeField] private TextMeshProUGUI number;

    TextMeshProUGUI[] info = new TextMeshProUGUI[3];
    

    public void ClearAllInfo()
    {
        name.text = string.Empty;
        city.text = string.Empty;
        number.text = string.Empty;
    }
}
