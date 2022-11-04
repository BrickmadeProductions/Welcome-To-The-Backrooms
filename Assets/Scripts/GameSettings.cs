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
	public static void SetMeshRenderers(Transform top, bool io)
	{
		if (top.gameObject.GetComponent<Renderer>())
			top.gameObject.GetComponent<Renderer>().enabled = io;

		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Renderer>())
					item.gameObject.GetComponent<Renderer>().enabled = io;
				SetMeshRenderers(item, io);
			}
			else
			{
				if (item.gameObject.GetComponent<Renderer>())
					item.gameObject.GetComponent<Renderer>().enabled = io;
			}
		}
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
		if (top.gameObject.GetComponent<Collider>() != null)
			top.gameObject.GetComponent<Collider>().enabled = OnOff;

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
				if (!item.GetComponent<InteractableObject>())
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

[Serializable]
public struct CraftingPair
{
	public List<OBJECT_TYPE> objectsRequired;

	public OBJECT_TYPE outCome;

	public bool shouldDestroyItem1;
	public bool shouldDestroyItem2;

	public string craftingAnimation;

	public AudioClip craftingAudio;
}
public class GameSettings : MonoBehaviour
{
	//story stuff
	public AudioClipData JASAudioData;

	//

	public TextMeshProUGUI demoText;

	public static List<string> cachedSaveIdentifiers;

	public TextMeshProUGUI devModeInfo;

	//BrickmadeProductions, king, wahoo, RJC, Constant, WoodE, SCY, LeepMeep
	public static readonly List<ulong> teamMemberSteamIDs = new List<ulong> { 76561199226044925, 76561198017133391, 76561198139743119, 76561198109625129, 76561198968340030, 76561198374741749, 76561199067040929, 76561198970004846 };

	public BackroomsLevelWorld worldInstance = null;
	public CutSceneHandler cutSceneHandler;


	private List<GenericMenu> gameMenuDatabase;

	[SerializeField]
	public List<GenericMenu> GameplayMenuDataBase => gameMenuDatabase;

	public List<CraftingPair> possibleCraftingPairs;

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

	public static volatile bool WORLD_FINISHED_LOADING = false;

	public static volatile bool SCENE_FINISHED_LOADING = false;

	//keybinds
	public Button currentlySelectedKeyBindButton = null;
	Coroutine waitingForInputToKeyBind = null;

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

	public static bool isLoadingScene;

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
	
		m_referenceCount++;

		if (m_referenceCount > 1)
		{
			DestroyImmediate(gameObject);
			return;
		}

		m_instance = this;

		DontDestroyOnLoad(gameObject);

		GameScreen();

		Init();

		player = null;

		positionOffset = new Vector2((float)cursor.width / 2f - 40f, (float)cursor.height / 2f - 50f);

