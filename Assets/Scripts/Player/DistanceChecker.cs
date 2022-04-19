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

        if (GameSettings.LEVEL_LOADED && SceneManager.GetActiveScene().name != "RoomHomeScreen")
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