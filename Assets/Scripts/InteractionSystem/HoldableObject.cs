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

	//building system
	public bool isPlaced = false;

	public bool animationPlaying;

	public List<AnimationClip> LMBAnimationClips;

	public List<string> LMBAnimationBools;

	public List<AnimationClip> RMBAnimationClips;

	public List<string> RMBAnimationBools;

	public string CustomHoldAnimation = "";

	private Vector3 pushAmt;

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
	}

	private IEnumerator playAnimation(string boolName, int animChosen, bool LMB)
	{
		yield return null;

		GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(boolName, value: true);

		animationPlaying = true;

		//Debug.Log(GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime);
		yield return new WaitUntil(() => GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 2f);

		animationPlaying = false;

		GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(boolName, value: false);
	}

	public override void Use(InteractionSystem player, bool LMB)
	{
		if (!animationPlaying)
		{
			if (LMB && LMBAnimationBools.Count > 0)
			{
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].clip = player.GetComponent<PlayerController>().swingSounds[Random.Range(0, player.GetComponent<PlayerController>().swingSounds.Length)];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].pitch = Random.Range(0.9f, 1.1f);
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].Stop();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].Play();

				int choice = Random.Range(0, LMBAnimationBools.Count);
				StartCoroutine(playAnimation(LMBAnimationBools[choice], choice, LMB));
			}
			else if (!LMB && RMBAnimationBools.Count > 0)
			{
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].clip = player.GetComponent<PlayerController>().swingSounds[Random.Range(0, player.GetComponent<PlayerController>().swingSounds.Length)];
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].pitch = Random.Range(0.9f, 1.1f);
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].Stop();
				player.GetComponent<PlayerController>().head.GetComponents<AudioSource>()[2].Play();

				int choice = Random.Range(0, RMBAnimationBools.Count);
				StartCoroutine(playAnimation(RMBAnimationBools[choice], choice, LMB));
			}
		}
	}

	public override void AddToInv(InteractionSystem interactionSystem)
	{
		interactionSystem.inventorySlots.Add(this);
		interactionSystem.currentlyLookingAt.gameObject.SetActive(value: false);
		interactionSystem.currentlyLookingAt = null;
		Debug.Log("Added Object " + name);
		transform.SetParent(interactionSystem.inventoryObject.transform);
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

		if (SceneManager.GetActiveScene().name != "HomeScreen" && SceneManager.GetActiveScene().name != "IntroSequence" && GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
		{
			foreach (string lMBAnimationBool in LMBAnimationBools)
			{
				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(lMBAnimationBool, value: false);
			}
			foreach (string rMBAnimationBool in RMBAnimationBools)
			{
				GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(rMBAnimationBool, value: false);
			}
		}
		transform.position += pushAmt;
		pushAmt *= 0.95f;
	}
}
