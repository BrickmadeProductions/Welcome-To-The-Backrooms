using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ObjectWithWeight
{
    public OBJECT_TYPE type;
    public int weight;
}
[System.Serializable]
public struct EntityWithWeight
{
    public ENTITY_TYPE type;
    public int weight;
}
public class WeightedRandom
{

    public static GameObject ReturnEntityBySpawnChances(List<EntityWithWeight> spawnableEntities)
    {
        int[] weights = new int[spawnableEntities.Count];

        for (int i = 0; i < spawnableEntities.Count; i++)
        {
            weights[i] = spawnableEntities[i].weight;
        }

        int randomWeight = Random.Range(0, weights.Sum());

        for (int i = 0; i < weights.Length; ++i)
        {
            randomWeight -= weights[i];

            if (randomWeight < 0)
            {
                return GameSettings.Instance.EntityDatabase[spawnableEntities[i].type].gameObject;
            }
        }

        return GameSettings.Instance.EntityDatabase[ENTITY_TYPE.PARTYGOER].gameObject;
    }

    public static GameObject ReturnItemBySpawnChances(List<ObjectWithWeight> spawnableItems)
    {
        int[] weights = new int[spawnableItems.Count];

        for (int i = 0; i < spawnableItems.Count; i++)
        {
            weights[i] = spawnableItems[i].weight;
        }

        int randomWeight = Random.Range(0, weights.Sum());

        for (int i = 0; i < weights.Length; ++i)
        {
            randomWeight -= weights[i];

            if (randomWeight < 0)
            {
                return GameSettings.Instance.PropDatabase[spawnableItems[i].type].gameObject;
            }
        }

        return GameSettings.Instance.PropDatabase[OBJECT_TYPE.CHAIR].gameObject;
    }
}
