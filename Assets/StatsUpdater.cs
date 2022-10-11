using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI playerWalkedStats;
    public TextMeshProUGUI globalWalkedStats;

    void Awake()
    {
		StartCoroutine(SetTotalDistanceTraveldMeters());
	}

    public IEnumerator SetTotalDistanceTraveldMeters()
	{
		yield return new WaitUntil(() => SteamManager.Initialized);

		while (true)
		{
			float globalDistanceStatTotal;
			float playerDistanceStatTotal;

			SteamUserStats.RequestCurrentStats();

			yield return SteamUserStats.RequestGlobalStats(0);

			SteamUserStats.GetStat("GLOBAL_DISTANCE_TRAVELED_METERS", out globalDistanceStatTotal);
			globalWalkedStats.text = "PLAYERBASE DISTANCE TRAVELED: " + globalDistanceStatTotal + " M";

			SteamUserStats.GetStat("DISTANCE_TRAVELED_METERS", out playerDistanceStatTotal);
			playerWalkedStats.text = "YOUR DISTANCE TRAVELED: " + (int)playerDistanceStatTotal + " M";

			yield return new WaitForSecondsRealtime(1f);
		}

	}
}
