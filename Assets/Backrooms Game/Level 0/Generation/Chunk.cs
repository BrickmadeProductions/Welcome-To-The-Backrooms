// Chunk
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	private Dictionary<int, Tile> tileset;

	public List<Tile> tile_grid;

	public float magnification;

	public float tileWidth;

	public float tileHeight;

	public int x_offset;

	public int z_offset;


	public bool ALL_TILES_GENERATED = false;

	public bool ALL_OBJECTS_AND_ENTITES_LOADED = false;

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

		StartCoroutine(LoadInObjectsAndEntities());

	}

	public IEnumerator LoadInObjectsAndEntities()
	{
		yield return new WaitUntil(() => ALL_TILES_GENERATED);

		string chunkKey = chunkPosX + "," + chunkPosY + "," + chunkPosZ;

		//load objects and entities from saved file
		if (parentGenerator.allChunks.ContainsKey(chunkPosX + "," + chunkPosY + "," + chunkPosZ))
		{
			saveableData = parentGenerator.allChunks[chunkKey];
			saveableData.instance = this;

			foreach (KeyValuePair<string, SaveableEntity> entity in saveableData.entityData.entityClusterData.ToArray())
			{
				parentGenerator.LoadSavedEntity(entity.Value, this);
			}

			foreach (KeyValuePair<string, SaveableProp> prop in saveableData.propData.propClusterData.ToArray())
			{
				parentGenerator.LoadSavedProp(prop.Value, this);
			}

			SaveChunkTileGrid();

			parentGenerator.allChunks[chunkKey] = saveableData;
		}

		else
		{
			foreach (Tile tile in tile_grid)
			{
				tile.SpawnPresetItems();
			}

			SaveChunkTileGrid();

			parentGenerator.allChunks.Add(name, saveableData);
		}


		ALL_OBJECTS_AND_ENTITES_LOADED = true;
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

	private void CreateTileset(List<Tile> tiles)
	{
		tileset = new Dictionary<int, Tile>();
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
				int idUsingPerlin = GetIDUsingWeightedRandom(x, z);
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
				int idUsingPerlin = GetIDUsingWeightedRandom(i, j);
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
		float num = Mathf.Clamp01(Mathf.PerlinNoise((float)(random.Next(-100000, 100000) + x - x_offset + (chunkPosX)) / magnification, (float)(random.Next(-100000, 100000) + z - z_offset + (chunkPosZ)) / magnification)) * (float)tileset.Count;
		if (num == (float)tileset.Count)
		{
			num = tileset.Count - 1;
		}
		return Mathf.FloorToInt(num);
	}

	private int GetIDUsingWeightedRandom(int x, int z)
	{

		return WeightedRandomSpawning.ReturnWeightedTileIDBySpawnChance(parentGenerator.Tiles);
	}

	private void CreateTile(int tile_id, int x, int y, int z)
	{
		GameObject tileToSpawn;

		/*if (x % GameSettings.Instance.worldInstance.regTileSpace == 0 && y % GameSettings.Instance.worldInstance.regTileSpace == 0  && z % GameSettings.Instance.worldInstance.regTileSpace == 0)
		{
			tileToSpawn = GameSettings.Instance.worldInstance.RegTiles[0];
        }*/
		if (tile_id > tileset.Count - 1)
		{
			tileToSpawn = tileset[0].gameObject;
		}
        else
        {
			tileToSpawn = tileset[tile_id].gameObject;
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
	}
}
