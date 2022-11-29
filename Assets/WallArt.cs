using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallArt : MonoBehaviour
{
    public Material[] possibleArts;
    public GameObject artObject;

    // Start is called before the first frame update
    void Awake()
    {
        //Random.InitState(GameSettings.Instance.worldInstance.worldDataSeed);

        if (Random.Range(0f, 1f) < 0.04f)
        {
            GameObject art = Instantiate(artObject, transform);
            art.GetComponent<Renderer>().material = possibleArts[Random.Range(0, possibleArts.Length)];
        }
           
    }

}
