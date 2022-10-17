using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LightingBiomeColor
{
    public BIOME_ID biomeID;
    [ColorUsage(true, true)]
    public Color emissionColor;
}

public class WTTBLightData : MonoBehaviour
{
    BIOME_ID assossiatedBiome;

    public List<AudioClip> lightSounds;

    public List<LightingBiomeColor> biomeColors;

    Dictionary<BIOME_ID, Color> biomeColorsDictionary;

    public Renderer light_renderer;
    public Light Light;
    Material emissionMat;

    [ColorUsage(true, true)]
    Color setColor;

    float defaultIntensity;

    bool on = true;
    public bool broken;

    public bool hasPower;

    // Start is called before the first frame update
    void Awake()
    {
        biomeColorsDictionary = new Dictionary<BIOME_ID, Color>();

        foreach (LightingBiomeColor color in biomeColors)
        {
            biomeColorsDictionary.Add(color.biomeID, color.emissionColor);
        }

        assossiatedBiome = GetComponentInParent<Tile>().biomeID;

        //color data
        defaultIntensity = Light.intensity;
        emissionMat = light_renderer.material;

        //set default color
        setColor = emissionMat.GetColor("_EmissionColor");

        //is it broken?
        broken = Random.Range(0f, 1f) < 0.07f ? true : false;

        if (biomeColorsDictionary.Count > 0)
        {
            //set color data
            switch (assossiatedBiome)
            {
                case BIOME_ID.LEVEL_0_YELLOW_ROOMS:

                    setColor = biomeColorsDictionary[BIOME_ID.LEVEL_0_YELLOW_ROOMS];
                    Light.color = biomeColorsDictionary[BIOME_ID.LEVEL_0_YELLOW_ROOMS];
                    break;

                case BIOME_ID.LEVEL_0_RED_ROOMS:

                    setColor = biomeColorsDictionary[BIOME_ID.LEVEL_0_RED_ROOMS];
                    Light.color = biomeColorsDictionary[BIOME_ID.LEVEL_0_RED_ROOMS];
                    Light.intensity /= 4f;
                    Light.range /= 3f;
                    break;



            }
        }
        else
        {
            setColor = Color.white;
            Light.color = Color.white;
        }
       

        //set power info data
        if (GameSettings.Instance != null)

            if (GameSettings.Instance.worldInstance != null)
            {
                if (GameSettings.Instance.worldInstance.currentWorldEvent != GAMEPLAY_EVENT.LIGHTS_OUT)
                    hasPower = true;
                else
                {
                    transform.parent.GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);
                    hasPower = false;
                }
            }
            else
            {
                hasPower = true;
            }
        

        if (broken)
        {
            StartCoroutine(randomIO());

            if (lightSounds.Count > 0)
            {
                int clipChoice = Random.Range(0, lightSounds.Count);
                GetComponent<AudioSource>().clip = lightSounds[clipChoice];
                GetComponent<AudioSource>().Play();
            }

            Light.intensity = 2f;
            Light.intensity += Random.Range(-1.5f, 2.5f);

        }
        else
        {
            SetState(true);
        }

        
    }
    IEnumerator randomIO()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.025f, 2f));

            if (hasPower)
            {
                if (Light.shadows == LightShadows.Soft)
                {
                    Light.shadows = LightShadows.None;
                }

                on = !on;

                if (on)
                {
                    SetState(true);
                }
                else
                {
                    SetState(false);
                }

            }
            else
            {
                if (Light.shadows == LightShadows.None)
                {
                    Light.shadows = LightShadows.Soft;
                }

                Light.intensity = defaultIntensity / 5f;
                yield return new WaitUntil(() => GameSettings.Instance.worldInstance.currentWorldEvent != GAMEPLAY_EVENT.LIGHTS_OUT);
            }
            
        }
       

    }

    public void SetState(bool on)
    {
        if (hasPower)
        {
            if (on)
            {
                Light.enabled = true;

                emissionMat.EnableKeyword("_EMISSION");
                emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                emissionMat.SetColor("_EmissionColor", setColor);

                Light.intensity = defaultIntensity;
                Light.intensity += Random.Range(-1.5f, 2f);

            }
            else
            {
                Light.enabled = false;

                emissionMat.DisableKeyword("_EMISSION");
                emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                emissionMat.SetColor("_EmissionColor", Color.black);

                Light.intensity = defaultIntensity;
                Light.intensity += Random.Range(-1.5f, 2f);
            }
        }
        else
        {
            Light.enabled = false;

            emissionMat.DisableKeyword("_EMISSION");
            emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            emissionMat.SetColor("_EmissionColor", Color.black);

            Light.intensity = defaultIntensity;
            Light.intensity += Random.Range(-1.5f, 2f);
        }
        
       
    }
}
