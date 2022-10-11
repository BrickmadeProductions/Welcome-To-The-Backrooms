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

	bool buildOn;

	private PlayerController player;

	public InteractableObject currentlyLookingAt;
	public GameObject currentPlaceItemPrefab;
	Material transparentPlaceMaterialGood;
	Material transparentPlaceMaterialCollision;

	Quaternion buildRotationOffset = Quaternion.identity;
	float currentRotZ = 0;
	float currentRotX = 0;

	public Transform dropLocation;

	public RawImage pickup;
	public RawImage open;

	public List<HoldableObject> inventorySlots;

	public GameObject inventoryObject;

	public LayerMask placingLayerMask;
	public LayerMask grabbingLayerMask;


	private void Awake()
	{
		buildOn = false;
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
		//Destroy(top.GetComponent<Rigidbody>());

		foreach (Transform item in top)
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach(Collider collider in item.gameObject.GetComponents<Collider>())
                    {
						collider.isTrigger = true;
					}
				
					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() == null)
						item.gameObject.AddComponent<WTTB_ExtraCollisionData>();
				}
				SetAllCollidersToTrigger(item);
			}
			else
			{
				if (item.gameObject.GetComponent<Collider>() != null)
				{
					foreach (Collider collider in item.gameObject.GetComponents<Collider>())
					{
						collider.isTrigger = true;
					}

					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() == null)
						item.gameObject.AddComponent<WTTB_ExtraCollisionData>();
				}
			}
		}
	}

	private bool CheckForCollision(Transform trans)
	{
		//Debug.Log(trans.childCount);
		//Debug.Log(trans.name);
		foreach (Transform item in trans.GetChild(0))
		{
			if (item.childCount > 0)
			{
				if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() != null)
				{

					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>().isCollidingTrigger)
					{
						return true;


					}

					else
                    {
						
						return false;

                    }

				}


			}
			else
			{
				if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>() != null)
				{
					if (item.gameObject.GetComponent<WTTB_ExtraCollisionData>().isCollidingTrigger)
					{
						return true;

					}

					else
					{
						
						return false;

					}

				}
			}
		}

		return false;
		
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

		//player.holding.transform.position += player.head.transform.forward * 1.2f;
		player.holding.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		player.holding.Throw(player.head.transform.forward * ((player.GetComponent<CharacterController>().velocity.magnitude / 10) + 1) * player.holding.ThrowMultiplier * player.holding.GetComponent<Rigidbody>().mass);

		player.holding = null;

	    //player.bodyAnim.SetBool("isThrowing", false);
	}

	//raycast can go through multiple objects
	public void SetHolding(InteractableObject holdableObject)
	{
		player.holding = (HoldableObject)holdableObject;

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

	public void SetPlace(Vector3 location, Quaternion rotation)
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
		player.holding.transform.rotation = rotation;
		player.holding = null;

		if (currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
		}

		currentPlaceItemPrefab = null;
	}

	private void PickupSystem()
	{
		if (Input.GetButton("Grab") && currentlyLookingAt != null && player.holding == null && currentlyLookingAt.gameObject.tag != "Usable")
		{
			//pickup
			SetHolding(currentlyLookingAt.GetComponent<HoldableObject>());
		}

		if (player.holding != null)
        {

			if (Input.GetMouseButton(1) && !player.bodyAnim.GetBool("isPreparingThrow") && !player.bodyAnim.GetBool("isThrowing"))
			{
				player.bodyAnim.SetBool("isPreparingThrow", true);
			}

			/*if (Input.GetButtonDown("Throw") && (Mathf.Abs(player.head.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !player.holding.GetComponent<HoldableObject>().large))
			{
				SetThrow();
			}*/
			if (Input.GetMouseButtonUp(1))
			{
				//Debug.Log("Throw");
				
				player.bodyAnim.SetBool("isPreparingThrow", false);
				player.bodyAnim.SetTrigger("isThrowing");
				SetThrow();
			}

			if (Input.GetButtonDown("Drop") && (Mathf.Abs(player.head.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !player.holding.GetComponent<HoldableObject>().large))
			{
				SetDrop();
			}
		}

		if (currentlyLookingAt != null && currentlyLookingAt.gameObject.tag == "Usable" && Input.GetButtonDown("Grab"))
		{
			currentlyLookingAt.Use(this, false);
		}
<<<<<<< Updated upstream
		if (Input.GetButtonDown("Pickup") && inventorySlots.Count < inventoryLimit && currentlyLookingAt != null)
=======
		
	}
	void LateUpdate()
	{
		
		if (GameSettings.Instance.PauseMenuOpen || GameSettings.Instance.IsCutScene || GetComponent<InventoryMenuSystem>().menuOpen)
>>>>>>> Stashed changes
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
		/*else if (Input.GetMouseButtonDown(1) && player.holding != null)
		{
			player.holding.Use(this, LMB: false);
		}*/
	}

	private void Update()
	{
		if (GameSettings.Instance.PauseMenuOpen)
		{
			return;
		}
		//0 = closest
		RaycastHit[] raycastForGrabbing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 2f, grabbingLayerMask)
							  orderby h.distance
							  select h).ToArray();

		RaycastHit[] raycastForPlacing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 6f, placingLayerMask)
										   orderby h.distance
										   select h).ToArray();

		if (raycastForGrabbing.Length > 0 && raycastForGrabbing[0].collider.transform.parent.parent.gameObject != null)
		{
			GameObject interactable = raycastForGrabbing[0].collider.transform.parent.parent.gameObject;

			if (interactable.gameObject.layer == 9)
			{
				currentlyLookingAt = interactable.GetComponent<HoldableObject>();
			}
			else if (interactable.gameObject.layer == 10)
			{
				currentlyLookingAt = interactable.GetComponent<InteractableDoor>();
			}
			else if (interactable.gameObject.layer == 17)
			{
				currentlyLookingAt = interactable.GetComponent<InteractableButton>();
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
			if (player.holding != null)
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
					currentPlaceItemPrefab.transform.rotation = player.transform.localRotation * buildRotationOffset;

					//Debug.Log(CheckForCollision(currentPlaceItemPrefab.transform));

					if (CheckForCollision(currentPlaceItemPrefab.transform))
                    {
						canPlaceAtLocation = false;
						ChangeAllMeshMaterials(currentPlaceItemPrefab.transform, transparentPlaceMaterialCollision);

                    }

						
					else
					{
						canPlaceAtLocation = true;
						ChangeAllMeshMaterials(currentPlaceItemPrefab.transform, transparentPlaceMaterialGood);
					}

					if (Input.GetButton("RotateBuildSystemRight"))
                    {
						currentRotZ -= 75f * Time.deltaTime;
						
					}

					if (Input.GetButton("RotateBuildSystemLeft"))
					{
						currentRotZ += 75f * Time.deltaTime;
						
					}

					if (Input.GetButton("RotateBuildSystemUp"))
					{
						currentRotX += 75f * Time.deltaTime;

					}

					if (Input.GetButton("RotateBuildSystemDown"))
					{
						currentRotX -= 75f * Time.deltaTime;

					}

					buildRotationOffset = Quaternion.Euler(currentRotX, 0, currentRotZ);
				}
				if (Input.GetButtonDown("ToggleBuilding"))
                {
					buildOn = !buildOn;
                }
					

				if (Input.GetMouseButtonDown(1) && currentPlaceItemPrefab != null && buildOn)
				{
					if (canPlaceAtLocation)
                    {
						Destroy(currentPlaceItemPrefab.gameObject);
						SetPlace(currentPlaceItemPrefab.transform.position, currentPlaceItemPrefab.transform.rotation);
						currentRotX = 0;
						currentRotZ = 0;
						buildRotationOffset = Quaternion.identity;
						currentPlaceItemPrefab = null;
					}

				}
			}

		}

		//destroy building indicator prefab
		if ((player.holding == null || !buildOn) && currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
			currentRotX = 0;
			currentRotZ = 0;
			buildRotationOffset = Quaternion.identity;
			currentPlaceItemPrefab = null;
		}


		if (currentlyLookingAt != null)
		{
			if (currentlyLookingAt.GetComponent<InteractableObject>())
			{
<<<<<<< Updated upstream
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
=======

				case 9:
					interact.gameObject.SetActive(true);
					interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";

					break;

				case 10:
					interact.gameObject.SetActive(true);
					interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[E] Open";

					break;

				case 20:

					if (GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) > 0)
					{
						interact.gameObject.SetActive(true);
						interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[E] No-Clip";
					}
					break;

				case 25:
					interact.gameObject.SetActive(true);
					interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[E] Inspect";

					break;

			}

		}

		else
		{
			interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
			interact.gameObject.SetActive(value: false);
		}

		//throwing and dropping

		//pickup if nothing in hand
		if (Input.GetButton("Grab") && currentlyLookingAt != null && currentlyLookingAt.layer != 25 && currentlyLookingAt.layer != 20 && currentlyLookingAt.layer != 14 && canGrab)
		{
			/*StartCoroutine(StartPickup(currentlyLookingAt));*/
			FinalizePickup(currentlyLookingAt.GetComponent<InteractableObject>());

		}

		if (GetObjectInHand() != null)
		{

			//throw system, only small objects
			if (!GetObjectInHand().large && GetObjectInHand().ThrowAble)
            {
				if (Input.GetMouseButton(1) && !player.bodyAnim.GetBool("isPreparingThrow") && (!buildOn || raycastForPlacing.Length == 0))
				{
					//Debug.Log("PREP Throw I.S.");
					player.bodyAnim.SetBool("isPreparingThrow", true);
					player.bodyAnim.ResetTrigger("isThrowing");
				}

				if (Input.GetMouseButtonUp(1) && (!buildOn || raycastForPlacing.Length == 0))
				{
					//Debug.Log("Throw I.S.");

					player.bodyAnim.SetBool("isPreparingThrow", false);
					player.bodyAnim.SetTrigger("isThrowing");


				}
			}
			

			if (Input.GetButtonDown("Drop") && (Mathf.Abs(player.neck.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !GetObjectInHand().GetComponent<HoldableObject>().large))
			{
				SetDrop();
>>>>>>> Stashed changes
			}
		}
		else
		{
<<<<<<< Updated upstream
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
=======
			if (GetObjectInHand() != null)
			{
				GetObjectInHand().Use(this, LMB: true);
			}
		}else if (Input.GetMouseButtonDown(1) && GetObjectInHand() != null)
		{
			GetObjectInHand().Use(this, LMB: false);
		}
	}
>>>>>>> Stashed changes
}