		Cursor.SetCursor(cursor, positionOffset, CursorMode.Auto);


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
			Debug.Log(item2.name);
			EntityDatabase.Add(item2.type, item2);
		}

		LoadInSettingsPrefs();

	}
    public void ResetSettingsPrefs()
    {

    }
	
	public void LoadInSettingsPrefs()
	{
		StartCoroutine(OnLoadAsync());
	}

	private IEnumerator OnLoadAsync()
	{
		while (ActiveScene == SCENE.INTRO)
		{
			yield return new WaitForEndOfFrame();
		}

		ConnectSettings();

		setAmbientOcclusion(PlayerPrefs.HasKey("AO") ? (PlayerPrefs.GetInt("AO") == 0 ? false : true) : true);
		setAntiAliasing(PlayerPrefs.HasKey("AA") ? (PlayerPrefs.GetInt("AA") == 0 ? false : true) : true);
		setVignette(PlayerPrefs.HasKey("VIGNETTE") ? (PlayerPrefs.GetInt("VIGNETTE") == 0 ? false : true) : true);
		setBloom(PlayerPrefs.HasKey("BLOOM") ? (PlayerPrefs.GetInt("BLOOM") == 0 ? false : true) : true);
		setChromatic(PlayerPrefs.HasKey("CHROM") ? (PlayerPrefs.GetInt("CHROM") == 0 ? false : true) : true);
		setScreenRes(PlayerPrefs.HasKey("SCREEN_RES_CHOICE") ? PlayerPrefs.GetInt("SCREEN_RES_CHOICE") : 0);
		setFullscreen(PlayerPrefs.HasKey("FULLSCREEN") ? (PlayerPrefs.GetInt("FULLSCREEN") == 0 ? false : true) : true);
		setVSync(PlayerPrefs.HasKey("VSYNC") ? (PlayerPrefs.GetInt("VSYNC") == 0 ? false : true) : false);
		setMotionBlur(PlayerPrefs.HasKey("MOTION_BLUR") ? (PlayerPrefs.GetInt("MOTION_BLUR") == 0 ? false : true) : false);
		setFOV(PlayerPrefs.HasKey("FOV") ? PlayerPrefs.GetFloat("FOV") : 80f);
		setMasterVolume(PlayerPrefs.HasKey("MASTER_VOLUME") ? PlayerPrefs.GetFloat("MASTER_VOLUME") : 1);
		setSensitivity(PlayerPrefs.HasKey("SENSITIVITY") ? PlayerPrefs.GetFloat("SENSITIVITY") : 1.5f);
		setBloodAndGore(PlayerPrefs.HasKey("BLOOD_AND_GORE") ? (PlayerPrefs.GetInt("BLOOD_AND_GORE") == 0 ? false : true) : true);
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

		LastSavedScene = PlayerPrefs.HasKey("LAST_SAVED_SCENE") ? (SCENE)PlayerPrefs.GetInt("LAST_SAVED_SCENE") : SCENE.ROOM;

		setScreenRes(screenResIndex);
		setFullscreen(fullScreenEnabled);
	}
	public void ResetGame()
    {
		StartCoroutine(ResetGameAsync());
    }
	public void DestroySaveFile()
    {
		SaveMaster.DeleteSave(0);

		PlayerPrefs.SetInt("LAST_SAVED_SCENE", (int)SCENE.ROOM);
	}
	public IEnumerator ResetGameAsync()
	{
		LastSavedScene = SCENE.ROOM;
		LoadScene(SCENE.HOMESCREEN);

		yield return new WaitUntil(() => ActiveScene == SCENE.HOMESCREEN);

		SaveMaster.DeleteSave(0);
		SaveMaster.SetSlot(0, true);

		SaveMaster.SyncLoad();
		PlayerPrefs.SetInt("LAST_SAVED_SCENE", (int)SCENE.ROOM);

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
		Time.timeScale = tf ? 0f : 1f;

		pauseMenuOpen = tf;
	}

	private void Update()
	{
		
		//close menu otherwise open settings
		if (Input.GetButtonDown("Esc") && !IsCutScene && !isLoadingScene)
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
	public void setKeyBind(Button button)
    {

		currentlySelectedKeyBindButton = button;

		if (waitingForInputToKeyBind == null)
		{
			waitingForInputToKeyBind = StartCoroutine(waitForKeyBindInput());
		}
		else
		{
			StopCoroutine(waitingForInputToKeyBind);
			waitingForInputToKeyBind = StartCoroutine(waitForKeyBindInput());

		}
		
	}
	private readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));
	IEnumerator waitForKeyBindInput()
    {
		while (true)
        {
			if (Input.anyKeyDown)
			{
				foreach (KeyCode keyCode in keyCodes)
				{
					if (Input.GetKey(keyCode))
					{
						currentlySelectedKeyBindButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = keyCode.ToString();
						break;
					}
				}
			}
			
		}
    }
	public void setFullscreen(bool fullscreen)
	{
		ConnectSettings();
		fullScreenEnabled = fullscreen;
		Screen.SetResolution(sX, sY, fullscreen);

		PlayerPrefs.SetInt("FULLSCREEN", fullscreen ? 1 : 0);
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
				sY = 360;
				break;
		}
		screenResIndex = res;
		Screen.SetResolution(sX, sY, fullScreenEnabled);

		PlayerPrefs.SetInt("SCREEN_RES_CHOICE", res);
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

		PlayerPrefs.SetFloat("SENSITIVITY", sens);
	}

	public void setFOV(float fov)
	{
		ConnectSettings();
		this.fov = (int)fov;
		Camera.main.fieldOfView = fov;

		PlayerPrefs.SetFloat("FOV", fov);
	}

	public void setMasterVolume(float volume)
	{
		ConnectSettings();
		masterVolume = volume;
		audioHandler.master.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20f);

		PlayerPrefs.SetFloat("MASTER_VOLUME", volume);
	}

	public void setAmbientOcclusion(bool io)
	{
		ConnectSettings();
		ambientOcclusion.active = io;
		ambientOcclusionEnabled = io;

		PlayerPrefs.SetInt("AO", io ? 1 : 0);
	}

	public void setMotionBlur(bool io)
	{
		ConnectSettings();
		motionBlur.active = io;
		motionBlurEnabled = io;

		PlayerPrefs.SetInt("MOTION_BLUR", io ? 1 : 0);
	}

	public void setChromatic(bool io)
	{
		ConnectSettings();
		chrom.active = io;
		chromEnabled = io;

		PlayerPrefs.SetInt("CHROM", io ? 1 : 0);
	}

	public void setVignette(bool io)
	{
		ConnectSettings();
		vignette.active = io;
		vignetteEnabled = io;

		PlayerPrefs.SetInt("VIGNETTE", io ? 1 : 0);
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

		PlayerPrefs.SetInt("AA", io ? 1 : 0);
	}

	public void setBloom(bool io)
	{
		ConnectSettings();
		bloom.active = io;
		bloomEnabled = io;

		PlayerPrefs.SetInt("BLOOM", io ? 1 : 0);
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

		PlayerPrefs.SetInt("VSYNC", io ? 1 : 0);
	}

	public void setBloodAndGore(bool io)
    {
		bloodandgore = io;

		if (Instance.worldInstance != null)
        {
			foreach (GameObject bloodAndGoreObject in worldInstance.globalBloodAndGoreObjects)
			{
				bloodAndGoreObject.SetActive(io);

			}
			foreach (Weapon weapon in FindObjectsOfType<Weapon>())
			{
				weapon.WeaponBloodRenderer.material.SetFloat("_Wetness", io ? weapon.bloodAmount : 0);
			}
		}

		PlayerPrefs.SetInt("BLOOD_AND_GORE", io ? 1 : 0);
	}
	public IEnumerator SaveAllProgress()
	{
		Player.GetComponent<PlayerController>().distance.SetMetersTraveledStats();

		saveIcon.SetBool("StopSave", false);

		saveIcon.SetTrigger("Save");

		if (worldInstance != null)
        {
			worldInstance.SaveAllObjectsAndEntitiesInChunks(worldInstance.allChunks.ToArray());
		}

		yield return new WaitUntil(() => !WORLD_SAVING);

		SaveMaster.SyncSave();
		SaveMaster.WriteActiveSaveToDisk();

		yield return new WaitUntil(() => SaveMaster.isDoneSaving);

		Debug.Log("Saved All Data!");

		saveIcon.SetBool("StopSave", true);

		PlayerPrefs.SetInt("LAST_SAVED_SCENE", (int)LastSavedScene);

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
		if (!isLoadingScene)
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
		if (!isLoadingScene)
			StartCoroutine(PreLoadScene(id));
	}

	public void LoadScene(int id)
	{
		if (!isLoadingScene)
			StartCoroutine(PreLoadScene((SCENE)id));

	}

	private IEnumerator PreLoadScene(SCENE id)
	{
		isLoadingScene = true;

		//before loading world, save level
		if (worldInstance != null)
        {

			if (id != SCENE.HOMESCREEN)
				worldInstance.OnMoveToNewLevel();
            
			//DEMO
			/*else
            {
				ResetGame();

			}
			*/
			yield return StartCoroutine(SaveAllProgress());
		}

		SCENE_LOADED = false;

		LEVEL_SAVE_LOADED = false;

		SPAWN_REGION_GENERATED = false;

		PLAYER_DATA_LOADED = false;

		WORLD_FINISHED_LOADING = false;

		SCENE_FINISHED_LOADING = false;

		yield return SceneManager.LoadSceneAsync((int)id, LoadSceneMode.Single);

		ActiveScene = id;

		//scene has been loaded
		//post load

		if (ActiveScene != 0 && player == null)
		{
			player = Instantiate(playerPrefab);

			player.GetComponent<PlayerController>().playerCamera.enabled = false;
		}

		audioHandler.ResetSoundTrackLoopState();

		switch (id)
		{

			case SCENE.ROOM:

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

				player.transform.position = new Vector3(3.5f, 1f, 0.7f);

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

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

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
		}
			
		
		audioHandler.SetUpAudio(id);

		SCENE_LOADED = true;

		GAME_FIRST_LOADED = false;

		//SaveMaster.SyncSave();
		SaveMaster.WriteActiveSaveToDisk();

		Steam.UpdateRichPresence();

		//loading screen wait for level to load
		if (worldInstance != null)
		{
			//loading screen
			yield return SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

			yield return new WaitUntil(() => WORLD_FINISHED_LOADING);

			if (worldInstance.gameplay_events_possible.Count() > 0)
			{
				StartCoroutine(worldInstance.TrackTime());
				StartCoroutine(worldInstance.TryStartRandomEvents());
			}

			StartCoroutine(worldInstance.SaveDataEveryXMinutes(5f));
			StartCoroutine(worldInstance.TrySpawnEntities());

			//DEMO
			//StartCoroutine(worldInstance.TimeLimitForDemo());

			yield return new WaitForSecondsRealtime(2f);

			SceneManager.UnloadScene(2);

			//DEMO
			//Instance.demoText.gameObject.SetActive(true);

		}
		else
        {
			Instance.demoText.gameObject.SetActive(false);
		}

		if (player.GetComponent<PlayerController>() != null)
			player.GetComponent<PlayerController>().playerCamera.enabled = true;

		SCENE_FINISHED_LOADING = true;

		isLoadingScene = false;



	}
	public bool AmInSavableScene()
	{
		if (ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN && ActiveScene != SCENE.ROOM && ActiveScene != SCENE.LOADING)
		{
			return true;
		}
		return false;
	}

    private void OnApplicationQuit()
    {
		//DEMO
		//ResetGameIMMIDIATE();
	}
}
