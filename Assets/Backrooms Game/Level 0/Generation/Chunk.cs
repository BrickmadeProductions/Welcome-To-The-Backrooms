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
		List<ContainerObject> containersInThisChunk = new List<ContainerObject>();

		yield return new WaitUntil(() => ALL_TILES_GENERATED);

		string chunkKey = chunkPosX + "," + chunkPosY + "," + chunkPosZ;

		//load objects and entities from saved file
		if (parentGenerator.allChunks.ContainsKey(chunkPosX + "," + chunkPosY + "," + chunkPosZ))
		{
			saveableData = parentGenerator.allChunks[chunkKey];
			saveableData.instance = this;

			foreach (KeyValuePair<string, SaveableEntity> entity in saveableData.entityData.entityClusterData.ToArray())
			{
				Entity entityToSpawn = parentGenerator.LoadSavedEntity(entity.Value, this);
			}

			foreach (KeyValuePair<string, SaveableProp> prop in saveableData.propData.propClusterData.ToArray())
			{
				InteractableObject objectToSpawn = parentGenerator.LoadSavedProp(prop.Value, this);

				//this is a container, load in objects that belong in its slots
				if (parentGenerator.containersInWorld.ContainsKey(prop.Key))
					containersInThisChunk.Add((ContainerObject)objectToSpawn);
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


		foreach(ContainerObject container in containersInThisChunk)
        {
			//Debug.Log("Loading In slots for " + container.name);
			container.LoadInSlots(parentGenerator.containersInWorld[container.GetWorldID()]);
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
				int tileIDToGenerate = GetTileIDFromWorldLocation(x, z);
				CreateTile(tileIDToGenerate, x, chunkPosY, z);
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
				int tileIDToGenerate = GetTileIDFromWorldLocation(i, j);
				CreateTile(tileIDToGenerate, i, chunkPosY, j);
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

	//get biome id
	//make a list of all tiles that have that biome id
	//get a random from weighted list of tiles in that biome
	private int GetTileIDFromWorldLocation(int x, int z)
	{
		float worldPrelinID = 
			Mathf.PerlinNoise(
				((float)(chunkPosX) / magnification) + ((float)parentGenerator.worldDataSeed / 10000f) + 0.01f,
				((float)(chunkPosZ) / magnification) + ((float)parentGenerator.worldDataSeed / 10000f) + 0.01f) * 1.4f;

		float biomePerlinID =
			Mathf.PerlinNoise(
				((float)(chunkPosX) / magnification) + ((float)parentGenerator.biomeDataSeed / 10000f) + 0.01f,
				((float)(chunkPosZ) / magnification) + ((float)parentGenerator.biomeDataSeed / 10000f) + 0.01f) * 1.1f;

		biomePerlinID = Mathf.Clamp01(biomePerlinID);
		worldPrelinID = Mathf.Clamp01(worldPrelinID);

		//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

		//cube.transform.localScale = new Vector3(tileWidth, 5, tileWidth);
		//cube.transform.position = new Vector3(chunkPosX * tileWidth, 25f, chunkPosZ * tileWidth);
		//cube.GetComponent<Renderer>().material.color = new Color(worldPrelinID, worldPrelinID, worldPrelinID);

		//Debug.Log("World Perlin: " + worldPrelinID);
		//Debug.Log("Biome Perlin: " + biomePerlinID);

		BIOME_ID biomeID = BIOME_ID.LEVEL_0_YELLOW_ROOMS;


		//Debug.Log(GameSettings.Instance.ActiveScene);

		
		switch (GameSettings.Instance.ActiveScene)
        {

			case (SCENE.LEVEL0):

				if (worldPrelinID >= 0.3f && worldPrelinID <= 1.01)
				{
					biomeID = BIOME_ID.LEVEL_0_YELLOW_ROOMS;

				}
				else if (worldPrelinID >= 0f && worldPrelinID < 0.3f)
				{
					if (biomePerlinID >= 0f && biomePerlinID < 0.3f)
					{
						biomeID = BIOME_ID.LEVEL_0_RED_ROOMS;
					}
					else if (biomePerlinID >= 0.3f && biomePerlinID < 0.6f)
					{
						biomeID = BIOME_ID.LEVEL_0_OVERGROWN;
					}
					else if (biomePerlinID >= 0.6f && biomePerlinID < 0.8f)
					{
						biomeID = BIOME_ID.LEVEL_0_PILLAR_ROOMS;
					}
					else if (biomePerlinID >= 0.8f && biomePerlinID < 0.9f)
					{
						biomeID = BIOME_ID.LEVEL_0_PILLAR_ROOMS;
					}
					else if (biomePerlinID >= 0.9f && biomePerlinID < 1.01f)
					{
						biomeID = BIOME_ID.LEVEL_0_RED_ROOMS;
					}

				}

				break;

			case (SCENE.LEVEL1):

				Debug.Log(worldPrelinID + " " + biomePerlinID);

				if ((Mathf.Abs(chunkPosZ) % 15) == 0 && chunkPosZ != 0)
				{
					biomeID = BIOME_ID.LEVEL_1_VOID_CUTS;
					break;
				}
				
				if (worldPrelinID >= 0.5f && worldPrelinID < 1.01)
				{
					biomeID = BIOME_ID.LEVEL_1_PARKING_GARAGE;

				}
				else if (worldPrelinID >= 0f && worldPrelinID < 0.5f)
				{
					if (biomePerlinID >= 0f && biomePerlinID < 0.5f)
					{
						biomeID = BIOME_ID.LEVEL_1_PARKING_GARAGE;
					}
					else if (biomePerlinID >= 0.5f && biomePerlinID <= 1.01f)
					{
						biomeID = BIOME_ID.LEVEL_1_MAZE;
					}


				}
				


				break;

        }




		//Debug.Log(biomeID.ToString());

		List<Tile> potentialTiles = new List<Tile>();

		foreach (KeyValuePair<int, WorldTileData> tileData in parentGenerator.tileDataList)
        {
			if (tileData.Value.biomeID == biomeID)
            {
				potentialTiles.Add(tileData.Value.prefab);
			}
				
        }
		int random = WeightedRandomSpawning.ReturnWeightedTileIDBySpawnChance(potentialTiles);

		return random;
	}

	private void CreateTile(int tile_id, int x, int y, int z)
	{


		GameObject tileToSpawn;

		/*if (x % GameSettings.Instance.worldInstance.regTileSpace == 0 && y % GameSettings.Instance.worldInstance.regTileSpace == 0  && z % GameSettings.Instance.worldInstance.regTileSpace == 0)
		{
			tileToSpawn = GameSettings.Instance.worldInstance.RegTiles[0];
        }*/

		tileToSpawn = parentGenerator.tileDataList[tile_id].prefab.gameObject;



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

		//CombineMeshes(createdTile);
		
	}
}
