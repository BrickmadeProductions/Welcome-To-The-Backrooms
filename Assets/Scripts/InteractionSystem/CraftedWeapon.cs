using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class CraftedWeapon : HoldableObject
{
    public bool Flying;
    public float rotationAmount;
    public bool stuckInWall;
    public AudioClip[] stuckSounds;
    public FixedJoint ObjectConnectionJoint;

    public WeaponPiece currentBlade;
    public WeaponPiece currentFasten;

    public Transform bladeLocation;
    public Transform fastenLocation;

    public InventoryObjectData originalData;

    public override void Init()
    {
        base.Init();

        originalData = new InventoryObjectData
        {
            description = inventoryObjectData.description,
            name = inventoryObjectData.name,
            image = inventoryObjectData.image,
            inventoryWeight = inventoryObjectData.inventoryWeight,
        };

    }
    public override void OnLoadFinished()
    {
        StartCoroutine(LoadInDetails());

    }
    public IEnumerator LoadInDetails()
    {
        yield return new WaitUntil(() => GameSettings.WORLD_FINISHED_LOADING);

        if (saveableData.metaData.ContainsKey("BLADEWorldID"))
        {
            AddPiece(GameSettings.Instance.worldInstance.FindPropInWorldByKey(GetMetaData("BLADEWorldID")).gameObject.GetComponent<WeaponPiece>(), connectedInvItem);
            SetStat("Blade Durability", currentBlade.durabilityLeft.ToString());
        }

        if (saveableData.metaData.ContainsKey("FASTENWorldID"))
        {
            AddPiece(GameSettings.Instance.worldInstance.FindPropInWorldByKey(GetMetaData("FASTENWorldID")).gameObject.GetComponent<WeaponPiece>(), connectedInvItem);
            SetStat("Fasten Durability", currentFasten.durabilityLeft.ToString());
        }
    }

    public void BreakPiece(WeaponPiece breakPiece, InventoryItem dataToChange = null)
    {

        breakPiece.gameObject.transform.parent = null;
        BPUtil.SetAllChildrenToLayer(breakPiece.transform, 9);
        BPUtil.SetAllColliders(breakPiece.transform, true);

        breakPiece.GetComponent<HoldableObject>().SetMetaData("Durability", breakPiece.durabilityLeft.ToString());

        switch (breakPiece.locationToPlace)
        {
            case WEAPON_PIECE_TYPE.FASTEN:

                GameSettings.Instance.worldInstance.RemoveProp(currentFasten.GetComponent<HoldableObject>().GetWorldID(), true);

                currentFasten = null;

                RemoveStat("Fasten Durability");

                RemoveStat("Fasten Type");

                break;

            case WEAPON_PIECE_TYPE.BLADE:

                breakPiece.GetComponentInChildren<Weapon>().connetedObject = null;

                currentBlade = null;

                RemoveStat("Blade Durability");

                RemoveStat("Blade Type");

                RemoveStat("Damage");

               
                Destroy(breakPiece.GetComponentInChildren<FixedJoint>());
                Destroy(breakPiece.GetComponentInChildren<Rigidbody>());
                ObjectConnectionJoint = null;

                if (dataToChange != null)
                {
                    dataToChange.nameText.text = originalData.name;
                    dataToChange.itemImageLocation.texture = originalData.image;
                    dataToChange.descriptionText.text = originalData.description;
                }
                else
                {
                    inventoryObjectData.name = originalData.name;
                    inventoryObjectData.image = originalData.image;
                    inventoryObjectData.description = originalData.description;
                }
                    

                break;
        }

        RemoveMetaData(breakPiece.locationToPlace.ToString() + "WorldID");

        RemoveMetaData(breakPiece.locationToPlace.ToString() + "Type");
        RemoveMetaData(breakPiece.locationToPlace.ToString() + "Durability");

        if (rb != null)
            rb.constraints = RigidbodyConstraints.None;

        breakPiece.GetComponent<AudioSource>().Play();

    }
    public bool AddPiece(WeaponPiece part, InventoryItem dataToChange = null)
    {

        switch (part.locationToPlace)
        {
            case WEAPON_PIECE_TYPE.FASTEN:

                if (currentFasten != null)
                    return false;

                SetStat("Fasten Type", part.assosiatedObject.ToString());

                SetStat("Fasten Durability", part.durabilityLeft.ToString());

                part.gameObject.transform.parent = fastenLocation;

                part.transform.position = fastenLocation.position;
                part.transform.localRotation = Quaternion.identity;
                currentFasten = part;

                Destroy(currentFasten.GetComponent<Rigidbody>());
                currentFasten.GetComponent<HoldableObject>().rb = null;

                break;

            case WEAPON_PIECE_TYPE.BLADE:

                if (currentBlade != null)
                    return false;

                SetStat("Blade Type", part.assosiatedObject.ToString());

                SetStat("Blade Durability", part.durabilityLeft.ToString());

                part.GetComponentInChildren<Weapon>().connetedObject = this;

                part.gameObject.transform.parent = bladeLocation;
                part.transform.position = bladeLocation.position;
                part.transform.localRotation = Quaternion.identity;

                currentBlade = part;

                Destroy(currentBlade.GetComponent<Rigidbody>());
                currentBlade.GetComponent<HoldableObject>().rb = null;

                GetComponentInChildren<Weapon>().SearchForConnectedItem();

                break;
        }

        SetMetaData(part.locationToPlace.ToString() + "WorldID", part.GetComponent<HoldableObject>().GetWorldID());

        SetMetaData(part.locationToPlace.ToString() + "Type", part.locationToPlace.ToString());
        SetMetaData(part.locationToPlace.ToString() + "Durability", part.durabilityLeft.ToString());

        
        
        if (part.weaponNameChange != "")
        {
            inventoryObjectData.name = part.weaponNameChange;
            //only change inventory slot data if active, else store it for later
            if (dataToChange != null)
                dataToChange.nameText.text = inventoryObjectData.name;
        }
                


        if (part.changeTexture != null)
        {
            inventoryObjectData.image = part.changeTexture;
            //only change inventory slot data if active, else store it for later
            if (dataToChange != null)
                dataToChange.itemImageLocation.texture = inventoryObjectData.image;
        }



        
        BPUtil.SetAllColliders(part.transform, false);
        BPUtil.SetAllChildrenToLayer(part.transform, 23);
        //part.GetComponent<HoldableObject>().rb.isKinematic = true;
        //part.GetComponent<HoldableObject>().rb.constraints = RigidbodyConstraints.FreezeAll;

        return true;
    }

    //throwing
    public void OnTriggerEnter(Collider hit)
    {
        

        
        
    }

    /*public void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && stuckInWall)
        {
            
        }
    }*/
    public void OnTriggerExit(Collider hit)
    {
        
        
        

    }

    void LateUpdate()
    {
        if (Flying)
        {
          /* transform.right =
            Vector3.Slerp(-transform.right, holdableObject.velocity.normalized, Time.deltaTime * 15);*/
        }
    }

    public override void Drop(Vector3 force)
    {
        
        base.Drop(force);

        if (currentBlade != null)
        {
            
            currentBlade.GetComponent<HoldableObject>().rb = currentBlade.gameObject.AddComponent<Rigidbody>();
            currentBlade.GetComponent<HoldableObject>().rb.isKinematic = true;
            currentBlade.GetComponent<HoldableObject>().rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (currentFasten != null)
        {
            currentFasten.GetComponent<HoldableObject>().rb = currentFasten.gameObject.AddComponent<Rigidbody>();
            currentFasten.GetComponent<HoldableObject>().rb.isKinematic = true;
            currentFasten.GetComponent<HoldableObject>().rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        rb.angularVelocity = rb.transform.right * rotationAmount * rotationAmount;
        Flying = true;


    }

    public override void Pickup(InteractionSystem player, bool RightHand)
    {
        base.Pickup(player, RightHand);

        if (currentBlade != null)
        {
            Destroy(currentBlade.GetComponent<Rigidbody>());
            currentBlade.GetComponent<HoldableObject>().rb = null;
            
        }
        if (currentFasten != null)
        {
            Destroy(currentFasten.GetComponent<Rigidbody>());
            currentFasten.GetComponent<HoldableObject>().rb = null;
        }

        Flying = false;
        stuckInWall = false;
        //rb.constraints = RigidbodyConstraints.FreezeAll;
    }

}
