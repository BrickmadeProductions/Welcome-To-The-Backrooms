using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingBehavior : MonoBehaviour
{

    public GameObject normalLight;
    public GameObject brokenLight;
    public GameObject vent;
    public Transform[] ventLocations;
    public Transform[] lightLocations;
    // Start is called before the first frame update
    void Start()
    {
        float value1 = Random.value;
        //determine if a light should spawn
        if (value1 <= 0.6f)
        {
            //determine the type
            float value2 = Random.value;
            if (value2 <= 0.9f)
            {
                //determine where
                float value5 = Random.value;
                if (value5 <= 0.5f)
                {
                    Instantiate(normalLight, lightLocations[0].gameObject.transform);
                }
                else if (value5 > 0.5f && value5 < 1f)
                {
                    Instantiate(normalLight, lightLocations[1].gameObject.transform);
                }
            }
            else
            {
                //determine where
                float value6 = Random.value;
                if (value6 <= 0.5f)
                {
                    Instantiate(brokenLight, lightLocations[0].gameObject.transform);
                }
                else if (value6 > 0.5f && value6 < 1f)
                {
                    Instantiate(brokenLight, lightLocations[1].gameObject.transform);
                }
            }
        }
        float value3 = Random.value;
        //determine if a vent should spawn
        if (value3 <= 0.3f)
        {
            //determine where
            float value4 = Random.value;
            if (value4 <= 0.3f)
            {
                Instantiate(vent, ventLocations[0].gameObject.transform);
            }
            else if (value4 > 0.3f && value4 < 0.6f)
            {
                Instantiate(vent, ventLocations[1].gameObject.transform);
            }
            else if (value4 >= 0.6f)
            {
                Instantiate(vent, ventLocations[2].gameObject.transform);
            }
        }

    }

    
}
