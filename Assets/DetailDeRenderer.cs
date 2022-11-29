using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailDeRenderer : MonoBehaviour
{
    public float distance  = 1000f;
    public List<GameObject> objects;
    void Awake()
    {
        StartCoroutine(CheckPlayerDistance());
    }

    IEnumerator CheckPlayerDistance()
    {
        while (true)
        {
            foreach (GameObject obj in objects)

                if (Vector3.Distance(GameSettings.GetLocalPlayer().transform.position, obj.transform.position) > distance)
                {

                    obj.SetActive(false);
                    yield return new WaitUntil(() => Vector3.Distance(GameSettings.GetLocalPlayer().transform.position, obj.transform.position) <= distance);

                }
                else
                {
                     obj.SetActive(true);
                     yield return new WaitUntil(() => Vector3.Distance(GameSettings.GetLocalPlayer().transform.position, obj.transform.position) > distance);
                }

            yield return new WaitForSecondsRealtime(0.25f);


        }
    }

}
