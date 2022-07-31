using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;


public class Steam : MonoBehaviour
{
    //Start is called before the first frame update
    void Start()
    {
        GetSteamDevAccessData();

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
