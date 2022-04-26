using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using Lowscope.Saving;
using UnityEngine.UI;
using TMPro;

public class GameSettings : MonoBehaviour, ISaveable
{

    private Dictionary<int, Entity> globalEntityList;

    public Dictionary<int, Entity> GlobalEntityList { get { return globalEntityList; } set { globalEntityList = GlobalEntityList; } }

    private List<InteractableObject> globalObjectsList;

    public List<InteractableObject> GlobalObjectsList { get { return globalObjectsList; } set { globalObjectsList = GlobalObjectsList; } }

    private List<Light> globalLightsList;

    public List<Light> GlobalLightsList { get { return globalLightsList; } set { globalLightsList = GlobalLightsList; } }


    private PostProcessVolume post;

    public PostProcessVolume Post { get { return post; } }


    private Vignette vignette;

    public Vignette Vignette { get { return vignette; } }


    private ChromaticAberration chrom;

    public ChromaticAberration Chrom { get { return chrom; } }


    private Grain grain;

    public Grain Grain { get { return grain; } }


    private Bloom bloom;

    public Bloom Bloom { get { return bloom; } }


    private ColorGrading colorGrading;

    public ColorGrading ColorGrading { get { return colorGrading; } }


    private AmbientOcclusion ambientOcclusion;

    public AmbientOcclusion AbientOcclusion { get { return ambientOcclusion; } }

    private MotionBlur motionBlur;

    public MotionBlur MotionBlur { get { return motionBlur; } }

    private AudioMixer master;

    public AudioMixer Master { get { return master; } }


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

    public GameObject playerPrefab;

    private GameObject player;
    public GameObject Player { get { return player; } }

    public GameObject Logo;



    public Vector2 positionOffset;

    private int sX = 1920, sY = 1080;

    private int textureRes;

    //settings
    public int ScreenResIndex { get { return screenResIndex; } }
    private int screenResIndex = 2; //1920, 1080

    [SerializeField]
    public TMP_Dropdown screenResDropDown;
    public bool FullScreenEnabled { get { return fullScreenEnabled; } }
    private bool fullScreenEnabled = true;

    [SerializeField]
    public Toggle fullScreenButton;
    public bool VSyncEnabled { get { return vSyncEnabled; } }
    private bool vSyncEnabled = false;

    [SerializeField]
    public Toggle vsyncButton;

    public bool AntiAliasingEnabled { get { return antiAliasingEnabled; } }
    private bool antiAliasingEnabled = true;

    [SerializeField]
    public Toggle antiAliasingButton;

    public bool AmbientOcclusionEnabled { get { return ambientOcclusionEnabled; } }
    private bool ambientOcclusionEnabled = true;

    [SerializeField]
    public Toggle ambientOcclusionButton;

    public bool BloomEnabled { get { return bloomEnabled; } }
    private bool bloomEnabled = true;

    [SerializeField]
    public Toggle bloomButton;

    public bool ChromEnabled { get { return chromEnabled; } }
    private bool chromEnabled = true;

    [SerializeField]
    public Toggle chromButton;

    public bool MotionBlurEnabled { get { return motionBlurEnabled; } }
    private bool motionBlurEnabled = true;

    [SerializeField]
    public Toggle motionBlurButton;

    public bool VignetteEnabled { get { return vignetteEnabled; } }
    private bool vignetteEnabled = true;

    //other settings
    public int FOV { get { return fov; } }
    private int fov = 80;

    [SerializeField]
    public Slider fovSlider;

    public float Sensitivity { get { return sensitivity; } }
    private float sensitivity = 1.5f;

    [SerializeField]
    public Slider sensitivitySlider;

    //volume
    public float MasterVolume { get { return masterVolume; } }
    private float masterVolume = 1f;

    [SerializeField]
    public Slider masterVolumeSlider;

    private float musicVolume;
    private float entityVolume;
    private float AmbientVolume;

    private static int m_referenceCount = 0;

    private static GameSettings m_instance;

    public static bool LEVEL_LOADED = false;

    private bool cutScene = false;
    public bool IsCutScene { get { return cutScene; } }

    private bool pauseMenuOpen = false;

    public bool PauseMenuOpen { get { return pauseMenuOpen; } }

    bool smilerLogoOn = true;

    void Awake()
    {
        GameScreen();
        Init();

        player = null;

        positionOffset = new Vector2(cursor.width / 2f - 40f, (cursor.height / 2f) - 100f);

        Cursor.SetCursor(cursor, positionOffset, CursorMode.Auto);

        m_referenceCount++;
        if (m_referenceCount > 1)
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        m_instance = this;
        // Use this line if you need the object to persist across scenes
        DontDestroyOnLoad(gameObject);

    }
    void Init()
    {
        globalEntityList = new Dictionary<int, Entity>();
        globalObjectsList = new List<InteractableObject>();
        globalLightsList = new List<Light>();

        post = GetComponent<PostProcessVolume>();

        level0Mixer = Resources.Load<AudioMixer>("Audio/Level0");
        level1Mixer = Resources.Load<AudioMixer>("Audio/Level1");
    }

