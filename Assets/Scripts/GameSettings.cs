// GameSettings
using System;
using System.Collections;
using System.Collections.Generic;
using Lowscope.Saving;
using Lowscope.Saving.Components;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Steamworks;
using Lowscope.Saving.Core;
using Lowscope.Saving.Data;
using System.Linq;

public class BPUtil : MonoBehaviour
{
	/// <summary>
	/// returns GameObjects even if they are disabled
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static GameObject FindGameObject(string name)
	{
		Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
		for (int i = 0; i < objs.Length; i++)
		{
			if (objs[i].hideFlags == HideFlags.None)
			{
				if (objs[i].gameObject.name == name)
				{
					return objs[i].gameObject;
				}
			}
		}
		return null;
	}
	public static void SetAllChildrenToLayer(Transform top, int layer)
	{
		top.gameObject.layer = layer;
		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				item.gameObject.layer = layer;
				SetAllChildrenToLayer(item, layer);
			}
			else
			{
				item.gameObject.layer = layer;
			}
		}
	}
	public static void SetAllColliders(Transform top, bool OnOff)
	{
		//Destroy(top.GetComponent<Rigidbody>());

		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						if (!item.gameObject.GetComponent<Collider>().isTrigger)
							collider.enabled = OnOff;
					}

				}
				SetAllColliders(item, OnOff);
			}
			else
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						if (!item.gameObject.GetComponent<Collider>().isTrigger)
							collider.enabled = OnOff;
					}
				}
			}
		}
	}

	public static void SetAllCollidersToTrigger(Transform top)
	{
		//Destroy(top.GetComponent<Rigidbody>());

		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						collider.isTrigger = true;
					}

					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() == null)
						item.gameObject.AddComponent<WTTB_ExtraCollisionData>();
				}
				if (item.gameObject.GetComponent<Weapon>() != null)
				{
					Destroy(item.gameObject.GetComponent<Weapon>());
				}
				SetAllCollidersToTrigger(item);
			}
			else
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						collider.isTrigger = true;
					}

					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() == null)
						item.gameObject.AddComponent<WTTB_ExtraCollisionData>();
				}
				if (item.gameObject.GetComponent<Weapon>() != null)
				{
					Destroy(item.gameObject.GetComponent<Weapon>());
				}
			}
		}
	}
}
public enum SCENE
{
	INTRO,
	HOMESCREEN,
	LOADING,
	ROOM,
	LEVEL0,
	LEVEL1,
	LEVEL2,
	LEVELFUN,
	LEVELRUN,
	FOURKEYS_CLIPPINGZONE
}
public enum GAMEPLAY_EVENT
{
	NONE,
	LIGHTS_OUT
}
public class GameSettings : MonoBehaviour, ISaveable
{
	[Serializable]
	public struct SaveData
	{
		public SCENE lastSavedScene;

		public bool bloodAndGore;

		public bool ao;

		public bool bloom;

		public bool antiAliasing;

		public bool chrom;

		public bool vsync;

		public bool motionBlur;

		public bool fullScreen;

		public int screenResIndex;

		public int fov;

		public float sensitivity;

		public float masterVolume;

	}

	public static List<string> cachedSaveIdentifiers;

	public TextMeshProUGUI devModeInfo;

	//BrickmadeProductions, king, wahoo, RJC, Constant, WoodE
	public static readonly List<ulong> teamMemberSteamIDs = new List<ulong> { 76561199226044925, 76561198017133391, 76561198139743119, 76561198109625129, 76561198968340030, 76561198374741749 };

	public BackroomsLevelWorld worldInstance = null;
	public CutSceneHandler cutSceneHandler;

	private List<GenericMenu> gameMenuDatabase;

	[SerializeField]
	public List<GenericMenu> GameplayMenuDataBase => gameMenuDatabase;

	private SaveData runtimeSaveData;

	[SerializeField]
	private List<InteractableObject> propDatabase;

	public Dictionary<OBJECT_TYPE, InteractableObject> PropDatabase;

	public InventoryItem inventoryItemPrefab;

	[SerializeField]
	private List<Entity> entityDatabase;

