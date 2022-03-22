using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public float MoveAmount;
 
    public float MoveSpeed;
 
    float MoveOnX;
 
    float MoveOnY;
     
    Vector3 DefaultPos;
 
    Vector3 NewPos;

    private void Awake()
    {
        DefaultPos = Vector3.zero;
    }
   
   
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Mathf.Sin(Time.realtimeSinceStartup * 0.05f) * MoveAmount, Mathf.Sin(Time.realtimeSinceStartup *  0.015f) * MoveAmount / 8f);



    }
}
