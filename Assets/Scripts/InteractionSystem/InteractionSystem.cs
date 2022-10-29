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
	private InventorySystem inventory;

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

	public bool canCraft = true;

	public float pickupTime = 0.75f;

	public LayerMask placingLayerMask;
	public LayerMask grabbingLayerMask;

	public List<string> punchingAnimations;

	public List<Weapon> fists;

	private void Awake()
	{
		buildOn = false;
		transparentPlaceMaterialGood = Resources.Load("Materials/TransparentPlaceMaterialGood", typeof(Material)) as Material;
		transparentPlaceMaterialCollision = Resources.Load("Materials/TransparentPlaceMaterialCollision", typeof(Material)) as Material;
		player = GetComponent<PlayerController>();
		inventory = GetComponent<InventorySystem>();
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

	public HoldableObject GetObjectInRightHand()
    {
		if (inventory.rHand.itemsInSlot.Count > 0)
			return inventory.rHand.itemsInSlot[0].connectedObject;

		else return null;
	}

	public HoldableObject GetObjectInLeftHand()
	{
		if (inventory.lHand.itemsInSlot.Count > 0)
			return inventory.lHand.itemsInSlot[0].connectedObject;

		else return null;
	}

	public void SetThrow()
	{

		BPUtil.SetAllChildrenToLayer(GetObjectInRightHand().transform, 9);
		BPUtil.SetAllColliders(GetObjectInRightHand().transform, true);

		//GetObjectInRightHand().GetComponent<HoldableObject>().rb.isKinematic = false;
		GetObjectInRightHand().transform.parent = null;
		
		player.holdLocation.GetComponent<AudioSource>().pitch = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
		player.holdLocation.GetComponent<AudioSource>().Play();

		SceneManager.MoveGameObjectToScene(GetObjectInRightHand().gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((GetObjectInRightHand().CustomHoldAnimation != "") ? GetObjectInRightHand().CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);

		GetObjectInRightHand().transform.position = player.RHandLocation.transform.position;

		GetObjectInRightHand().transform.rotation = Quaternion.LookRotation(player.playerCamera.transform.forward, player.playerCamera.transform.up);
		GetObjectInRightHand().Drop(player.playerCamera.transform.forward.normalized * GetObjectInRightHand().ThrowMultiplier * ((player.GetComponent<Rigidbody>().velocity.magnitude / 10) + 1));
		
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
			yield return new WaitForSecondsRealtime(Time.deltaTime * 50f);
			interact.fillAmount = currentTime;

		}

		if (currentTime > 0.1f)

			yield break;

        else if (holdableObject)
        {
			FinalizePickup(holdableObject, true);
        }
	}
	public void SetOffHandIKInfo(HoldableObject objectToTest)
    {
		Transform offhandIK = objectToTest.offHandIKPoint;

		if (offhandIK != null)
		{
			GetComponent<PlayerController>().builder.layers[1].active = true;
			GetComponent<PlayerController>().offHandIK.data.target = offhandIK;
			GetComponent<PlayerController>().builder.Build();
		}
		else
		{
			GetComponent<PlayerController>().builder.layers[1].active = false;
			GetComponent<PlayerController>().offHandIK.data.target = null;
			GetComponent<PlayerController>().builder.Build();
		}
	}

	public void FinalizePickup(InteractableObject holdableObject, bool playAnimation)
	{
		InventorySlot slotToAddTo = player.GetComponent<InventorySystem>().GetNextAvailableInventorySlot((HoldableObject)holdableObject);


		if (slotToAddTo != null)
        {
			if (playAnimation)
            {
				if (slotToAddTo == player.GetComponent<InventorySystem>().rHand)
				{
					canGrab = false;
					player.bodyAnim.SetTrigger("isGrabbingRight");

				}


				else
				{

					player.bodyAnim.SetTrigger("isGrabbingLeft");
				}
			}


			slotToAddTo.AddItemToSlot((HoldableObject)holdableObject);

			holdableObject.Pickup(this, true);
			
		}

		

	}
	public void SetDrop(InventorySlot slot)
	{

		BPUtil.SetAllChildrenToLayer(slot.itemsInSlot[0].connectedObject.transform, 9);
		BPUtil.SetAllColliders(slot.itemsInSlot[0].connectedObject.transform, true);

		slot.itemsInSlot[0].connectedObject.transform.parent = null;
		slot.itemsInSlot[0].connectedObject.Drop(Vector3.zero);

		SceneManager.MoveGameObjectToScene(slot.itemsInSlot[0].connectedObject.gameObject, SceneManager.GetActiveScene());
		
		//only stop animation if in right hand
		if (slot == inventory.rHand)
        {
			player.bodyAnim.SetBool((slot.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slot.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);
			player.bodyAnim.SetBool("isHoldingLarge", value: false);
		}
		

		slot.itemsInSlot[0].connectedObject.transform.position = dropLocation.position;

		slot.RemoveItemFromSlot(slot.itemsInSlot[0].connectedObject, true);

		if (slot == GetComponent<InventorySystem>().rHand)
        {
			GetComponent<PlayerController>().offHandIK.data.target = null;
			GetComponent<PlayerController>().builder.layers[1].active = false;
			GetComponent<PlayerController>().builder.Build();
		}
		

	}
	public void SetPlace(Vector3 location, Quaternion rotation)
	{
		BPUtil.SetAllChildrenToLayer(GetObjectInRightHand().transform, 9);
		GetObjectInRightHand().transform.parent = null;
		GetObjectInRightHand().GetComponent<HoldableObject>().saveableData.instance = GetObjectInRightHand().GetComponent<HoldableObject>();

		SceneManager.MoveGameObjectToScene(GetObjectInRightHand().gameObject, SceneManager.GetActiveScene());
		player.bodyAnim.SetBool((GetObjectInRightHand().CustomHoldAnimation != "") ? GetObjectInRightHand().CustomHoldAnimation : "isHoldingSmall", value: false);
		player.bodyAnim.SetBool("isHoldingLarge", value: false);
		//GetObjectInRightHand().GetComponent<HoldableObject>().rb.isKinematic = false;
		GetObjectInRightHand().transform.position = location;
		GetObjectInRightHand().transform.rotation = rotation;
		
		inventory.rHand.RemoveItemFromSlot(GetObjectInRightHand(), true);

		if (currentPlaceItemPrefab != null)
		{
			Destroy(currentPlaceItemPrefab.gameObject);
		}

		currentPlaceItemPrefab = null;
	}
	
	//called after inventory item is added
	public void OnInventoryItemAddedToSlot_CallBack(InventorySlot slotFrom, InventorySlot slotTo)
    {
		if (GetComponent<InventorySystem>().rHand.itemsInSlot.Count > 0)
        {
			SetOffHandIKInfo(GetComponent<InventorySystem>().rHand.itemsInSlot[0].connectedObject);
		}
			
        else
        {
			GetComponent<PlayerController>().offHandIK.data.target = null;
			GetComponent<PlayerController>().builder.layers[1].active = false;
			GetComponent<PlayerController>().builder.Build();
		}
		//item got added to inv from outside to right hand
		if (slotTo == GetComponent<InventorySystem>().rHand && slotFrom == null)
        {
			player.bodyAnim.SetBool((slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: true);
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);

			StartCoroutine(slotTo.itemsInSlot[0].connectedObject.playItemAnimation("Open"));

			//slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().rb.isKinematic = true;
			slotTo.itemsInSlot[0].connectedObject.transform.parent = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.RHandLocation.transform);
			slotTo.itemsInSlot[0].connectedObject.transform.position = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.RHandLocation.transform.position);
			slotTo.itemsInSlot[0].connectedObject.transform.localRotation = Quaternion.identity;

			slotTo.itemsInSlot[0].connectedObject.Pickup(this, true);
			return;
		}
		//item got added to inv from outside and is going into a new slot
		if (slotTo != GetComponent<InventorySystem>().rHand && slotFrom == null)
		{
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);
			return;
		}
		//item swapped inside inv
		if (slotTo != GetComponent<InventorySystem>().rHand && slotFrom != GetComponent<InventorySystem>().rHand)
		{
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);
			return;
		}
		//item was moved to hand and not swapped
		if (slotTo == GetComponent<InventorySystem>().rHand && slotFrom.itemsInSlot.Count == 0)
		{
			player.bodyAnim.SetBool((slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: true);
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);

			StartCoroutine(slotTo.itemsInSlot[0].connectedObject.playItemAnimation("Open"));

			//slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().rb.isKinematic = true;
			slotTo.itemsInSlot[0].connectedObject.transform.parent = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.RHandLocation.transform);
			slotTo.itemsInSlot[0].connectedObject.transform.position = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.RHandLocation.transform.position);
			slotTo.itemsInSlot[0].connectedObject.transform.localRotation = Quaternion.identity;

			slotTo.itemsInSlot[0].connectedObject.Pickup(this, true);
			return;
		}
		//item moved away from hand and was not swapped (hand empty now)
		if (slotTo != GetComponent<InventorySystem>().rHand && slotFrom.itemsInSlot.Count == 0)
		{
			player.bodyAnim.SetBool((slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);

			StartCoroutine(slotTo.itemsInSlot[0].connectedObject.playItemAnimation("Close"));

			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);
			return;
		}

		//item swapped into hand from another slot
		if (slotTo == GetComponent<InventorySystem>().rHand && slotFrom.itemsInSlot.Count > 0)
		{
			player.bodyAnim.SetBool((slotFrom.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotFrom.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);

			BPUtil.SetAllChildrenToLayer(slotFrom.itemsInSlot[0].connectedObject.transform, 23);

			StartCoroutine(slotFrom.itemsInSlot[0].connectedObject.playItemAnimation("Close"));

			player.bodyAnim.SetBool((slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: true);
			
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);
			
			StartCoroutine(slotTo.itemsInSlot[0].connectedObject.playItemAnimation("Open"));

			//slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().rb.isKinematic = true;
			slotTo.itemsInSlot[0].connectedObject.transform.parent = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.RHandLocation.transform);
			slotTo.itemsInSlot[0].connectedObject.transform.position = (slotTo.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.RHandLocation.transform.position);
			slotTo.itemsInSlot[0].connectedObject.transform.localRotation = Quaternion.identity;

			slotTo.itemsInSlot[0].connectedObject.Pickup(this, true);

			
			return;
		}
		//item swapped away from hand into another slot
		if (slotFrom == GetComponent<InventorySystem>().rHand && slotTo.itemsInSlot.Count > 0)
		{
			player.bodyAnim.SetBool((slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotTo.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);
			BPUtil.SetAllChildrenToLayer(slotTo.itemsInSlot[0].connectedObject.transform, 23);

			StartCoroutine(slotTo.itemsInSlot[0].connectedObject.playItemAnimation("Close"));

			player.bodyAnim.SetBool((slotFrom.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? slotFrom.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: true);
			BPUtil.SetAllChildrenToLayer(slotFrom.itemsInSlot[0].connectedObject.transform, 23);

			StartCoroutine(slotFrom.itemsInSlot[0].connectedObject.playItemAnimation("Open"));

			//slotFrom.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().rb.isKinematic = true;
			slotFrom.itemsInSlot[0].connectedObject.transform.parent = (slotFrom.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform : player.RHandLocation.transform);
			slotFrom.itemsInSlot[0].connectedObject.transform.position = (slotFrom.itemsInSlot[0].connectedObject.GetComponent<HoldableObject>().large ? player.holdLocation.transform.position : player.RHandLocation.transform.position);
			slotFrom.itemsInSlot[0].connectedObject.transform.localRotation = Quaternion.identity;

			slotFrom.itemsInSlot[0].connectedObject.Pickup(this, true);

			return;
		}


	}
	void Update()
	{
		
		if (GameSettings.Instance.PauseMenuOpen || GameSettings.Instance.IsCutScene || inventory.menuOpen)
		{
			player.bodyAnim.SetBool("isPreparingThrow", false);
			player.bodyAnim.ResetTrigger("isThrowing");

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
			if (GetObjectInRightHand() != null && buildOn)
			{
				//add in indicator prefab
				if (currentPlaceItemPrefab == null)
				{
					InteractableObject heldObjectTemplate = null;

					GameSettings.Instance.PropDatabase.TryGetValue(GetObjectInRightHand().type, out heldObjectTemplate);

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
		if (((GetObjectInRightHand() == null || !buildOn) && currentPlaceItemPrefab != null) || raycastForPlacing.Length == 0 && currentPlaceItemPrefab != null)
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
					interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[F] Pickup";

					if (!player.hasGivenItemBreakingNotification && currentlyLookingAt.GetComponent<HoldableObject>().type == OBJECT_TYPE.CHAIR)
					{
						GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU CAN DESTROY CERTAIN OBJECTS AND GAIN MATERIALS (EX: CHAIRS) BY HITTING THEM OR JUMPING ON THEM");
						player.hasGivenItemBreakingNotification = true;
					}

					break;

				case 10:

					interact.gameObject.SetActive(true);
					interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[E] Open";

					break;

				case 20:

					if (GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) > 0)
					{
						interact.gameObject.SetActive(true);
						interact.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "[E] Attempt No-Clip";
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
		if (Input.GetButtonDown("Grab") && currentlyLookingAt != null && currentlyLookingAt.layer != 25 && currentlyLookingAt.layer != 20 && currentlyLookingAt.layer != 14 && canGrab)
		{
			if (currentlyLookingAt.GetComponent<HoldableObject>())

				if (!currentlyLookingAt.GetComponent<HoldableObject>().large)

					FinalizePickup(currentlyLookingAt.GetComponent<InteractableObject>(), true);

		}
		//double check
		if (Input.GetMouseButtonUp(1))
        {
			//Debug.Log("Throw");
			player.bodyAnim.SetBool("isPreparingThrow", false);
			player.bodyAnim.SetTrigger("isThrowing");
		}
					
		//throwing system
		if (GetObjectInRightHand() != null)
		{

			//throw system, only small objects
			if (!GetObjectInRightHand().large)
            {
			
				if (Input.GetMouseButton(1) && !player.bodyAnim.GetBool("isPreparingThrow") && (!buildOn || raycastForPlacing.Length == 0))
				{
					GetComponent<PlayerController>().offHandIK.data.target = null;
					GetComponent<PlayerController>().builder.layers[1].active = false;
					GetComponent<PlayerController>().builder.Build();
					player.bodyAnim.SetBool("isPreparingThrow", true);
					player.bodyAnim.ResetTrigger("isThrowing");
				}


				else if (Input.GetMouseButtonUp(1))
				{
					//Debug.Log("Throw");
					player.bodyAnim.SetBool("isPreparingThrow", false);
					player.bodyAnim.SetTrigger("isThrowing");


				}
				
			}

			if (Input.GetButtonDown("Drop") && (Mathf.Abs(player.neck.transform.localRotation.x * Mathf.Rad2Deg) < 20f || !GetObjectInRightHand().GetComponent<HoldableObject>().large))
			{
				SetDrop(inventory.rHand);
			}
		}

		if (currentlyLookingAt != null && (currentlyLookingAt.gameObject.layer == 25 || currentlyLookingAt.gameObject.layer == 10) && Input.GetButtonDown("Use"))
		{

			currentlyLookingAt.GetComponent<InteractableObject>().Use(this, LMB: false);
		}

		if (Input.GetMouseButtonDown(0))
		{
			bool hasCraftingRecipe = false;

			if (GetObjectInRightHand() != null)
			{
				/*//begin crafting
				if (GetObjectInLeftHand() != null)
                {
					if (canCraft)
					{
						

						foreach (CraftingPair pair in GameSettings.Instance.possibleCraftingPairs)
                        {
							player.bodyAnim.ResetTrigger(pair.craftingAnimation);

							if (GetObjectInLeftHand().type == pair.leftHand && GetObjectInRightHand().type == pair.rightHand)
							{
								StartCoroutine(GetObjectInLeftHand().playItemAnimation("Open"));
								hasCraftingRecipe = true;
								player.bodyAnim.SetTrigger(pair.craftingAnimation);
								break;
							}
						}
							
							
					}
                }*/
                /*if (!hasCraftingRecipe)
                {
					
				}*/

				GetObjectInRightHand().Use(this, LMB: true);

			}
            else
            {
				string animChoice = punchingAnimations[UnityEngine.Random.Range(0, punchingAnimations.Count)];

				bool canPunch = true;
				//reset punching
				foreach (string animation in punchingAnimations)
				{
					player.bodyAnim.ResetTrigger(animation);
					if (player.bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(animation))
						canPunch = false;
				}

				if (canPunch)
                {

					player.bodyAnim.SetTrigger(animChoice);
				}
					
			}
		}


		if (player.bodyAnim.GetCurrentAnimatorStateInfo(1).IsName("Punch_Right"))
		{
			fists[0].gameObject.GetComponent<Collider>().enabled = true;
			fists[1].gameObject.GetComponent<Collider>().enabled = false;
			

		}
		else if (player.bodyAnim.GetCurrentAnimatorStateInfo(1).IsName("Punch_Left"))
		{
			fists[0].gameObject.GetComponent<Collider>().enabled = false;
			fists[1].gameObject.GetComponent<Collider>().enabled = true;
			

		}
		else
        {
			fists[0].gameObject.GetComponent<Collider>().enabled = false;
			fists[1].gameObject.GetComponent<Collider>().enabled = false;
		}


	}

}

