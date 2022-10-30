using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WEAPON_PIECE_TYPE
{
    FASTEN,
    BLADE
}

public class WeaponPiece : MonoBehaviour
{
    //weapon piece connected to crafted weapon, not a real prop
    public int durabilityLeft;
    public OBJECT_TYPE assosiatedObject;
    public WEAPON_PIECE_TYPE locationToPlace;
    public string weaponNameChange;

    public Texture changeTexture;

    public void OnHoldableLoaded()
    {
        durabilityLeft = int.Parse(GetComponent<HoldableObject>().saveableData.metaData["Durability"]);
        GetComponent<HoldableObject>().SetStat("Durability", durabilityLeft.ToString());
    }
    private void Awake()
    {
        GetComponent<HoldableObject>().SetMetaData("Durability", durabilityLeft.ToString());
        GetComponent<HoldableObject>().SetStat("Durability", durabilityLeft.ToString());
        
        GetComponent<HoldableObject>().onLoad += OnHoldableLoaded;
    }
}
