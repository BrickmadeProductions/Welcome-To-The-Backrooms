using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : HoldableObject
{ 
    Color emissionColor;
    public Material emission;
    public Light flashLight;
    AudioSource lightAudio;

    GameObject runtimeFlashLightCollider;

    bool on = false;

    private void Start()
    {
        emission = transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material;

        lightAudio = GetComponent<AudioSource>();

        emissionColor = emission.GetColor("_EmissionColor");


        emission.EnableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", Color.black);
        emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

    }
    public override void Use(InteractionSystem player, bool LMB)
    {
        if (LMB)
        {
            on = !on;

            switch (on)
            {
                case true:
                    emission.EnableKeyword("_EMISSION");
                    emission.SetColor("_EmissionColor", emissionColor);
                    emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    runtimeFlashLightCollider = Instantiate(flashLight.gameObject, transform.GetChild(0));
                    break;

                case false:
                    emission.EnableKeyword("_EMISSION");
                    emission.SetColor("_EmissionColor", Color.black);
                    emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    Destroy(runtimeFlashLightCollider);
                    break;
            }

            lightAudio.pitch = UnityEngine.Random.Range(0.6f, 1.3f);
            lightAudio.PlayOneShot(lightAudio.clip);
        }
        

    }

    public override void Throw(Vector3 force)
    {
        
    }

    public override void AddToInv(InteractionSystem player)
    {
        
    }
}
