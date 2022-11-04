using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STORY_TILE
{
    JAS
}

public class StoryTile : MonoBehaviour
{
    public STORY_TILE type;


    private void Awake()
    {
        GetComponent<Tile>().spawnItems += SetupSpawnedItems;
        
    }

    public void SetupSpawnedItems()
    {
        switch (type)
        {
            case STORY_TILE.JAS:

                Vector3 spawnLocationKey = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(GetComponent<Tile>().itemSpawnLocations[0].transform.position);

                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z))
                {
                    
                    CassetPlayer player = (CassetPlayer)GetComponent<Tile>().itemSpawnLocations[0].SpawnItem();
                    player.SetMetaData("STORY_OBJECT", "JAS");
                    player.SetStat("Clip", "Jeremiah and Seth");
                    player.savedClips.Add(GameSettings.Instance.JASAudioData);

                }


                break;
        }

        
    }
}
