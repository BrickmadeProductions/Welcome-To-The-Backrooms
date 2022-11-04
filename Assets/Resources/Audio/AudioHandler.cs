using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


struct SceneMusicData
{
    public AudioMixerGroup levelMixer;

    public AudioClip ambience;

    public AudioClip[] soundTracks;
}

struct EventMusicData
{
    public AudioClip eventBegin;

    public AudioClip eventTrack;

    public AudioClip eventEnd;

};

public class AudioHandler : MonoBehaviour
{
    public Coroutine playingSoundTrackLoop = null;
    public Coroutine playingEventTrackLoop = null;

    //event tracks
    public AudioClip lightsOutBegin;

    public AudioClip lightsOutTrack;

    public AudioClip lightsOutEnd;

    Dictionary<GAMEPLAY_EVENT, EventMusicData> eventMusicDictionary;

    Dictionary<SCENE, SceneMusicData> sceneMusicDictionary;

    public AudioMixer master;

    //level 0
    AudioMixerGroup level0Mixer;

    public AudioClip ambience0Track;


    public AudioClip sound0Track;
    //

    //level 1
    AudioMixerGroup level1Mixer;

    public AudioClip ambience1Track;


    public AudioClip sound1Track;
    //

    //level 2
    AudioMixerGroup level2Mixer;

    public AudioClip ambience2Track;


    public AudioClip sound2Track;
    //

    //level FUN
    AudioMixerGroup levelFUNMixer;

    public AudioClip ambienceFUNTrack;


    public AudioClip soundFUNTrack;
    //

    //level !
    AudioMixerGroup levelRUNMixer;

    public AudioClip ambienceRUNTrack;


    public AudioClip soundRUNTrack;
    //

    //level CLIPPINGZONES
    AudioMixerGroup clippingZoneMixer;

    public AudioClip[] clippingZoneTracks;

    //microphone
    public int audioSampleRate = 44100;
    public string microphone;

    private List<string> micDevices = new List<string>();

    public AudioClip RecordToAudioClip()
    {
        return Microphone.Start(microphone, false, 10, audioSampleRate);
    }
    public void StopRecording()
    {
        Microphone.End(microphone);

    }
    //


    void Awake()
    {
        PopulateMusicData();

        // get all available microphones
        foreach (string device in Microphone.devices)
        {
            if (microphone == null)
            {
                //set default mic to first mic found.
                microphone = device;
            }
            micDevices.Add(device);
        }
        microphone = micDevices[0];
        Debug.Log("Attached MICROPHONE: " + microphone);
    }

