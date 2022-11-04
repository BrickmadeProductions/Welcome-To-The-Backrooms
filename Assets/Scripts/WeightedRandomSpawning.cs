using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ObjectSpawnData
{
    public OBJECT_TYPE type;
    public List<BIOME_ID> biomesSpawnsIn;
    public int weight;

}
[System.Serializable]
public struct EntitySpawnData
{
    public ENTITY_TYPE type;
    public List<BIOME_ID> biomesSpawnsIn;
    public int weight;
    
}

public class WeightedRandomSpawning
{
    public static int ReturnWeightedTileIDBySpawnChance(List<Tile> tiles)
    {
        int[] weights = new int[tiles.Count];

        for (int i = 0; i < tiles.Count; i++)
        {
            weights[i] = tiles[i].SpawnWeight;
        }

        int random = Random.Range(0, weights.Sum());

        Tile tileToSpawn = tiles.First(i => (random -= i.SpawnWeight) < 0);

        if (tileToSpawn.GetComponent<StoryTile>() != null)
        {
            if (GameSettings.Instance.worldInstance.storyTilesFoundInThisWorld[tileToSpawn.GetComponent<StoryTile>().type] == true)
            {
                tiles.Remove(tileToSpawn);

                tileToSpawn = tiles.First(i => (random -= i.SpawnWeight) < 0);

                return tileToSpawn.id;
            }
        }

        return tileToSpawn.id;

       /* int randomWeight = Random.Range(0, weights.Sum());

        for (int i = 0; i < weights.Length; ++i)
        {
            randomWeight -= weights[i];

            if (randomWeight < 0)
            {
                return tiles[i].id;
            }
        }

        return 0;*/
    }
    public static GameObject ReturnEntityBySpawnChances(List<EntitySpawnData> spawnableEntities)
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

    public static GameObject ReturnItemBySpawnChances(List<ObjectSpawnData> spawnableItems)
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
