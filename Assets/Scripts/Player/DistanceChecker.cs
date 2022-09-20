using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DistanceChecker : MonoBehaviour
{
    public float GetAverageSpeed() { return distanceTraveled / timePassed; }

    public float distanceTraveled = 0f;

    int metersTraveledTotal = 0;
    int metersTraveledBeforeStatChange = 0;
    public TextMeshProUGUI metersTraveledText;

    float timePassed = 0f;

    public Transform tx;
    Vector3 lastPosition;

   

    void Start()
    {
        lastPosition = tx.position;
    }
    void FixedUpdate()
    {
        if (metersTraveledTotal >= 1)
        {
            Steam.AddAchievment("WALK_1");
        }
        if (metersTraveledTotal >= 5000)
        {
            Steam.AddAchievment("WALK_5000");
        }
        if (metersTraveledTotal >= 10000)
        {
            Steam.AddAchievment("WALK_10000");
        }
        if (GameSettings.LEVEL_LOADED && GameSettings.Instance.ActiveScene != SCENE.ROOM && GameSettings.Instance.Player.GetComponent<Rigidbody>().velocity.magnitude < 20f)
        {
            //Debug.Log((lastPosition - tx.position).magnitude);
            distanceTraveled += (lastPosition - tx.position).magnitude;
            timePassed += Time.deltaTime;
            lastPosition = tx.position;

            metersTraveledBeforeStatChange += ((int)((lastPosition - tx.position).magnitude) / 3);
            metersTraveledTotal = ((int)distanceTraveled / 3);

            metersTraveledText.text = metersTraveledTotal + " M";
        }
        
    }

    public int GetMetersTraveledTotal()
    {
        return metersTraveledTotal;
    }
    public int SetMetersTraveledStats()
    {
        int traveled = metersTraveledBeforeStatChange;

        Steam.IncrementStat("DISTANCE_TRAVELED_METERS", traveled);
        Steam.IncrementStat("GLOBAL_DISTANCE_TRAVELED_METERS", traveled);

        metersTraveledBeforeStatChange = 0;

        return traveled;
    }
}