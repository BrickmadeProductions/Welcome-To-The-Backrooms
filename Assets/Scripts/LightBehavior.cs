using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBehavior : MonoBehaviour
{

    public GameObject normalLight;
    public GameObject brokenLight;
    // Start is called before the first frame update
    void Start()
    {
        float value1 = Random.value;
        //determine if a light should spawn
        if (value1 <= 0.5f)
        {
            float value2 = Random.value;
            if (value2 <= 0.8f)
            {
                Instantiate(normalLight, transform.parent);
            }
            else
            {
                Instantiate(brokenLight, transform.parent);
            }
        }
        
    }

    
}