    //component game save loader
    [System.Serializable]
    public struct SaveData
    {
        //game data
        public int lastSavedScene;

        //settings data
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

    //component game save loader
    public void OnLoad(string data)
    {
        StartCoroutine(OnLoadAsync(data));
    }

    IEnumerator OnLoadAsync(string data)
    {
        SaveData saveData = JsonUtility.FromJson<SaveData>(data);

        while (ActiveScene == SCENE.INTRO)
        {
            yield return new WaitForEndOfFrame();
            Debug.Log("Waiting for Level to load before loading game data...");
        }

        ConnectSettings();
        LoadSettings();

        ambientOcclusionEnabled = saveData.ao;
        ambientOcclusionButton.isOn = AmbientOcclusionEnabled;

        bloomEnabled = saveData.bloom;
        bloomButton.isOn = BloomEnabled;

        antiAliasingEnabled = saveData.antiAliasing;
        antiAliasingButton.isOn = AntiAliasingEnabled;

        chromEnabled = saveData.chrom;
        chromButton.isOn = ChromEnabled;

        vSyncEnabled = saveData.vsync;
        vsyncButton.isOn = VSyncEnabled;

        motionBlurEnabled = saveData.motionBlur;
        motionBlurButton.isOn = MotionBlurEnabled;

        fullScreenEnabled = saveData.fullScreen;
        fullScreenButton.isOn = FullScreenEnabled;

        screenResIndex = saveData.screenResIndex;
        screenResDropDown.value = ScreenResIndex;

        fov = saveData.fov;
        fovSlider.value = FOV;

        masterVolume = saveData.masterVolume;
        masterVolumeSlider.value = MasterVolume;

        sensitivity = saveData.sensitivity;
        sensitivitySlider.value = Sensitivity;

        LastSavedScene = (SCENE)saveData.lastSavedScene;

        setScreenRes(screenResIndex);
        setFullscreen(fullScreenEnabled);
        //graphics settings loaded
    }
    //component game save loader
    public string OnSave()
    {
        return JsonUtility.ToJson(new SaveData() { 

            ao = AmbientOcclusionEnabled, 
            bloom = BloomEnabled,
            antiAliasing = AntiAliasingEnabled,
            chrom = ChromEnabled,
            vsync = VSyncEnabled,
            motionBlur = MotionBlurEnabled,
            lastSavedScene = (int)LastSavedScene,
            fov = FOV,
            fullScreen = FullScreenEnabled,
            screenResIndex = ScreenResIndex,
            masterVolume = MasterVolume,
            sensitivity = Sensitivity

        });
    }

    public bool OnSaveCondition()
    {
        return true;
    }

    public static GameSettings Instance
    {
        get
        {
            return m_instance;
        }
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !cutScene)
        {
            SettingsScreen();

        }

        Logo.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * 3, Mathf.Sin(Time.realtimeSinceStartup * 0.5f) * -3, 0);

