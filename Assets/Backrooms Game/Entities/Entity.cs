// Entity
using System.Collections;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public struct SaveableEntity
{
	public string runTimeID;

	public float[] location;

	public float[] rotationEuler;

	public ENTITY_TYPE type;

	[JsonIgnore]
	public Entity instance;

	public override string ToString()
	{
		return type.ToString() + "-" + runTimeID;
	}
}
[Serializable]
public struct EntityDrop
{
	public OBJECT_TYPE type;

	public Transform locationOnBody;

	public int dropChance;


}

public abstract class Entity : MonoBehaviour
{
	public SaveableEntity saveableData;

	public float maxHealth;

	public float health;

	public float agrivation;

	public float hunger;

	public ENTITY_TYPE type;

	public bool canAttack;

	public Animator entityAnimator;

	public AudioClip[] movementNoises;

	public AudioSource movementNoiseSource;

	public AudioClip[] attackNoises;

	public AudioSource attackNoiseSource;

	public AudioClip[] hurtNoises;

	public AudioSource hurtNoisesSource;

	public int damage;

	public float sanityMultiplier;

	public int maxAllowed;

	public int despawnDistance;

	public int entityViewDistance = 500;

	public int memoryOfTargetLocationInSeconds;

	private Coroutine rememberTargetLocation;

	public LayerMask sightMask;

	public bool canSeeTarget;

	public bool playerCanSee;

	public float spawnChance;

	public float speed;

	public bool stunned;

	public float stunTime;

	public Transform eyes;

	public SkinnedMeshRenderer entitySkin;

	private bool isDespawned;

	public string runTimeID;

	public GameObject bloodPrefab;

	public Transform currentTarget;

	List<EntityAudioAttractor> currentPossibleTargets;

	public List<EntityDrop> drops;

	public List<InteractableObject> activeDropsHeld;