	public Dictionary<ENTITY_TYPE, Entity> EntityDatabase;

	public static volatile bool GAME_FIRST_LOADED = true;

	public static volatile bool LEVEL_SAVE_LOADED = false;

	public static volatile bool SPAWN_REGION_GENERATED = false;

	public static volatile bool SCENE_LOADED = false;

	public static volatile bool PLAYER_DATA_LOADED = false;

	public static volatile bool WORLD_SAVING = false;

	public string activeUser;

	public Animator saveIcon;

	public AudioHandler audioHandler;

	private PostProcessVolume post;

	private Vignette vignette;

	private ChromaticAberration chrom;

	private Grain grain;

	private Bloom bloom;

	private ColorGrading colorGrading;

	private AmbientOcclusion ambientOcclusion;

	private MotionBlur motionBlur;

	public Texture2D cursor;

	public PostProcessProfile homeScreenRoomProfile;

	public PostProcessProfile homeScreenProfile;

	public PostProcessProfile level0Profile;
	public PostProcessProfile level1Profile;

	public PostProcessProfile level2Profile;

	public PostProcessProfile levelRunProfile;

	public PostProcessProfile clippingZonesProfile;

	public GameObject playerPrefab;

	private GameObject player;

	public GameObject Logo;

	public Vector2 positionOffset;

	private int sX = 1920;

	private int sY = 1080;

	private int textureRes;

	private int screenResIndex = 2;

	[SerializeField]
	public TMP_Dropdown screenResDropDown;

	private bool fullScreenEnabled = true;

	[SerializeField]
	public Toggle fullScreenButton;

	private bool vSyncEnabled;

	[SerializeField]
	public Toggle vsyncButton;

	private bool antiAliasingEnabled = true;

	[SerializeField]
	public Toggle antiAliasingButton;

	private bool ambientOcclusionEnabled = true;

	[SerializeField]
	public Toggle ambientOcclusionButton;

	private bool bloomEnabled = true;

	[SerializeField]
	public Toggle bloomButton;

	private bool chromEnabled = true;

	[SerializeField]
	public Toggle chromButton;

	private bool motionBlurEnabled = true;

	[SerializeField]
	public Toggle motionBlurButton;

	private bool vignetteEnabled = true;

	private int fov = 80;

	[SerializeField]
	public Slider fovSlider;

	private float sensitivity = 1.5f;

	[SerializeField]
	public Slider sensitivitySlider;

	private float masterVolume = 0f;

	[SerializeField]
	public Slider masterVolumeSlider;

	[SerializeField]
	public Toggle bloodAndGoreToggle;

	private bool bloodandgore = true;

	[SerializeField]
	public GameObject cheatSheetObject;
	public CheatSheet cheatSheet;

	private float musicVolume;

	private float entityVolume;

	private float AmbientVolume;

	private static int m_referenceCount = 0;

	private static GameSettings m_instance;

	private bool cutScene;

	private bool pauseMenuOpen = false;

	private bool smilerLogoOn = true;

	public GameObject mainScreen;

	public GameObject settingsScreen;

	public GameObject deleteSaveScreen;

	public SCENE ActiveScene;

	public SCENE LastSavedScene = SCENE.ROOM;
	public PostProcessVolume Post => post;

	public Vignette Vignette => vignette;

	public ChromaticAberration Chrom => chrom;

	public Grain Grain => grain;

	public Bloom Bloom => bloom;

	public ColorGrading ColorGrading => colorGrading;

	public AmbientOcclusion AbientOcclusion => ambientOcclusion;

	public MotionBlur MotionBlur => motionBlur;

	public GameObject Player => player;

	public int ScreenResIndex => screenResIndex;

	public bool FullScreenEnabled => fullScreenEnabled;

	public bool VSyncEnabled => vSyncEnabled;

	public bool AntiAliasingEnabled => antiAliasingEnabled;

	public bool AmbientOcclusionEnabled => ambientOcclusionEnabled;

	public bool BloomEnabled => bloomEnabled;

	public bool ChromEnabled => chromEnabled;

