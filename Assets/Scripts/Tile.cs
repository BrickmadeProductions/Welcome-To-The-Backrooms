using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	//tile id
    public int id;
	
	//biome this tile is assossiated with
	public BIOME_ID biomeID;

	public Vector2Int tilePos;

	public List<Transform> entitySpawnLocations;

	public List<ItemSpawner> itemSpawnLocations;

	public int SpawnWeight;

	public void SpawnPresetItems()
    {
		foreach (ItemSpawner spawner in itemSpawnLocations)
		{
			spawner.SpawnItem();
		}
	}
}
