using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaButton : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.01f;
    }
}
