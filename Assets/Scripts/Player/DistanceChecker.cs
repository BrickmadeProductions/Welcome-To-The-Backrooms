using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DistanceChecker : MonoBehaviour
{
    public int GetDistanceTraveled() { return getDistanceTraveled(); }
    public float GetAverageSpeed() { return distanceTraveled / timePassed; }

    public float distanceTraveled = 0f;

    int metersTraveled = 0;
    public TextMeshProUGUI metersTraveledText;

    float timePassed = 0f;

    public Transform tx;
    Vector3 lastPosition;

   

    void Start()
    {
        lastPosition = tx.position;
    }
    void Update()
    {
        if (metersTraveled >= 1)
        {
            Steam.AddAchievment("WALK_1");
        }
        if (metersTraveled >= 5000)
        {
            Steam.AddAchievment("WALK_5000");
        }
        if (metersTraveled >= 10000)
        {
            Steam.AddAchievment("WALK_10000");
        }

        if (GameSettings.LEVEL_LOADED && GameSettings.Instance.ActiveScene != GameSettings.SCENE.ROOM)
        {
            distanceTraveled += (lastPosition - tx.position).magnitude;
            timePassed += Time.deltaTime;
            lastPosition = tx.position;

            metersTraveled = ((int)distanceTraveled / 3);

            metersTraveledText.text = metersTraveled + " M";
        }
        
    }
    int getDistanceTraveled()
    {
        return metersTraveled;
    }
}