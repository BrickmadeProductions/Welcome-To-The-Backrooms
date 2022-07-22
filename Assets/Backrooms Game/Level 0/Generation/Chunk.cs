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

	public List<int> tile_gridData;

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

	public List<Tile> tile_grid;

	public float magnification;

	public float tileWidth;

	public float tileHeight;

	public int x_offset;

	public int z_offset;

	private int RoomId;

	public bool ALL_TILES_GENERATED;

	public bool ALL_OBJECTS_AND_ENTITES_LOADED;

	public void CreateChunk(int posX, int posY, int posZ, BackroomsLevelWorld parentGenerator, bool shouldGenerateInstantly, List<int> tile_grid)
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

		if (tile_grid.Count == 0)
        {
			if (shouldGenerateInstantly)
			{
				GenerateRandomMap();
			}
			else
			{
				StartCoroutine(GenerateRandomMap(2));
			}
        }
        else
        {
			if (shouldGenerateInstantly)
			{
				GenerateMapFromGrid(tile_grid);
			}
			else
			{
				StartCoroutine(GenerateMapFromGrid(2, tile_grid));
			}
		}
		
	}

	private void Awake()
	{
		tile_grid = new List<Tile>();
	}

	public void SaveChunkTileGrid()
    {
		List<int> tile_grid_save = new List<int>(tile_grid.Count);

		foreach (Tile tile in tile_grid)
        {
			tile_grid_save.Add(tile.id);
        }

		saveableData.tile_gridData = tile_grid_save;
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
			for (int z = 0; z < parentGenerator.chunk_width; z++)
			{
				for (int i = 0; i < framesPerTile; i++)
				{
					yield return null;
				}
				int idUsingPerlin = GetIdUsingPerlin(x, z);
				CreateTile(idUsingPerlin, x, chunkPosY, z);
			}
		}
		ALL_TILES_GENERATED = true;
	}

	private void GenerateRandomMap()
	{
		for (int i = 0; i < parentGenerator.chunk_width; i++)
		{
			for (int j = 0; j < parentGenerator.chunk_width; j++)
			{
				int idUsingPerlin = GetIdUsingPerlin(i, j);
				CreateTile(idUsingPerlin, i, chunkPosY, j);
			}
		}
		ALL_TILES_GENERATED = true;
	}

	private IEnumerator GenerateMapFromGrid(int framesPerTile, List<int> grid)
	{
		int tilesCreated = 0;
		for (int x = 0; x < parentGenerator.chunk_width; x++)
		{
			for (int z = 0; z < parentGenerator.chunk_width; z++)
			{
				for (int i = 0; i < framesPerTile; i++)
				{
					yield return null;
				}
				
				CreateTile(grid[tilesCreated], x, chunkPosY, z);
				tilesCreated++;
			}
		}
		ALL_TILES_GENERATED = true;
	}

	private void GenerateMapFromGrid(List<int> grid)
	{
		int tilesCreated = 0;
		for (int i = 0; i < parentGenerator.chunk_width; i++)
		{
			for (int j = 0; j < parentGenerator.chunk_width; j++)
			{
				CreateTile(grid[tilesCreated], i, chunkPosY, j);
				tilesCreated++;
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
		GameObject tileToSpawn;

		if (tile_id > tileset.Count - 1)
		{
			tileToSpawn = tileset[0];
		}
        else
        {
			tileToSpawn = tileset[tile_id];
		}

		

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

		createdTile.GetComponent<Tile>().id = tile_id;

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
