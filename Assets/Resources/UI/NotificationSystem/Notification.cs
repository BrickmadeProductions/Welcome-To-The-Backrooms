using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public TextMeshProUGUI description;
    
    public void SetDesc(string desc)
    {
        description.text = desc;
    }
}