	public bool MotionBlurEnabled => motionBlurEnabled;

	public bool VignetteEnabled => vignetteEnabled;

	public int FOV => fov;

	public float Sensitivity => sensitivity;

	public float MasterVolume => masterVolume;

	public bool BloodAndGore => bloodandgore;

	public bool IsCutScene => cutScene;

	public bool PauseMenuOpen => pauseMenuOpen;

	public static GameSettings Instance => m_instance;


	private void Awake()
	{
	
		GameScreen();

		Init();

		player = null;

		positionOffset = new Vector2((float)cursor.width / 2f - 40f, (float)cursor.height / 2f - 100f);

		Cursor.SetCursor(cursor, positionOffset, CursorMode.Auto);

		m_referenceCount++;

		if (m_referenceCount > 1)
		{
			DestroyImmediate(gameObject);
			return;
		}

		m_instance = this;

		DontDestroyOnLoad(gameObject);

		


	}

    private void Init()
	{

		post = GetComponent<PostProcessVolume>();
		cutSceneHandler = GetComponent<CutSceneHandler>();
		gameMenuDatabase = new List<GenericMenu>();

		cachedSaveIdentifiers = new List<string>();

		PropDatabase = new Dictionary<OBJECT_TYPE, InteractableObject>();

		foreach (InteractableObject item in propDatabase)
		{
			PropDatabase.Add(item.type, item);
		}

		EntityDatabase = new Dictionary<ENTITY_TYPE, Entity>();

		foreach (Entity item2 in entityDatabase)
		{
			EntityDatabase.Add(item2.type, item2);
		}

	}

	public void OnLoad(string data)
	{
		StartCoroutine(OnLoadAsync(data));
	}

	private IEnumerator OnLoadAsync(string data)
	{
		while (ActiveScene == SCENE.INTRO)
		{
			yield return new WaitForEndOfFrame();
		}

		runtimeSaveData = JsonConvert.DeserializeObject<SaveData>(data);
		ConnectSettings();
		LoadSettings(runtimeSaveData);
		//set buttons
	    ambientOcclusionButton.isOn = AmbientOcclusionEnabled;
		bloomButton.isOn = BloomEnabled;
		antiAliasingButton.isOn = AntiAliasingEnabled;
		chromButton.isOn = ChromEnabled;
		vsyncButton.isOn = VSyncEnabled;
		motionBlurButton.isOn = MotionBlurEnabled;
		fullScreenButton.isOn = FullScreenEnabled;
		screenResDropDown.value = ScreenResIndex;
		fovSlider.value = FOV;
		masterVolumeSlider.value = MasterVolume;
		sensitivitySlider.value = Sensitivity;
		bloodAndGoreToggle.isOn = BloodAndGore;

		LastSavedScene = runtimeSaveData.lastSavedScene;

		setScreenRes(screenResIndex);
		setFullscreen(fullScreenEnabled);
	}

	public void OnLoadNoData()
	{
		setAmbientOcclusion(true);
		setAntiAliasing(true);
		setVignette(true);
		setBloom(true);
		setChromatic(true);
		setScreenRes(0);
		setFullscreen(true);
		setVSync(false);
		setMotionBlur(false);
		setFOV(80f);
		setMasterVolume(1f);
		//setSensitivity(data.sensitivity);
	}

	public string OnSave()
	{
		runtimeSaveData = new SaveData
		{
			ao = AmbientOcclusionEnabled,
			bloom = BloomEnabled,
			antiAliasing = AntiAliasingEnabled,
			chrom = ChromEnabled,
			vsync = VSyncEnabled,
			motionBlur = MotionBlurEnabled,
			lastSavedScene = LastSavedScene,
			fov = FOV,
			fullScreen = FullScreenEnabled,
			screenResIndex = ScreenResIndex,
			masterVolume = MasterVolume,
			sensitivity = Sensitivity,
			bloodAndGore = bloodandgore
		};
		return JsonConvert.SerializeObject(runtimeSaveData);
	}

