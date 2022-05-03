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

	private Dictionary<OBJECT_TYPE, InteractableObject> levelProps;

	private Dictionary<ENTITY_TYPE, Entity> levelEntities;

	public int regTileSpace;

	public int seed;

	private void Awake()
	{
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
		}
	}

	public IEnumerator TrySpawnEntityEveryFrame()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(0.05f);
			KeyValuePair<ENTITY_TYPE, Entity>[] array = levelEntities.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<ENTITY_TYPE, Entity> keyValuePair = array[i];
				float num = UnityEngine.Random.Range(0f, 0.99f);
				if (keyValuePair.Value.spawnChance > num)
				{
					Chunk value = loadedChunks.ElementAt(UnityEngine.Random.Range(0, loadedChunks.Count)).Value;
					string key = value.chunkPosX + "," + value.chunkPosY + "," + value.chunkPosZ;
					int index = UnityEngine.Random.Range(0, value.tile_grid.Count - 1);
					Tile tile = value.tile_grid[index];
					if (tile.entitySpawnLocations.Count > 0)
					{
						AddNewEntity(tile.entitySpawnLocations[UnityEngine.Random.Range(0, tile.entitySpawnLocations.Count)].position, keyValuePair.Value, value);
					}
					else
					{
						AddNewEntity(new Vector3((float)tile.tilePos.x * value.tileWidth + (float)(value.chunkPosX * chunk_width) * value.tileWidth, 0f, (float)tile.tilePos.y * value.tileWidth + (float)(value.chunkPosZ * chunk_width) * value.tileWidth), keyValuePair.Value, value);
					}
					allChunks[key] = value.saveableData;
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
		levelProps = new Dictionary<OBJECT_TYPE, InteractableObject>();
		levelEntities = new Dictionary<ENTITY_TYPE, Entity>();
		foreach (OBJECT_TYPE item in propsThatCanSpawnOnThisLevel)
		{
			InteractableObject value = null;
			GameSettings.Instance.PropDatabase.TryGetValue(item, out value);
			if (value != null)
			{
				levelProps.Add(item, value);
			}
		}
		foreach (ENTITY_TYPE item2 in entitiesThatCanSpawnOnThisLevel)
		{
			Entity value2 = null;
			GameSettings.Instance.EntityDatabase.TryGetValue(item2, out value2);
			if (value2 != null)
			{
				levelEntities.Add(item2, value2);
			}
		}
		loadedChunks = new Dictionary<string, Chunk>();
		allChunks = new Dictionary<string, SerealizedChunk>();
		currentRoomNumber = 0;
		currentChunkNumber = 0;
		StartCoroutine(Init());
	}

	private IEnumerator Init()
	{
		Debug.Log("Waiting For Scene To Load Before Loading Chunks");
		yield return new WaitUntil(() => GameSettings.LEVEL_LOADED);
		Debug.Log("Waiting For Player Data To Load Before Loading Chunks");
		yield return new WaitUntil(() => GameSettings.PLAYER_DATA_LOADED_IN_SCENE);
		yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
		for (int x = newPlayerChunkLocation.x - viewDistance; x < newPlayerChunkLocation.x + viewDistance; x++)
		{
			for (int z = newPlayerChunkLocation.z - viewDistance; z < newPlayerChunkLocation.z + viewDistance; z++)
			{
				for (int y = (ThreeDimensional ? (newPlayerChunkLocation.y - layerDistance) : 0); y < ((!ThreeDimensional) ? 1 : (newPlayerChunkLocation.y + layerDistance)); y++)
				{
					GameObject chunk = GenerateChunk(x, y, z, shouldGenInstantly: true);
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
		Dictionary<string, SaveableProp> dictionary = new Dictionary<string, SaveableProp>();
		KeyValuePair<string, SerealizedChunk>[] array = allChunks.ToArray();
		foreach (KeyValuePair<string, SerealizedChunk> keyValuePair in array)
		{
			foreach (KeyValuePair<string, SaveableProp> propClusterDatum in keyValuePair.Value.propData.propClusterData)
			{
				dictionary.Add(propClusterDatum.Key, propClusterDatum.Value);
			}
		}
		InteractableObject[] array2 = UnityEngine.Object.FindObjectsOfType<InteractableObject>();
		foreach (InteractableObject interactableObject in array2)
		{
			if (!dictionary.ContainsKey(interactableObject.saveableData.type.ToString() + "-" + interactableObject.saveableData.runTimeID))
			{
				interactableObject.GenerateID(this);
				Vector3 vector = GetChunkKeyAtWorldLocation(interactableObject.transform.position);
				string key = vector.x + "," + vector.y + "," + vector.z;
				string key2 = interactableObject.type.ToString() + "-" + interactableObject.runTimeID;
				allChunks[key].propData.propClusterData.Add(key2, interactableObject.saveableData);
				Debug.Log("Loaded over a " + interactableObject.type.ToString() + " From other scene..");
			}
		}
		GameSettings.LEVEL_SAVE_LOADED = true;
		StartCoroutine(SaveDataEveryXMinutes(0.05f));
		StartCoroutine(TrySpawnEntityEveryFrame());
		Debug.Log("Done With Spawn Region");
	}

	private void Update()
	{
		UpdateChunks();
	}

	public string OnSave()
	{
		SaveAllObjectsAndEntities();
		WorldSaveData worldSaveData = default(WorldSaveData);
		worldSaveData.savedSeed = seed;
		worldSaveData.savedChunks = allChunks;
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
			Debug.LogException(exception);
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

	private void TryGenChunks()
	{
		newPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);
		if (newPlayerChunkLocation != oldPlayerChunkLocation && !isLoadingChunks)
		{
			isLoadingChunks = true;
			for (int i = newPlayerChunkLocation.x - viewDistance; i < newPlayerChunkLocation.x + viewDistance; i++)
			{
				for (int j = newPlayerChunkLocation.z - viewDistance; j < newPlayerChunkLocation.z + viewDistance; j++)
				{
					for (int k = (ThreeDimensional ? (newPlayerChunkLocation.y - layerDistance) : 0); k < ((!ThreeDimensional) ? 1 : (newPlayerChunkLocation.y + layerDistance)); k++)
					{
						if (GenerateChunk(i, k, j, shouldGenInstantly: false) != null)
						{
							currentChunkNumber++;
						}
					}
				}
			}
			ManageLoadedChunks();
			isLoadingChunks = false;
		}
		oldPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);
	}

	public void ManageLoadedChunks()
	{
		KeyValuePair<string, Chunk>[] array = loadedChunks.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, Chunk> keyValuePair = array[i];
			Chunk value = keyValuePair.Value;
			if (Vector3.Distance(new Vector3(value.chunkPosX, value.chunkPosY, value.chunkPosZ), GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position)) > (float)viewDistance)
			{
				KeyValuePair<string, SaveableProp>[] array2 = value.saveableData.propData.propClusterData.ToArray();
				foreach (KeyValuePair<string, SaveableProp> keyValuePair2 in array2)
				{
					UnityEngine.Object.Destroy(keyValuePair2.Value.instance.gameObject);
				}
				KeyValuePair<string, SaveableEntity>[] array3 = value.saveableData.entityData.entityClusterData.ToArray();
				foreach (KeyValuePair<string, SaveableEntity> keyValuePair3 in array3)
				{
					UnityEngine.Object.Destroy(keyValuePair3.Value.instance.gameObject);
				}
				loadedChunks.Remove(keyValuePair.Key);
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
	}

	private GameObject GenerateChunk(int chunkX, int chunkY, int chunkZ, bool shouldGenInstantly)
	{
		if (!ChunkLocationLoaded(chunkX, chunkY, chunkZ))
		{
			Chunk component = UnityEngine.Object.Instantiate(level_chunkGenerator).GetComponent<Chunk>();
			component.name = chunkX + "," + chunkY + "," + chunkZ;
			component.GetComponent<Chunk>().CreateChunk(chunkX, chunkY, chunkZ, this, shouldGenInstantly);
			component.transform.position = new Vector3((float)chunkX * ChunkSize(), (float)chunkY * ChunkHeight(), (float)chunkZ * ChunkSize());
			loadedChunks.Add(chunkX + "," + chunkY + "," + chunkZ, component);
			StartCoroutine(LoadInObjectsAndEntities(component));
			return component.gameObject;
		}
		return null;
	}

	private IEnumerator LoadInObjectsAndEntities(Chunk chunk)
	{
		yield return new WaitUntil(() => chunk.ALL_TILES_GENERATED);
		string key = chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ;
		if (allChunks.ContainsKey(chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ))
		{
			chunk.saveableData = allChunks[key];
			chunk.saveableData.instance = chunk;
			KeyValuePair<string, SaveableEntity>[] array = chunk.saveableData.entityData.entityClusterData.ToArray();
			foreach (KeyValuePair<string, SaveableEntity> keyValuePair in array)
			{
				LoadSavedEntity(keyValuePair.Value, chunk);
			}
			KeyValuePair<string, SaveableProp>[] array2 = chunk.saveableData.propData.propClusterData.ToArray();
			foreach (KeyValuePair<string, SaveableProp> keyValuePair2 in array2)
			{
				LoadSavedProp(keyValuePair2.Value, chunk);
			}
			allChunks[key] = chunk.saveableData;
		}
		else
		{
			KeyValuePair<OBJECT_TYPE, InteractableObject>[] array3 = levelProps.ToArray();
			for (int i = 0; i < array3.Length; i++)
			{
				KeyValuePair<OBJECT_TYPE, InteractableObject> keyValuePair3 = array3[i];
				float num = UnityEngine.Random.Range(0f, 0.99f);
				int index = UnityEngine.Random.Range(0, chunk.tile_grid.Count);
				Tile tile = chunk.tile_grid[index];
				UnityEngine.Random.Range(0, tile.propSpawnLocations.Count);
				if (keyValuePair3.Value.spawnChance > num && tile.propSpawnLocations.Count > 0)
				{
					int index2 = UnityEngine.Random.Range(0, tile.propSpawnLocations.Count);
					AddNewProp(tile.propSpawnLocations[index2].position, tile.propSpawnLocations[index2].rotation, keyValuePair3.Value, chunk);
				}
			}
			allChunks.Add(key, chunk.saveableData);
		}
		chunk.ALL_OBJECTS_AND_ENTITES_LOADED = true;
		SaveAllObjectsAndEntities();
	}

	public bool CheckWorldForEntityKey(string key)
	{
		using (Dictionary<string, SerealizedChunk>.Enumerator enumerator = allChunks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.entityData.entityClusterData.ContainsKey(key))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public Entity AddNewEntity(Vector3 position, Entity entity, Chunk chunk)
	{
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{
			foreach (KeyValuePair<string, SaveableEntity> entityClusterDatum in loadedChunk.Value.saveableData.entityData.entityClusterData)
			{
				if (entityClusterDatum.Value.type == entity.type)
				{
					num++;
				}
			}
			num2 += loadedChunk.Value.saveableData.entityData.entityClusterData.Count;
		}
		if (num < entity.maxAllowed && num2 <= 15)
		{
			Entity component = UnityEngine.Object.Instantiate(entity.gameObject).GetComponent<Entity>();
			component.GenerateID(this);
			component.gameObject.transform.position = position;
			chunk.saveableData.entityData.entityClusterData.Add(component.type.ToString() + "-" + component.runTimeID, component.Save());
			return component;
		}
		return null;
	}

	public Entity LoadSavedEntity(SaveableEntity entityData, Chunk chunk)
	{
		if (GameSettings.Instance.EntityDatabase.ContainsKey(entityData.type))
		{
			Entity value = null;
			GameSettings.Instance.EntityDatabase.TryGetValue(entityData.type, out value);
			Entity entity = UnityEngine.Object.Instantiate(value);
			entity.Load(entityData);
			chunk.saveableData.entityData.entityClusterData[entity.type.ToString() + "-" + entity.runTimeID] = entity.saveableData;
			return entity;
		}
		return null;
	}

	public bool RemoveEntity(string key)
	{
		using (Dictionary<string, SerealizedChunk>.Enumerator enumerator = allChunks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, SerealizedChunk> current = enumerator.Current;
				if (current.Value.entityData.entityClusterData.ContainsKey(key))
				{
					current.Value.entityData.entityClusterData.TryGetValue(key, out var value);
					UnityEngine.Object.Destroy(value.instance.gameObject);
					current.Value.entityData.entityClusterData.Remove(key);
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool CheckWorldForPropKey(string key)
	{
		using (Dictionary<string, SerealizedChunk>.Enumerator enumerator = allChunks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.propData.propClusterData.ContainsKey(key))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public InteractableObject AddNewProp(Vector3 position, Quaternion rotation, InteractableObject item, Chunk chunk)
	{
		int num = 0;
		foreach (KeyValuePair<string, SerealizedChunk> allChunk in allChunks)
		{
			num += allChunk.Value.propData.propClusterData.Count;
		}
		if (num <= 100)
		{
			InteractableObject component = UnityEngine.Object.Instantiate(item.gameObject).GetComponent<InteractableObject>();
			component.GenerateID(this);
			component.gameObject.transform.position = position;
			component.gameObject.transform.rotation = rotation;
			chunk.saveableData.propData.propClusterData.Add(component.type.ToString() + "-" + component.runTimeID, component.Save());
			return component;
		}
		return null;
	}

	public InteractableObject LoadSavedProp(SaveableProp savedObjectData, Chunk chunk)
	{
		if (GameSettings.Instance.PropDatabase.ContainsKey(savedObjectData.type))
		{
			InteractableObject value = null;
			GameSettings.Instance.PropDatabase.TryGetValue(savedObjectData.type, out value);
			InteractableObject interactableObject = UnityEngine.Object.Instantiate(value);
			interactableObject.Load(savedObjectData);
			chunk.saveableData.propData.propClusterData[interactableObject.type.ToString() + "-" + interactableObject.runTimeID] = interactableObject.saveableData;
			return interactableObject;
		}
		return null;
	}

	public bool RemoveProp(string key)
	{
		using (Dictionary<string, SerealizedChunk>.Enumerator enumerator = allChunks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, SerealizedChunk> current = enumerator.Current;
				if (current.Value.propData.propClusterData.ContainsKey(key))
				{
					current.Value.propData.propClusterData.TryGetValue(key, out var value);
					UnityEngine.Object.Destroy(value.instance.gameObject);
					current.Value.propData.propClusterData.Remove(key);
					return true;
				}
				return false;
			}
		}
		return false;
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
			if (value != null && CheckPointIntersection((float)value.chunkPosX * ChunkSize() - ChunkSize() / 2f, (float)value.chunkPosY * ChunkHeight() - ChunkHeight() / 2f, (float)value.chunkPosZ * ChunkSize() - ChunkSize() / 2f, (float)value.chunkPosX * ChunkSize() + ChunkSize() / 2f, (float)value.chunkPosY * ChunkHeight() + ChunkHeight() / 2f, (float)value.chunkPosZ * ChunkSize() + ChunkSize() / 2f, GameSettings.Instance.Player.transform.position.x, GameSettings.Instance.Player.transform.position.y, GameSettings.Instance.Player.transform.position.z))
			{
				return value;
			}
		}
		return null;
	}

	public void UpdateChunks()
	{
		if ((!GameSettings.LEVEL_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.LEVEL_GENERATED) || GameSettings.Instance.Player == null)
		{
			Debug.Log("Waiting For Level To Load");
		}
		else if (gen_enabled)
		{
			TryGenChunks();
		}
	}

	public void SaveAllObjectsAndEntities()
	{
		KeyValuePair<string, SerealizedChunk>[] array = allChunks.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, SerealizedChunk> keyValuePair = array[i];
			KeyValuePair<string, SaveableEntity>[] array2 = keyValuePair.Value.entityData.entityClusterData.ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				KeyValuePair<string, SaveableEntity> keyValuePair2 = array2[j];
				if (!(keyValuePair2.Value.instance != null))
				{
					continue;
				}
				Vector3 vector = GetChunkKeyAtWorldLocation(keyValuePair2.Value.instance.transform.position);
				string key = keyValuePair.Key;
				string text = vector.x + "," + vector.y + "," + vector.z;
				string key2 = keyValuePair2.Value.type.ToString() + "-" + keyValuePair2.Value.instance.runTimeID;
				if (keyValuePair.Key != text)
				{
					if (allChunks.ContainsKey(text))
					{
						allChunks[key].entityData.entityClusterData.Remove(key2);
						allChunks[text].entityData.entityClusterData.Add(key2, keyValuePair2.Value.instance.Save());
					}
					else
					{
						Debug.LogError("COULD NOT SAVE -> CHUNKS DOESN'T CONTAIN KEY: " + text);
					}
				}
				else if (allChunks.ContainsKey(text))
				{
					allChunks[text].entityData.entityClusterData[key2] = keyValuePair2.Value.instance.Save();
				}
				else
				{
					Debug.LogError("COULD NOT SAVE: " + keyValuePair2.Value.type.ToString() + "-" + keyValuePair2.Value.instance.runTimeID);
				}
			}
			KeyValuePair<string, SaveableProp>[] array3 = keyValuePair.Value.propData.propClusterData.ToArray();
			for (int j = 0; j < array3.Length; j++)
			{
				KeyValuePair<string, SaveableProp> keyValuePair3 = array3[j];
				if (!(keyValuePair3.Value.instance != null))
				{
					continue;
				}
				Vector3 vector2 = GetChunkKeyAtWorldLocation(keyValuePair3.Value.instance.transform.position);
				string key3 = keyValuePair.Key;
				string text2 = vector2.x + "," + vector2.y + "," + vector2.z;
				string key4 = keyValuePair3.Value.type.ToString() + "-" + keyValuePair3.Value.instance.runTimeID;
				if (keyValuePair.Key != text2)
				{
					if (allChunks.ContainsKey(text2))
					{
						allChunks[key3].propData.propClusterData.Remove(key4);
						allChunks[text2].propData.propClusterData.Add(key4, keyValuePair3.Value.instance.Save());
					}
					else
					{
						Debug.LogError("COULD NOT SAVE -> CHUNKS DOESN'T CONTAIN KEY: " + text2);
					}
				}
				else if (allChunks.ContainsKey(text2))
				{
					allChunks[text2].propData.propClusterData[key4] = keyValuePair3.Value.instance.Save();
				}
				else
				{
					Debug.LogError("COULD NOT SAVE: " + keyValuePair3.Value.type.ToString() + "-" + keyValuePair3.Value.instance.runTimeID);
				}
			}
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Shutting Down");
	}
}
