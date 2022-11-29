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

    int oldDistanceTravled;
    int newDistanceTravled;

    public TextMeshProUGUI metersTraveledText;

    float timePassed = 0f;

    public Transform tx;
    public Vector3 lastPosition;


    void FixedUpdate()
    {

        if (GameSettings.PLAYER_DATA_LOADED && GameSettings.SCENE_LOADED && SHOULD_READ_STEPS)
        {
            oldDistanceTravled = (int)(distanceTraveled / 3f);

            distanceTraveled += (lastPosition - tx.position).magnitude;

            newDistanceTravled = (int)(distanceTraveled / 3f);

            metersTraveledText.text = (int)(distanceTraveled / 3f) + " M";

            timePassed += Time.deltaTime;
            lastPosition = tx.position;

            if (oldDistanceTravled < newDistanceTravled)
            {
                Steam.IncrementStat("DISTANCE_TRAVELED_METERS", newDistanceTravled - oldDistanceTravled);
            }
            
        }
    }

    public void LoadInData(PlayerSaveData saveData)
    {
        distanceTraveled = saveData.distanceTraveledSaved;
    }

    
}