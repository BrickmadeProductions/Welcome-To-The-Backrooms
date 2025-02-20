using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatSheet : MonoBehaviour
{
    public Dropdown itemSpawnDropdown;
    public Dropdown entitySpawnDropdown;
    public Dropdown levelDropdown;
    public Dropdown eventDropdown;

    ENTITY_TYPE currentEntityTypeChoice;
    OBJECT_TYPE currentObjectTypeChoice;
    SCENE currentLevelChoice;
    GAMEPLAY_EVENT currentEventChoice;

    public bool AIEnabled = true;
    public bool invincible = false;
    public bool noClip = false;

    public TextMeshProUGUI timeSinceLastEventText;

    void Awake()
    {
        LoadAllLevelsToDropDown();
        LoadAllEntityTypesToDropDown();
        LoadAllItemTypesToDropDown();
        LoadAllEventsToDropDown();

        StartCoroutine(UpdateVariables());
    }
    IEnumerator UpdateVariables()
    {
        while (true)
        {
            if (GameSettings.Instance.worldInstance != null)
            {
                timeSinceLastEventText.text = "Current Event: " + GameSettings.Instance.worldInstance.currentWorldEvent.ToString() + "\nTime Since Last Event: " + GameSettings.Instance.worldInstance.timeInSecondsSinceLastEvent;
            }
                
            yield return new WaitForSecondsRealtime(1f);
        }
       
    }

    public void ResetSteamStats()
    {
        Steam.ClearAllSteamStats();
    }
    public void LevelTeleport()
    {
        Debug.Log(currentLevelChoice);
        GameSettings.Instance.LoadScene(currentLevelChoice);
        GameSettings.Instance.CheatMenu(false);
    }

    public void LoadAllLevelsToDropDown()
    {
        string[] objects = Enum.GetNames(typeof(SCENE));
        List<string> objectList = new List<string>(objects);

        //remove scenes that dont involve levels
        objectList.Remove("INTRO");
        objectList.Remove("HOMESCREEN");
        objectList.Remove("ROOM");
        objectList.Remove("LOADING");

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

    public void LoadAllEventsToDropDown()
    {
        string[] objects = Enum.GetNames(typeof(GAMEPLAY_EVENT));
        List<string> objectList = new List<string>(objects);

        eventDropdown.AddOptions(objectList);
    }

    public void LevelSelectionDropDownCallBack(int index)
    {
        //excempt the first 3
        
        currentLevelChoice = (SCENE)(index + 4);
        
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

    public void EventDropDownCallBack(int index)
    {
        currentEventChoice = (GAMEPLAY_EVENT)index;
        Debug.Log("| CHEATSHEET | Selected: " + currentEventChoice);
    }

    public void SceneSelection(int index)
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
        if (!io)
            KillAllEntities();

        GameSettings.Instance.worldInstance.spawnEntities = io; 
    }

    public void KillAllEntities()
    {
        GameSettings.Instance.worldInstance.RemoveAllEntities();
    }
    public void RemoveAllProps()
    {
        GameSettings.Instance.worldInstance.RemoveAllProps(true);
    }
    public void Take5Damage()
    {
        GameSettings.GetLocalPlayer().playerHealth.TakeDamage(5f, 1f, 0f, false, DAMAGE_TYPE.UNKNOWN);
    }
    public void EntityAI(bool io)
    {
        
        AIEnabled = io;
        
    }

    public void SpawnCheatedItem()
    {
        Vector3 spawnLocation = GameSettings.GetLocalPlayer().head.transform.position + GameSettings.GetLocalPlayer().head.transform.forward * 5;

        spawnLocation.y = GameSettings.GetLocalPlayer().head.transform.position.y;

        GameObject objectToSpawn = GameSettings.Instance.PropDatabase[currentObjectTypeChoice].gameObject;
        

        GameSettings.Instance.worldInstance.AddNewProp(
        spawnLocation,
        GameSettings.GetLocalPlayer().transform.rotation,
        objectToSpawn);

        Debug.Log("| CHEATSHEET | Successfully Spawned: " + objectToSpawn.GetComponent<InteractableObject>().type);
    }

    public void SpawnCheatedEntity()
    {
        Vector3 spawnLocation = GameSettings.GetLocalPlayer().head.transform.position + GameSettings.GetLocalPlayer().head.transform.forward * 5;

        spawnLocation.y = GameSettings.GetLocalPlayer().transform.position.y;

        GameObject entityToSpawn = GameSettings.Instance.EntityDatabase[currentEntityTypeChoice].gameObject;

        //Debug.Log(entityToSpawn.type);

        GameSettings.Instance.worldInstance.AddNewEntity(
        spawnLocation,
        Quaternion.identity,
        entityToSpawn);

        Debug.Log("| CHEATSHEET | Successfully Spawned: " + entityToSpawn.GetComponent<Entity>().type);
    }

    public void StartEvent()
    {
       
        GameSettings.Instance.worldInstance.StartEvent(currentEventChoice);
        Debug.Log("| CHEATSHEET | Successfully Started: " + currentEventChoice);

    }

    public void StopEvent()
    {
        GameSettings.Instance.worldInstance.OnEventEnd();

        if (GameSettings.Instance.audioHandler.playingEventTrackLoop != null)
        {
            StopCoroutine(GameSettings.Instance.audioHandler.playingEventTrackLoop);
            GameSettings.Instance.audioHandler.playingEventTrackLoop = null;

            Debug.Log("| CHEATSHEET | Successfully Started: " + currentEventChoice);
        }
        

        
    }

    public void SetCurrentObjectChoice(OBJECT_TYPE type)
    {

    }
    public void SetCurrentEntityChoice(ENTITY_TYPE type)
    {

    }

    public void SetCurrentEventChoice(GAMEPLAY_EVENT type)
    {

    }



}
