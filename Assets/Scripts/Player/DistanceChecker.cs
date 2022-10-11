using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DistanceChecker : MonoBehaviour
{
    public static bool SHOULD_READ_STEPS = false;
    public float GetAverageSpeed() { return distanceTraveled / timePassed; }

    public float distanceTraveled = 0f;

    float metersTraveledBeforeStatChange = 0;
    public TextMeshProUGUI metersTraveledText;

    float timePassed = 0f;

    public Transform tx;
    public Vector3 lastPosition;

   
    void FixedUpdate()
    {
        
        if (GameSettings.PLAYER_DATA_LOADED && GameSettings.SCENE_LOADED && SHOULD_READ_STEPS)
        {
            //Debug.Log((lastPosition - tx.position).magnitude);
            float increasedDistance = (lastPosition - tx.position).magnitude;

            distanceTraveled += increasedDistance;
            timePassed += Time.deltaTime;
            lastPosition = tx.position;

            metersTraveledBeforeStatChange += increasedDistance / 3;

            metersTraveledText.text = (int)(distanceTraveled / 3) + " M";
        }
        
    }

    public float SetMetersTraveledStats()
    {
        float traveled = metersTraveledBeforeStatChange;

        Steam.IncrementStat("DISTANCE_TRAVELED_METERS", traveled);
        Steam.IncrementStat("GLOBAL_DISTANCE_TRAVELED_METERS", traveled);

        metersTraveledBeforeStatChange = 0;

        return traveled;
    }

    public void LoadInData(PlayerSaveData saveData)
    {
        distanceTraveled = saveData.distanceTraveledSaved;
    }

    
}