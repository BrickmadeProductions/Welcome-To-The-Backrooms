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

	public enum SCENE
	{
		INTRO,
		HOMESCREEN,
		ROOM,
		LEVEL0,
		LEVEL1,
		LEVEL2,
		LEVELFUN,
		LEVELRUN
	}

	public BackroomsLevelWorld worldInstance = null;

	private SaveData runtimeSaveData;

	[SerializeField]
	private List<InteractableObject> propDatabase;

	public Dictionary<OBJECT_TYPE, InteractableObject> PropDatabase;

	[SerializeField]
	private List<Entity> entityDatabase;

	public Dictionary<ENTITY_TYPE, Entity> EntityDatabase;

	public static volatile bool GAME_FIRST_LOADED = true;

	public static volatile bool LEVEL_SAVE_LOADED = false;

	public static volatile bool LEVEL_GENERATED = false;

	public static volatile bool LEVEL_LOADED = false;

	public static volatile bool PLAYER_DATA_LOADED_IN_SCENE = false;

<<<<<<< Updated upstream
=======
	public string activeUser;

	public Animator saveIcon;

>>>>>>> Stashed changes
	private PostProcessVolume post;

	private Vignette vignette;

	private ChromaticAberration chrom;

	private Grain grain;

	private Bloom bloom;

	private ColorGrading colorGrading;

	private AmbientOcclusion ambientOcclusion;

	private MotionBlur motionBlur;

	private AudioMixer master;

	public Texture2D cursor;

	public PostProcessProfile homeScreenRoomProfile;

	public PostProcessProfile homeScreenProfile;

	public AudioMixer level0Mixer;

	public PostProcessProfile level0Profile;

	public AudioClip level0Ambience;

	public AudioMixer level1Mixer;

	public PostProcessProfile level1Profile;

	public AudioClip level1Ambience;

	public AudioMixer level2Mixer;

	public PostProcessProfile level2Profile;

	public AudioClip level2Ambience;

	public AudioMixer levelRunMixer;

	public PostProcessProfile levelRunProfile;

	public AudioClip levelRunAmbience;

	public GameObject playerPrefab;

	private GameObject player;

	public GameObject Logo;

	public Vector2 positionOffset;

	private int sX = 1920;

	private int sY = 1080;

	private int textureRes;

	public static bool IS_SAVING = false;

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

	private float masterVolume = 0.99f;

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

	public AudioMixer Master => master;

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
		
		cheatSheet = GetComponent<CheatSheet>();

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
    private void Start()
    {
		SaveMaster.SyncLoad();
		
	}

    private void Init()
	{
		post = GetComponent<PostProcessVolume>();
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
		level0Mixer = Resources.Load<AudioMixer>("Audio/Level0");
		level1Mixer = Resources.Load<AudioMixer>("Audio/Level1");
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
		
		Saveable[] array = SaveMaster.GetAllSavables().ToArray();
		foreach (Saveable saveable in array)
		{
			if (saveable != GetComponent<Saveable>())
			{
				SaveMaster.WipeSaveable(saveable);
			}
		}
		SaveMaster.WriteActiveSaveToDisk();
		LastSavedScene = SCENE.ROOM;
		LoadScene(SCENE.HOMESCREEN);

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
		if (Input.GetKeyDown(KeyCode.Escape) && !cutScene)
		{
			SettingsScreen();
		}
		Logo.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * 3f, Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * -3f, 0f);
		if ((double)UnityEngine.Random.value >= 0.99)
		{
			smilerLogoOn = !smilerLogoOn;
			Logo.transform.GetChild(0).gameObject.SetActive(smilerLogoOn);
		}
		if (Input.GetButtonDown("CheatSheet") && ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN)
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
<<<<<<< Updated upstream
=======
		if (worldInstance != null)
			SaveAllProgress();

