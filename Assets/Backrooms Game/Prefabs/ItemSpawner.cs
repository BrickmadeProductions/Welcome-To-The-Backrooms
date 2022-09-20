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


    public void Start()
    {
        StartCoroutine(WaitUntilInValidChunk());
    }

    //spawn once in a valid chunk
    public IEnumerator WaitUntilInValidChunk()
    {
        while (GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position) == null && !GameSettings.LEVEL_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.LEVEL_GENERATED)
        {
            yield return null;
        }

        GameObject objectToSpawn = random ? WeightedRandom.ReturnItemBySpawnChances(GameSettings.Instance.worldInstance.worldPropSpawnTable) : GameSettings.Instance.PropDatabase[typeToSpawn].gameObject;

        //random prop in world table
        if (random)
        {
            GameSettings.Instance.worldInstance.AddNewProp(transform.position, Quaternion.Euler(transform.localRotation.x, UnityEngine.Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z), objectToSpawn);
        }
        //typeToSpawn preset
        else
        {
            GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn);
        }
    }

}