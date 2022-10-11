using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBox : InteractableObject
{
    /*public List<ObjectWithWeight> lootTable;*/

    public List<ItemSpawner> itemSpawnLocations;

    public Animator anim;

    public float chanceForItemToBeInASlot;

    public bool hasOpened;

    public override void Hold(InteractionSystem player, bool RightHand)
    {
        
    }

    public override void Init()
    {
        hasOpened = false;
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
            foreach (ItemSpawner itemSpawn in itemSpawnLocations)
            {
                 if (chanceForItemToBeInASlot > Random.Range(0, 0.99f))
                 {
                    itemSpawn.SpawnItem();
                 }

            }

            hasOpened = true;
            SetMetaData("hasOpened", "true");

            anim.ResetTrigger("Open");
            anim.SetTrigger("Open");


        }

        
        
        
    }
}
