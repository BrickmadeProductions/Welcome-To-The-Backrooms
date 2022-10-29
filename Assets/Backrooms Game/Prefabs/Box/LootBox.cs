using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBox : InteractableObject
{
    /*public List<ObjectWithWeight> lootTable;*/

    public List<Transform> itemSpawnLocations;

    public List<ObjectSpawnData> lootBoxSpawns;

    public Animator anim;

    public float chanceForItemToBeInASlot;

    public bool hasOpened;

    public override void Pickup(InteractionSystem player, bool RightHand)
    {
        
    }

    public override void Init()
    {
        hasOpened = false;
        SetMetaData("hasOpened", "false");
        anim.SetTrigger("Close");
    }

    public override void OnLoadFinished()
    {
        if (hasOpened)
        {
            anim.SetTrigger("Open");
        }
        else
        {
            anim.SetTrigger("Close");
        }
    }
    public override void OnSaveFinished()
    {

    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        if (!hasOpened)
        {
           

            foreach (Transform itemSpawn in itemSpawnLocations)
            {
                 if (chanceForItemToBeInASlot > Random.Range(0, 0.99f))
                 {
                    GameObject itemToSpawn = WeightedRandomSpawning.ReturnItemBySpawnChances(lootBoxSpawns);
                    
                    if (itemToSpawn.GetComponent<HoldableObject>() != null)

                        if (!itemToSpawn.GetComponent<HoldableObject>().large)
                        {
                            GameSettings.Instance.worldInstance.AddNewProp(itemSpawn.position, Quaternion.identity, itemToSpawn);
                        }
                        
                 }

            }

            hasOpened = true;
            SetMetaData("hasOpened", "true");

            anim.ResetTrigger("Open");
            anim.SetTrigger("Open");


    }

        
        
        
    }
}
