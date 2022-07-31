using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingBehavior : MonoBehaviour
{

    public GameObject light;
    public GameObject vent;
    public Transform[] ventLocations;
    public Transform[] lightLocations;
    // Start is called before the first frame update
    void Start()
    {
        //determine the type
        int index = 0;

        foreach (Transform lightT in lightLocations)
        {
            if (Random.value <= 0.35f)
            {
                if (Random.value <= 0.7f)
                {
                    lightLocations[index].gameObject.SetActive(true);
                }

                else
                {
                    //broken
                    lightLocations[index].gameObject.SetActive(true);
                }
            }
           
            index++;
        }

        //determine where
        index = 0;

        foreach (Transform ventT in ventLocations)
        {
            if (Random.value <= 0.2f)
            {
                if (Random.value <= 0.5f)

                    ventLocations[index].gameObject.SetActive(true);
            }
            index++;
        }
        

    }

    
}
