// BackroomsLevelWorld
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lowscope.Saving;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct EntityCluster
{
	public Dictionary<string, SaveableEntity> entityClusterData;
}

[Serializable]
public struct PropCluster
{
	public Dictionary<string, SaveableProp> propClusterData;
}

[Serializable]
public struct WorldSaveData
{
	public int savedSeed;

	public Dictionary<string, SerealizedChunk> savedChunks;
}

public class BackroomsLevelWorld : MonoBehaviour, ISaveable
{
	private class ThreadedActionManager
	{
		public bool doActions;

		public bool isDoingActions;

		public Queue<ThreadedAction> actionQueue;

		public void UpdateListener()
		{
			if (!doActions || isDoingActions)
			{
				return;
			}
			isDoingActions = true;
			foreach (ThreadedAction item in actionQueue)
			{
				item.DoAction();
				item.ActionCompleted = true;
			}
			doActions = false;
			isDoingActions = false;
		}

		public bool DoAllQueuedActions()
		{
			if (!isDoingActions)
			{
				doActions = true;
				return true;
			}
			return false;
		}

		public void AddActionToQueue(ThreadedAction action)
		{
			actionQueue.Enqueue(action);
		}
	}

	public abstract class ThreadedAction
	{
		public bool ActionCompleted;

		public abstract void DoAction();
	}

	public List<GameObject> globalBloodAndGoreObjects;

	public bool spawnEntities = true;
	public bool entityAI = true;

	public bool gen_enabled;

	public bool ThreeDimensional;

	[HideInInspector]
	public bool isLoadingChunks;

	[HideInInspector]
	public int currentRoomNumber;

	[HideInInspector]
	public int currentChunkNumber;

	[HideInInspector]
	public Vector3Int oldPlayerChunkLocation = Vector3Int.zero;

	[HideInInspector]
	public Vector3Int newPlayerChunkLocation = Vector3Int.zero;

	public GameObject level_chunkGenerator;

	public int chunk_width;

	public int layerDistance;

	public volatile Dictionary<string, Chunk> loadedChunks;

	public volatile Dictionary<string, SerealizedChunk> allChunks;

	[Range(1f, 5f)]
	public int viewDistance;

	[Header("Chunk Information")]
	[SerializeField]
	public GameObject Exit;

	[SerializeField]
	public List<GameObject> SpecialTiles;

	[SerializeField]
	public List<GameObject> Tiles;

	public List<GameObject> RegTiles;

	public List<OBJECT_TYPE> propsThatCanSpawnOnThisLevel;

	public List<ENTITY_TYPE> entitiesThatCanSpawnOnThisLevel;

	private Dictionary<OBJECT_TYPE, InteractableObject> levelPropDatabase;

	private Dictionary<ENTITY_TYPE, Entity> levelEntityDatabase;

	public int regTileSpace;

	public int seed;

	private void Awake()
	{
		
		GameSettings.Instance.worldInstance = this;

		globalBloodAndGoreObjects = new List<GameObject>();

		seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		Begin();
	}

