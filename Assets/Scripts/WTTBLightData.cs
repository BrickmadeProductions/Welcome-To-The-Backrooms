using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WTTBLightData : MonoBehaviour
{

    public List<AudioClip> lightSounds;

    public Renderer light_renderer;
    public Light Light;
    Material emissionMat;
    Color emissionColor;

    float defaultIntensity;

    bool on = true;
    public bool broken;

    public bool hasPower;

    // Start is called before the first frame update
    void Awake()
    {
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
        
        

        defaultIntensity = Light.intensity;
        emissionMat = light_renderer.material;
        emissionColor = emissionMat.GetColor("_EmissionColor");

        broken = Random.Range(0f, 1f) < 0.07f ? true : false;
        
        if (broken)
            StartCoroutine(randomIO());

        if (broken)
        {
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
                emissionMat.SetColor("_EmissionColor", emissionColor);

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
