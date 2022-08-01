// HoldableObject
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoldableObject : InteractableObject
{
	public float durability;

	public GameObject[] breakablePrefabs;

	public AudioClip[] hitClips;

	public AudioClip[] breakClips;

	public int inventoryWeight = 1;

	public Rigidbody holdableObject;

	public float ThrowMultiplier;

	private bool broken;

	public bool large;

	public bool autoSwing;

	//building system
	public bool canPlace;
	public bool isPlaced = false;

	public bool animationPlaying;

	public List<string> LMBAnimations;

	/*public List<AnimationClip> RMBAnimationClips;

	public List<string> RMBAnimationBools;*/

	public string CustomHoldAnimation = "";

	private Vector3 pushAmt;

	private int currentAnimChoice = 0;

	private IEnumerator waitToPlaySound()
	{
		yield return new WaitForSeconds(2f);
		playSounds = true;
	}

	public override void Init()
	{
		playSounds = false;
		broken = false;
		holdableObject = GetComponent<Rigidbody>();
		StartCoroutine(waitToPlaySound());
	}

	public override void Throw(Vector3 force)
	{
		holdableObject.velocity = force * ThrowMultiplier;
		//holdableObject.AddForce();
	}

	private IEnumerator playAnimation()
	{
		//yield return new WaitUntil(() => !animationPlaying);

		if (autoSwing)
        {
			while (Input.GetMouseButton(0))
			{
				currentAnimChoice = Random.Range(0, LMBAnimations.Count);

				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(LMBAnimations[currentAnimChoice], true);

				animationPlaying = true;

				yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(LMBAnimations[currentAnimChoice]));

				yield return new WaitForSecondsRealtime(GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(LMBAnimations[currentAnimChoice], false);

				animationPlaying = false;
			}

			
		}

		else if (!animationPlaying)
        {
			currentAnimChoice = Random.Range(0, LMBAnimations.Count);

			GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(LMBAnimations[currentAnimChoice], true);

			animationPlaying = true;

			//yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).IsName(LMBAnimations[currentAnimChoice]));

			yield return new WaitForSecondsRealtime(GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).length / GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(1).speed - 0.25f);

			GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(LMBAnimations[currentAnimChoice], false);

			animationPlaying = false;
		}

	}

	public override void Use(InteractionSystem player, bool LMB)
	{
		if (LMBAnimations.Count > 0)
		{
			if (LMB && !animationPlaying)
			{
				StartCoroutine(playAnimation());
			}
			/*else if (!LMB && RMBAnimationBools.Count > 0)
			{
				int choice = Random.Range(0, RMBAnimationBools.Count);
				StartCoroutine(playAnimation(RMBAnimationBools[choice], choice, LMB));
			}*/
		}
	}

	public override void AddToInv(InteractionSystem interactionSystem)
	{
		interactionSystem.inventorySlots.Add(this);
		interactionSystem.currentlyLookingAt.gameObject.SetActive(value: false);
		interactionSystem.currentlyLookingAt = null;
		Debug.Log("Added Object " + base.name);
		base.transform.SetParent(interactionSystem.inventoryObject.transform);
	}
  
    public override void Hold(InteractionSystem player)
    {

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
			if (!transform.gameObject.GetComponent<AudioSource>().isPlaying && hitClips.Length != 0)
			{
				transform.gameObject.GetComponent<AudioSource>().clip = hitClips[Random.Range(0, hitClips.Length)];
				transform.gameObject.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.15f, 0.15f);
				transform.gameObject.GetComponent<AudioSource>().Play();
			}
			if (breakablePrefabs.Length == 0)
			{
				return;
			}
			if (collision.relativeVelocity.magnitude >= 5f)
			{
				durability -= collision.relativeVelocity.magnitude;
			}
			if (durability < 0f && !broken && breakClips.Length != 0)
			{
				AudioSource.PlayClipAtPoint(breakClips[Random.Range(0, breakClips.Length)], transform.position);
				GameObject[] array = breakablePrefabs;
				for (int i = 0; i < array.Length; i++)
				{
					Instantiate(array[i], transform.position, transform.rotation).GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity / 2f;
				}
				Destroy(gameObject);
				broken = true;
			}
		}
	}

	private void FixedUpdate()
	{
		
		transform.position += pushAmt;
		pushAmt *= 0.95f;
	}
}
