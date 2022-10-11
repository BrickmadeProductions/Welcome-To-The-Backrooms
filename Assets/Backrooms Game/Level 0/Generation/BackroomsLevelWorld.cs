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

	public PlayerLocationData playerData;

	public GAMEPLAY_EVENT currentWorldEventSaved;

	public Dictionary<string, SerealizedChunk> savedChunks;

	public float timeInSecondsSinceLastEventSaved;
}


[Serializable]
public struct PlayerLocationData
{
	public float[] savedPosition;

	public float[] savedNeckRotationEuler;

	public float[] savedHeadRotationEuler;

	public float[] savedBodyRotationEuler;

	public float savedRotationX;

	public float savedRotationY;

}

public class BackroomsLevelWorld : MonoBehaviour, ISaveable
{
	public PlayerLocationData playerLocationData;

	public GAMEPLAY_EVENT currentWorldEvent;

	public float timeInSecondsSinceLastEvent;
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

	[HideInInspector]
	public List<GameObject> globalBloodAndGoreObjects;

	[HideInInspector]
	public bool spawnEntities = true;
	[HideInInspector]
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
	public List<Tile> SpecialTiles;

	[SerializeField]
	public List<Tile> Tiles;

	public List<Tile> RegTiles;

	//spawn tables for random objects
	public List<ObjectWithWeight> worldPropSpawnTable;

	public List<EntityWithWeight> worldEntitySpawnTable;

	public int regTileSpace;

	public int seed;

	public GAMEPLAY_EVENT[] gameplay_events_possible;

	private void Awake()
	{
		GameSettings.Instance.worldInstance = this;

		globalBloodAndGoreObjects = new List<GameObject>();

		seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

		Pre_Init();
	}

