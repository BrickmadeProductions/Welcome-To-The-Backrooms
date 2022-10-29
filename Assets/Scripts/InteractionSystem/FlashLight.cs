using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : HoldableObject
{ 
    Color emissionColor;
    public Material emission;
    public Light flashLight;
    AudioSource lightAudio;

    public bool on = false;
    public bool canTurnOn = true;

    Coroutine losePowerOverTime = null;
    public void OnLoadBatteries(int amount, bool playSound)
    {
        flashLight.intensity = GetComponent<Loadable>().amountLoaded / 100f;
        canTurnOn = true;
    }
    public void OnUnloadBatteries(int amount, bool playSound)
    {

    }
    public IEnumerator LosePowerOverTime()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(3f);

            GetComponent<Loadable>().UnloadObject(1, false);
            flashLight.intensity = GetComponent<Loadable>().amountLoaded / 100f;

            if (GetComponent<Loadable>().amountLoaded <= 0)
            {
                canTurnOn = false;
                break;
            }
        }
        
    }

    public override void OnLoadFinished()
    {
        GetComponent<Loadable>().amountLoaded = int.Parse(saveableData.metaData["amountLoaded"]);

        SetStat(GetComponent<Loadable>().stat_ammoName, GetComponent<Loadable>().amountLoaded.ToString());
        SetMetaData("amountLoaded", GetComponent<Loadable>().amountLoaded.ToString());
        
        if (GetComponent<Loadable>().amountLoaded <= 0)
        {
            canTurnOn = false;
        }


        if (on)
        {
            emission.EnableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", emissionColor);
            emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            flashLight.gameObject.SetActive(true);
            if (losePowerOverTime == null)
                losePowerOverTime = StartCoroutine(LosePowerOverTime());
        }
        else
        {
            emission.EnableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", Color.black);
            emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            flashLight.gameObject.SetActive(false);
            if (losePowerOverTime != null)
            {
                StopCoroutine(losePowerOverTime);
                losePowerOverTime = null;
            }
        }

        flashLight.intensity = GetComponent<Loadable>().amountLoaded / 100f;

    }

    public override void Init()
    {
        base.Init();

        GetComponent<Loadable>().onLoadAmmo += OnLoadBatteries;
        GetComponent<Loadable>().onUnloadAmmo += OnUnloadBatteries;

        int RandomStartAmount = Random.Range(0, 100);

        GetComponent<Loadable>().amountLoaded = RandomStartAmount;


        SetStat(GetComponent<Loadable>().stat_ammoName, GetComponent<Loadable>().amountLoaded.ToString());
        SetMetaData("amountLoaded", GetComponent<Loadable>().amountLoaded.ToString());

      

        emission = transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material;

        lightAudio = GetComponent<AudioSource>();

        emissionColor = emission.GetColor("_EmissionColor");

        emission.EnableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", Color.black);
        emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        flashLight.gameObject.SetActive(false);




    }
    public void Update()
    {
        if (on)
        {
            if (GameSettings.Instance.worldInstance.GetBiomeCurrentPlayerIsIn() == BIOME_ID.LEVEL_0_RED_ROOMS)
            {
                lightAudio.pitch = UnityEngine.Random.Range(0.6f, 1.3f);
                lightAudio.PlayOneShot(lightAudio.clip);

                on = false;

                SetMetaData("on", on.ToString());

                emission.EnableKeyword("_EMISSION");
                emission.SetColor("_EmissionColor", Color.black);
                emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                flashLight.gameObject.SetActive(false);

            }


        }



    }
    public override void Use(InteractionSystem player, bool LMB)
    {
        lightAudio.pitch = UnityEngine.Random.Range(0.6f, 1.3f);
        lightAudio.PlayOneShot(lightAudio.clip);

        if (LMB && GameSettings.Instance.worldInstance.GetBiomeCurrentPlayerIsIn() != BIOME_ID.LEVEL_0_RED_ROOMS && canTurnOn)
        {
            on = !on;

            SetMetaData("on", on.ToString());

            switch (on)
            {
                case true:
                    if (losePowerOverTime == null)
                        losePowerOverTime = StartCoroutine(LosePowerOverTime());

                    emission.EnableKeyword("_EMISSION");
                    emission.SetColor("_EmissionColor", emissionColor);
                    emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    flashLight.gameObject.SetActive(true);

                    break;

                case false:

                    if (losePowerOverTime != null)
                    {
                        StopCoroutine(losePowerOverTime);
                        losePowerOverTime = null;
                    }
                       
                    emission.EnableKeyword("_EMISSION");
                    emission.SetColor("_EmissionColor", Color.black);
                    emission.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    flashLight.gameObject.SetActive(false);

                    break;
            }

           
        }
        else
        {
            on = false;
        }


    }


}