        if (Random.value >= 0.99)
        {
            smilerLogoOn = !smilerLogoOn;

            Logo.transform.GetChild(0).gameObject.SetActive(smilerLogoOn);

        }
        
    }

    void LoadSettings()
    {
        
        setAmbientOcclusion(ambientOcclusionEnabled);
        setAntiAliasing(antiAliasingEnabled);
        setVignette(vignetteEnabled);
        setBloom(bloomEnabled);
        setChromatic(chromEnabled);
        setScreenRes(screenResIndex);
        setFullscreen(fullScreenEnabled);
        setVSync(vSyncEnabled);
        setMotionBlur(motionBlurEnabled);
        setFOV(fov);
        setMasterVolume(masterVolume);
        setSensitivity(sensitivity);
        //setTextureRes(0);
    }
    void ConnectSettings()
    {
        post.profile.TryGetSettings(out ambientOcclusion);
        post.profile.TryGetSettings(out vignette);
        post.profile.TryGetSettings(out chrom);
        post.profile.TryGetSettings(out grain);
        post.profile.TryGetSettings(out bloom);
        post.profile.TryGetSettings(out colorGrading);
        post.profile.TryGetSettings(out motionBlur);
    }

    public GameObject mainScreen;
    public GameObject settingsScreen;

    public void BackFromSettings()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "HomeScreen":
                HomeScreen();
                break;

            default:
                GameScreen();
                break;
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
        Time.timeScale = 1;
        mainScreen.transform.gameObject.SetActive(true);
        settingsScreen.transform.gameObject.SetActive(false);
    }

    public void SettingsScreen()
    {
        setPauseMenuOpen(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        settingsScreen.transform.gameObject.SetActive(true);
        mainScreen.transform.gameObject.SetActive(false);
    }

    public void GameScreen()
    {
        setPauseMenuOpen(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
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
        master.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
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

            switch (io)
            {
                case false:
                    antiAliasingEnabled = false;
                    Camera.main.gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.None;
                    break;

                case true:
                    antiAliasingEnabled = true;
                    Camera.main.gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    break;
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
        switch (io)
        {
            case false:

                vSyncEnabled = false;

                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;

                break;

            case true:

                vSyncEnabled = true;

                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = Screen.currentResolution.refreshRate;

                break;
        }
        vSyncEnabled = io;

    }

    void OnDestroy()
    {
        m_referenceCount--;
        if (m_referenceCount == 0)
        {
            m_instance = null;
        }

    }

    public enum SCENE: int
    {
        INTRO = 0,
        HOMESCREEN = 1,
        ROOM = 2, 
        LEVEL0 = 3,
        LEVEL1 = 4,
        LEVEL2 = 5,
    }

    SCENE ActiveScene = SCENE.INTRO;
    SCENE LastSavedScene = SCENE.ROOM; //init with room

    public void LoadSavedScene() 
    { 
        switch (LastSavedScene)
        {
            //dont load from saved scenes if homescreen or intro was the last saved scene
            case SCENE.HOMESCREEN:
                LoadScene(SCENE.ROOM);
                break;
            case SCENE.INTRO:
                LoadScene(SCENE.ROOM);
                break;
        }
        if (LastSavedScene != SCENE.INTRO && LastSavedScene != SCENE.HOMESCREEN)
        {
            LoadScene(LastSavedScene);
        }
        
    }
    public void LoadScene(SCENE id)
    {
        StartCoroutine(PreLoadScene(id));
    }

    IEnumerator PreLoadScene(SCENE id)
    {
        LEVEL_LOADED = false;
        yield return SceneManager.LoadSceneAsync((int)id, LoadSceneMode.Single);
        PostLoadScene(id);
    }

    void PostLoadScene(SCENE id)
    {
        //homescreen or first loaded
        if (player == null || player.name == "CameraContainer")
        {
            player = Instantiate(playerPrefab);
        }

        switch (id)
        {
            case SCENE.ROOM:

                post.profile = homeScreenRoomProfile;

                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level0Ambience;
                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();

                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
                player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];

                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
                master = level0Mixer;

                GameScreen();

                break;

            case SCENE.HOMESCREEN:

                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);
               

                if (player != null)
                    Destroy(player);


                player = GameObject.Find("CameraContainer");
                player.transform.GetChild(0).GetComponent<AudioSource>().Play();

                post.profile = homeScreenProfile;
                master = level0Mixer;
                //player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level0Ambience;
                //player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();


                HomeScreen();

                break;

            case SCENE.LEVEL0:

                player.transform.position = new Vector3(0, 2f, 0);

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

                player.transform.position = new Vector3(0, 2f, 0);

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

                player.transform.position = new Vector3(8, 6.5f, 0);

                post.profile = level2Profile;


                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = level2Ambience;
                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();

                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];
                player.GetComponent<PlayerController>().feet.GetComponents<AudioSource>()[0].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];

                player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].outputAudioMixerGroup = level2Mixer.FindMatchingGroups("Master")[0];
                master = level2Mixer;

                GameScreen();

                break;
        }

        ActiveScene = id;
        if (ActiveScene != SCENE.INTRO && ActiveScene != SCENE.HOMESCREEN)
            LastSavedScene = ActiveScene;

        LEVEL_LOADED = true;
    }

    public Entity AddEntity(Vector3 position, Entity entity)
    {
        int typeAmountInGame = 0;

        foreach (KeyValuePair<int, Entity> e in GlobalEntityList)
        {
            if (e.Value.type == entity.type)
            {
                typeAmountInGame++;
            }
        }

        //only instatiate if under the amount allowed on the game, balancing reasons. Max 15 entities total around the player
        if (typeAmountInGame < entity.maxAllowed && globalEntityList.Count <= 100)
        {
            Entity spawnedEntity = Instantiate(entity);
            spawnedEntity.gameObject.transform.position = position;

            //generate a key for our entity
            spawnedEntity.ID = Random.Range(-1000, 1000);

            //regen if its already there, (this shouldnt happen EVER)
            while (GlobalEntityList.ContainsKey(spawnedEntity.ID))
                spawnedEntity.ID = Random.Range(-1000, 1000);

            GlobalEntityList.Add(spawnedEntity.ID, spawnedEntity);

            return spawnedEntity;
        }

        return null;
    }

    public InteractableObject AddItem(Vector3 position, InteractableObject item)
    {

        if (globalObjectsList.Count <= 100)
        {
            InteractableObject spawnedObject = Instantiate(item);
            spawnedObject.gameObject.transform.position = position;

            globalObjectsList.Add(item);

            return spawnedObject;
        }

        return null;
    }

   


}
