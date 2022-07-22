using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLoading : MonoBehaviour
{
    public RectTransform bar;
    float currentRotation = 0;
    // Update is called once per frame
    void Update()
    {
        currentRotation+=10f;
        bar.transform.rotation = new Quaternion(0,0, currentRotation * Time.deltaTime, 0);
    }
}
