using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBox : InteractableObject
{
    public List<ObjectWithWeight> lootTable;

    public List<Transform> itemSpawnLocations;

    public Animator anim;

    public float chanceForItemToBeInASlot;

    bool hasOpened;

    public override void Hold(InteractionSystem player, bool RightHand)
    {
        
    }

    public override void Init()
    {
        hasOpened = false;
        anim.ResetTrigger("Open");
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        if (!hasOpened)
        {
            foreach (Transform itemSpawn in itemSpawnLocations)
            {
                if (chanceForItemToBeInASlot > Random.Range(0, 0.99f)){

                    //setup in loadedchunks Prop data
                    GameObject item = WeightedRandom.ReturnItemBySpawnChances(lootTable);
                    
                    InteractableObject spawnedProp = GameSettings.Instance.worldInstance.AddNewProp(itemSpawn.transform.position, Quaternion.Euler(transform.localRotation.x, Random.Range(transform.localRotation.y - 15f, transform.localRotation.y + 15f), transform.localRotation.z), item);
                    
                    Debug.Log("LOOTBOX CONTAINED: " + spawnedProp.GetComponent<HoldableObject>().type);

                    //spawnedProp.transform.parent = transform;
                }

                anim.ResetTrigger("Open");
                anim.SetTrigger("Open");

                hasOpened = true;
            }

            
        }

        
        
        
    }
}
