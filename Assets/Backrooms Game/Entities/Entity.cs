// Entity
using System.Collections;
using UnityEngine;
using System;
using Newtonsoft.Json;

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

	public int memoryOfPlayerLocationInSeconds;

	private Coroutine rememberPlayerLocation;

	public LayerMask sightMask;

	public bool canSeePlayer;

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

	public void GenerateID(BackroomsLevelWorld world)
	{
		runTimeID = UnityEngine.Random.Range(0, 1000).ToString();

		while (world.CheckWorldForEntityKey(type.ToString() + "-" + runTimeID))
		{
			runTimeID = UnityEngine.Random.Range(0, 1000).ToString();
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
	}

	private void Start()
	{
		StartCoroutine(AI());
		Debug.Log("Spawned " + type);
	}

	private IEnumerator RememberPlayerLocation()
	{
		yield return new WaitForSeconds(memoryOfPlayerLocationInSeconds);
		canSeePlayer = false;
		rememberPlayerLocation = null;
		attackNoiseSource.Stop();
	}

	private void Update()
	{
		UpdateEntity();

		if ((Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > (float)despawnDistance && !isDespawned) || health <= 0f)
		{
			Despawn();
			isDespawned = true;
			return;
		}
		Vector3 position = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.position;
		if (Vector3.Distance(eyes.transform.position, position) < (float)entityViewDistance && Physics.Raycast(eyes.transform.position, position - eyes.transform.position, out var hitInfo, despawnDistance, sightMask))
		{
			if (hitInfo.collider.gameObject.layer == 11)
			{
				canSeePlayer = true;
			}
			else if (rememberPlayerLocation == null)
			{
				rememberPlayerLocation = StartCoroutine(RememberPlayerLocation());
			}
		}
	}

	public IEnumerator StunTimer()
	{
		stunned = true;
		yield return new WaitForSeconds(stunTime);
		stunned = false;
	}

	private void OnBecameVisible()
	{
		playerCanSee = true;
	}

	private void OnBecameInvisible()
	{
		playerCanSee = false;
	}

	public abstract void UpdateEntity();

	public abstract IEnumerator AI();

	public abstract void Despawn();

	public void OnDestroy()
	{
	}
}

public enum ENTITY_TYPE : int
{
    WINDOW = 2,
    SMILER = 3,
    DEATHMOTH = 4,
    CLUMP = 5,
    DULLER = 4,
    HOUND = 8,
    PARTYGOER = 67
}