    public void PopulateMusicData()
    {
        sceneMusicDictionary = new Dictionary<SCENE, SceneMusicData>();
        eventMusicDictionary = new Dictionary<GAMEPLAY_EVENT, EventMusicData>();

        level0Mixer = Resources.Load<AudioMixer>("Audio/Level0").FindMatchingGroups("Master")[0];
        level1Mixer = Resources.Load<AudioMixer>("Audio/Level1").FindMatchingGroups("Master")[0];
        level2Mixer = Resources.Load<AudioMixer>("Audio/Level2").FindMatchingGroups("Master")[0];
        levelFUNMixer = Resources.Load<AudioMixer>("Audio/LevelFUN").FindMatchingGroups("Master")[0];
        levelRUNMixer = Resources.Load<AudioMixer>("Audio/LevelRUN").FindMatchingGroups("Master")[0];
        clippingZoneMixer = Resources.Load<AudioMixer>("Audio/ClippingZone").FindMatchingGroups("Master")[0];

        sceneMusicDictionary.Add(SCENE.LEVEL0, new SceneMusicData()
        {
            ambience = ambience0Track,
            levelMixer = level0Mixer,
            soundTracks = new AudioClip[] { sound0Track },
        });

        sceneMusicDictionary.Add(SCENE.LEVEL1, new SceneMusicData()
        {
            ambience = ambience1Track,
            levelMixer = level1Mixer,
            soundTracks = new AudioClip[] { sound1Track },
        });

        sceneMusicDictionary.Add(SCENE.LEVEL2, new SceneMusicData()
        {
            ambience = ambience2Track,
            levelMixer = level2Mixer,
            soundTracks = new AudioClip[] { sound2Track },
        });

        sceneMusicDictionary.Add(SCENE.LEVELFUN, new SceneMusicData()
        {
            ambience = ambienceFUNTrack,
            levelMixer = levelFUNMixer,
            soundTracks = new AudioClip[] { soundFUNTrack },
        }); ;

        sceneMusicDictionary.Add(SCENE.LEVELRUN, new SceneMusicData()
        {
            ambience = ambienceRUNTrack,
            levelMixer = levelRUNMixer,
            soundTracks = new AudioClip[] { soundRUNTrack },
        });

        sceneMusicDictionary.Add(SCENE.FOURKEYS_CLIPPINGZONE, new SceneMusicData()
        {
            ambience = ambienceRUNTrack,
            levelMixer = clippingZoneMixer,
            soundTracks = clippingZoneTracks,

        });


        //events
        eventMusicDictionary.Add(GAMEPLAY_EVENT.LIGHTS_OUT, new EventMusicData()
        {
            eventBegin = lightsOutBegin,
            eventTrack = lightsOutTrack,
            eventEnd = lightsOutEnd,
        });
    }
    IEnumerator SoundTrackLoop(SceneMusicData data, bool playInstantly)
    {
        GetComponent<AudioSource>().volume = 0.14f;
        GetComponent<AudioSource>().loop = false;

        if (!playInstantly)
            yield return new WaitForSecondsRealtime(Random.Range(150, 300));

        while (true)
        {
            GetComponent<AudioSource>().Stop();

            if (GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().sanity > 50f)
            {
                if (data.soundTracks.Length > 0)
                {
                    GetComponent<AudioSource>().clip = data.soundTracks[Random.Range(0, data.soundTracks.Length)];
                    GetComponent<AudioSource>().Play();
                }
                else
                {
                    GetComponent<AudioSource>().clip = sound0Track;
                    GetComponent<AudioSource>().Play();
                }

                yield return new WaitForSecondsRealtime(Random.Range(900, 3600));

            }

            yield return new WaitForSecondsRealtime(5f);
           
        }
       
    }
    public IEnumerator StartEventTrack(GAMEPLAY_EVENT gameplay_event, float timeIntoEventInSeconds, float totalEventTimeInSeconds)
    {
        SceneMusicData data = sceneMusicDictionary[GameSettings.Instance.ActiveScene];

        if (data.ambience != null)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().Stop();
        }
        else
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().Stop();

        }

        GetComponent<AudioSource>().volume = 0.75f;

        GetComponent<AudioSource>().loop = false;

        if (timeIntoEventInSeconds == 0)
        {
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().clip = eventMusicDictionary[gameplay_event].eventBegin;
            GetComponent<AudioSource>().Play();

            yield return new WaitForSecondsRealtime(1f);

        }

        GameSettings.Instance.worldInstance.OnEvenStart();

        yield return new WaitForSecondsRealtime(15f);

        GetComponent<AudioSource>().volume = 0.4f;

        float accumulatedTimeInSeconds = timeIntoEventInSeconds;        

        while (accumulatedTimeInSeconds < totalEventTimeInSeconds)
        {
            GetComponent<AudioSource>().clip = eventMusicDictionary[gameplay_event].eventTrack;
            GetComponent<AudioSource>().Play();

            float timeBetweenTrackPlaying = Random.Range(30f, 120f);
            float trackTime = GetComponent<AudioSource>().clip.length;
            
            yield return new WaitForSecondsRealtime(timeBetweenTrackPlaying + trackTime);

            accumulatedTimeInSeconds += timeBetweenTrackPlaying + trackTime;

            Debug.Log(accumulatedTimeInSeconds + " < " + totalEventTimeInSeconds);
        }      

        GetComponent<AudioSource>().volume = 0.85f;

        GetComponent<AudioSource>().clip = eventMusicDictionary[GameSettings.Instance.worldInstance.currentWorldEvent].eventEnd;
        GetComponent<AudioSource>().Play();

        yield return new WaitUntil(() => !GetComponent<AudioSource>().isPlaying);

        GameSettings.Instance.worldInstance.OnEventEnd();

        
    }
    //play intro, wait, then play a random level track every 5 to 10 miunutes
    public void StartSoundTrack(SCENE scene, bool playInstantly)
    {
        GetComponent<AudioSource>().Stop();

        SceneMusicData data = sceneMusicDictionary[GameSettings.Instance.ActiveScene];

        if (data.ambience != null)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().clip = data.ambience;
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().Play();
        }
        else
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().clip = ambience0Track;
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.GetChild(2).GetComponent<AudioSource>().Play();

        }

        if (sceneMusicDictionary.ContainsKey(scene))
        {
            data = sceneMusicDictionary[scene];
        }

        playingSoundTrackLoop = StartCoroutine(SoundTrackLoop(data, playInstantly));

   

       

    }
    public void ResetSoundTrackLoopState()
    {
        if (playingEventTrackLoop != null)
        {
            StopCoroutine(playingEventTrackLoop);
            playingEventTrackLoop = null;
        }
        if (playingSoundTrackLoop != null)
        {
            StopCoroutine(playingSoundTrackLoop);
            playingSoundTrackLoop = null;
        }

        GetComponent<AudioSource>().Stop();
    }
    public void SetUpAudio(SCENE scene)
    {

        //setup audio data
        SceneMusicData data = sceneMusicDictionary[SCENE.LEVEL0];

        if (sceneMusicDictionary.ContainsKey(scene))
        {
            data = sceneMusicDictionary[scene];
        }
        
        GetComponent<AudioSource>().outputAudioMixerGroup = data.levelMixer;

        master = data.levelMixer.audioMixer;
        
        //fix audio to current mixer
        foreach (AudioSource source in FindObjectsOfTypeAll(typeof(AudioSource)))
        {
            //casset player is excluded
            if (source.gameObject.GetComponent<CassetPlayer>() == null)
            {
                if (source.gameObject.GetComponents<AudioSource>().Length > 0)
                {
                    foreach (AudioSource mSource in source.GetComponents<AudioSource>())
                    {
                        mSource.outputAudioMixerGroup = data.levelMixer;
                    }
                }

                source.outputAudioMixerGroup = data.levelMixer;
            }

        }
        if (GameSettings.Instance.Player.GetComponent<InventorySystem>() != null)
            foreach (InventorySlot invSlot in GameSettings.Instance.Player.GetComponent<InventorySystem>().GetAllInvSlots())
            {
                if (invSlot.itemsInSlot.Count > 0)
                {
                    if (GetComponent<AudioSource>() != null)

                        GetComponent<AudioSource>().outputAudioMixerGroup = GameSettings.Instance.audioHandler.master.outputAudioMixerGroup;

                    foreach (AudioSource audioSource in invSlot.itemsInSlot[0].connectedObject.GetComponentsInChildren<AudioSource>())
                    {
                        audioSource.outputAudioMixerGroup = sceneMusicDictionary[scene].levelMixer;
                    }

                
                }


            }


        GameSettings.Instance.setMasterVolume(PlayerPrefs.GetFloat("MASTER_VOLUME"));
        //starts soundtrack for first time after world load -> BackroomsLevelWorld.cs
    }

    public void SceneSoundTrackStart(SCENE scene, bool playInstantly)
    {

        ResetSoundTrackLoopState();
        StartSoundTrack(scene, playInstantly);
    }

    public void EventSoundTrackStart(GAMEPLAY_EVENT gameplay_event, float timeIntoEvent, float howLongItLastsInSeconds)
    {
        ResetSoundTrackLoopState();
        playingEventTrackLoop = StartCoroutine(StartEventTrack(gameplay_event, timeIntoEvent, howLongItLastsInSeconds));
    }
}
