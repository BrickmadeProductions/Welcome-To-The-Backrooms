using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ItemSpawner : MonoBehaviour
{
    public OBJECT_TYPE typeToSpawn;

    //go off world loot table if random
    public bool random;
    public bool spawnOnce;
    public bool spawnInstantly;


    public void Start()
    {
        if (spawnInstantly)
            StartCoroutine(SpawnWhenInValidChunk());
    }

    public void SpawnItem()
    {
        StartCoroutine(SpawnWhenInValidChunk());
    }

    public IEnumerator SpawnWhenInValidChunk()
    {
        //spawn once in a valid chunk
        while (GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position) == null && !GameSettings.LEVEL_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.LEVEL_GENERATED)
        {
            yield return null;
        }
        
        GameObject objectToSpawn = random ? WeightedRandom.ReturnItemBySpawnChances(GameSettings.Instance.worldInstance.worldPropSpawnTable) : GameSettings.Instance.PropDatabase[typeToSpawn].gameObject;

        Vector3 spawnLocationKey = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position);
        //random prop in world table
        if (random)
        {
            

            if (spawnOnce)
            {
                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z))
                {
                    GameSettings.Instance.worldInstance.AddNewProp(transform.position, Quaternion.Euler(transform.localRotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z), objectToSpawn);
                }
            }
            
            else
            {
                GameSettings.Instance.worldInstance.AddNewProp(transform.position, Quaternion.Euler(transform.localRotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z), objectToSpawn);
            }
           
        }
        //typeToSpawn preset
        else
        {
            if (spawnOnce)
            {
                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z))
                {
                    GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
                }
            }
            else
            {
                GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
            }
        }
    }

}