// HoldableObject
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct InventoryObjectData
{
	public Texture image;

	public string name;

	public int inventoryWeight;

	public string description;

}
public class HoldableObject : InteractableObject
{
	public List<OBJECT_TYPE> canBeLoadedWith;

	public InventoryObjectData inventoryObjectData;

	//general
	public float durability;

	public GameObject[] breakablePrefabs;
	public Transform[] breakLocations;

	public AudioClip[] hitClips;

	public AudioClip[] breakClips;

	public Rigidbody rb;

	public float ThrowMultiplier = 2;

	private bool broken;

	public bool large;

	public bool autoSwing;

	public bool shouldDisableCrossHair;

	public InventoryItem connectedInvItem;

	//building system
	public bool canPlace;
	public bool isPlaced = false;

	public bool canBeUsed = true;

	//animations
	public bool animationPlaying;

	public Transform offHandIKPoint;

	public string StartUseAnimation;

	public List<string> UseAnimations;

	public string CustomHoldAnimation = "";

	public int currentAnimChoice = 0;

	private Vector3 pushAmt;

	public List<string> ItemObjectAnimations;


	private IEnumerator waitToPlaySound()
	{
		yield return new WaitForSecondsRealtime(2f);
		playSounds = true;
	}

	public override void Init()
	{
		stats = new Dictionary<string, string>();
		/*if (GetComponent<Animator>() != null)
		{
			StartCoroutine(playItemAnimation("Close"));
		}*/

		playSounds = false;
		broken = false;
		rb = GetComponent<Rigidbody>();
		
		StartCoroutine(waitToPlaySound());
	}
	public override void Drop(Vector3 force)
	{
		rb = gameObject.AddComponent<Rigidbody>();
		rb.velocity = force * ThrowMultiplier * ThrowMultiplier;
		if (GetComponent<Animator>() != null)
		{
			StartCoroutine(playItemAnimation("Close"));
		}
		


	}

	public IEnumerator playPlayerAnimation()
	{
		//starting animation when using
		if (StartUseAnimation != "")
        {
			GameSettings.GetLocalPlayer().bodyAnim.SetTrigger(StartUseAnimation);

			yield return new WaitUntil(() => GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(StartUseAnimation));

		}

		if (autoSwing)
        {
			//GameSettings.GetLocalPlayer().bodyAnim.ResetTrigger(StartUseAnimation);

			while (Input.GetMouseButton(0))
			{
				currentAnimChoice = UnityEngine.Random.Range(0, UseAnimations.Count);

				GameSettings.GetLocalPlayer().bodyAnim.SetBool(UseAnimations[currentAnimChoice], true);

				animationPlaying = true;

				yield return new WaitUntil(() => GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(UseAnimations[currentAnimChoice]));

				yield return new WaitForSecondsRealtime(GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

				GameSettings.GetLocalPlayer().bodyAnim.SetBool(UseAnimations[currentAnimChoice], false);
				GameSettings.GetLocalPlayer().bodyAnim.SetBool(StartUseAnimation, false);

				animationPlaying = false;
			}

			
		}

		else if (!animationPlaying)
        {
			currentAnimChoice = UnityEngine.Random.Range(0, UseAnimations.Count);

			GameSettings.GetLocalPlayer().bodyAnim.SetTrigger(UseAnimations[currentAnimChoice]);

			animationPlaying = true;

			yield return new WaitUntil(() => GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(UseAnimations[currentAnimChoice]));

			yield return new WaitForSecondsRealtime(GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.GetLocalPlayer().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

			GameSettings.GetLocalPlayer().bodyAnim.ResetTrigger(UseAnimations[currentAnimChoice]);
			GameSettings.GetLocalPlayer().bodyAnim.SetBool(StartUseAnimation, false);

			animationPlaying = false;
		}

		GameSettings.GetLocalPlayer().bodyAnim.ResetTrigger(StartUseAnimation);
	}

	public IEnumerator playItemAnimation(string animation)
	{
		if (GetComponent<Animator>() != null)
		{
			GetComponent<Animator>().SetTrigger(animation);

			yield return new WaitUntil(() => GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(animation));

			yield return new WaitForSecondsRealtime(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length / GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).speed);

			GetComponent<Animator>().ResetTrigger(animation);
		}
		
	}

	public override void Use(InteractionSystem player, bool LMB)
	{
		if (UseAnimations.Count > 0 && !GameSettings.Instance.cheatSheetObject.gameObject.activeSelf && canBeUsed)
		{
			if (LMB && !animationPlaying)
			{
				StartCoroutine(playPlayerAnimation());
			}
			/*else if (!LMB && RMBAnimationBools.Count > 0)
			{
				int choice = Random.Range(0, RMBAnimationBools.Count);
				StartCoroutine(playAnimation(RMBAnimationBools[choice], choice, LMB));
			}*/
		}
	}
	
    public override void Pickup(InteractionSystem player, bool RightHand)
    {

		

		if (RightHand)

			StartCoroutine(playItemAnimation("Open"));

		else
		{
			StartCoroutine(playItemAnimation("Close"));
		}
		
		

	}
    
	public void TakeDamage(float damage)
    {
		if (!(transform.gameObject.GetComponent<AudioSource>() != null) || !playSounds)
		{
			return;
		}
		if (breakablePrefabs.Length == 0)
		{
			return;
		}
		if (!transform.gameObject.GetComponent<AudioSource>().isPlaying && hitClips.Length != 0)
		{
			transform.gameObject.GetComponent<AudioSource>().clip = hitClips[UnityEngine.Random.Range(0, hitClips.Length)];
			transform.gameObject.GetComponent<AudioSource>().pitch = 1f + UnityEngine.Random.Range(-0.15f, 0.15f);
			transform.gameObject.GetComponent<AudioSource>().Play();
		}

		durability -= damage;

		if (durability < 0f && !broken)
		{
			if (type == OBJECT_TYPE.CHAIR)
				Steam.IncrementStat("CHAIRS_BROKEN", 1);

			AudioSource.PlayClipAtPoint(breakClips[UnityEngine.Random.Range(0, breakClips.Length)], transform.position);
			for (int i = 0; i < breakablePrefabs.Length; i++)
			{
				GameSettings.Instance.worldInstance.AddNewProp(breakLocations[i].position, breakLocations[i].rotation, GameSettings.Instance.PropDatabase[breakablePrefabs[i].GetComponent<HoldableObject>().type].gameObject);
			}
			broken = true;
			GameSettings.Instance.worldInstance.RemoveProp(GetWorldID(), true);
			
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude >= 4f)
			TakeDamage(collision.relativeVelocity.magnitude);

	}

    public override void OnSaveFinished()
    {
		onSave?.Invoke();
    }

    public override void OnLoadFinished()
    {
		onLoad?.Invoke();
	}
}
