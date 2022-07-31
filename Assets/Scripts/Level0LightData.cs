using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Level0LightData : MonoBehaviour
{

    public List<AudioClip> lightSounds;

    new Renderer renderer;
    Material emissionMat;
    Color emissionColor;

    float defaultIntensity;

    bool on = true;
    public bool broken;

    // Start is called before the first frame update
    void Awake()
    {
        defaultIntensity = transform.parent.GetChild(0).GetComponent<Light>().intensity;
        renderer = GetComponent<Renderer>();
        emissionMat = renderer.material;
        emissionColor = emissionMat.GetColor("_EmissionColor");

        broken = Random.Range(0f, 1f) < 0.1f ? true : false;
        StartCoroutine(randomIO());

        if (broken)
        {
            if (lightSounds.Count > 0)
            {
                int clipChoice = Random.Range(0, lightSounds.Count);
                GetComponent<AudioSource>().clip = lightSounds[clipChoice];
                GetComponent<AudioSource>().Play();
            }
            
            transform.parent.GetChild(0).GetComponent<Light>().intensity = 2f;
            transform.parent.GetChild(0).GetComponent<Light>().intensity += Random.Range(-1.5f, 2.5f);

        }
        else
        {
            Activate(true);
        }
    }
    IEnumerator randomIO()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.025f, 2f));

            if (broken)
            {
                on = !on;

                if (on)
                {
                    Activate(true);
                }
                else
                {
                    Activate(false);
                }
            }
        }
       

    }

    public void Activate(bool on)
    {
        
        if (on)
        {
            transform.parent.GetChild(0).GetComponent<Light>().enabled = true;

            emissionMat.EnableKeyword("_EMISSION");
            emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            emissionMat.SetColor("_EmissionColor", emissionColor);

            transform.parent.GetChild(0).GetComponent<Light>().intensity = defaultIntensity;
            transform.parent.GetChild(0).GetComponent<Light>().intensity += Random.Range(-1.5f, 2f);

        }
        else
        {
            transform.parent.GetChild(0).GetComponent<Light>().enabled = false;

            emissionMat.DisableKeyword("_EMISSION");
            emissionMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            emissionMat.SetColor("_EmissionColor", Color.black);

            transform.parent.GetChild(0).GetComponent<Light>().intensity = defaultIntensity;
            transform.parent.GetChild(0).GetComponent<Light>().intensity += Random.Range(-1.5f, 2f);
        }
        
        
       
    }
}
