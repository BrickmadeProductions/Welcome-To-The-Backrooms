using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public delegate void BiomeEdgeHandler();
	public BiomeEdgeHandler biomeEdgeHandler;

	public void EdgeHandler()
	{
		biomeEdgeHandler?.Invoke();
	}

	//tile id
	public int id;
	
	//biome this tile is assossiated with
	public BIOME_ID biomeID;

	public Vector2Int tilePos;

	public List<Transform> entitySpawnLocations;

	public List<ItemSpawner> itemSpawnLocations;

	public int SpawnWeight;

	/// <summary>
	/// Use this to be able to check if spawned in items are new or from the save
	/// </summary>

	public delegate void SpawnItems();
	public SpawnItems spawnItems;

	private void Start()
	{
		StartCoroutine(WaitForEdgeDetection());
	}
	IEnumerator WaitForEdgeDetection()
    {
		yield return new WaitUntil(() => GetComponentInParent<Chunk>().ALL_TILES_GENERATED);

		if (biomeID == BIOME_ID.LEVEL_0_TALL_ROOMS)
		{
			EdgeHandler();

		}
	}
    public void SpawnPresetItems()
    {
		spawnItems?.Invoke();

		foreach (ItemSpawner spawner in itemSpawnLocations)
		{
			if (spawner.spawnInstantly)
				spawner.SpawnItem();
		}
	}
}
