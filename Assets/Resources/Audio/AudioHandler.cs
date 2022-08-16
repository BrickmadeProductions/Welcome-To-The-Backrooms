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

public class AudioHandler : MonoBehaviour
{
    Coroutine randomlyPlayTracks = null;

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
    }
    //play intro, wait, then play a random level track every 5 to 10 miunutes
    public IEnumerator playSceneSoundTrack(SCENE scene)
    {
        GetComponent<AudioSource>().Stop();
        
        SceneMusicData data;

        sceneMusicDictionary.TryGetValue(scene, out data);

        if (data.ambience != null)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = data.ambience;
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();
        }
        else
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].clip = ambience0Track;
            GameSettings.Instance.Player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[1].Play();

        }

        /*if (GameSettings.Instance.LastSavedScene != GameSettings.SCENE.HOMESCREEN)
        {
          *//*  if (data.introTrack != null)
            {
                GetComponent<AudioSource>().clip = data.introTrack;
                GetComponent<AudioSource>().Play();

                yield return new WaitUntil(() => !GetComponent<AudioSource>().isPlaying);
            }*//*
            else
            {
                GetComponent<AudioSource>().clip = intro;
                GetComponent<AudioSource>().Play();
            }
        }*/
       


        while (true)
        {

            GetComponent<AudioSource>().Stop();

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

            yield return new WaitForSeconds(Random.Range(300, 600));
        }

       

    }

    public void SetUpAudio(SCENE scene)
    {
        //setup audio data
        SceneMusicData data;

        if (sceneMusicDictionary.TryGetValue(scene, out data))
        {
            GetComponent<AudioSource>().outputAudioMixerGroup = sceneMusicDictionary[scene].levelMixer;

            foreach (AudioSource source in FindObjectsOfType<AudioSource>())
            {
                //casset player is excluded
                if (source.gameObject.GetComponent<CassetPlayer>() == null)
                {
                    if (source.gameObject.GetComponents<AudioSource>().Length > 0)
                    {
                        foreach (AudioSource mSource in source.GetComponents<AudioSource>())
                        {
                            mSource.outputAudioMixerGroup = sceneMusicDictionary[scene].levelMixer;
                        }
                    }

                    source.outputAudioMixerGroup = sceneMusicDictionary[scene].levelMixer;
                }
                
            }

            master = sceneMusicDictionary[scene].levelMixer.audioMixer;
        }

        if (randomlyPlayTracks != null)
        {
            StopCoroutine(randomlyPlayTracks);
        }
        randomlyPlayTracks = StartCoroutine(playSceneSoundTrack(scene));
       
        
    }
}
