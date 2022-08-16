using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;

public class Steam : MonoBehaviour
{
    //Start is called before the first frame update
    void Start()
    {
        GetSteamDevAccessData();

    }

    public static void UpdateRichPresence()
    {
        

        switch (GameSettings.Instance.ActiveScene)
        {
            case SCENE.ROOM:

                SteamFriends.SetRichPresence("steam_display", "#AtHome");

                break;
            case SCENE.HOMESCREEN:

                SteamFriends.SetRichPresence("steam_display", "#MainScreen");
                break;
            case SCENE.LEVEL0:

                SteamFriends.SetRichPresence("roamingstatus", "Level 0");
                SteamFriends.SetRichPresence("steam_display", "#Roaming");

                break;
            case SCENE.LEVEL1:

                SteamFriends.SetRichPresence("roamingstatus", "Level 1");
                SteamFriends.SetRichPresence("steam_display", "#Roaming");

                break;
            case SCENE.LEVEL2:

                SteamFriends.SetRichPresence("roamingstatus", "Level 2");
                SteamFriends.SetRichPresence("steam_display", "#Roaming");

                break;
            case SCENE.FOURKEYS_CLIPPINGZONE:

                SteamFriends.SetRichPresence("steam_display", "Lost in the Clipping Zones");

                break;

            case SCENE.LEVELFUN:

                SteamFriends.SetRichPresence("roamingstatus", "Level =)");
                break;

            case SCENE.LEVELRUN:

                SteamFriends.SetRichPresence("roamingstatus", "Level !");
                break;
        }


        
    }
    public static void IncrementStat(string statName, float amount)
    {
        float currentStat;
        SteamUserStats.GetStat(statName, out currentStat);
        SteamUserStats.SetStat(statName, currentStat + amount);
        SteamUserStats.StoreStats();
    }
    public static void IncrementStat(string statName, int amount)
    {
        int currentStat;
        SteamUserStats.GetStat(statName, out currentStat);
        SteamUserStats.SetStat(statName, currentStat + amount);
        SteamUserStats.StoreStats();
    }
    public static void DecrementStat(string statName, float amount)
    {
        float currentStat;
        SteamUserStats.GetStat(statName, out currentStat);
        SteamUserStats.SetStat(statName, currentStat - amount);
        SteamUserStats.StoreStats();
    }

    public static void DecrementStat(string statName, int amount)
    {
        int currentStat;
        SteamUserStats.GetStat(statName, out currentStat);
        SteamUserStats.SetStat(statName, currentStat - amount);
        SteamUserStats.StoreStats();

    }
    public static void AddAchievment(string achievment)
    {
        bool ahcieved;

        SteamUserStats.GetAchievement(achievment, out ahcieved);

        if (!ahcieved)
        {
            if (SteamUserStats.SetAchievement(achievment))
                Debug.Log("Awarded Achievement!" + " " + achievment);
            else
            {
                Debug.Log("Failed To Award Achievement!" + " " + achievment);
            }
            SteamUserStats.StoreStats();
        }
        

    }

    public static void RevokeAchievment(string achievment)
    {
        bool ahcieved;

        SteamUserStats.GetAchievement(achievment, out ahcieved);

        if (!ahcieved)
        {
            SteamUserStats.ClearAchievement(achievment);
            SteamUserStats.StoreStats();
        }
    }
    
    public static void ClearAllSteamStats()
    {
        if (SteamUserStats.ResetAllStats(true))
        {
            Debug.Log("Successfully Reset Stats");
        }
        //SteamUserStats.StoreStats();
    }

    static void GetSteamDevAccessData()
    {
    #if !DISABLESTEAMWORKS

        if (!SteamManager.Initialized)
        {
            GameSettings.Instance.activeUser = "Guest";
        }
        else
        {
            ulong name = SteamUser.GetSteamID().m_SteamID;

            GameSettings.Instance.activeUser = name.ToString();

            if (GameSettings.teamMemberSteamIDs.Contains(name))
            {
                GameSettings.Instance.teamMemberMode.gameObject.SetActive(true);
                GameSettings.Instance.teamMemberMode.text = GameSettings.Instance.teamMemberMode.text + " " + SteamFriends.GetPersonaName();
                
            }
        }

    #else
        GameSettings.Instance.activeUser = "Guest";

    #endif
    }


}
