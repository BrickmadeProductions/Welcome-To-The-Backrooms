using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loadable : MonoBehaviour
{
    private HoldableObject connectedObject;
    public string stat_ammoName;
    public int amountLoaded = 0;
    public int maxLoad;

    public AudioClip loadNoise;
    public AudioClip unloadNoise;

    public delegate void OnLoadAmmo(int amount, bool playSound);
    public OnLoadAmmo onLoadAmmo;

    public delegate void OnUnLoadAmmo(int amount, bool playSound);
    public OnUnLoadAmmo onUnloadAmmo;

    private void Awake()
    {
        connectedObject = GetComponent<HoldableObject>();
    }

    public void LoadObject(int amount, bool playSound)
    {
        onLoadAmmo?.Invoke(amount, playSound);

        if (playSound)
        {
            GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().audioObject.clip = loadNoise;
            GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().audioObject.Play();
        }
        

        amountLoaded += amount;
        connectedObject.SetStat(stat_ammoName, amountLoaded.ToString());
        connectedObject.SetMetaData("amountLoaded", amountLoaded.ToString());
    }
    public void UnloadObject(int amount, bool playSound)
    {
        onUnloadAmmo?.Invoke(amount, playSound);

        if (playSound)
        {
            GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().audioObject.clip = unloadNoise;
            GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().audioObject.Play();
        }
        amountLoaded -= amount;
        connectedObject.SetStat(stat_ammoName, amountLoaded.ToString());
        connectedObject.SetMetaData("amountLoaded", amountLoaded.ToString());
    }

}
