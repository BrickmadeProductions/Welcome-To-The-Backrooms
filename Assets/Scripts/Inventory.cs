using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public HoldableObject[] inventory;

    // Start is called before the first frame update
    void Awake()
    {
        inventory = new HoldableObject[10];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
