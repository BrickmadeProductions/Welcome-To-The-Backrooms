// Entity
using System.Collections;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public struct SaveableEntity
{
	public string runTimeID;

	public float[] location;

	public float[] rotationEuler;

	public ENTITY_TYPE type;

	public Dictionary<string, string> metaData;

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

[Serializable]
public struct EnityActionWithWeight
{
	public int Weight;
	public ENTITY_ACTION Action;
}

public enum ENTITY_ACTION
{
	AMB_IDLE,
	AMB_WANDERING,
	ATK_PUNCHING,
	ATK_HOLDING,
	ATK_SHOVING,
	MOV_CORNER_PLAYER,
	MOV_SURROUND_PLAYER,
	MOV_MOVE_TOWARD_TARGET,
	MOV_MOVE_TOWARD_TARGET_STRAFE,
	MOV_HOLD_DISTANCE_FROM_TARGET

}

public abstract class Entity : MonoBehaviour
{


	public Transform entityViewDetectionPoint;

	public SaveableEntity saveableData;

	Coroutine updateSanity = null;

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

	public bool isDespawned = false;

	public string runTimeID;

	public GameObject bloodPrefab;

	public Transform currentTarget;

	public Transform tempTarget;

	List<EntityAttractor> currentPossibleTargets;

	public List<EntityDrop> drops;

	public List<InteractableObject> activeDropsHeld;

	//metadata is saved on save with the object data
	public Dictionary<string, string> activeMetaData;

	//acheivment
	bool testingRemainCalm = false;


	/// <summary>
	/// Calls when all variables have been pulled from this object
	/// </summary>
	public abstract void OnSaveFinished();

	/// <summary>
	/// Calls when all variables have been loaded to this object
	/// </summary>
	public abstract void OnLoadFinished();

	public string GetMetaData(string field)
	{

		if (activeMetaData.ContainsKey(field))
		{

			return activeMetaData[field];
		}
		else
		{
			return null;
		}


	}
	public void SetMetaData(string field, string value)
	{

		if (activeMetaData.ContainsKey(field))
		{

			activeMetaData[field] = value;
		}
		else
		{
			activeMetaData.Add(field, value);
		}


	}

	public void RemoveMetaData(string field)
	{
		if (activeMetaData.ContainsKey(field))
		{

			activeMetaData.Remove(field);
		}
	}

	public abstract void OnEventStart();
	public abstract void OnEventEnd();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EntityAttractor>())
        {
			currentPossibleTargets.Add(other.GetComponent<EntityAttractor>());

		}
    }
    private void OnTriggerExit(Collider other)
    {
		if (other.GetComponent<EntityAttractor>())
		{
			currentPossibleTargets.Remove(other.GetComponent<EntityAttractor>());

		}
	}

	private void SetCurrentTarget()
    {
		Transform best = null;

		if (currentPossibleTargets.Count > 1)
        {
			for (int i = 0; i < currentPossibleTargets.Count; i++)
			{
				if (i > 1)
                {
					if (currentPossibleTargets[i].priority > currentPossibleTargets[i - 1].priority)
					{
						best = currentPossibleTargets[i].target;

					}
                }
				else best = currentPossibleTargets[0].target;


			}
		}
		else if (currentPossibleTargets.Count == 1)
        {
			best = currentPossibleTargets[0].target;
		}
		

		currentTarget = best;

		if (currentPossibleTargets.Count == 0)
		{
			currentTarget = GameSettings.GetLocalPlayer().playerCamera.transform;
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

		if (saveableData.location.Length > 0)
			transform.position = new Vector3(saveableData.location[0], saveableData.location[1], saveableData.location[2]);
		
		if (saveableData.rotationEuler.Length > 0)
			transform.rotation = Quaternion.Euler(saveableData.rotationEuler[0], saveableData.rotationEuler[1], saveableData.rotationEuler[2]);

		gameObject.name = type.ToString() + "-" + runTimeID;
		saveableData.instance = this;

		if (saveableData.metaData != null)
			if (saveableData.metaData.Count > 0)
			{
				activeMetaData = saveableData.metaData;

				foreach (KeyValuePair<string, string> data in activeMetaData)
				{

					FieldInfo metaDataVariable = GetType().GetField(data.Key);

					if (metaDataVariable != null)
					{
						Type t = Nullable.GetUnderlyingType(metaDataVariable.FieldType) ?? metaDataVariable.FieldType;
						object safeValue = ((data.Value == null) ? null : Convert.ChangeType(data.Value, t));
						metaDataVariable.SetValue(this, safeValue);

						//metaDataVariable.SetValue(this, Convert.ChangeType(data.Value, metaDataVariable.PropertyType), null);


					}
					else
					{
						//Debug.LogWarning(data.Key + " WAS NOT FOUND AS A TYPE");
					}


				}
			}
		OnLoadFinished();
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
			instance = this,
			metaData = activeMetaData
		};
		OnSaveFinished();
		return saveableData;
	}

	private void Awake()
	{
		activeMetaData = new Dictionary<string, string>();
		currentPossibleTargets = new List<EntityAttractor>();

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
	}

	private void Update()
	{
		SetCurrentTarget();
		UpdateEntity();
		
		
		RaycastHit[] hits = Physics.RaycastAll(eyes.transform.position, currentTarget.position - eyes.transform.position, Vector3.Distance(currentTarget.position, eyes.transform.position), sightMask);

		//Debug.Log(hits.Length);
		//Debug.DrawRay(eyes.transform.position, eyes.transform.forward.normalized * Vector3.Distance(currentTarget.position, eyes.transform.position), Color.blue);

		//only can see the target
		if (hits.Length == 1 && isVisible(GameSettings.GetLocalPlayer().playerCamera, entityViewDetectionPoint))
		{
			

			if (updateSanity == null)
			{
				playerCanSee = true;
				updateSanity = StartCoroutine(PlayerCanSee());

			}
		}
		else
		{
			if (updateSanity != null)
			{
				StopCoroutine(updateSanity);
				updateSanity = null;
				playerCanSee = false;
			}

		}

		
        

		if ((Vector3.Distance(GameSettings.GetLocalPlayer().transform.position, transform.position) > despawnDistance && !isDespawned))
		{
			Despawn();
			isDespawned = true;
			return;
		}

		//was stabbed by someone
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
			float angle = Vector3.SignedAngle(currentTarget.position - eyes.transform.position, eyes.transform.forward, transform.up);
			
			//Debug.Log(hits.Length);
			//Debug.DrawRay(eyes.transform.position, eyes.transform.forward.normalized * Vector3.Distance(currentTarget.position, eyes.transform.position), Color.blue);

			//only can see the target
			if (hits.Length == 1 && hits[0].transform.gameObject.GetComponent<EntityAttractor>() != null)
            {
				//Debug.Log((Vector3.Distance(eyes.transform.position, currentTarget.position) < 6f));
				//determines if it can see the target
				if (Vector3.Distance(eyes.transform.position, currentTarget.position) < entityViewDistance
				&&
				(angle < 50 && angle > -50))
				{
					canSeeTarget = true;

					if (rememberTargetLocation == null)
					{
						rememberTargetLocation = StartCoroutine(RememberTargetLocation());
					}
				}
				
				
				
			}
				
		}
		
		
		
		
	}

	public IEnumerator StunTimer()
	{
		stunned = true;
		yield return new WaitForSecondsRealtime(stunTime);
		stunned = false;
	}


	public IEnumerator PlayerCanSee()
    {
		float totalTimeCanSee = 0f;

		while (playerCanSee)
        {
			yield return new WaitForSecondsRealtime(5f);
			totalTimeCanSee += 5f;
			GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeSanity(GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().sanity * sanityMultiplier);
			if (totalTimeCanSee >= 10f && !canSeeTarget)
            {
				Steam.AddAchievment("REMAIN_CALM");
            }
		}
		
    }

	public abstract void UpdateEntity();

	public abstract IEnumerator AI();

	public abstract void Despawn();

	public void OnDestroy()
	{
		
	}

	bool isVisible(Camera c, Transform go)
	{
		var planes = GeometryUtility.CalculateFrustumPlanes(c);
		var point = go.position;

		foreach (var plane in planes)
		{
			if (plane.GetDistanceToPoint(point) < 0)
				return false;
		}
		return true;
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
