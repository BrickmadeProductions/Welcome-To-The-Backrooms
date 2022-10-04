using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int id;

	public Vector2Int tilePos;

	public List<Transform> entitySpawnLocations;

	public List<ItemSpawner> itemSpawnLocations;

	public void SpawnPresetItems()
    {
		foreach (ItemSpawner spawner in itemSpawnLocations)
		{
			spawner.SpawnItem();
		}
	}
}
