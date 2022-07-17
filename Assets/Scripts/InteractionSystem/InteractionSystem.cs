// InteractionSystem
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
	private int inventoryLimit = 2;

	private int currentSelectedInventorySlot = -1;

	private bool inventoryOpened;

	private bool canPlaceAtLocation;

	private PlayerController player;

	public InteractableObject currentlyLookingAt;
	public GameObject currentPlaceItemPrefab;
	Material transparentPlaceMaterialGood;
	Material transparentPlaceMaterialCollision;

	public Transform dropLocation;

	public RawImage pickup;
	public RawImage open;

	public List<HoldableObject> inventorySlots;

	public GameObject inventoryObject;

	public LayerMask placingLayerMask;

	

	private void Awake()
	{
		transparentPlaceMaterialGood = Resources.Load("Materials/TransparentPlaceMaterialGood", typeof(Material)) as Material;
		transparentPlaceMaterialCollision = Resources.Load("Materials/TransparentPlaceMaterialCollision", typeof(Material)) as Material;
		inventorySlots = new List<HoldableObject>();
		player = GetComponent<PlayerController>();
	}

	private void SetAllChildrenToLayer(Transform top, int layer)
	{
		top.gameObject.layer = layer;
		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				item.gameObject.layer = layer;
				SetAllChildrenToLayer(item, layer);
			}
			else
			{
				item.gameObject.layer = layer;
			}
		}
	}

	private void SetAllCollidersToTrigger(Transform top)
	{
		if (top.gameObject.GetComponent<Collider>() != null)
		{
			foreach (Collider collider in top.gameObject.GetComponents<Collider>())
			{
				collider.isTrigger = true;
				
			}
			if (top.gameObject.GetComponent<WTTB_ExtraCollisionData>() == null)
				top.gameObject.AddComponent<WTTB_ExtraCollisionData>();
		}
		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						Destroy(collider);
					}
				}
				SetAllCollidersToTrigger(item);
			}
			else
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						Destroy(collider);
					}
				}
			}
		}
	}

	private bool CheckForTriggerCollision(Transform top)
	{
		if (top.gameObject.GetComponent<WTTB_ExtraCollisionData>() != null)

			if (top.gameObject.GetComponent<WTTB_ExtraCollisionData>().isCollidingTrigger)
            {
				Debug.Log("Collision");
				return true;
			}	

			else
			{
				return false;
			}
        else
        {
			return false;
        }
		
	}


	private void ChangeAllMeshMaterials(Transform top, Material mat)
	{
		if (top.gameObject.GetComponent<Renderer>() != null)
        
			top.gameObject.GetComponent<Renderer>().material = mat;
		
			
		
		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Renderer>() != null)
					item.gameObject.GetComponent<Renderer>().material = mat;

				ChangeAllMeshMaterials(item, mat);
			}
			else
			{
				if (item.gameObject.GetComponent<Renderer>() != null)
					item.gameObject.GetComponent<Renderer>().material = mat;
			}
		}
	}

	private void InventoryManager()
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

	public void SetThrow()
	{
		
		SetAllChildrenToLayer(player.holding.transform, 9);
		player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = false;
		player.holding.transform.parent = null;
		
		player.holdLocation.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.1f, 0.1f);
		player.holdLocation.GetComponent<AudioSource>().Play();
		Collider[] components = player.holding.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		SceneManager.MoveGameObjectToScene(player.holding.gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((player.holding.CustomHoldAnimation != "") ? player.holding.CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);

		player.holding.Throw(player.head.transform.forward * 400f * player.holding.GetComponent<Rigidbody>().mass);

		player.holding = null;
	}

	public void SetHolding()
	{
		player.holding = currentlyLookingAt.GetComponent<HoldableObject>();

		if (player.holding.GetComponent<HoldableObject>().large)
		{
			SetAllChildrenToLayer(player.holding.transform, 14);
		}
		else
		{
			SetAllChildrenToLayer(player.holding.transform, 13);
		}

		Collider[] components = player.holding.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
		player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = true;
		player.holding.transform.parent = (player.holding.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.handLocation.transform);
		player.holding.transform.position = (player.holding.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.handLocation.transform.position);
		Quaternion localRotation = Quaternion.Euler(player.head.transform.localRotation.x / 2f, player.head.transform.localRotation.y, player.head.transform.localRotation.z);

		if (player.holding.GetComponent<HoldableObject>().large)
		{
			player.bodyAnim.SetBool("isHoldingLarge", value: true);
			player.playerHealth.canRun = false;
			player.currentPlayerState = PlayerController.PLAYERSTATES.WALK;
		}
		else
		{
			player.bodyAnim.SetBool((player.holding.CustomHoldAnimation != "") ? player.holding.CustomHoldAnimation : "isHoldingSmall", value: true);
		}
		player.holding.transform.localRotation = localRotation;

		player.holding.Hold(this);
	}

	public void SetDrop()
	{
		SetAllChildrenToLayer(player.holding.transform, 9);
		player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = false;
		player.holding.transform.parent = null;
		player.holding.GetComponent<HoldableObject>().saveableData.instance = player.holding.GetComponent<HoldableObject>();
		player.holding.Throw(-Vector3.up);
		Collider[] components = player.holding.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		SceneManager.MoveGameObjectToScene(player.holding.gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((player.holding.CustomHoldAnimation != "") ? player.holding.CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);
		player.holding = null;
	}

	public void SetPlace(Vector3 location)
	{
		SetAllChildrenToLayer(player.holding.transform, 9);
		player.holding.transform.parent = null;
		player.holding.GetComponent<HoldableObject>().saveableData.instance = player.holding.GetComponent<HoldableObject>();
		Collider[] components = player.holding.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		SceneManager.MoveGameObjectToScene(player.holding.gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((player.holding.CustomHoldAnimation != "") ? player.holding.CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);
		player.holding.GetComponent<HoldableObject>().holdableObject.isKinematic = false;
		player.holding.transform.position = location;
		player.holding.transform.rotation = player.transform.rotation;
		player.holding = null;

		if (currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
		}

		currentPlaceItemPrefab = null;
	}

	private void PickupSystem()
	{

		if (player.holding == null && Input.GetButton("Hold") && currentlyLookingAt != null && currentlyLookingAt.gameObject.tag != "Usable")
		{
			SetHolding();
		}
		if (Input.GetButtonDown("Place"))
		{
			//SetPlace();
		}
		if (Input.GetButtonDown("Throw") && player.holding != null && (Mathf.Abs(player.head.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !player.holding.GetComponent<HoldableObject>().large))
		{
			SetThrow();
		}
		else if (Input.GetButtonDown("Drop") && player.holding != null && (Mathf.Abs(player.head.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !player.holding.GetComponent<HoldableObject>().large))
		{
			SetDrop();
		}
		if (currentlyLookingAt != null && currentlyLookingAt.gameObject.tag == "Usable" && Input.GetButtonDown("Hold"))
		{
			currentlyLookingAt.Use(this, false);
		}
		if (Input.GetButtonDown("Pickup") && inventorySlots.Count < inventoryLimit && currentlyLookingAt != null)
		{
			currentlyLookingAt.AddToInv(this);
			currentSelectedInventorySlot++;
		}
		if (Input.GetButtonDown("Drop") && inventorySlots.Count > 0)
		{
			SetAllChildrenToLayer(inventoryObject.transform.GetChild(currentSelectedInventorySlot), 9);
			DropInventoryObject();
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (player.holding != null)
			{
				player.holding.Use(this, LMB: true);
			}
		}
		else if (Input.GetMouseButtonDown(1) && player.holding != null)
		{
			player.holding.Use(this, LMB: false);
		}
	}

	private void Update()
	{
		if (GameSettings.Instance.PauseMenuOpen)
		{
			return;
		}
		//0 = closest
		RaycastHit[] raycastForGrabbing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 2f, -2049)
							  orderby h.distance
							  select h).ToArray();

		RaycastHit[] raycastForPlacing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 10f, placingLayerMask)
										   orderby h.distance
										   select h).ToArray();

		if (raycastForGrabbing.Length != 0)
		{
			if (raycastForGrabbing[0].collider.GetComponent<HoldableObject>() != null && raycastForGrabbing[0].collider.gameObject.layer == 9)
			{
				currentlyLookingAt = raycastForGrabbing[0].collider.GetComponent<HoldableObject>();
			}
			else if (raycastForGrabbing[0].collider.GetComponent<InteractableDoor>() != null && raycastForGrabbing[0].collider.gameObject.layer == 10)
			{
				currentlyLookingAt = raycastForGrabbing[0].collider.GetComponent<InteractableDoor>();
			}
			else if (raycastForGrabbing[0].collider.GetComponent<InteractableButton>() != null && raycastForGrabbing[0].collider.gameObject.layer == 17)
			{
				currentlyLookingAt = raycastForGrabbing[0].collider.GetComponent<InteractableButton>();
			}
			else
			{
				currentlyLookingAt = null;
			}
			
		}
		
		else if (raycastForGrabbing.Length == 0)
		{

			currentlyLookingAt = null;
		}

		if (raycastForPlacing.Length != 0)
		{
			if (player.holding != null && player.holding.canPlace)
			{
				if (currentPlaceItemPrefab == null)
				{
					InteractableObject heldObjectTemplate = null;
					GameSettings.Instance.PropDatabase.TryGetValue(player.holding.type, out heldObjectTemplate);

					currentPlaceItemPrefab = Instantiate(heldObjectTemplate.gameObject);

					//destroy the script so it doesnt do weird shit

					SetAllCollidersToTrigger(currentPlaceItemPrefab.transform);
					SetAllChildrenToLayer(currentPlaceItemPrefab.transform, 21);
					Destroy(currentPlaceItemPrefab.GetComponent<InteractableObject>());
				}
				else if (currentPlaceItemPrefab != null && raycastForPlacing.Length > 0)
				{
					currentPlaceItemPrefab.transform.position = raycastForPlacing[0].point;
					currentPlaceItemPrefab.transform.rotation = player.transform.rotation;
					
					if (CheckForTriggerCollision(currentPlaceItemPrefab.transform))
                    {
						canPlaceAtLocation = false;
						ChangeAllMeshMaterials(currentPlaceItemPrefab.transform, transparentPlaceMaterialCollision);

                    }

						
					else
					{
						canPlaceAtLocation = true;
						ChangeAllMeshMaterials(currentPlaceItemPrefab.transform, transparentPlaceMaterialGood);
					}
				}

				if (player.holding.canPlace && Input.GetMouseButtonDown(1) && currentPlaceItemPrefab != null)
				{
					if (canPlaceAtLocation)
                    {
						Destroy(currentPlaceItemPrefab.gameObject);
						SetPlace(currentPlaceItemPrefab.transform.position);
						currentPlaceItemPrefab = null;
					}
					

				}
			}

		}


		if (currentlyLookingAt != null)
		{
			if ((bool)currentlyLookingAt.GetComponent<InteractableObject>())
			{
				switch (currentlyLookingAt.gameObject.layer)
				{
					case 9:
						pickup.gameObject.SetActive(value: true);
						break;
					case 10:
						open.gameObject.SetActive(value: true);
						break;
					case 17:
						open.gameObject.SetActive(value: true);
						break;
				}
			}
		}
		else
		{
			pickup.gameObject.SetActive(value: false);
			open.gameObject.SetActive(value: false);
		}
		PickupSystem();
	}

	private IEnumerator DropObjectAtCorrectTime()
	{
		yield return new WaitForSeconds(0.5f);
		inventorySlots[currentSelectedInventorySlot].gameObject.SetActive(value: true);
		inventorySlots[currentSelectedInventorySlot].gameObject.transform.parent = null;
		inventorySlots[currentSelectedInventorySlot].gameObject.transform.position = player.handLocation.transform.position;
		inventorySlots[currentSelectedInventorySlot].gameObject.transform.rotation = player.handLocation.transform.rotation;
		inventorySlots.RemoveAt(currentSelectedInventorySlot);
		currentSelectedInventorySlot--;
		player.bodyAnim.SetBool("DropItem", value: false);
	}

	private void DropInventoryObject()
	{
		player.bodyAnim.SetBool("DropItem", value: true);
		StartCoroutine(DropObjectAtCorrectTime());
	}
}