	public IEnumerator SaveDataEveryXMinutes(float minutes)
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(minutes * 60f);
			yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
			yield return new WaitUntil(() => !GameSettings.IS_SAVING);
			SaveAllObjectsAndEntities();
			GameSettings.SaveAllProgress();
		}
	}

	public IEnumerator TrySpawnEntityEveryFrame()
	{
		while (true && GameSettings.Instance.ActiveScene != GameSettings.SCENE.INTRO && GameSettings.Instance.ActiveScene != GameSettings.SCENE.HOMESCREEN && spawnEntities)
		{
			yield return new WaitForSecondsRealtime(0.05f);

			foreach (KeyValuePair<ENTITY_TYPE, Entity> entity in GameSettings.Instance.EntityDatabase)
			{
				if (entitiesThatCanSpawnOnThisLevel.Contains(entity.Value.type))
				{
					float spawnChanceSelection = UnityEngine.Random.Range(0f, 0.99f);

					if (entity.Value.spawnChance > spawnChanceSelection)
					{
						Chunk chunk = loadedChunks.ElementAt(UnityEngine.Random.Range(0, loadedChunks.Count)).Value;

						string key = chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ;

						int tileToSpawnIn = UnityEngine.Random.Range(0, chunk.tile_grid.Count - 1);

						Tile tile = chunk.tile_grid[tileToSpawnIn];

						if (tile.entitySpawnLocations.Count > 0)
						{
							AddNewEntity(tile.entitySpawnLocations[UnityEngine.Random.Range(0, tile.entitySpawnLocations.Count)].position, entity.Value.gameObject, chunk);
						}
						else
						{
							AddNewEntity(new Vector3((float)tile.tilePos.x * chunk.tileWidth + (float)(chunk.chunkPosX * chunk_width) * chunk.tileWidth, 0f, (float)tile.tilePos.y * chunk.tileWidth + (float)(chunk.chunkPosZ * chunk_width) * chunk.tileWidth), entity.Value.gameObject, chunk);
						}

						allChunks[key] = chunk.saveableData;
					}
				}

			}
		}
	}

	public void Begin()
	{
		if (!gen_enabled)
		{
			return;
		}


		levelPropDatabase = new Dictionary<OBJECT_TYPE, InteractableObject>();
		levelEntityDatabase = new Dictionary<ENTITY_TYPE, Entity>();

		foreach (OBJECT_TYPE propID in propsThatCanSpawnOnThisLevel)
		{
			InteractableObject propToGet = null;

			GameSettings.Instance.PropDatabase.TryGetValue(propID, out propToGet);

			if (propToGet != null)
			{
				levelPropDatabase.Add(propID, propToGet);
			}
		}

		foreach (ENTITY_TYPE entityID in entitiesThatCanSpawnOnThisLevel)
		{
			Entity entityToGet = null;

			GameSettings.Instance.EntityDatabase.TryGetValue(entityID, out entityToGet);

			if (entityToGet != null)
			{
				levelEntityDatabase.Add(entityID, entityToGet);
			}
		}

		loadedChunks = new Dictionary<string, Chunk>();
		allChunks = new Dictionary<string, SerealizedChunk>();

		currentRoomNumber = 0;
		currentChunkNumber = 0;

		Debug.Log("Begin Init");
		StartCoroutine(Init());
	}

	private IEnumerator Init()
	{
		Debug.Log("Waiting For Scene To Load Before Loading Chunks");

		yield return new WaitUntil(() => GameSettings.LEVEL_LOADED);

		Debug.Log("Waiting For Player Data To Load Before Loading Chunks");

		yield return new WaitUntil(() => GameSettings.PLAYER_DATA_LOADED_IN_SCENE);

		Debug.Log("Player Data save loaded");

		yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
		//yield return new WaitUntil(() => GameSettings.Instance.Player != null);

		Debug.Log("Level save loaded");

		newPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);

		//Debug.Log(newPlayerChunkLocation + " " + viewDistance);

		for (int x = newPlayerChunkLocation.x - viewDistance; x < newPlayerChunkLocation.x + viewDistance; x++)
		{
			//Debug.Log(newPlayerChunkLocation + " " + viewDistance);
			for (int z = newPlayerChunkLocation.z - viewDistance; z < newPlayerChunkLocation.z + viewDistance; z++)
			{
				for (int y = (ThreeDimensional ? (newPlayerChunkLocation.y - layerDistance) : 0); y < ((!ThreeDimensional) ? 1 : (newPlayerChunkLocation.y + layerDistance)); y++)
				{
					Chunk chunk = LoadInChunk(x, y, z, shouldGenInstantly: true);

					if (chunk != null)
					{
						yield return new WaitUntil(() => chunk.GetComponent<Chunk>().ALL_TILES_GENERATED);
						yield return new WaitUntil(() => chunk.GetComponent<Chunk>().ALL_OBJECTS_AND_ENTITES_LOADED);
						currentChunkNumber++;
					}
				}
			}
		}

		GameSettings.LEVEL_GENERATED = true;

		Dictionary<string, SaveableProp> propsFromOtherScenes = new Dictionary<string, SaveableProp>();

		foreach (KeyValuePair<string, SerealizedChunk> serealizedChunks in allChunks.ToArray())
		{
			foreach (KeyValuePair<string, SaveableProp> propClusterData in serealizedChunks.Value.propData.propClusterData)
			{
				if (!propsFromOtherScenes.ContainsKey(propClusterData.Key))
                {
					propsFromOtherScenes.Add(propClusterData.Key, propClusterData.Value);
				}
				
			}
		}

		//load in objets brought from other scenes
		foreach (InteractableObject iObject in FindObjectsOfType<InteractableObject>())
		{
			if (!propsFromOtherScenes.ContainsKey(iObject.saveableData.type.ToString() + "-" + iObject.saveableData.runTimeID))
			{
				iObject.GenerateID(this);

				Vector3 position = GetChunkKeyAtWorldLocation(iObject.transform.position);

				string key = position.x + "," + position.y + "," + position.z;

				string key2 = iObject.type.ToString() + "-" + iObject.runTimeID;

				allChunks[key].propData.propClusterData.Add(key2, iObject.saveableData);

				Debug.Log("Loaded over a " + iObject.type.ToString() + " From other scene..");
			}
		}

		GameSettings.LEVEL_SAVE_LOADED = true;

		StartCoroutine(SaveDataEveryXMinutes(0.05f));
		StartCoroutine(TrySpawnEntityEveryFrame());

		Debug.Log("Done With Spawn Region");
	}

	/*private void Update()
	{
		//UpdateChunks();
	}*/

	public string OnSave()
	{
		
		WorldSaveData worldSaveData = new WorldSaveData
		{

			savedSeed = seed,
			savedChunks = allChunks
		};

		return JsonConvert.SerializeObject(worldSaveData);
	}

	private IEnumerator OnLoadAsync(string data)
	{
		yield return new WaitUntil(() => GameSettings.LEVEL_LOADED);
		
		try
		{
			LoadSaveData(JsonConvert.DeserializeObject<WorldSaveData>(data));
		}
		catch (ArgumentException exception)
		{
			Debug.LogError("LEVEL_READ_ERROR");
			//Debug.LogException(exception);
		}
		GameSettings.LEVEL_SAVE_LOADED = true;
		Debug.Log("Level Save Loaded");
	}

	private void LoadSaveData(WorldSaveData saveData)
	{
		seed = saveData.savedSeed;

		if (saveData.savedChunks != null)
		{
			allChunks = saveData.savedChunks;
		}
		else
		{
			Debug.LogError("LEVEL_READ_ERROR");
		}
	}

	public void OnLoad(string data)
	{
		if (!GameSettings.LEVEL_SAVE_LOADED)
		{
			StartCoroutine(OnLoadAsync(data));
		}
	}

	public void OnLoadNoData()
	{

		GameSettings.LEVEL_SAVE_LOADED = true;
		Debug.LogError("No Save To Load");
	}

	public bool OnSaveCondition()
	{
		if (GameSettings.LEVEL_LOADED)
		{
			return GameSettings.LEVEL_SAVE_LOADED;
		}
		return false;
	}

	private void UpdateChunks()
	{
		newPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);

		if (newPlayerChunkLocation != oldPlayerChunkLocation && !isLoadingChunks)
		{
			isLoadingChunks = true;

			for (int x = newPlayerChunkLocation.x - viewDistance; x < newPlayerChunkLocation.x + viewDistance; x++)
			{
				for (int z = newPlayerChunkLocation.z - viewDistance; z < newPlayerChunkLocation.z + viewDistance; z++)
				{
					for (int y = (ThreeDimensional ? (newPlayerChunkLocation.y - layerDistance) : 0); y < ((!ThreeDimensional) ? 1 : (newPlayerChunkLocation.y + layerDistance)); y++)
					{
						Chunk chunk = LoadInChunk(x, y, z, shouldGenInstantly: false);
						
						if (chunk != null)
						{
							currentChunkNumber++;
						}
					}
				}
			}
			UnloadChunks();
			isLoadingChunks = false;
		}

		oldPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);
	}

	public void UnloadChunks()
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks.ToArray())
		{
			Chunk chunk = loadedChunk.Value;

			if (Vector3.Distance(new Vector3(chunk.chunkPosX, chunk.chunkPosY, chunk.chunkPosZ), GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position)) > viewDistance)
			{
				foreach (KeyValuePair<string, SaveableProp> prop in chunk.saveableData.propData.propClusterData.ToArray())
				{
					//dont use remove so its still in the database
					if (prop.Value.instance != null)

						Destroy(prop.Value.instance.gameObject);

				}

				foreach (KeyValuePair<string, SaveableEntity> entity in chunk.saveableData.entityData.entityClusterData.ToArray())
				{
					//dont use remove so its still in the database
					if (entity.Value.instance != null)

						Destroy(entity.Value.instance.gameObject);


				}

				loadedChunks.Remove(loadedChunk.Key);

				Destroy(chunk.gameObject);

				SaveAllObjectsAndEntities();
			}
		}

		
	}

	private Chunk LoadInChunk(int chunkX, int chunkY, int chunkZ, bool shouldGenInstantly)
	{
		if (!ChunkLocationLoaded(chunkX, chunkY, chunkZ))
		{
			//Debug.Log("Chunk Gen");

			Chunk chunk = Instantiate(level_chunkGenerator).GetComponent<Chunk>();

			chunk.name = chunkX + "," + chunkY + "," + chunkZ;

			if (!allChunks.ContainsKey(chunk.name))
            {
				chunk.GetComponent<Chunk>().CreateChunk(chunkX, chunkY, chunkZ, this, shouldGenInstantly, new List<int>(0));
			}

            else
            {

				chunk.GetComponent<Chunk>().CreateChunk(chunkX, chunkY, chunkZ, this, shouldGenInstantly, allChunks[chunk.name].tile_gridData);
			}


			chunk.transform.position = new Vector3(chunkX * ChunkSize(), chunkY * ChunkHeight(), chunkZ * ChunkSize());

			loadedChunks.Add(chunkX + "," + chunkY + "," + chunkZ, chunk);

			foreach (Tile tile in chunk.tile_grid)
			{
				//Debug.Log("Spawning Prop");
				tile.SpawnPresetProps();
			}

			StartCoroutine(LoadInObjectsAndEntities(chunk));

			return chunk;
		}
		return null;
	}

	private IEnumerator LoadInObjectsAndEntities(Chunk chunk)
	{
		yield return new WaitUntil(() => chunk.ALL_TILES_GENERATED);

		//Debug.Log(chunk.tile_grid.Count);
		
		string chunkKey = chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ;

		//load
		if (allChunks.ContainsKey(chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ))
		{
			chunk.saveableData = allChunks[chunkKey];
			chunk.saveableData.instance = chunk;

			foreach (KeyValuePair<string, SaveableEntity> entity in chunk.saveableData.entityData.entityClusterData.ToArray())
			{
				LoadSavedEntity(entity.Value, chunk);
			}

			foreach (KeyValuePair<string, SaveableProp> prop in chunk.saveableData.propData.propClusterData.ToArray())
			{
				LoadSavedProp(prop.Value, chunk);
			}

			allChunks[chunkKey] = chunk.saveableData;
		}

		//generate
		else
		{
			int tileIndex = UnityEngine.Random.Range(0, chunk.tile_grid.Count);

			Tile tile = chunk.tile_grid[tileIndex];

			foreach (Transform propSpawnLocation in tile.randomPropSpawnLocations)
			{
				float spawnChanceSelection = UnityEngine.Random.Range(0f, 1.01f);

				foreach (KeyValuePair<OBJECT_TYPE, InteractableObject> prop in levelPropDatabase.ToArray())
				{
					UnityEngine.Random.Range(0, tile.randomPropSpawnLocations.Count);

					if (prop.Value.spawnChance < spawnChanceSelection && tile.randomPropSpawnLocations.Count > 0)
					{
						int spawnLocationChoice = UnityEngine.Random.Range(0, tile.randomPropSpawnLocations.Count);

						AddNewProp(tile.randomPropSpawnLocations[spawnLocationChoice].position, tile.randomPropSpawnLocations[spawnLocationChoice].rotation, prop.Value.gameObject, chunk);
						break;
					}
				
				}
			}
			chunk.SaveChunkTileGrid();
			allChunks.Add(chunkKey, chunk.saveableData);
		}
		

		chunk.ALL_OBJECTS_AND_ENTITES_LOADED = true;
	}

	public bool CheckWorldForEntityKey(string key)
	{
		foreach (KeyValuePair<string, SerealizedChunk> chunk in allChunks)
		{
			
			if (chunk.Value.entityData.entityClusterData.ContainsKey(key))
			{
				return true;
			}
            else
            {
				return false;
            }
			
		}
		return false;
	}

	public Entity AddNewEntity(Vector3 position, GameObject entity, Chunk chunk)
	{
		int entityCount = 0;
		int totalEntities = 0;
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{
			foreach (KeyValuePair<string, SaveableEntity> entityClusterData in loadedChunk.Value.saveableData.entityData.entityClusterData)
			{
				if (entityClusterData.Value.type == entity.GetComponent<Entity>().type)
				{
					entityCount++;
				}
			}
			totalEntities += loadedChunk.Value.saveableData.entityData.entityClusterData.Count;
		}

		if (entityCount < entity.GetComponent<Entity>().maxAllowed && totalEntities <= 50)
		{
			Entity entityComponent = Instantiate(entity).GetComponent<Entity>();
			entityComponent.GenerateID(this);
			entityComponent.gameObject.transform.position = position;
			chunk.saveableData.entityData.entityClusterData.Add(entityComponent.type.ToString() + "-" + entityComponent.runTimeID, entityComponent.Save());

			return entityComponent;
		}
		return null;
	}

	public Entity LoadSavedEntity(SaveableEntity entityData, Chunk chunk)
	{
		if (GameSettings.Instance.EntityDatabase.ContainsKey(entityData.type))
		{

			GameObject entityToSpawn = Instantiate(GameSettings.Instance.EntityDatabase[entityData.type].gameObject);

			entityToSpawn.GetComponent<Entity>().Load(entityData);

			chunk.saveableData.entityData.entityClusterData[entityToSpawn.GetComponent<Entity>().type.ToString() + "-" + entityToSpawn.GetComponent<Entity>().runTimeID] = entityToSpawn.GetComponent<Entity>().saveableData;

			return entityToSpawn.GetComponent<Entity>();

		}
		return null;
	}

	public bool RemoveEntity(string key)
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			if (loadedChunk.Value.saveableData.entityData.entityClusterData.ContainsKey(key))
			{
				Destroy(loadedChunk.Value.saveableData.entityData.entityClusterData[key].instance.gameObject);

				loadedChunk.Value.saveableData.entityData.entityClusterData.Remove(key);

				return true;

			}

		}

		foreach (KeyValuePair<string, SerealizedChunk> allChunk in allChunks)
		{

			if (allChunk.Value.entityData.entityClusterData.ContainsKey(key))
			{

				allChunk.Value.entityData.entityClusterData.Remove(key);

				return true;
			}
			
		}

		return false;
	}

	public void RemoveAllEntities()
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			foreach (KeyValuePair<string, SaveableEntity> entity in loadedChunk.Value.saveableData.entityData.entityClusterData.ToList())
			{

				RemoveEntity(entity.Key);

				loadedChunk.Value.saveableData.entityData.entityClusterData.Remove(entity.Key);

			}
			
		}

		foreach (KeyValuePair<string, SerealizedChunk> allChunk in allChunks)
		{

			foreach (KeyValuePair<string, SaveableEntity> entity in allChunk.Value.entityData.entityClusterData.ToList())
			{

				RemoveEntity(entity.Key);

				allChunk.Value.entityData.entityClusterData.Remove(entity.Key);

			}
		}
	
	}

	public bool CheckWorldForPropKey(string key)
	{
		foreach (KeyValuePair<string, SerealizedChunk> chunk in allChunks)
		{

			if (chunk.Value.propData.propClusterData.ContainsKey(key))
			{
				return true;
			}
			else
			{
				return false;
			}

		}
		return false;
	}

	public InteractableObject AddNewProp(Vector3 position, Quaternion rotation, GameObject item, Chunk chunk)
	{
		int propCount = 0;
		foreach (KeyValuePair<string, Chunk> loadedChunks in loadedChunks)
		{
			propCount += loadedChunks.Value.saveableData.propData.propClusterData.Count;
		}
		if (propCount <= 100)
		{
			InteractableObject prop = Instantiate(item).GetComponent<InteractableObject>();

			prop.GenerateID(this);

			prop.gameObject.transform.position = position;

			prop.gameObject.transform.rotation = rotation;
			
			chunk.saveableData.propData.propClusterData.Add(prop.type.ToString() + "-" + prop.runTimeID, prop.Save());

			return prop;
		}
		return null;
	}

	public InteractableObject LoadSavedProp(SaveableProp savedObjectData, Chunk chunk)
	{
		if (GameSettings.Instance.PropDatabase.ContainsKey(savedObjectData.type))
		{
			GameObject spawnedObject = Instantiate(GameSettings.Instance.PropDatabase[savedObjectData.type].gameObject);

			spawnedObject.GetComponent<InteractableObject>().Load(savedObjectData);

			chunk.saveableData.propData.propClusterData[spawnedObject.GetComponent<InteractableObject>().type.ToString() + "-" + spawnedObject.GetComponent<InteractableObject>().runTimeID] = spawnedObject.GetComponent<InteractableObject>().saveableData;

			return spawnedObject.GetComponent<InteractableObject>();
		}
		return null;
	}

	public bool RemoveProp(string key)
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			if (loadedChunk.Value.saveableData.propData.propClusterData.ContainsKey(key))
			{
				Destroy(loadedChunk.Value.saveableData.propData.propClusterData[key].instance.gameObject);

				loadedChunk.Value.saveableData.propData.propClusterData.Remove(key);

				return true;

			}

		}

		foreach (KeyValuePair<string, SerealizedChunk> allChunk in allChunks)
		{

			if (allChunk.Value.propData.propClusterData.ContainsKey(key))
			{
				allChunk.Value.propData.propClusterData.Remove(key);

				return true;

			}

		}
		return false;
	}

	public void RemoveAllProps()
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			foreach (KeyValuePair<string, SaveableProp> prop in loadedChunk.Value.saveableData.propData.propClusterData.ToList())
			{

				RemoveProp(prop.Key);

				Debug.Log(loadedChunk.Value.saveableData.propData.propClusterData.Remove(prop.Key));

			}

		}

		foreach (KeyValuePair<string, SerealizedChunk> allChunks in allChunks)
		{

			foreach (KeyValuePair<string, SaveableProp> prop in allChunks.Value.propData.propClusterData.ToList())
			{

				RemoveProp(prop.Key);

				Debug.Log(allChunks.Value.propData.propClusterData.Remove(prop.Key));

			}
		}

	}

	public bool ChunkLocationLoaded(int x, int y, int z)
	{
		if (loadedChunks.ContainsKey(x + "," + y + "," + z))
		{
			return true;
		}
		return false;
	}

	public bool CheckPointIntersection(float x1, float y1, float z1, float x2, float y2, float z2, float x, float y, float z)
	{
		if (x > x1 && x < x2 && y > y1 && y < y2 && z > z1 && z < z2)
		{
			return true;
		}
		return false;
	}

	public float ChunkSize()
	{
		return level_chunkGenerator.GetComponent<Chunk>().tileWidth * (float)chunk_width;
	}

	public float ChunkHeight()
	{
		return level_chunkGenerator.GetComponent<Chunk>().tileHeight * (float)layerDistance;
	}

	public float RoomSize()
	{
		return level_chunkGenerator.GetComponent<Chunk>().tileWidth;
	}
	public Vector3Int GetChunkKeyAtWorldLocation(Vector3 worldPos)
	{
		int y = 0;
		if (ThreeDimensional)
		{
			y = Mathf.FloorToInt(worldPos.y / ChunkHeight());
		}
		return new Vector3Int(Mathf.FloorToInt((worldPos.x + RoomSize() / 2f) / ChunkSize()), y, Mathf.FloorToInt((worldPos.z + RoomSize() / 2f) / ChunkSize()));
	}

	public Chunk GetLoadedChunkAtPlayerLocation()
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{
			Chunk value = loadedChunk.Value;
			Vector3Int chunkPos = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);

			string chunkPosKey = chunkPos.x + "," + chunkPos.y + "," + chunkPos.z;

			if (value != null && loadedChunk.Key == chunkPosKey)
			{
				return value;
			}
		}
		return null;
	}
	public void Update()
    {
		UpdateWorld();
    }
	public void UpdateWorld()
	{
		if ((!GameSettings.LEVEL_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.LEVEL_GENERATED) || GameSettings.Instance.Player == null)
		{
			Debug.Log("Waiting For Level To Load");
		}
		else if (gen_enabled)
		{
			UpdateChunks();
		}
	}

	public void SaveAllObjectsAndEntities()
	{

		KeyValuePair<string, SerealizedChunk>[] serializedChunks = allChunks.ToArray();

		for (int i = 0; i < serializedChunks.Length; i++)
		{
			KeyValuePair<string, SerealizedChunk> chunkData = serializedChunks[i];

			KeyValuePair<string, SaveableEntity>[] entityDataList = chunkData.Value.entityData.entityClusterData.ToArray();

			for (int j = 0; j < entityDataList.Length; j++)
			{
				KeyValuePair<string, SaveableEntity> entityData = entityDataList[j];

				if (entityData.Value.instance == null)
				{
					continue;
				}

				Vector3 chunkLocationKey = GetChunkKeyAtWorldLocation(entityData.Value.instance.transform.position);

				string chunkKey = chunkData.Key;

				string chunkLocation = chunkLocationKey.x + "," + chunkLocationKey.y + "," + chunkLocationKey.z;

				string entityID = entityData.Value.type.ToString() + "-" + entityData.Value.instance.runTimeID;

				

				if (chunkData.Key != chunkLocation)
				{
					if (allChunks.ContainsKey(chunkLocation))
					{
						allChunks[chunkKey].entityData.entityClusterData.Remove(entityID);

						if (!allChunks[chunkLocation].entityData.entityClusterData.ContainsKey(entityID))
							allChunks[chunkLocation].entityData.entityClusterData.Add(entityID, entityData.Value.instance.Save());

						Debug.Log(chunkData.Key + " " + chunkLocation + " " + entityID);
					}
					else
					{
						Debug.LogError("COULD NOT SAVE -> CHUNKS DOESN'T CONTAIN KEY: " + chunkLocation);
					}
				}

				else if (allChunks.ContainsKey(chunkLocation))
				{
					allChunks[chunkLocation].entityData.entityClusterData[entityID] = entityData.Value.instance.Save();
				}

				else
				{
					Debug.LogError("COULD NOT SAVE: " + entityData.Value.type.ToString() + "-" + entityData.Value.instance.runTimeID);
				}
			}

			KeyValuePair<string, SaveableProp>[] serializedProps = chunkData.Value.propData.propClusterData.ToArray();

			for (int j = 0; j < serializedProps.Length; j++)
			{
				KeyValuePair<string, SaveableProp> propData = serializedProps[j];

				if (propData.Value.instance == null)
				{
					continue;
				}

				Vector3 location = GetChunkKeyAtWorldLocation(propData.Value.instance.transform.position);

				string chunkKey = chunkData.Key;
				string chunkLocation = location.x + "," + location.y + "," + location.z;
				string propKey = propData.Value.type.ToString() + "-" + propData.Value.instance.runTimeID;

				if (chunkData.Key != chunkLocation)
				{
					if (allChunks.ContainsKey(chunkLocation))
					{
						allChunks[chunkKey].propData.propClusterData.Remove(propKey);

						if (!allChunks[chunkLocation].propData.propClusterData.ContainsKey(propKey))
							allChunks[chunkLocation].propData.propClusterData.Add(propKey, propData.Value.instance.Save());
					}
					else
					{
						Debug.LogError("COULD NOT SAVE -> CHUNKS DOESN'T CONTAIN KEY: " + chunkLocation);
					}
				}
				else if (allChunks.ContainsKey(chunkLocation))
				{
					allChunks[chunkLocation].propData.propClusterData[propKey] = propData.Value.instance.Save();
				}
				else
				{
					Debug.LogError("COULD NOT SAVE: " + propData.Value.type.ToString() + "-" + propData.Value.instance.runTimeID);
				}
			}
		}
		//Debug.Log("Saved All World Data...");
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Shutting Down");
	}
}
