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
	public InventoryObjectData inventoryObjectData;

	//general
	public float durability;

	public GameObject[] breakablePrefabs;

	public AudioClip[] hitClips;

	public AudioClip[] breakClips;

	public Rigidbody rb;

	public float ThrowMultiplier = 2;

	private bool broken;

	public bool large;

	public bool autoSwing;

	//building system
	public bool canPlace;
	public bool isPlaced = false;

	public bool canBeUsed = true;

	//animations
	public bool animationPlaying;
	

	public string StartUseAnimation;

	public List<string> UseAnimations;

	public string CustomHoldAnimation = "";

	private int currentAnimChoice = 0;

	private Vector3 pushAmt;

	public List<string> ItemObjectAnimations;

	private IEnumerator waitToPlaySound()
	{
		yield return new WaitForSecondsRealtime(2f);
		playSounds = true;
	}

	public override void Init()
	{

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
			GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetTrigger(StartUseAnimation);

			yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(StartUseAnimation));

		}

		if (autoSwing)
        {
			//GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.ResetTrigger(StartUseAnimation);

			while (Input.GetMouseButton(0))
			{
				currentAnimChoice = UnityEngine.Random.Range(0, UseAnimations.Count);

				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(UseAnimations[currentAnimChoice], true);

				animationPlaying = true;

				yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(UseAnimations[currentAnimChoice]));

				yield return new WaitForSecondsRealtime(GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(UseAnimations[currentAnimChoice], false);

				animationPlaying = false;
			}

			
		}

		else if (!animationPlaying)
        {
			currentAnimChoice = UnityEngine.Random.Range(0, UseAnimations.Count);

			GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetTrigger(UseAnimations[currentAnimChoice]);

			animationPlaying = true;

			yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(UseAnimations[currentAnimChoice]));

			yield return new WaitForSecondsRealtime(GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

			GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.ResetTrigger(UseAnimations[currentAnimChoice]);

			animationPlaying = false;
		}

		GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.ResetTrigger(StartUseAnimation);
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
    private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			_ = collision.rigidbody != null;
		}
		else
		{
			if (!(transform.gameObject.GetComponent<AudioSource>() != null) || !playSounds || !(collision.relativeVelocity.magnitude >= 4f))
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
			
			durability -= collision.relativeVelocity.magnitude;
			
			if (durability < 0f && !broken)
			{
				AudioSource.PlayClipAtPoint(breakClips[UnityEngine.Random.Range(0, breakClips.Length)], transform.position);
				GameObject[] array = breakablePrefabs;
				for (int i = 0; i < array.Length; i++)
				{
					Instantiate(array[i], transform.position, transform.rotation).GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity / 2f;
				}
				GameSettings.Instance.worldInstance.RemoveProp(GetWorldID(), true);
				broken = true;
			}
		}
	}
    public override void OnLoadFinished()
    {
       
    }
    public override void OnSaveFinished()
    {
        
    }
    private void FixedUpdate()
	{
		
		transform.position += pushAmt;
		pushAmt *= 0.95f;
	}
}
