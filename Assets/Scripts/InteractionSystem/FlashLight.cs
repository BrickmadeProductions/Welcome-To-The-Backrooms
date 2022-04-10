using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : HoldableObject
{ 
    Color emissionColor;
    public Material emission;
    public Light flashLight;
    AudioSource lightAudio;

    bool on = false;

    private void Start()
    {
        emission = transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material;

        lightAudio = GetComponent<AudioSource>();

        emissionColor = emission.GetColor("_EmissionColor");


        //turn off on start
        flashLight.enabled = on;
        emission.EnableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", Color.black);
        emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

    }
    public override void Use(InteractionSystem player)
    {
        on = !on;

        flashLight.enabled = on;
        
        switch (on)
        {
            case true:
                emission.EnableKeyword("_EMISSION");
                emission.SetColor("_EmissionColor", emissionColor);
                emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                break;

            case false:
                emission.EnableKeyword("_EMISSION");
                emission.SetColor("_EmissionColor", Color.black);
                emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                break;
        }

        lightAudio.pitch = UnityEngine.Random.Range(0.7f, 1.1f);
        lightAudio.PlayOneShot(lightAudio.clip);

    }

    public override void Throw(Vector3 force)
    {
        
    }

    public override void Grab(InteractionSystem player)
    {
        
    }
}