	public abstract void OnEventStart();
	public abstract void OnEventEnd();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<EntityAudioAttractor>())
        {
			currentPossibleTargets.Add(collision.collider.GetComponent<EntityAudioAttractor>());

		}
    }
    private void OnCollisionExit(Collision collision)
    {
		if (collision.collider.GetComponent<EntityAudioAttractor>())
		{
			currentPossibleTargets.Remove(collision.collider.GetComponent<EntityAudioAttractor>());

		}
	}

	private void SetCurrentTarget()
    {
		Transform best = null;

		for (int i = 1; i < currentPossibleTargets.Count + 1; i++)
        {
			if (currentPossibleTargets[i].priority > currentPossibleTargets[i - 1].priority)
            {
				best = currentPossibleTargets[i].target;

			}
        }

		currentTarget = best;

		if (currentPossibleTargets.Count == 0)
		{
			currentTarget = GameSettings.Instance.Player.transform;
		}

	}
    public void GenerateID(BackroomsLevelWorld world)
	{
		runTimeID = UnityEngine.Random.Range(0, 1000000000).ToString();

		float tries = 0;

		while (world.CheckWorldForEntityKey(type.ToString() + "-" + runTimeID))
		{
			tries++;

			runTimeID = UnityEngine.Random.Range(0, 1000000000).ToString();

			if (tries > 5f)
				break;

		}

		gameObject.name = type.ToString() + "-" + runTimeID;

		Save();
	}

	public void Load(SaveableEntity entityData)
	{
		saveableData = entityData;
		type = saveableData.type;
		runTimeID = saveableData.runTimeID;
		transform.position = new Vector3(saveableData.location[0], saveableData.location[1], saveableData.location[2]);
		transform.rotation = Quaternion.Euler(saveableData.rotationEuler[0], saveableData.rotationEuler[1], saveableData.rotationEuler[2]);
		gameObject.name = type.ToString() + "-" + runTimeID;
		saveableData.instance = this;
	}

	public SaveableEntity Save()
	{
		saveableData = new SaveableEntity
		{
			runTimeID = runTimeID,
			location = new float[3]
			{
				transform.position.x,
				transform.position.y,
				transform.position.z
			},
			rotationEuler = new float[3]
			{
				transform.rotation.eulerAngles.x,
				transform.rotation.eulerAngles.y,
				transform.rotation.eulerAngles.z
			},
			type = type,
			instance = this
		};

		return saveableData;
	}

	private void Awake()
	{

		currentPossibleTargets = new List<EntityAudioAttractor>();

		hurtNoisesSource.outputAudioMixerGroup = GameSettings.Instance.audioHandler.master.FindMatchingGroups("Master")[0];
		attackNoiseSource.outputAudioMixerGroup = GameSettings.Instance.audioHandler.master.FindMatchingGroups("Master")[0];
		movementNoiseSource.outputAudioMixerGroup = GameSettings.Instance.audioHandler.master.FindMatchingGroups("Master")[0];
		Init();
	}

	abstract public void Init();

	private void Start()
	{

		//correct if not on floor
		RaycastHit[] hits;
		float distance = 10f;

		hits = Physics.RaycastAll(transform.position, Vector3.down, distance);

		if (hits.Length > 0)
		{
			foreach (RaycastHit hit in hits)
			{
				//only if floor
				if (hit.transform.gameObject.layer == 19)
				{
					transform.position = hit.point;
					continue;
				}


			}

		}
		StartCoroutine(AI());
	}

	private IEnumerator RememberTargetLocation()
	{
		yield return new WaitForSecondsRealtime(memoryOfTargetLocationInSeconds);
		canSeeTarget = false;
		rememberTargetLocation = null;
		attackNoiseSource.Stop();
	}

	private void Update()
	{
		SetCurrentTarget();
		UpdateEntity();

		if ((Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > despawnDistance && !isDespawned))
		{
			isDespawned = true;
			Despawn();
			return;
		}
		if (health <= 0)
        {
			if (activeDropsHeld.Count > 0)
			{
				foreach (InteractableObject spawnedObject in activeDropsHeld)
				{
					BPUtil.SetAllColliders(spawnedObject.transform, true);

					spawnedObject.transform.parent = null;

					spawnedObject.GetComponent<Rigidbody>().isKinematic = false;

				}
			}


			Despawn();
			isDespawned = true;
			return;
		}

		
		if (currentTarget != null)
        {
			float angle = Vector3.SignedAngle(currentTarget.position - eyes.transform.position, transform.forward, transform.up);

			if (Physics.Raycast(eyes.transform.position, currentTarget.position - eyes.transform.position, out var hitInfo, despawnDistance, sightMask))

				//Debug.Log((Vector3.Distance(eyes.transform.position, currentTarget.position) < 6f));
				//determines if it can see the target
				if (Vector3.Distance(eyes.transform.position, currentTarget.position) < entityViewDistance
				&&
				(angle < 50 && angle > -50))
				{
					if (hitInfo.collider.gameObject.layer == 11)
					{
						canSeeTarget = true;
					}
					else if (rememberTargetLocation == null)
					{
						rememberTargetLocation = StartCoroutine(RememberTargetLocation());
					}
				}
				//it can always know its there if its close enough
				else if (Vector3.Distance(eyes.transform.position, currentTarget.position) < (GameSettings.Instance.Player.GetComponent<PlayerController>().currentPlayerState == PlayerController.PLAYERSTATES.RUN ? 70f : 10f))
					canSeeTarget = true;
		}
		
		
		
		
	}

	public IEnumerator StunTimer()
	{
		stunned = true;
		yield return new WaitForSecondsRealtime(stunTime);
		stunned = false;
	}

	private void OnBecameVisible()
	{
		playerCanSee = true;
		StartCoroutine(UpdatePlayerSanity());
	}

	private void OnBecameInvisible()
	{
		playerCanSee = false;
	}

	public IEnumerator UpdatePlayerSanity()
    {
		while (playerCanSee)
        {
			yield return new WaitForSecondsRealtime(5f);
			GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().ChangeSanity(-UnityEngine.Random.Range(canSeeTarget ? 5f : 2f, canSeeTarget ? 15f : 8f) * (health / 100));

		}
		
    }

	public abstract void UpdateEntity();

	public abstract IEnumerator AI();

	public abstract void Despawn();

	public void OnDestroy()
	{
		Debug.Log("Despawned " + type.ToString() + "-" + runTimeID);

		//GameSettings.Instance.worldInstance.RemoveEntity(type.ToString() + "-" + runTimeID);

	}
}

public enum ENTITY_TYPE
{
    WINDOW,
    SMILER,
    DEATHMOTH,
    CLUMP,
    DULLER,
    HOUND,
    PARTYGOER
}