	public void ResetGame()
    {
		StartCoroutine(ResetGameAsync());
    }
	public IEnumerator ResetGameAsync()
	{

		//dont wipe saveables because the main menu has savables active from this object

		LastSavedScene = SCENE.ROOM;
		LoadScene(SCENE.HOMESCREEN);

		yield return new WaitUntil(() => ActiveScene == SCENE.HOMESCREEN);

		SaveMaster.DeleteSave(0);

		SaveFileUtility.LoadSave(0, true);
		SaveMaster.SetSlot(0, false);

		SaveMaster.SyncSave();

	}

	public bool OnSaveCondition()
	{
		return true;
	}

	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void setCutScene(bool tf)
	{
		cutScene = tf;
	}

	public void setPauseMenuOpen(bool tf)
	{
		pauseMenuOpen = tf;
	}

	private void Update()
	{
		
		//close menu otherwise open settings
		if (Input.GetButtonDown("Esc") && !IsCutScene)
		{
			foreach (GenericMenu menu in GameplayMenuDataBase)
			{
				//toggle off the old menu
				if (menu.menuOpen)
				{
					menu.ToggleMenu();
				}
			}

			if (PauseMenuOpen)
            {
				GameScreen();

			}
			else SettingsScreen();
		}

		Logo.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * 3f, Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * -3f, 0f);
		
