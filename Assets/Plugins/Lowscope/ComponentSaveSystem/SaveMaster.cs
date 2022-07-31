// Lowscope.Saving.SaveMaster
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Lowscope.Saving;
using Lowscope.Saving.Components;
using Lowscope.Saving.Core;
using Lowscope.Saving.Data;
using Lowscope.Saving.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("")]
[DefaultExecutionOrder(-9015)]
public class SaveMaster : MonoBehaviour
{
	private static SaveMaster instance;

	private static GameObject saveMasterTemplate;

	private static Dictionary<string, int> loadedSceneNames = new Dictionary<string, int>();

	private static HashSet<int> duplicatedSceneHandles = new HashSet<int>();

	private static Dictionary<int, SaveInstanceManager> saveInstanceManagers = new Dictionary<int, SaveInstanceManager>();

	private static bool isQuittingGame;

	public static bool isDoneSaving = false;

	private static SaveGame activeSaveGame = null;

	private static int activeSlot = -1;

	private static List<Saveable> saveables = new List<Saveable>();

	private Action<int> onSlotChangeBegin = delegate
	{
	};

	private Action<int> onSlotChangeDone = delegate
	{
	};

	private Action<int> onWritingToDiskBegin = delegate
	{
	};

	private Action<int> onWritingToDiskDone = delegate
	{
	};

	public static Action<int> OnSlotChangeBegin
	{
		get
		{
			return instance.onSlotChangeBegin;
		}
		set
		{
			instance.onSlotChangeBegin = value;
		}
	}

	public static Action<int> OnSlotChangeDone
	{
		get
		{
			return instance.onSlotChangeDone;
		}
		set
		{
			instance.onSlotChangeDone = value;
		}
	}

	public static Action<int> OnWritingToDiskBegin
	{
		get
		{
			return instance.onWritingToDiskBegin;
		}
		set
		{
			instance.onWritingToDiskBegin = value;
		}
	}

