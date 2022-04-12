using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    //inventorySystem

    //2 pockets to start with

    //you can find backpacks in relevant locations (10 extra slots)

    //no concrete UI

    //each item has a weight that prevents you from placing a lot of large items in the backpack, certain items like chairs have to be broken down to fit in the backpack

    //find backpacks in crates in level 1

    //crafting system

    //each item has a specific type of way it is crafted, such as pouring bottles into eachother


    int inventoryLimit = 2;
    int currentSelectedInventorySlot = 0;

    bool inventoryOpened = false;
    
    
    PlayerController player;

    public InteractableObject currentlyLookingAt;
    public Transform dropLocation;
    public TextMeshProUGUI pickup;
    public TextMeshProUGUI open;
    public List<HoldableObject> inventorySlots;
    // Update is called once per frame

    private void Awake()
    {
        inventorySlots = new List<HoldableObject>();
        player = GetComponent<PlayerController>();
    }

    void setAllChildrenToLayer(Transform top, int layer)
    {
        top.gameObject.layer = layer;

        foreach (Transform child in top)
        {
            if (child.childCount > 0)
            {
                child.gameObject.layer = layer;
                setAllChildrenToLayer(child, layer);

            }
            else
            {
                child.gameObject.layer = layer;
            }

        }
    }

    void InventoryManager()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            if (inventoryOpened)
            {
                player.Crouch();
                inventoryOpened = false;
            }
            else
            {
                player.UnCrouch();
                inventoryOpened = true;
            }
        }
        

    }

    void PickupSystem()
    {
        //pickup (hold)
        if (player.holding == null && Input.GetButton("Hold") && currentlyLookingAt != null && currentlyLookingAt.gameObject.tag != "Usable")
        { 
            player.holding = currentlyLookingAt.GetComponent<HoldableObject>();

            if (player.holding.GetComponent<HoldableObject>().large)
                setAllChildrenToLayer(player.holding.transform, 14);
            else
            {
                setAllChildrenToLayer(player.holding.transform, 13);
            }
        }

        //throw
        if (Input.GetButtonDown("Throw") && player.holding != null && (player.head.transform.localRotation.x * Mathf.Rad2Deg < 15 || !player.holding.GetComponent<HoldableObject>().large))
        {
            
            setAllChildrenToLayer(player.holding.transform, 9);

            player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = false;

            player.holding.transform.parent = null;

            player.holding.Throw((player.head.transform.forward * 400f) * player.holding.GetComponent<Rigidbody>().mass);

            player.holdLocation.GetComponent<AudioSource>().pitch = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
            player.holdLocation.GetComponent<AudioSource>().Play();

            foreach (Collider col in player.holding.GetComponents<Collider>())
            {
                col.enabled = true;
            }

            SceneManager.MoveGameObjectToScene(player.holding.gameObject, SceneManager.GetActiveScene());

            player.holding = null;

            player.bodyAnim.SetBool("isHoldingSmall", false);
            player.bodyAnim.SetBool("isHoldingLarge", false);
         


        }

        //drop (hold)
        else if (Input.GetButtonDown("Drop") && player.holding != null)
        {
            setAllChildrenToLayer(player.holding.transform, 9);

            player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = false;

            player.holding.transform.parent = null;

            player.holding.Throw(-Vector3.up);


            foreach (Collider col in player.holding.GetComponents<Collider>())
            {
                col.enabled = true;
            }
            

            SceneManager.MoveGameObjectToScene(player.holding.gameObject, SceneManager.GetActiveScene());

            player.holding = null;

            player.bodyAnim.SetBool("isHoldingSmall", false);
            player.bodyAnim.SetBool("isHoldingLarge", false);


        }

        //holding
        else if (player.holding != null)
        {
            foreach (Collider col in player.holding.GetComponents<Collider>())
            {
                col.enabled = false;
            }

            player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = true;

            player.holding.transform.parent = player.holding.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.handLocation.transform; //hand or both hands

            player.holding.transform.position = player.holding.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.handLocation.transform.position;

            Quaternion holdRotation = Quaternion.Euler(player.head.transform.localRotation.x / 2, player.head.transform.localRotation.y, player.head.transform.localRotation.z);

            if (player.holding.GetComponent<HoldableObject>().large)
            {
                //player.playerHealth.canRun = false;
                //player.currentPlayerState = PlayerController.PLAYERSTATES.WALK;

                player.bodyAnim.SetBool("isHoldingLarge", true);
            }
            else
            {
                
                player.bodyAnim.SetBool("isHoldingSmall", true);
            } 
                    

            player.holding.transform.localRotation = holdRotation;

            

        }

        //interactables / doors / usable objects / etc.
        if (currentlyLookingAt != null && currentlyLookingAt.gameObject.tag == "Usable" && Input.GetButtonDown("Hold"))
        {
            Debug.Log("Use " + currentlyLookingAt.name);
            currentlyLookingAt.Use(this);
        }

        //pickup (inventory)
        if (Input.GetButtonDown("Pickup") && inventorySlots.Count < inventoryLimit && currentlyLookingAt != null)
        {
            currentlyLookingAt.Grab(this);

        }

        //drop (inventory)
        if (Input.GetButtonDown("Drop") && inventorySlots.Count > 0)
        {
            setAllChildrenToLayer(player.holding.transform, 9);

            dropInventoryObject();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (player.holding != null)
                player.holding.Use(this);
        }
    }

    void Update()
    {

        //==============//
        //holding system//
        //==============//
        Debug.DrawRay(player.playerCamera.transform.position, player.playerCamera.transform.forward * 2f, Color.red);

        //test for a holdable object
        RaycastHit[] hits = Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 2f, ~(1 << 11)).OrderBy(h => h.distance).ToArray(); ;
        
        if (hits.Length > 0)
        {
            //Debug.Log(hits[0].collider.name + " " + hits[0].collider.gameObject.layer + " ");

            if (hits[0].collider.GetComponent<HoldableObject>() != null && (hits[0].collider.gameObject.layer == 9))
            {

                currentlyLookingAt = hits[0].collider.GetComponent<HoldableObject>();
            

            }

            else if (hits[0].collider.GetComponent<InteractableDoor>() != null && (hits[0].collider.gameObject.layer == 10))
            {

                currentlyLookingAt = hits[0].collider.GetComponent<InteractableDoor>();

            }
            else
            {
                currentlyLookingAt = null;
            }

        }

        else if (hits.Length == 0)
        {
            currentlyLookingAt = null;
        }



        if (currentlyLookingAt != null)
        {
            if (currentlyLookingAt.GetComponent<InteractableDoor>() != null || currentlyLookingAt.GetComponent<HoldableObject>() != null)
            {

                switch (currentlyLookingAt.gameObject.layer)
                {
                    case 9:
                        pickup.gameObject.SetActive(true);
                        break;
                    case 10:
                        open.gameObject.SetActive(true);
                        break;
                }

            }

        }

        else
        {

            pickup.gameObject.SetActive(false);

            open.gameObject.SetActive(false);



        }

        PickupSystem();

        
    }


    private void dropInventoryObject()
    {
        inventorySlots[currentSelectedInventorySlot].gameObject.SetActive(true);
        inventorySlots[currentSelectedInventorySlot].gameObject.transform.parent = null;
        inventorySlots[currentSelectedInventorySlot].gameObject.transform.position = dropLocation.transform.position;
        inventorySlots.RemoveAt(currentSelectedInventorySlot);


    }
}