		if ((double)UnityEngine.Random.value >= 0.99)
		{
			smilerLogoOn = !smilerLogoOn;
			Logo.transform.GetChild(0).gameObject.SetActive(smilerLogoOn);
		}
		if (Input.GetButtonDown("CheatSheet") && ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN && teamMemberSteamIDs.Contains(SteamUser.GetSteamID().m_SteamID))
        {
			cheatSheetObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			CheatMenu(!cheatSheetObject.activeSelf);
		}

	}

	private void LoadSettings(SaveData data)
	{
		setAmbientOcclusion(data.ao);
		setAntiAliasing(data.antiAliasing);
		setVignette(vignetteEnabled);
		setBloom(data.bloom);
		setChromatic(data.chrom);
		setScreenRes(data.screenResIndex);
		setFullscreen(data.fullScreen);
		setVSync(data.vsync);
		setMotionBlur(data.motionBlur);
		setFOV(data.fov);
		setMasterVolume(data.masterVolume);
		setSensitivity(data.sensitivity);
	}

	private void ConnectSettings()
	{
		post.profile.TryGetSettings(out ambientOcclusion);
		post.profile.TryGetSettings(out vignette);
		post.profile.TryGetSettings(out chrom);
		post.profile.TryGetSettings(out grain);
		post.profile.TryGetSettings(out bloom);
		post.profile.TryGetSettings(out colorGrading);
		post.profile.TryGetSettings(out motionBlur);
	}

	public void BackFromSettings()
	{
		string text = SceneManager.GetActiveScene().name;

		if (text != null && text == "HomeScreen")
		{
			HomeScreen();
		}
		else
		{
			GameScreen();
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void HomeScreen()
	{
		setPauseMenuOpen(false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 1f;
		mainScreen.transform.gameObject.SetActive(true);
		settingsScreen.transform.gameObject.SetActive(false);
	}

	public void SettingsScreen()
	{
		if (worldInstance != null)
			StartCoroutine(SaveAllProgress());

		setPauseMenuOpen(true);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		//Time.timeScale = 0f;
		settingsScreen.transform.gameObject.SetActive(true);
		mainScreen.transform.gameObject.SetActive(false);
	}

	public void GameScreen()
	{
		setPauseMenuOpen(tf: false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1f;
		settingsScreen.transform.gameObject.SetActive(false);
		mainScreen.transform.gameObject.SetActive(false);
	}

	public void setFullscreen(bool fullscreen)
	{
		ConnectSettings();
		fullScreenEnabled = fullscreen;
		Screen.SetResolution(sX, sY, fullscreen);
	}

	public void setScreenRes(int res)
	{
		ConnectSettings();
		switch (res)
		{
			case 0:
				sX = 3840;
				sY = 2160;
				break;
			case 1:
				sX = 2560;
				sY = 1440;
				break;
			case 2:
				sX = 1920;
				sY = 1080;
				break;
			case 3:
				sX = 1280;
				sY = 720;
				break;
			case 4:
				sX = 640;
				sY = 480;
				break;
		}
		screenResIndex = res;
		Screen.SetResolution(sX, sY, fullScreenEnabled);
	}

	public void setTextureRes(int res)
	{
		ConnectSettings();
		QualitySettings.masterTextureLimit = textureRes;
	}

	public void setSensitivity(float sens)
	{
		ConnectSettings();
		sensitivity = sens;
	}

	public void setFOV(float fov)
	{
		ConnectSettings();
		this.fov = (int)fov;
		Camera.main.fieldOfView = fov;
	}

	public void setMasterVolume(float volume)
	{
		ConnectSettings();
		masterVolume = volume;
		audioHandler.master.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20f);
	}

	public void setAmbientOcclusion(bool io)
	{
		ConnectSettings();
		ambientOcclusion.active = io;
		ambientOcclusionEnabled = io;
	}

	public void setMotionBlur(bool io)
	{
		ConnectSettings();
		motionBlur.active = io;
		motionBlurEnabled = io;
	}

	public void setChromatic(bool io)
	{
		ConnectSettings();
		chrom.active = io;
		chromEnabled = io;
	}

	public void setVignette(bool io)
	{
		ConnectSettings();
		vignette.active = io;
		vignetteEnabled = io;
	}

	public void setAntiAliasing(bool io)
	{
		ConnectSettings();

		if (Camera.main.gameObject.GetComponent<PostProcessLayer>() != null)
		{
			if (!io)
			{
				antiAliasingEnabled = false;
				Camera.main.gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.None;
			}
			else
			{
				antiAliasingEnabled = true;
				Camera.main.gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
			}
		}
		antiAliasingEnabled = io;
	}

	public void setBloom(bool io)
	{
		ConnectSettings();
		bloom.active = io;
		bloomEnabled = io;
	}
	public void setVSync(bool io)
	{
		ConnectSettings();
		if (!io)
		{
			vSyncEnabled = false;
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
		}
		else
		{
			vSyncEnabled = true;
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = Screen.currentResolution.refreshRate;
		}
		vSyncEnabled = io;
	}

	public void setBloodAndGore(bool io)
    {
		bloodandgore = io;

		foreach (GameObject bloodAndGoreObject in worldInstance.globalBloodAndGoreObjects)
        {
			bloodAndGoreObject.SetActive(io);
        }
    }
	public static IEnumerator SaveAllProgress()
	{
		Instance.Player.GetComponent<PlayerController>().distance.SetMetersTraveledStats();

		Instance.saveIcon.SetBool("StopSave", false);

		Instance.saveIcon.SetTrigger("Save");

		if (Instance.worldInstance != null)
        {
			Instance.worldInstance.SaveAllObjectsAndEntitiesInChunks(Instance.worldInstance.allChunks.ToArray());
		}

		yield return new WaitUntil(() => !WORLD_SAVING);

		SaveMaster.SyncSave();
		SaveMaster.WriteActiveSaveToDisk();

		yield return new WaitUntil(() => SaveMaster.isDoneSaving);

		Debug.Log("Saved All Data!");

		Instance.saveIcon.SetBool("StopSave", true);

	}

	private void OnDestroy()
	{
		m_referenceCount--;
		if (m_referenceCount == 0)
		{
			m_instance = null;
		}
	}

	public void LoadSavedScene()
	{
		switch (LastSavedScene)
		{
			case SCENE.HOMESCREEN:
				LoadScene(SCENE.ROOM);
				break;
			case SCENE.INTRO:
				LoadScene(SCENE.ROOM);
				break;
			case SCENE.LOADING:
				LoadScene(SCENE.ROOM);
				break;
		}
		if (LastSavedScene != SCENE.INTRO && LastSavedScene != SCENE.HOMESCREEN && LastSavedScene != SCENE.LOADING && LastSavedScene != SCENE.ROOM)
		{
			LoadScene(LastSavedScene);
		}
        else
        {
			LoadScene(SCENE.ROOM);
		}
	}

	public void CheatMenu(bool io)
    {
		cheatSheetObject.SetActive(io);

		if (io && !pauseMenuOpen && !cutScene)
        {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
        else
        {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		
	}

	public void LoadScene(SCENE id)
	{
		StartCoroutine(PreLoadScene(id));
	}

	public void LoadScene(int id)
	{
		StartCoroutine(PreLoadScene((SCENE)id));

	}

	private IEnumerator PreLoadScene(SCENE id)
	{
		
		//before loading world, save level
		if (worldInstance != null)
        {
			if (id != SCENE.HOMESCREEN)
				worldInstance.OnMoveToNewLevel();

			yield return StartCoroutine(SaveAllProgress());

		}

		SCENE_LOADED = false;

		LEVEL_SAVE_LOADED = false;

		SPAWN_REGION_GENERATED = false;

		PLAYER_DATA_LOADED = false;

		yield return SceneManager.LoadSceneAsync((int)id, LoadSceneMode.Single);

		ActiveScene = id;

		//scene has been loaded
		//post load

		if (ActiveScene != 0 && player == null)
		{
			player = Instantiate(playerPrefab);
		}

		audioHandler.ResetSoundTrackLoopState();

		switch (id)
		{

			case SCENE.ROOM:

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

				//player.transform.position = new Vector3(0f, 1.1f, 0.7f);
				player.transform.position = new Vector3(0f, 2f, 0.7f);

				post.profile = homeScreenRoomProfile;

				LEVEL_SAVE_LOADED = true;
				PLAYER_DATA_LOADED = true;

				GameScreen();

				player.GetComponent<PlayerHealthSystem>().WakeUpRoom();

				break;

			case SCENE.HOMESCREEN:

				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(1).gameObject.SetActive(false);
				transform.GetChild(2).gameObject.SetActive(false);

				if (player != null)
				{
					Destroy(player);
					player = null;
				}

				player = GameObject.Find("CameraContainer");

				player.transform.GetChild(0).GetComponent<AudioSource>().Play();

				post.profile = homeScreenRoomProfile;

				LEVEL_SAVE_LOADED = true;
				PLAYER_DATA_LOADED = true;

				HomeScreen();

				break;

			case SCENE.LEVEL0:

				player.transform.position = new Vector3(0f, 0.5f, 0f);

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = level0Profile;

				GameScreen();

				break;

			case SCENE.LEVEL1:

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				player.transform.position = new Vector3(0f, 1.75f, 0f);

				post.profile = level1Profile;


				GameScreen();

				break;

			case SCENE.LEVEL2:

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = level2Profile;
				GameScreen();

				break;

			case SCENE.LEVELFUN:

				player.transform.position = new Vector3(0, 1, 0);

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

				post.profile = level0Profile;

				GameScreen();

				break;

			case SCENE.LEVELRUN:

				player.transform.position = new Vector3(0, 0, 0);

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = levelRunProfile;

				GameScreen();

				break;

			case SCENE.FOURKEYS_CLIPPINGZONE:

				player.transform.position = new Vector3(0, 1, 0);

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = clippingZonesProfile;

				GameScreen();

				break;
		}
			

		ConnectSettings();

		if (AmInSavableScene())
		{
			LastSavedScene = ActiveScene;
			audioHandler.SetUpAudio(id, ActiveScene == SCENE.LEVELRUN);
		}

		SCENE_LOADED = true;

		GAME_FIRST_LOADED = false;

		//SaveMaster.SyncLoad();
		SaveMaster.WriteActiveSaveToDisk();

		Steam.UpdateRichPresence();

		//loading screen wait for level to load
		if (worldInstance != null)
		{
			//loading screen
			yield return SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

			yield return new WaitUntil(() => SPAWN_REGION_GENERATED);

			SceneManager.UnloadScene(2);
			
		}


	}
	public bool AmInSavableScene()
	{
		if (ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN && ActiveScene != SCENE.ROOM && ActiveScene != SCENE.LOADING)
		{
			return true;
		}
		return false;
	}

	void OnApplicationQuit()
    {
		SaveAllProgress();
    }
}