	public static Action<int> OnWritingToDiskDone
	{
		get
		{
			return instance.onWritingToDiskDone;
		}
		set
		{
			instance.onWritingToDiskDone = value;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void CreateInstance()
	{
		GameObject obj = new GameObject("Save Master");
		obj.AddComponent<SaveMaster>();
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		UnityEngine.Object.DontDestroyOnLoad(obj);
	}

	public static List<Saveable> GetAllSavables()
	{
		return saveables;
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		if (activeSaveGame != null)
		{
			if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
			{
				duplicatedSceneHandles.Remove(scene.GetHashCode());
			}
			else if (loadedSceneNames.ContainsKey(scene.name))
			{
				loadedSceneNames.Remove(scene.name);
			}
			if (saveInstanceManagers.ContainsKey(scene.GetHashCode()))
			{
				saveInstanceManagers.Remove(scene.GetHashCode());
			}
		}
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
	{
		if (activeSaveGame != null)
		{
			if (!loadedSceneNames.ContainsKey(scene.name))
			{
				loadedSceneNames.Add(scene.name, scene.GetHashCode());
			}
			else
			{
				duplicatedSceneHandles.Add(scene.GetHashCode());
			}
			if (!string.IsNullOrEmpty(activeSaveGame.Get($"SaveMaster-{scene.name}-IM")) && !saveInstanceManagers.ContainsKey(scene.GetHashCode()))
			{
				SpawnInstanceManager(scene);
			}
		}
	}

	public static SaveInstanceManager SpawnInstanceManager(Scene scene, string id = "")
	{
		if (!string.IsNullOrEmpty(id) && duplicatedSceneHandles.Contains(scene.GetHashCode()))
		{
			duplicatedSceneHandles.Remove(scene.GetHashCode());
		}
		if (saveInstanceManagers.ContainsKey(scene.GetHashCode()))
		{
			return null;
		}
		GameObject obj = new GameObject("Save Instance Manager");
		obj.gameObject.SetActive(value: false);
		SaveInstanceManager saveInstanceManager = obj.AddComponent<SaveInstanceManager>();
		Saveable saveable = obj.AddComponent<Saveable>();
		SceneManager.MoveGameObjectToScene(obj, scene);
		string text = (string.IsNullOrEmpty(id) ? scene.name : $"{scene.name}-{id}");
		saveable.SaveIdentification = string.Format("{0}-{1}", "SaveMaster", text);
		saveable.AddSaveableComponent("IM", saveInstanceManager, reloadData: true);
		saveInstanceManagers.Add(scene.GetHashCode(), saveInstanceManager);
		saveInstanceManager.SceneID = text;
		saveInstanceManager.Saveable = saveable;
		obj.gameObject.SetActive(value: true);
		return saveInstanceManager;
	}

	public static bool DeactivatedObjectExplicitly(GameObject gameObject)
	{
		if (gameObject.scene.isLoaded)
		{
			return !isQuittingGame;
		}
		return false;
	}

	public static int GetActiveSlot()
	{
		return activeSlot;
	}

	public static bool HasUnusedSlots()
	{
		return SaveFileUtility.GetAvailableSaveSlot() != -1;
	}

	public static int[] GetUsedSlots()
	{
		return SaveFileUtility.GetUsedSlots();
	}

	public static bool IsSlotUsed(int slot)
	{
		return SaveFileUtility.IsSlotUsed(slot);
	}

	public static bool SetSlotToLastUsedSlot(bool notifyListeners)
	{
		int @int = PlayerPrefs.GetInt("SM-LastUsedSlot", -1);
		if (@int == -1)
		{
			return false;
		}
		SetSlot(@int, notifyListeners);
		return true;
	}

	public static bool SetSlotToNewSlot(bool notifyListeners, out int slot)
	{
		int availableSaveSlot = SaveFileUtility.GetAvailableSaveSlot();
		if (availableSaveSlot == -1)
		{
			slot = -1;
			return false;
		}
		SetSlot(availableSaveSlot, notifyListeners);
		slot = availableSaveSlot;
		return true;
	}

	public static void ClearSlot(bool clearAllListeners = true, bool notifySave = true)
	{
		if (clearAllListeners)
		{
			ClearListeners(notifySave);
		}
		activeSlot = -1;
		activeSaveGame = null;
	}

	public static void SetSlotAndCopyActiveSave(int slot)
	{
		OnSlotChangeBegin(slot);
		activeSlot = slot;
		activeSaveGame = SaveFileUtility.LoadSave(slot, createIfEmpty: true);
		SyncReset();
		SyncSave();
		OnSlotChangeDone(slot);
	}

	public static void SetSlot(int slot, bool reloadSaveables, SaveGame saveGame = null)
	{
		if (activeSlot == slot && saveGame == null)
		{
			UnityEngine.Debug.LogWarning("Already loaded this slot.");
			return;
		}
		if (SaveSettings.Get().autoSaveOnSlotSwitch && activeSaveGame != null)
		{
			WriteActiveSaveToDisk();
		}
		if (SaveSettings.Get().cleanSavedPrefabsOnSlotSwitch)
		{
			ClearActiveSavedPrefabs();
		}
		if (slot < 0 || slot > SaveSettings.Get().maxSaveSlotCount)
		{
			UnityEngine.Debug.LogWarning("SaveMaster: Attempted to set illegal slot.");
			return;
		}
		OnSlotChangeBegin(slot);
		activeSlot = slot;
		activeSaveGame = ((saveGame == null) ? SaveFileUtility.LoadSave(slot, createIfEmpty: true) : saveGame);
		if (reloadSaveables)
		{
			SyncLoad();
		}
		SyncReset();
		PlayerPrefs.SetInt("SM-LastUsedSlot", slot);
		OnSlotChangeDone(slot);
	}

	public static DateTime GetSaveCreationTime(int slot)
	{
		if (slot == activeSlot)
		{
			return activeSaveGame.creationDate;
		}
		if (!IsSlotUsed(slot))
		{
			return default(DateTime);
		}
		return GetSave(slot).creationDate;
	}

	public static DateTime GetSaveCreationTime()
	{
		return GetSaveCreationTime(activeSlot);
	}

	public static TimeSpan GetSaveTimePlayed(int slot)
	{
		if (slot == activeSlot)
		{
			return activeSaveGame.timePlayed;
		}
		if (!IsSlotUsed(slot))
		{
			return default(TimeSpan);
		}
		return GetSave(slot).timePlayed;
	}

	public static TimeSpan GetSaveTimePlayed()
	{
		return GetSaveTimePlayed(activeSlot);
	}

	public static int GetSaveVersion(int slot)
	{
		if (slot == activeSlot)
		{
			return activeSaveGame.gameVersion;
		}
		if (!IsSlotUsed(slot))
		{
			return -1;
		}
		return GetSave(slot).gameVersion;
	}

	public static int GetSaveVersion()
	{
		return GetSaveVersion(activeSlot);
	}

	private static SaveGame GetSave(int slot, bool createIfEmpty = true)
	{
		if (slot == activeSlot)
		{
			return activeSaveGame;
		}
		return SaveFileUtility.LoadSave(slot, createIfEmpty);
	}

	public static void WriteActiveSaveToDisk()
	{
		isDoneSaving = false;
		OnWritingToDiskBegin(activeSlot);
		
		if (activeSaveGame != null)
		{
			for (int i = 0; i < saveables.Count; i++)
			{
				saveables[i].OnSaveRequest(activeSaveGame);
			}
			SaveFileUtility.WriteSave(activeSaveGame, activeSlot);
		}
		else if (Time.frameCount != 0)
		{
			UnityEngine.Debug.Log("No save game is currently loaded... So we cannot save it");
		}
		
		OnWritingToDiskDone(activeSlot);
		isDoneSaving = true;
	}

	public static void WipeSceneData(string name, bool clearSceneSaveables = true)
	{
		if (activeSaveGame == null)
		{
			UnityEngine.Debug.LogError("Failed to wipe scene data: No save game loaded.");
			return;
		}
		if (clearSceneSaveables)
		{
			for (int num = saveables.Count - 1; num >= 0; num--)
			{
				if (saveables[num].gameObject.scene.name == name)
				{
					saveables[num].WipeData(activeSaveGame);
				}
			}
		}
		activeSaveGame.WipeSceneData(name);
	}

	public static void WipeSaveable(Saveable saveable)
	{
		if (activeSaveGame == null)
		{
			UnityEngine.Debug.LogError("Failed to wipe scene data: No save game loaded.");
		}
		else
		{
			saveable.WipeData(activeSaveGame);
		}
	}

	public static void ClearListeners(bool notifySave)
	{
		if (notifySave && activeSaveGame != null)
		{
			for (int num = saveables.Count - 1; num >= 0; num--)
			{
				saveables[num].OnSaveRequest(activeSaveGame);
			}
		}
		saveables.Clear();
	}

	public static void ReloadListener(Saveable saveable)
	{
		saveable.OnLoadRequest(activeSaveGame);
	}

	public static void AddListener(Saveable saveable)
	{
		if (saveable != null && activeSaveGame != null)
		{
			saveable.OnLoadRequest(activeSaveGame);
		}
		saveables.Add(saveable);
	}

	public static void AddListener(Saveable saveable, bool loadData)
	{
		if (loadData)
		{
			AddListener(saveable);
		}
		else
		{
			saveables.Add(saveable);
		}
	}

	public static void RemoveListener(Saveable saveable)
	{
		if (saveables.Remove(saveable) && saveable != null && activeSaveGame != null)
		{
			saveable.OnSaveRequest(activeSaveGame);
		}
	}

	public static void RemoveListener(Saveable saveable, bool saveData)
	{
		if (saveData)
		{
			RemoveListener(saveable);
		}
		else
		{
			saveables.Remove(saveable);
		}
	}

	public static void DeleteSave(int slot)
	{
		SaveFileUtility.DeleteSave(slot);
		if (slot == activeSlot)
		{
			activeSlot = -1;
			activeSaveGame = null;
		}
	}

	public static void DeleteSave()
	{
		DeleteSave(activeSlot);
	}

	public static void SyncSave()
	{
		if (activeSaveGame == null)
		{
			UnityEngine.Debug.LogWarning("SaveMaster Request Save Failed: No active SaveGame has been set. Be sure to call SetSaveGame(index)");
			return;
		}
		int count = saveables.Count;
		for (int i = 0; i < count; i++)
		{
			saveables[i].OnSaveRequest(activeSaveGame);
		}
	}

	public static void SyncLoad()
	{
		if (activeSaveGame == null)
		{
			UnityEngine.Debug.LogWarning("SaveMaster Request Load Failed: No active SaveGame has been set. Be sure to call SetSlot(index)");
			return;
		}
		int count = saveables.Count;
		for (int i = 0; i < count; i++)
		{
			saveables[i].OnLoadRequest(activeSaveGame);
		}
	}

	public static void SyncReset()
	{
		if (activeSaveGame == null)
		{
			UnityEngine.Debug.LogWarning("SaveMaster Request Load Failed: No active SaveGame has been set. Be sure to call SetSlot(index)");
			return;
		}
		int count = saveables.Count;
		for (int i = 0; i < count; i++)
		{
			saveables[i].ResetState();
		}
	}

	public static GameObject SpawnSavedPrefab(InstanceSource source, string filePath, Scene scene = default(Scene))
	{
		if (!HasActiveSaveLogAction("Spawning Object"))
		{
			return null;
		}
		if (scene == default(Scene))
		{
			scene = SceneManager.GetActiveScene();
		}
		if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
		{
			UnityEngine.Debug.Log($"Following scene has a duplicate name: {scene.name}. Ensure to call SaveMaster.SpawnInstanceManager(scene, id) with a custom ID after spawning the scene.");
			scene = SceneManager.GetActiveScene();
		}
		if (!saveInstanceManagers.TryGetValue(scene.GetHashCode(), out var value))
		{
			value = SpawnInstanceManager(scene);
		}
		return value.SpawnObject(source, filePath).gameObject;
	}

	public static bool GetSaveableData<T>(int slot, string saveableId, string componentId, out T data)
	{
		if (!IsSlotUsed(slot))
		{
			data = default(T);
			return false;
		}
		SaveGame save = GetSave(slot, createIfEmpty: false);
		if (save == null)
		{
			data = default(T);
			return false;
		}
		string text = save.Get($"{saveableId}-{componentId}");
		if (!string.IsNullOrEmpty(text))
		{
			data = JsonUtility.FromJson<T>(text);
			if (data != null)
			{
				return true;
			}
		}
		data = default(T);
		return false;
	}

	public static bool GetSaveableData<T>(string saveableId, string componentId, out T data)
	{
		if (activeSlot == -1)
		{
			data = default(T);
			return false;
		}
		return GetSaveableData<T>(activeSlot, saveableId, componentId, out data);
	}

	public static void SetInt(string key, int value)
	{
		if (HasActiveSaveLogAction("Set Int"))
		{
			activeSaveGame.Set($"IVar-{key}", value.ToString(), "Global");
		}
	}

	public static int GetInt(string key, int defaultValue = -1)
	{
		if (!HasActiveSaveLogAction("Get Int"))
		{
			return defaultValue;
		}
		string text = activeSaveGame.Get($"IVar-{key}");
		if (!string.IsNullOrEmpty(text))
		{
			return int.Parse(text);
		}
		return defaultValue;
	}

	public static void SetFloat(string key, float value)
	{
		if (HasActiveSaveLogAction("Set Float"))
		{
			activeSaveGame.Set($"FVar-{key}", value.ToString(), "Global");
		}
	}

	public static float GetFloat(string key, float defaultValue = -1f)
	{
		if (!HasActiveSaveLogAction("Get Float"))
		{
			return defaultValue;
		}
		string text = activeSaveGame.Get($"FVar-{key}");
		if (!string.IsNullOrEmpty(text))
		{
			return float.Parse(text);
		}
		return defaultValue;
	}

	public static void SetString(string key, string value)
	{
		if (HasActiveSaveLogAction("Set String"))
		{
			activeSaveGame.Set($"SVar-{key}", value, "Global");
		}
	}

	public static string GetString(string key, string defaultValue = "")
	{
		if (!HasActiveSaveLogAction("Get String"))
		{
			return defaultValue;
		}
		string text = activeSaveGame.Get($"SVar-{key}");
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return defaultValue;
	}

	private static bool HasActiveSaveLogAction(string action)
	{
		if (GetActiveSlot() == -1)
		{
			UnityEngine.Debug.LogWarning($"{action} Failed: no save slot set. Please call SetSaveSlot(int index)");
			return false;
		}
		return true;
	}

	private static void ClearActiveSavedPrefabs()
	{
		int sceneCount = SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (saveInstanceManagers.TryGetValue(sceneAt.GetHashCode(), out var value))
			{
				value.DestroyAllObjects();
			}
		}
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Debug.LogWarning("Duplicate save master found. Ensure that the save master has not been added anywhere in your scene.");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		SaveSettings saveSettings = SaveSettings.Get();
		if (saveSettings.loadDefaultSlotOnStart)
		{
			SetSlot(saveSettings.defaultSlot, reloadSaveables: true);
		}
		if (saveSettings.trackTimePlayed)
		{
			StartCoroutine(IncrementTimePlayed());
		}
		if (saveSettings.useHotkeys)
		{
			StartCoroutine(TrackHotkeyUsage());
		}
	}

	private IEnumerator TrackHotkeyUsage()
	{
		SaveSettings settings = SaveSettings.Get();
		while (true)
		{
			yield return null;
			if (settings.useHotkeys)
			{
				if (Input.GetKeyDown(settings.wipeActiveSceneData))
				{
					WipeSceneData(SceneManager.GetActiveScene().name);
				}
				if (Input.GetKeyDown(settings.saveAndWriteToDiskKey))
				{
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					WriteActiveSaveToDisk();
					stopwatch.Stop();
					UnityEngine.Debug.Log($"Synced objects & Witten game to disk. MS: {stopwatch.ElapsedMilliseconds.ToString()}");
				}
				if (Input.GetKeyDown(settings.syncSaveGameKey))
				{
					Stopwatch stopwatch2 = new Stopwatch();
					stopwatch2.Start();
					SyncSave();
					stopwatch2.Stop();
					UnityEngine.Debug.Log($"Synced (Save) objects. MS: {stopwatch2.ElapsedMilliseconds.ToString()}");
				}
				if (Input.GetKeyDown(settings.syncLoadGameKey))
				{
					Stopwatch stopwatch3 = new Stopwatch();
					stopwatch3.Start();
					SyncLoad();
					stopwatch3.Stop();
					UnityEngine.Debug.Log($"Synced (Load) objects. MS: {stopwatch3.ElapsedMilliseconds.ToString()}");
				}
			}
		}
	}

	private IEnumerator IncrementTimePlayed()
	{
		WaitForSeconds incrementSecond = new WaitForSeconds(1f);
		while (true)
		{
			yield return incrementSecond;
			if (activeSlot >= 0)
			{
				activeSaveGame.timePlayed = activeSaveGame.timePlayed.Add(TimeSpan.FromSeconds(1.0));
			}
		}
	}

	private void OnApplicationQuit()
	{
		if (SaveSettings.Get().autoSaveOnExit)
		{
			isQuittingGame = true;
			WriteActiveSaveToDisk();
		}
	}
}
