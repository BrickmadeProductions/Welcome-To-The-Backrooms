using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    #if !DISABLESTEAMWORKS

        if (!SteamManager.Initialized)
        {
            GameSettings.Instance.activeUser = "Guest";
        }
        else
        {
            CSteamID name = SteamUser.GetSteamID();

            GameSettings.Instance.activeUser = name.ToString();

        }

    #endif

        GameSettings.Instance.activeUser = "Guest";
    }
}
