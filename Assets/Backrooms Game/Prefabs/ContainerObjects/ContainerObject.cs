using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct SavedContainerInventoryData
{
    ///inventory slot name, saveableItem world id
    public Dictionary<string, string> propsInContainerSlots;

}

public class ContainerObject : HoldableObject, IEnumerable<InventorySlot>
{
    public SavedContainerInventoryData containerInventorySaveData;

    //dynamic ui inventory slot data
    public List<InventorySlot> storageSlots;

    //object that gets added to the players inventory screen
    public GameObject UIObject;

    public Vector3 ogUIPos;
    public Vector3 ogUIScale;
    public Quaternion ogUIRotation;

    public void SetSlotSaveData(string field, string value)
    {

        if (containerInventorySaveData.propsInContainerSlots.ContainsKey(field))
        {

            containerInventorySaveData.propsInContainerSlots[field] = value;
        }
        else
        {
            containerInventorySaveData.propsInContainerSlots.Add(field, value);
        }

        
    }

    public void RemoveSlotSaveData(string field)
    {

        if (containerInventorySaveData.propsInContainerSlots.ContainsKey(field))
        {

            containerInventorySaveData.propsInContainerSlots.Remove(field);
        }

        


    }

    public void SaveAllSlots()
    {
        foreach (InventorySlot slot in storageSlots)
        {
            if (slot.itemsInSlot.Count > 0)

                SetSlotSaveData(slot.name, slot.itemsInSlot[0].connectedObject.GetWorldID());

            else
                RemoveSlotSaveData(slot.name);
        }

        GameSettings.Instance.worldInstance.containersInWorld[GetWorldID()] = containerInventorySaveData;
    }
    public void LoadInSlots(SavedContainerInventoryData data)
    {
        foreach (InventorySlot slot in storageSlots)
        {
            if (data.propsInContainerSlots.ContainsKey(slot.name))
            {
                //Debug.Log("Adding Item: " + GameSettings.Instance.worldInstance.FindPropInWorldByKey(data.propsInContainerSlots[slot.name]).name);
                slot.AddItemToSlot((HoldableObject)GameSettings.Instance.worldInstance.FindPropInWorldByKey(data.propsInContainerSlots[slot.name]));

            }
        }
    }

    public override void OnSaveFinished()
    {
        SaveAllSlots();

    }

    public override void Init()
    {
        base.Init();
        containerInventorySaveData = new SavedContainerInventoryData
        {
            propsInContainerSlots = new Dictionary<string, string>()
        };

        if (!GameSettings.Instance.worldInstance.containersInWorld.ContainsKey(GetWorldID()))
            GameSettings.Instance.worldInstance.containersInWorld.Add(GetWorldID(), containerInventorySaveData);

        ogUIPos = UIObject.gameObject.GetComponent<RectTransform>().anchoredPosition3D;
        ogUIScale = UIObject.gameObject.GetComponent<RectTransform>().localScale;
        ogUIRotation = UIObject.gameObject.GetComponent<RectTransform>().rotation;
    }


    public override void Drop(Vector3 force)
    {
        base.Drop(force);
        UIObject.SetActive(false);
        UIObject.gameObject.transform.GetComponent<RectTransform>().SetParent(transform);
    }

    public void ConnectUIToPlayer()
    {
        UIObject.SetActive(true);
        UIObject.gameObject.GetComponent<RectTransform>().SetParent(GameSettings.Instance.Player.GetComponent<InventorySystem>().menuObject.transform);
        UIObject.gameObject.GetComponent<RectTransform>().anchoredPosition3D = ogUIPos;
        UIObject.gameObject.GetComponent<RectTransform>().localScale = ogUIScale;
        UIObject.gameObject.GetComponent<RectTransform>().rotation = ogUIRotation;

        foreach (InventorySlot slot in storageSlots)
        {
            if (slot.itemsInSlot.Count > 0)
            {
                slot.itemsInSlot[0].TryFindCanvas();
            }
        }
    }

    public IEnumerator<InventorySlot> GetEnumerator()
    {
        return storageSlots.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return storageSlots.GetEnumerator();
    }
}