	public IEnumerator SaveDataEveryXMinutes(float minutes)
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(minutes * 60f);
			yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
			StartCoroutine(GameSettings.SaveAllProgress());
		}
	}

	public IEnumerator TryStartRandomEvents()
	{
		yield return new WaitUntil(() => currentWorldEvent == GAMEPLAY_EVENT.NONE);

		yield return new WaitForSecondsRealtime(((12f + (UnityEngine.Random.Range(-5f, 5f)) - timeInSecondsSinceLastEvent) * 60f));

		StartEvent(gameplay_events_possible[UnityEngine.Random.Range(0, gameplay_events_possible.Length)]);

		while (true)
		{
			yield return new WaitForSecondsRealtime((30f * 60f) + UnityEngine.Random.Range(-5f, 5f));

			if (UnityEngine.Random.Range(0, 1f) < 0.5f)
			{
				StartEvent(gameplay_events_possible[UnityEngine.Random.Range(0, gameplay_events_possible.Length)]);

			}
		}
	}
	public void StartEvent(GAMEPLAY_EVENT gameplay_event)
    {
		StartCoroutine(GameSettings.SaveAllProgress());
		currentWorldEvent = gameplay_event;
		
	}

	//works
	IEnumerator TrackEventTimeStatus()
    {
		while (true)
        {
			if (currentWorldEvent == GAMEPLAY_EVENT.NONE)
			{
				yield return new WaitForSecondsRealtime(1f);
				timeInSecondsSinceLastEvent += 1f;
			}
            else
            {
				timeInSecondsSinceLastEvent = 0;
				yield return new WaitUntil(() => currentWorldEvent == GAMEPLAY_EVENT.NONE);
				

			}
		}
		
    }

	public void OnEvenStart()
    {
		foreach (Entity entity in FindObjectsOfType<Entity>())
		{
			if (entity != null)
			{
				entity.OnEventStart();

			}

		}

		switch (currentWorldEvent)
		{
			case GAMEPLAY_EVENT.LIGHTS_OUT:

				foreach (WTTBLightData backroomsLight in FindObjectsOfType<WTTBLightData>())
				{
					if (backroomsLight != null)
                    {
						backroomsLight.hasPower = false;
						backroomsLight.SetState(false);

						if (backroomsLight.gameObject.transform.parent.GetComponentInChildren<ParticleSystem>() != null)
							backroomsLight.gameObject.transform.parent.GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);

					}
					
				}

				break;

		}

	}

	public void OnEventEnd()
    {
		foreach (Entity entity in FindObjectsOfType<Entity>())
		{
			if (entity != null)
			{
				entity.OnEventEnd();

			}

		}


		switch (currentWorldEvent)
		{
			case GAMEPLAY_EVENT.LIGHTS_OUT:

				foreach (WTTBLightData backroomsLight in FindObjectsOfType<WTTBLightData>())
				{
					if (backroomsLight != null)
					{
						backroomsLight.hasPower = true;
						backroomsLight.SetState(true);

						if (backroomsLight.gameObject.transform.parent.GetComponentInChildren<ParticleSystem>() != null)
							backroomsLight.gameObject.transform.parent.GetComponentInChildren<ParticleSystem>().gameObject.SetActive(true);
					}
				}
				break;

		}

		currentWorldEvent = GAMEPLAY_EVENT.NONE;
	}
	public IEnumerator TrySpawnEntities()
	{
		while (GameSettings.Instance.ActiveScene != SCENE.INTRO && GameSettings.Instance.ActiveScene != SCENE.HOMESCREEN && spawnEntities && worldEntitySpawnTable.Count > 0)
		{
			yield return new WaitForSecondsRealtime(0.1f);

			GameObject entity = WeightedRandomSpawning.ReturnEntityBySpawnChances(worldEntitySpawnTable);

			if (entity.GetComponent<Entity>().spawnChance > UnityEngine.Random.Range(0, currentWorldEvent == GAMEPLAY_EVENT.NONE ? 0.99f : 0.25f))
			{
				Chunk chunk = loadedChunks.ElementAt(UnityEngine.Random.Range(0, loadedChunks.Count)).Value;

				string key = chunk.chunkPosX + "," + chunk.chunkPosY + "," + chunk.chunkPosZ;

				int tileToSpawnIn = UnityEngine.Random.Range(0, chunk.tile_grid.Count - 1);

				Tile tile = chunk.tile_grid[tileToSpawnIn];

				yield return new WaitUntil(() => chunk.ALL_TILES_GENERATED);

				if (tile.entitySpawnLocations.Count > 0)
				{
					AddNewEntity(tile.entitySpawnLocations[UnityEngine.Random.Range(0, tile.entitySpawnLocations.Count)].position, entity.gameObject);
				}
				else
				{
					AddNewEntity(new Vector3((float)tile.tilePos.x * chunk.tileWidth + (float)(chunk.chunkPosX * chunk_width) * chunk.tileWidth, 0f, (float)tile.tilePos.y * chunk.tileWidth + (float)(chunk.chunkPosZ * chunk_width) * chunk.tileWidth), entity);
				}

				allChunks[key] = chunk.saveableData;

			}
		}
	}

	public SCENE ReturnNextRandomLevel()
    {
		switch (GameSettings.Instance.ActiveScene)
        {
			case SCENE.LEVEL0:
            
				return SCENE.LEVEL1;
            
        }

		return SCENE.LEVEL0;
	}

	public void Pre_Init()
	{
		if (!gen_enabled)
		{
			return;
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

		yield return new WaitUntil(() => GameSettings.SCENE_LOADED);

		Debug.Log("Waiting For Player Data To Load Before Loading Chunks");	

		yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);

		Debug.Log("Level save loaded");

		StartCoroutine(GameSettings.Instance.Player.GetComponent<PlayerController>().LoadInWorldPlayerData());

		yield return new WaitUntil(() => GameSettings.PLAYER_DATA_LOADED);

		newPlayerChunkLocation = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);

		Debug.Log("Player Data save loaded");

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

		GameSettings.SPAWN_REGION_GENERATED = true;

		///load over objects from other scenes that havent been registered to this world yet from the players inventory
		foreach (InventorySlot invSlot in GameSettings.Instance.Player.GetComponent<InventorySystem>().GetAllInvSlots())
		{
			if (invSlot.itemsInSlot.Count > 0)
			{
				if (!CheckWorldForPropKey(invSlot.itemsInSlot[0].connectedObject.GetWorldID()))
				{
					Vector3 position = GetChunkKeyAtWorldLocation(GameSettings.Instance.Player.transform.position);

					string key = position.x + "," + position.y + "," + position.z;

					//load in objects from room or objects that the player has that havent been registered
					if (invSlot.itemsInSlot[0].connectedObject.runTimeID == "-1")
                    {
						invSlot.itemsInSlot[0].connectedObject.GenerateID();
						invSlot.itemsInSlot[0].connectedObject.ConnectIDToWorld(this);
					}

					allChunks[key].propData.propClusterData.Add(invSlot.itemsInSlot[0].connectedObject.GetWorldID(), invSlot.itemsInSlot[0].connectedObject.saveableData);

					Debug.Log("Loaded over a " + invSlot.itemsInSlot[0].connectedObject.type.ToString() + " From player inventory..");
				
					
				}


			}


		}
		//players inventory data has been loaded this runs after spawn chunks have generated, just add items to it
		GameSettings.Instance.Player.GetComponent<InventorySystem>().PutSavedItemsInInventory();

		if (currentWorldEvent != GAMEPLAY_EVENT.NONE)
			StartEvent(currentWorldEvent);

		if (gameplay_events_possible.Count() > 0)

			StartCoroutine(TryStartRandomEvents());

		StartCoroutine(TrackEventTimeStatus());

		

		StartCoroutine(SaveDataEveryXMinutes(5f));
		StartCoroutine(TrySpawnEntities());

		Debug.Log("Done With Spawn Region");
	}

	public string OnSave()
	{
		if (GameSettings.Instance != null)
		if (GameSettings.Instance.Player != null)
        {
			PlayerController player = GameSettings.Instance.Player.GetComponent<PlayerController>();

			float[] savedPosition = new float[3]
			{
			player.transform.position.x,
			player.transform.position.y,
			player.transform.position.z
			};
			float[] savedNeckRotationEuler = new float[3]
			{
			player.neck.transform.rotation.eulerAngles.x,
			player.neck.transform.rotation.eulerAngles.y,
			player.neck.transform.rotation.eulerAngles.z
			};
			float[] savedHeadRotationEuler = new float[3]
			{
			player.head.transform.rotation.eulerAngles.x,
			player.head.transform.rotation.eulerAngles.y,
			player.head.transform.rotation.eulerAngles.z
			};
			float[] savedBodyRotationEuler = new float[3]
			{
			player.transform.rotation.eulerAngles.x,
			player.transform.rotation.eulerAngles.y,
			player.transform.rotation.eulerAngles.z
			};
			playerLocationData = new PlayerLocationData()
			{
				savedPosition = savedPosition,
				savedNeckRotationEuler = savedNeckRotationEuler,
				savedHeadRotationEuler = savedHeadRotationEuler,
				savedBodyRotationEuler = savedBodyRotationEuler,
				savedRotationX = player.rotationX,
				savedRotationY = player.rotationY
			};
		}


		WorldSaveData worldSaveData = new WorldSaveData
		{
			playerData = playerLocationData,
			currentWorldEventSaved = currentWorldEvent,
			savedSeed = seed,
			savedChunks = allChunks,
			timeInSecondsSinceLastEventSaved = timeInSecondsSinceLastEvent
		};
		return JsonConvert.SerializeObject(worldSaveData);
	}

	private IEnumerator OnLoadAsync(string data)
	{
		yield return new WaitUntil(() => GameSettings.SCENE_LOADED);
		
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
		timeInSecondsSinceLastEvent = saveData.timeInSecondsSinceLastEventSaved;

		seed = saveData.savedSeed;

		currentWorldEvent = saveData.currentWorldEventSaved;

		UnityEngine.Random.InitState(seed);

		playerLocationData = saveData.playerData;

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
		timeInSecondsSinceLastEvent = 0f;
		UnityEngine.Random.InitState(seed);
		Debug.LogError("No Save To Load");
	}

	public bool OnSaveCondition()
	{
		if (GameSettings.SCENE_LOADED)
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

				SaveAllObjectsAndEntities();

				Destroy(chunk.gameObject);

				
			}
		}

		
	}

	private Chunk LoadInChunk(int chunkX, int chunkY, int chunkZ, bool shouldGenInstantly)
	{
		if (!ChunkLocationLoaded(chunkX, chunkY, chunkZ))
		{
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

			loadedChunks.Add(chunk.name, chunk);

			return chunk;
		}
		return null;
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

	public Entity AddNewEntity(Vector3 position, GameObject entity)
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
			

			Entity spawnedEntity = Instantiate(entity).GetComponent<Entity>();
			spawnedEntity.GenerateID(this);

			spawnedEntity.gameObject.transform.position = position;

			//automatically detect which chunk this entity is in
			Vector3 chunkVector = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(spawnedEntity.transform.position);
			string chunkKey = chunkVector.x + "," + chunkVector.y + "," + chunkVector.z;
			GameSettings.Instance.worldInstance.loadedChunks.TryGetValue(chunkKey, out Chunk chunk);

			if (chunk != null)
				chunk.saveableData.entityData.entityClusterData.Add(spawnedEntity.type.ToString() + "-" + spawnedEntity.runTimeID, spawnedEntity.Save());

			return spawnedEntity;
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

	public InteractableObject FindPropInWorldByKey(string key)
	{
		foreach (KeyValuePair<string, SerealizedChunk> chunk in allChunks)
		{

			if (chunk.Value.propData.propClusterData.ContainsKey(key))
			{
				
				return chunk.Value.propData.propClusterData[key].instance;
			}

		}
		return null;
	}

	public bool CheckWorldForPropKey(string key)
	{
		foreach (KeyValuePair<string, SerealizedChunk> chunk in allChunks)
		{

			if (chunk.Value.propData.propClusterData.ContainsKey(key))
			{
				return true;
			}

		}
		return false;
	}

	public InteractableObject AddNewProp(Vector3 position, Quaternion rotation, GameObject item)
	{
		int propCount = 0;
		foreach (KeyValuePair<string, Chunk> loadedChunks in loadedChunks)
		{
			propCount += loadedChunks.Value.saveableData.propData.propClusterData.Count;
		}
		if (propCount <= 100)
		{
			InteractableObject prop = Instantiate(item).GetComponent<InteractableObject>();

			prop.GenerateID();
			prop.ConnectIDToWorld(this);

			prop.gameObject.transform.position = position;

			prop.gameObject.transform.rotation = rotation;

			//automatically detect which chunk this item is in
			Vector3 chunkVector = GetChunkKeyAtWorldLocation(prop.transform.position);

			string chunkKey = chunkVector.x + "," + chunkVector.y + "," + chunkVector.z;

			loadedChunks.TryGetValue(chunkKey, out Chunk chunk);

			if (chunk != null)
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

			chunk.saveableData.propData.propClusterData[spawnedObject.GetComponent<InteractableObject>().GetWorldID()] = spawnedObject.GetComponent<InteractableObject>().saveableData;

			return spawnedObject.GetComponent<InteractableObject>();
		}
		
		return null;
	}

	public bool RemoveProp(string key, bool destroyObject)
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			if (loadedChunk.Value.saveableData.propData.propClusterData.ContainsKey(key))
			{
				if (destroyObject)
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
		Debug.LogError("Failed To Remove Prop! " + key);
		return false;
	}

	public void RemoveAllProps(bool destroyObjects)
	{
		foreach (KeyValuePair<string, Chunk> loadedChunk in loadedChunks)
		{

			foreach (KeyValuePair<string, SaveableProp> prop in loadedChunk.Value.saveableData.propData.propClusterData.ToList())
			{
				if (prop.Value.instance.transform != null)
					RemoveProp(prop.Key, destroyObjects);

				//Debug.Log(loadedChunk.Value.saveableData.propData.propClusterData.Remove(prop.Key));

			}

		}

		foreach (KeyValuePair<string, SerealizedChunk> allChunks in allChunks)
		{

			foreach (KeyValuePair<string, SaveableProp> prop in allChunks.Value.propData.propClusterData.ToList())
			{
				if (prop.Value.instance.transform != null)
					RemoveProp(prop.Key, destroyObjects);

				//Debug.Log(allChunks.Value.propData.propClusterData.Remove(prop.Key));

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
	public void Start()
    {
		StartCoroutine(UpdateWorld());
    }
	public IEnumerator UpdateWorld()
	{
		while (true)
        {
			if ((!GameSettings.SCENE_LOADED && !GameSettings.LEVEL_SAVE_LOADED && !GameSettings.SPAWN_REGION_GENERATED) || GameSettings.Instance.Player == null)
			{
				Debug.Log("Waiting For Level To Load");
			}
			else if (gen_enabled)
			{
				UpdateChunks();
			}

			yield return new WaitForSecondsRealtime(1f);
		}
		
	}

	public void OnMoveToNewLevel()
	{
		foreach (InventorySlot invSlot in GameSettings.Instance.Player.GetComponent<InventorySystem>().GetAllInvSlots())
		{
			if (invSlot.itemsInSlot.Count > 0)
				RemoveProp(invSlot.itemsInSlot[0].connectedObject.GetWorldID(), false);
		}
	}

	public void SaveAllObjectsAndEntities()
	{
		GameSettings.WORLD_SAVING = true;

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

						//Debug.Log(chunkData.Key + " " + chunkLocation + " " + entityID);
					}
					else
					{
						//Debug.LogError("COULD NOT SAVE -> CHUNKS DOESN'T CONTAIN KEY: " + chunkLocation);
					}
				}

				else if (allChunks.ContainsKey(chunkLocation))
				{
					allChunks[chunkLocation].entityData.entityClusterData[entityID] = entityData.Value.instance.Save();
				}

				else
				{
					//Debug.LogError("COULD NOT SAVE: " + entityData.Value.type.ToString() + "-" + entityData.Value.instance.runTimeID);
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

				//Debug.Log(chunkData.Key + " " + chunkLocation);

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
						//Debug.LogError("ERR FOR PROP: " + propKey + " COULD NOT SAVE PROP -> CHUNKS DOESN'T CONTAIN KEY: " + chunkLocation);
					}
				}
				else if (allChunks.ContainsKey(chunkLocation))
				{
					allChunks[chunkLocation].propData.propClusterData[propKey] = propData.Value.instance.Save();
				}
				else
				{
					//Debug.LogError("COULD NOT SAVE PROP: " + propData.Value.type.ToString() + "-" + propData.Value.instance.runTimeID);
				}
			}
		}
		Debug.Log("Saved All World Data...");
		GameSettings.WORLD_SAVING = false;
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Shutting Down");
	}
}
