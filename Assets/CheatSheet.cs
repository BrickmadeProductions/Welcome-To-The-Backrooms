using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatSheet : MonoBehaviour
{
    public Dropdown itemSpawnDropdown;
    public Dropdown entitySpawnDropdown;
    public Dropdown levelDropdown;

    ENTITY_TYPE currentEntityTypeChoice;
    OBJECT_TYPE currentObjectTypeChoice;
    GameSettings.SCENE currentLevelChoice;

    public bool AIEnabled = true;
    public bool invincible = false;
    public bool noClip = false;


    void Awake()
    {
        LoadAllLevelsToDropDown();
        LoadAllEntityTypesToDropDown();
        LoadAllItemTypesToDropDown();
    }

    public void LevelTeleport()
    {
        Debug.Log(currentLevelChoice);
        GameSettings.Instance.LoadScene(currentLevelChoice);
        GameSettings.Instance.CheatMenu(false);
    }

    public void LoadAllLevelsToDropDown()
    {
        string[] objects = Enum.GetNames(typeof(GameSettings.SCENE));
        List<string> objectList = new List<string>(objects);

        //remove scenes that dont involve levels
        objectList.Remove("INTRO");
        objectList.Remove("HOMESCREEN");
        objectList.Remove("ROOM");

        levelDropdown.AddOptions(objectList);
    }

    public void LoadAllItemTypesToDropDown()
    {
        string[] objects = Enum.GetNames(typeof(OBJECT_TYPE));
        List<string> objectList = new List<string>(objects);

        itemSpawnDropdown.AddOptions(objectList);
    }

    public void LoadAllEntityTypesToDropDown()
    {
        string[] entities = Enum.GetNames(typeof(ENTITY_TYPE));
        List<string> entitiesList = new List<string>(entities);

        entitySpawnDropdown.AddOptions(entitiesList);
    }

    public void LevelSelectionDropDownCallBack(int index)
    {
        //excempt the first 3
        
        currentLevelChoice = (GameSettings.SCENE)(index + 3);
        
    }

    public void EntityDropDownCallBack(int index)
    {
        currentEntityTypeChoice = (ENTITY_TYPE)index;
        Debug.Log("| CHEATSHEET | Selected: " + currentEntityTypeChoice);
    }

    public void ItemDropDownCallBack(int index)
    {
        currentObjectTypeChoice = (OBJECT_TYPE)index;
        Debug.Log("| CHEATSHEET | Selected: " + currentObjectTypeChoice);
    }

    public static void PopulateDropDownWithEnum(Dropdown dropdown, Enum targetEnum)//You can populate any dropdown with any enum with this method
    {
        Type enumType = targetEnum.GetType();//Type of enum(FormatPresetType in my example)
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();

        for (int i = 0; i < Enum.GetNames(enumType).Length; i++)//Populate new Options
        {
            newOptions.Add(new Dropdown.OptionData(Enum.GetName(enumType, i)));
        }

        dropdown.ClearOptions();//Clear old options
        dropdown.AddOptions(newOptions);//Add new options
    }

    public void NoClip(bool io)
    {
        GameSettings.Instance.cheatSheet.noClip = io;
    }
    public void Invincible(bool io)
    {
        GameSettings.Instance.cheatSheet.invincible = io;
    }

    public void SpawnEntities(bool io)
    {
        GameSettings.Instance.worldInstance.spawnEntities = io; 
    }

    public void KillAllEntities()
    {
        GameSettings.Instance.worldInstance.RemoveAllEntities();
    }
    public void RemoveAllProps()
    {
        GameSettings.Instance.worldInstance.RemoveAllProps();
    }

    public void EntityAI(bool io)
    {
        
        AIEnabled = io;
        
    }

    public void SpawnCheatedItem()
    {
        Vector3 spawnLocation = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.position + GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.forward * 5;

        spawnLocation.y = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.position.y;

        InteractableObject objectToSpawn = null;
        GameSettings.Instance.PropDatabase.TryGetValue(currentObjectTypeChoice, out objectToSpawn);

        GameSettings.Instance.worldInstance.AddNewProp(
        spawnLocation,
        GameSettings.Instance.Player.transform.rotation,
        objectToSpawn,
        GameSettings.Instance.worldInstance.GetLoadedChunkAtPlayerLocation());

        Debug.Log("| CHEATSHEET | Successfully Spawned: " + objectToSpawn.type);
    }

    public void SpawnCheatedEntity()
    {
        Vector3 spawnLocation = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.position + GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.forward * 5;

        spawnLocation.y = GameSettings.Instance.Player.GetComponent<PlayerController>().transform.position.y;

        Entity entityToSpawn = null;
        GameSettings.Instance.EntityDatabase.TryGetValue(currentEntityTypeChoice, out entityToSpawn);

        Debug.Log(entityToSpawn.type);

        GameSettings.Instance.worldInstance.AddNewEntity(
        spawnLocation,
        entityToSpawn,
        GameSettings.Instance.worldInstance.GetLoadedChunkAtPlayerLocation());

        Debug.Log("| CHEATSHEET | Successfully Spawned: " + entityToSpawn.type);
    }

    public void SetCurrentObjectChoice(OBJECT_TYPE type)
    {

    }
    public void SetCurrentEntityChoice(ENTITY_TYPE type)
    {

    }



}
