using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
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

    public int FOV { get { return fov; } }
    public float Sensitivity { get { return sensitivity; } }

    public Vector3 positionOffset = Vector3.zero;

    private int sX, sY;

    private bool fullScreen;

    //graphics
    private bool vSyncEnabled;
    private bool antiAliasingEnabled;
    private bool ambientOcclusionEnabled;
    private bool bloomEnabled;
    private bool chromEnabled;
    private bool motionBlurEnabled;
    private bool vignetteEnabled;

    //other settings
    private int fov;
    private float sensitivity;

    //volume
    private float masterVolume;
    private float musicVolume;
    private float entityVolume;
    private float AmbientVolume;

    private static int m_referenceCount = 0;

    private static GameSettings m_instance;

    public static bool LEVEL_LOADED = false;

    

    public static GameSettings Instance
    {
        get
        {
            return m_instance;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingsScreen();
        }

        
    }
    void Awake()
    {
        LoadDefaults();

        player = null;
        
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
    void LoadDefaults()
    {
        post = GetComponent<PostProcessVolume>();

        ConnectPost();

        vSyncEnabled = false;
        antiAliasingEnabled = true;
        ambientOcclusionEnabled = true;
        bloomEnabled = true;
        chromEnabled = true;
        motionBlurEnabled = false;
        vignetteEnabled = true;
        fullScreen = true;

        sX = 1920;
        sY = 1080;

        level0Mixer = Resources.Load<AudioMixer>("Audio/Level0");
        level1Mixer = Resources.Load<AudioMixer>("Audio/Level1");

        sensitivity = 1.5f;
        fov = 90;

        LoadPost();


    }

    void LoadPost()
    {
        setAmbientOcclusion(ambientOcclusionEnabled);
        setAntiAliasing(ambientOcclusionEnabled);
        setVignette(vignetteEnabled);
        setBloom(bloomEnabled);
        setChromatic(chromEnabled);
        setScreenRes(1);
        setFullscreen(fullScreen);
        setVSync(vSyncEnabled);
        setMotionBlur(motionBlurEnabled);
    }

    void ConnectPost()
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mainScreen.transform.gameObject.SetActive(true);
        settingsScreen.transform.gameObject.SetActive(false);
    }

    public void SettingsScreen()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        settingsScreen.transform.gameObject.SetActive(true);
        mainScreen.transform.gameObject.SetActive(false);
    }

    public void GameScreen()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        settingsScreen.transform.gameObject.SetActive(false);
        mainScreen.transform.gameObject.SetActive(false);
    }

    public void setFullscreen(bool fullscreen)
    {
        Screen.SetResolution(sX, sY, fullscreen);
    }

    public void setScreenRes(int res)
    {
        switch (res)
        {
            case 0:
                sX = 3840;
                sY = 2160;
                break;
            case 1:
                sX = 1920;
                sY = 1080;
                break;
            case 2:
                sX = 1280;
                sY = 720;
                break;
        }

        Screen.SetResolution(sX, sY, fullScreen);

    }

    public void setSensitivity(float sens)
    {
        sensitivity = sens;
        
    }

    public void setFOV(float fov)
    {
        this.fov = (int)fov;
        Camera.main.fieldOfView = fov;
    }

    public void setMasterVolume(float volume)
    {
        masterVolume = volume;
        master.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
    }
    public void setAmbientOcclusion(bool io)
    {
        
        ambientOcclusion.active = io;
        ambientOcclusionEnabled = io;
        ConnectPost();
    }
    public void setMotionBlur(bool io)
    {

        motionBlur.active = io;
        motionBlurEnabled = io;
        ConnectPost();
    }
    public void setChromatic(bool io)
    {
        chrom.active = io;
        chromEnabled = io;
        ConnectPost();
    }

    public void setVignette(bool io)
    {
        vignette.active = io;
        vignetteEnabled = io;
        ConnectPost();
    }


    public void setAntiAliasing(bool io)
    {
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
        ConnectPost();

    }

    public void setBloom(bool io)
    {
        bloom.active = io;
        bloomEnabled = io;
        ConnectPost();
    }

    public void setVSync(bool io)
    {
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
    
    public void LoadScene(string name)
    {
        LEVEL_LOADED = false;

        SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);

        if (SceneManager.GetActiveScene().name != name)
        {
            StartCoroutine("waitForSceneLoad", name);
        }

    }
 
    IEnumerator waitForSceneLoad(string name)
    {
        while (SceneManager.GetActiveScene().name != name)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().name == name)
        {
            PostLoadScene(name);
        }
    }

    void PostLoadScene(string name)
    {
        //homescreen or first loaded
        if (player == null || player.name == "CameraContainer")
        {
            player = Instantiate(playerPrefab);
        }

        switch (name)
        {

            case "RoomHomeScreen":

                post.profile = homeScreenRoomProfile;

                player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level0Ambience;
                player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();

                player.transform.GetChild(0).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
                player.transform.GetChild(1).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
                master = level0Mixer;

                GameScreen();

                break;

            case "Level 0":

                player.transform.position = new Vector3(0, 4.5f, 0);

                post.profile = level0Profile;

                player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level0Ambience;
                player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();

                player.transform.GetChild(0).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
                player.transform.GetChild(1).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level0Mixer.FindMatchingGroups("Master")[0];
                master = level0Mixer;

                GameScreen();

                break;

            case "Level 1":

                player.transform.position = new Vector3(15, 4, 15);

                post.profile = level1Profile;


                player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level1Ambience;
                player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();

                player.transform.GetChild(0).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
                player.transform.GetChild(1).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
                master = level1Mixer;


                GameScreen();

                break;

            case "Level 2":

                player.transform.position = new Vector3(8, 6.5f, 0);

                post.profile = level2Profile;


                player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level1Ambience;
                player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();

                player.transform.GetChild(0).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
                player.transform.GetChild(1).GetComponents<AudioSource>()[0].outputAudioMixerGroup = level1Mixer.FindMatchingGroups("Master")[0];
                master = level2Mixer;


                GameScreen();

                break;

            case "HomeScreen":

                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);

                if (player != null)
                    Destroy(player);


                player = GameObject.Find("CameraContainer");


                post.profile = homeScreenProfile;
                master = level0Mixer;
                //player.transform.GetChild(0).GetComponents<AudioSource>()[1].clip = level0Ambience;
                //player.transform.GetChild(0).GetComponents<AudioSource>()[1].Play();


                HomeScreen();

                break;
        }
        

        LEVEL_LOADED = true;

        Debug.Log("Loaded " + name);

        LoadPost();
        
    }


}
