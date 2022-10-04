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
	private PlayerController player;
	private InventoryMenuSystem inventory;

	//building
	public bool buildOn;

	private bool canPlaceAtLocation;
	//
	public GameObject currentlyLookingAt;
	public GameObject currentPlaceItemPrefab;
	Material transparentPlaceMaterialGood;
	Material transparentPlaceMaterialCollision;

	Quaternion buildRotationOffset = Quaternion.identity;
	float currentRotZ = 0;
	float currentRotX = 0;

	public Transform dropLocation;

	public Image interact;

	public Image Cursor;

	public bool canGrab = true;

	public float pickupTime = 0.75f;


	public LayerMask placingLayerMask;
	public LayerMask grabbingLayerMask;

	private void Awake()
	{
		buildOn = false;
		transparentPlaceMaterialGood = Resources.Load("Materials/TransparentPlaceMaterialGood", typeof(Material)) as Material;
		transparentPlaceMaterialCollision = Resources.Load("Materials/TransparentPlaceMaterialCollision", typeof(Material)) as Material;
		player = GetComponent<PlayerController>();
		inventory = GetComponent<InventoryMenuSystem>();
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

	public HoldableObject GetObjectInHand()
    {
		if (inventory.rHand.itemsInSlot.Count > 0)
			return inventory.rHand.itemsInSlot[0].connectedObject;

		else return null;
	}
	
	public void SetThrow()
	{
		
		BPUtil.SetAllChildrenToLayer(GetObjectInHand().transform, 9);
		BPUtil.SetAllColliders(GetObjectInHand().transform, true);

		GetObjectInHand().GetComponent<HoldableObject>().rb.isKinematic = false;
		GetObjectInHand().transform.parent = null;
		
		player.holdLocation.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.1f, 0.1f);
		player.holdLocation.GetComponent<AudioSource>().Play();

		SceneManager.MoveGameObjectToScene(GetObjectInHand().gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((GetObjectInHand().CustomHoldAnimation != "") ? GetObjectInHand().CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);

		GetObjectInHand().transform.position = player.RHandLocation.transform.position;

		GetObjectInHand().transform.rotation = Quaternion.LookRotation(player.playerCamera.transform.forward, player.playerCamera.transform.up);
		GetObjectInHand().Throw(player.playerCamera.transform.forward.normalized * GetObjectInHand().ThrowMultiplier * ((player.GetComponent<Rigidbody>().velocity.magnitude / 10) + 1));
		
		inventory.rHand.RemoveItemFromSlot(inventory.rHand.itemsInSlot[0].connectedObject, true);

		//player.bodyAnim.SetBool("isThrowing", false);
	}
	//raycast can go through multiple objects
	public IEnumerator StartPickup(InteractableObject holdableObject)
    {
		float currentTime = pickupTime;

		while (currentTime >= 0 && Input.GetButtonDown("Grab"))
		{
			currentTime--;
			yield return new WaitForSeconds(Time.deltaTime * 50f);
			interact.fillAmount = currentTime;

		}

		if (currentTime > 0.1f)

			yield break;

        else if (holdableObject)
        {
			FinalizePickup(holdableObject);
        }
	}
	public void FinalizePickup(InteractableObject holdableObject)
	{
		InventorySlot slotToAddTo = player.GetComponent<InventoryMenuSystem>().GetNextAvailableInventorySlot((HoldableObject)holdableObject);
		
		if (slotToAddTo != null)
        {
			if (slotToAddTo == player.GetComponent<InventoryMenuSystem>().rHand)
				player.bodyAnim.SetTrigger("isGrabbingRight");

			else
			{
				player.bodyAnim.SetTrigger("isGrabbingLeft");
			}		
			
			canGrab = false;

			
			/*if ((HoldableObject)currentlyLookingAt.large == true)
			{
				player.playerHealth.canRun = false;
				player.currentPlayerState = PlayerController.PLAYERSTATES.WALK;
			}
			//add to inventory
			else
			{}*/
			slotToAddTo.AddItemToSlot((HoldableObject)holdableObject);
			//GetObjectInRightHand().transform.localRotation = localRotation;

			GetObjectInHand().Hold(this, true);
		}
		
	}
	public void SetDrop()
	{

		BPUtil.SetAllChildrenToLayer(GetObjectInHand().transform, 9);
		BPUtil.SetAllColliders(GetObjectInHand().transform, true);

		GetObjectInHand().GetComponent<HoldableObject>().rb.isKinematic = false;
		GetObjectInHand().transform.parent = null;
		GetObjectInHand().GetComponent<HoldableObject>().saveableData.instance = GetObjectInHand().GetComponent<HoldableObject>();
		GetObjectInHand().Throw(-Vector3.up);

		SceneManager.MoveGameObjectToScene(GetObjectInHand().gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((GetObjectInHand().CustomHoldAnimation != "") ? GetObjectInHand().CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);

		inventory.rHand.RemoveItemFromSlot(GetObjectInHand(), true);

	}
	public void SetPlace(Vector3 location, Quaternion rotation)
	{
		BPUtil.SetAllChildrenToLayer(GetObjectInHand().transform, 9);
		GetObjectInHand().transform.parent = null;
		GetObjectInHand().GetComponent<HoldableObject>().saveableData.instance = GetObjectInHand().GetComponent<HoldableObject>();

		SceneManager.MoveGameObjectToScene(GetObjectInHand().gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((GetObjectInHand().CustomHoldAnimation != "") ? GetObjectInHand().CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);
		GetObjectInHand().GetComponent<HoldableObject>().rb.isKinematic = false;
		GetObjectInHand().transform.position = location;
		GetObjectInHand().transform.rotation = rotation;
		
		inventory.rHand.RemoveItemFromSlot(GetObjectInHand(), true);

		if (currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
		}

		currentPlaceItemPrefab = null;
	}
	
	//called after inventory item is added
	public void OnInventoryItemAddedToSlot_CallBack(InventorySlot slotUpdated, HoldableObject itemMoved)
    {
		//item was moved to hand
        if (slotUpdated == GetComponent<InventoryMenuSystem>().rHand)
        {
			player.bodyAnim.SetBool((GetObjectInHand().CustomHoldAnimation != "") ? GetObjectInHand().CustomHoldAnimation : "isHoldingSmall", value: true);

			if (GetObjectInHand().GetComponent<HoldableObject>().large)
			{
				BPUtil.SetAllChildrenToLayer(GetObjectInHand().transform, 14);
			}
			else
			{
				BPUtil.SetAllChildrenToLayer(GetObjectInHand().transform, 23);
			}
			
			GetObjectInHand().GetComponent<HoldableObject>().rb.isKinematic = true;
			GetObjectInHand().transform.parent = (GetObjectInHand().GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.RHandLocation.transform);
			GetObjectInHand().transform.position = (GetObjectInHand().GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.RHandLocation.transform.position);
			Quaternion localRotation = Quaternion.Euler(player.neck.transform.localRotation.x / 2f, player.neck.transform.localRotation.y, player.neck.transform.localRotation.z);
			
			GetObjectInHand().transform.localRotation = localRotation;

			GetObjectInHand().Hold(this, true);
		}
		//item moved away from hand
        else if (GetComponent<InventoryMenuSystem>().rHand.itemsInSlot.Count == 0)
        {
			player.bodyAnim.SetBool((itemMoved.CustomHoldAnimation != "") ? itemMoved.CustomHoldAnimation : "isHoldingSmall", value: false);

		}
		
	}
	void Update()
	{
		
		if (GameSettings.Instance.PauseMenuOpen || GameSettings.Instance.IsCutScene || GetComponent<InventoryMenuSystem>().menuOpen)
		{
			return;
		}

		if (Input.GetButtonDown("ToggleBuilding") && !Input.GetMouseButtonDown(1))

		
		if (Input.GetButtonDown("ToggleBuilding") && !Input.GetMouseButton(1))
		{
			buildOn = !buildOn;
		}
		//0 = closest
		RaycastHit[] raycastForGrabbing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 3f, grabbingLayerMask)
							  orderby h.distance
							  select h).ToArray();
		//Debug.DrawRay(player.playerCamera.transform.position, player.playerCamera.transform.forward * 6f, Color.red);
		
		RaycastHit[] raycastForPlacing = (from h in Physics.RaycastAll(new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward), 6f, placingLayerMask)
										   orderby h.distance
										   select h).ToArray();
		//update currently looking at
		if (raycastForGrabbing.Length > 0 && raycastForGrabbing[0].collider.transform.parent.parent.gameObject != null)
		{
			GameObject interactable = raycastForGrabbing[0].collider.transform.parent.parent.gameObject;

			currentlyLookingAt = interactable;
		}
		
		else if (raycastForGrabbing.Length == 0)
		{

			currentlyLookingAt = null;
		}

		if (raycastForPlacing.Length > 0)
		{
			if (GetObjectInHand() != null && buildOn)
			{
				//add in indicator prefab
				if (currentPlaceItemPrefab == null)
				{
					InteractableObject heldObjectTemplate = null;

					GameSettings.Instance.PropDatabase.TryGetValue(GetObjectInHand().type, out heldObjectTemplate);

					currentPlaceItemPrefab = Instantiate(heldObjectTemplate.gameObject);

					//destroy the script so it doesnt do weird shit

					BPUtil.SetAllCollidersToTrigger(currentPlaceItemPrefab.transform);
					BPUtil.SetAllChildrenToLayer(currentPlaceItemPrefab.transform, 21);
					Destroy(currentPlaceItemPrefab.GetComponent<InteractableObject>());
					currentPlaceItemPrefab.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				}
				//handle updating placing indicator
				else if (currentPlaceItemPrefab != null)
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

				//destroy if placed
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
						buildOn = false;
					}

				}
			}

		}

		//destroy building indicator prefab
		if (((GetObjectInHand() == null || !buildOn) && currentPlaceItemPrefab != null) || raycastForPlacing.Length == 0 && currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
			currentRotX = 0;
			currentRotZ = 0;
			buildRotationOffset = Quaternion.identity;
			currentPlaceItemPrefab = null;
		}

		if (currentlyLookingAt != null)
		{
			

			switch (currentlyLookingAt.layer)
			{

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
			if (!GetObjectInHand().large)
            {
				if (Input.GetMouseButton(1) && !player.bodyAnim.GetBool("isPreparingThrow") && (!buildOn || raycastForPlacing.Length == 0))
				{

					player.bodyAnim.SetBool("isPreparingThrow", true);
					player.bodyAnim.ResetTrigger("isThrowing");
				}

				if (Input.GetMouseButtonUp(1) && (!buildOn || raycastForPlacing.Length == 0))
				{
					//Debug.Log("Throw");

					player.bodyAnim.SetBool("isPreparingThrow", false);
					player.bodyAnim.SetTrigger("isThrowing");


				}
			}
			

			if (Input.GetButtonDown("Drop") && (Mathf.Abs(player.neck.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !GetObjectInHand().GetComponent<HoldableObject>().large))
			{
				SetDrop();
			}
		}

		if (currentlyLookingAt != null && (currentlyLookingAt.gameObject.layer == 25 || currentlyLookingAt.gameObject.layer == 10) && Input.GetButtonDown("Use"))
		{
			currentlyLookingAt.GetComponent<InteractableObject>().Use(this, LMB: false);
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (GetObjectInHand() != null)
			{
				GetObjectInHand().Use(this, LMB: true);
			}
		}
		/*else if (Input.GetMouseButtonDown(1) && player.holding != null)
		{
			player.holding.Use(this, LMB: false);
		}*/
	}

}
