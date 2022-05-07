// Chunk
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct SerealizedChunk
{
	public EntityCluster entityData;

	public PropCluster propData;

	[NonSerialized]
	public Chunk instance;
}
public class Chunk : MonoBehaviour
{
	private BackroomsLevelWorld parentGenerator;

	public SerealizedChunk saveableData;

	public int chunkPosX;

	public int chunkPosZ;

	public int chunkPosY;

	private Dictionary<int, GameObject> tileset;

	private List<List<int>> noise_grid;

	public List<Tile> tile_grid;

	public float magnification;

	public float tileWidth;

	public float tileHeight;

	public int x_offset;

	public int z_offset;

	private int RoomId;

	public bool ALL_TILES_GENERATED;

	public bool ALL_OBJECTS_AND_ENTITES_LOADED;

	public void CreateChunk(int posX, int posY, int posZ, BackroomsLevelWorld parentGenerator, bool shouldGenerateInstantly)
	{
		saveableData = new SerealizedChunk
		{
			entityData = new EntityCluster
			{
				entityClusterData = new Dictionary<string, SaveableEntity>()
			},
			propData = new PropCluster
			{
				propClusterData = new Dictionary<string, SaveableProp>()
			},
			instance = this
		};

		this.parentGenerator = parentGenerator;

		chunkPosX = posX;
		chunkPosY = posY;
		chunkPosZ = posZ;

		CreateTileset(parentGenerator.Tiles);

		if (shouldGenerateInstantly)
		{
			GenerateRandomMap();
		}
		else
		{
			StartCoroutine(GenerateRandomMap(3));
		}
	}

	private void Awake()
	{
		noise_grid = new List<List<int>>();
		tile_grid = new List<Tile>();
	}

	internal void UpdateChunkData()
	{
	}

	private void CreateTileset(List<GameObject> tiles)
	{
		tileset = new Dictionary<int, GameObject>();
		for (int i = 0; i < tiles.Count; i++)
		{
			tileset.Add(i, tiles[i]);
		}
	}

	private IEnumerator GenerateRandomMap(int framesPerTile)
	{
		for (int x = 0; x < parentGenerator.chunk_width; x++)
		{
			noise_grid.Add(new List<int>());
			for (int z = 0; z < parentGenerator.chunk_width; z++)
			{
				for (int i = 0; i < framesPerTile; i++)
				{
					yield return new WaitForEndOfFrame();
				}
				int idUsingPerlin = GetIdUsingPerlin(x, z);
				noise_grid[x].Add(idUsingPerlin);
				CreateTile(idUsingPerlin, x, chunkPosY, z);
			}
		}
		ALL_TILES_GENERATED = true;
	}

	private void GenerateRandomMap()
	{
		for (int i = 0; i < parentGenerator.chunk_width; i++)
		{
			noise_grid.Add(new List<int>());
			for (int j = 0; j < parentGenerator.chunk_width; j++)
			{
				int idUsingPerlin = GetIdUsingPerlin(i, j);
				noise_grid[i].Add(idUsingPerlin);
				CreateTile(idUsingPerlin, i, chunkPosY, j);
			}
		}
		ALL_TILES_GENERATED = true;
	}

	private int GetIdUsingPerlin(int x, int z)
	{
		System.Random random = new System.Random(parentGenerator.seed);
		float num = Mathf.Clamp01(Mathf.PerlinNoise((float)(random.Next(-100000, 100000) + x - x_offset * (chunkPosX + 1)) / magnification, (float)(random.Next(-100000, 100000) + z - z_offset * (chunkPosZ + 1)) / magnification)) * (float)tileset.Count;
		if (num == (float)tileset.Count)
		{
			num = tileset.Count - 1;
		}
		return Mathf.FloorToInt(num);
	}

	private void CreateTile(int tile_id, int x, int y, int z)
	{
		GameObject tileToSpawn = tileset[tile_id];

		GameObject createdTile = null;

		if (SceneManager.GetActiveScene().name != "HomeScreen")
		{
			if (createdTile == null)
			{
				createdTile = Instantiate(tileToSpawn, gameObject.transform);
			}
		}
		else
		{
			createdTile = Instantiate(tileToSpawn, gameObject.transform);
		}
		createdTile.name = $"tile_x{x + chunkPosX}_y{y + chunkPosY}_z{z + chunkPosZ}";

		createdTile.transform.localPosition = new Vector3(x * tileWidth, 0f, z * tileWidth);

		tile_grid.Add(createdTile.GetComponent<Tile>());

		createdTile.GetComponent<Tile>().tilePos = new Vector2Int(x, y);

		if (chunkPosY > 0)
		{
			Elevator componentInChildren = createdTile.GetComponentInChildren<Elevator>();
			if (componentInChildren != null)
			{
				Destroy(componentInChildren.gameObject);
			}
		}
	}
}
