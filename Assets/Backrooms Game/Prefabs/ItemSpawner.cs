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
    public float randomSpawnChance = 0.3f;


    /*public void Start()
    {
        if (spawnInstantly)
            StartCoroutine(SpawnWhenInValidChunk());
    }*/

    public InteractableObject SpawnItem()
    {
        List<ObjectSpawnData> itemsThatCanSpawnHere = new List<ObjectSpawnData>();

        foreach (ObjectSpawnData data in GameSettings.Instance.worldInstance.worldPropSpawnTable)
        {
            if (data.biomesSpawnsIn.Contains(GetComponentInParent<Tile>().biomeID))
            {
                itemsThatCanSpawnHere.Add(data);
            }
        }

        GameObject objectToSpawn = random ? WeightedRandomSpawning.ReturnItemBySpawnChances(itemsThatCanSpawnHere) : GameSettings.Instance.PropDatabase[typeToSpawn].gameObject;

        Vector3 spawnLocationKey = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position);
        //random prop in world table
        if (random)
        {
            if (spawnOnce)
            {

                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z) && UnityEngine.Random.Range(0f, 1f) < randomSpawnChance)
                {
                    
                    return GameSettings.Instance.worldInstance.AddNewProp(transform.position, new Quaternion(objectToSpawn.transform.rotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), objectToSpawn.transform.rotation.z, 1), objectToSpawn);
                }
            }

            else if (UnityEngine.Random.Range(0f, 1f) < randomSpawnChance)
            {
                return GameSettings.Instance.worldInstance.AddNewProp(transform.position, new Quaternion(objectToSpawn.transform.rotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), objectToSpawn.transform.rotation.z, 1), objectToSpawn);
            }
        }
        //typeToSpawn preset
        else
        {
            if (spawnOnce)
            {
                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z))
                {

                    return GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
                }
            }
            else
            {
                return GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
            }
        }

        return null;
    }

/*    public IEnumerator SpawnWhenInValidChunk()
    {
        *//*//spawn once in a valid chunk
        while (GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position) == null && !GameSettings.LEVEL_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.LEVEL_GENERATED)
        {
            Debug.Log("Waiting for valid chunk for ITEM");
            yield return null;
        }*//*
        
        GameObject objectToSpawn = random ? WeightedRandom.ReturnItemBySpawnChances(GameSettings.Instance.worldInstance.worldPropSpawnTable) : GameSettings.Instance.PropDatabase[typeToSpawn].gameObject;

        Vector3 spawnLocationKey = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position);
        //random prop in world table
        if (random)
        {
            if (spawnOnce)
            {
                
                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z) && UnityEngine.Random.Range(0f, 1f) > randomSpawnChance)
                {
                    Debug.Log("SPAWNING RANDOM ITEM, CHUNK NEVER LOADED BEFORE");
                    GameSettings.Instance.worldInstance.AddNewProp(transform.position, new Quaternion(transform.localEulerAngles.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z, 1), objectToSpawn);
                }
            }

            else if (UnityEngine.Random.Range(0f, 1f) > randomSpawnChance)
            {
                GameSettings.Instance.worldInstance.AddNewProp(transform.position, new Quaternion(transform.localRotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z, 1), objectToSpawn);
            }
        }
        //typeToSpawn preset
        else
        {
            if (spawnOnce)
            {
                if (!GameSettings.Instance.worldInstance.allChunks.ContainsKey(spawnLocationKey.x + "," + spawnLocationKey.y + "," + spawnLocationKey.z))
                {
                    Debug.Log("SPAWNING PRESET ITEM, CHUNK NEVER LOADED BEFORE");
                    GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
                }
            }
            else
            {
                GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
            }
        }
    }*/
}