>>>>>>> Stashed changes
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
		master.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20f);
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
	public static void SaveAllProgress()
	{
<<<<<<< Updated upstream
=======
		Instance.saveIcon.SetBool("StopSave", false);
		//Instance.saveIcon.gameObject.SetActive(true);
>>>>>>> Stashed changes
		IS_SAVING = true;

		SaveMaster.SyncSave();
		Debug.Log("Saved All Data!");

<<<<<<< Updated upstream
		IS_SAVING = false;
=======
		
		
		//Instance.saveIcon.gameObject.SetActive(false);
		IS_SAVING = false;

		Instance.saveIcon.SetBool("StopSave", true);

>>>>>>> Stashed changes
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
				Debug.LogError("LASTSAVEDSCENEERROR");
				LoadScene(SCENE.ROOM);
				break;
			case SCENE.INTRO:
				Debug.LogError("LASTSAVEDSCENEERROR");
				LoadScene(SCENE.ROOM);
				break;
		}
		if (LastSavedScene != 0 && LastSavedScene != SCENE.HOMESCREEN)
		{
			LoadScene(LastSavedScene);
		}
		//LoadScene(SCENE.LEVEL1);
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
		if (SceneManager.GetActiveScene().buildIndex != (int)id)
			StartCoroutine(PreLoadScene(id));
	}

	public void LoadScene(int id)
	{
		if (SceneManager.GetActiveScene().buildIndex != id)
			StartCoroutine(PreLoadScene((SCENE)id));
	}

	private IEnumerator PreLoadScene(SCENE id)
	{
		SaveAllProgress();

		if (ActiveScene != SCENE.INTRO)
			SceneManager.LoadSceneAsync(8, LoadSceneMode.Additive);

		yield return new WaitUntil(() => !IS_SAVING);

		LEVEL_LOADED = false;

		LEVEL_SAVE_LOADED = false;

		LEVEL_GENERATED = false;

		PLAYER_DATA_LOADED_IN_SCENE = false;

		/*if (Instance.worldInstance != null)
			yield return new WaitUntil(() => LEVEL_GENERATED);*/

		yield return SceneManager.LoadSceneAsync((int)id, LoadSceneMode.Single);


		//post load

		if (ActiveScene != 0 && player == null)
		{
			player = Instantiate(playerPrefab);
		}

		switch (id)
		{

			case SCENE.ROOM:

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

				//player.transform.position = new Vector3(0f, 1.1f, 0.7f);
				player.transform.position = new Vector3(0f, 3f, 0.7f);

				post.profile = homeScreenRoomProfile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level0Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];

				master = level0Mixer;

				LEVEL_SAVE_LOADED = true;
				PLAYER_DATA_LOADED_IN_SCENE = true;

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
				master = level0Mixer;

				LEVEL_SAVE_LOADED = true;
				PLAYER_DATA_LOADED_IN_SCENE = true;

				HomeScreen();

				break;

			case SCENE.LEVEL0:

				player.transform.position = new Vector3(0f, 2.5f, 0f);

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = level0Profile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level0Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];

				master = level0Mixer;
				GameScreen();

				break;

			case SCENE.LEVEL1:

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				player.transform.position = new Vector3(0f, 2f, 0f);

				post.profile = level1Profile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level1Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];

				master = level1Mixer;

				GameScreen();

				break;

			case SCENE.LEVEL2:

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = level2Profile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level2Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];

				master = level2Mixer;

				GameScreen();

				break;

			case SCENE.LEVELFUN:

				player.transform.position = new Vector3(0, 1, 0);

				player.GetComponent<PlayerController>().darkShield.SetActive(false);

				post.profile = level0Profile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level0Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];

				master = level0Mixer;

				GameScreen();

				break;

			case SCENE.LEVELRUN:

				player.transform.position = new Vector3(0, 1, 0);

				player.GetComponent<PlayerController>().darkShield.SetActive(true);

				post.profile = levelRunProfile;

				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level0Ambience;
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];

				master = level0Mixer;

				GameScreen();

				break;
		}
		ConnectSettings();
		ActiveScene = id;

		if (AmInSavableScene())
		{
			LastSavedScene = ActiveScene;
		}

		LEVEL_LOADED = true;

		if (ActiveScene != SCENE.ROOM && player.GetComponent<PlayerHealthSystem>() != null)
		{
			player.GetComponent<PlayerHealthSystem>().WakeUpOther();
		}

		GAME_FIRST_LOADED = false;
		
		SaveMaster.SyncLoad();


	}


	public bool AmInSavableScene()
	{
		if (ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN && ActiveScene != SCENE.ROOM)
		{
			return true;
		}
		return false;
	}
}
