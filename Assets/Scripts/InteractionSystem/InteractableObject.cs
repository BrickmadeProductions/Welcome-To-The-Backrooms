// InteractableObject
using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct SaveableProp
{
	public string runTimeID;

	public float[] location;

	public float[] rotationEuler;

	public string parentName;

	public OBJECT_TYPE type;

	[JsonIgnore]
	public InteractableObject instance;

	public override string ToString()
	{
		return type.ToString() + "-" + runTimeID;
	}
}

public abstract class InteractableObject : MonoBehaviour
{
	public SaveableProp saveableData;

	public OBJECT_TYPE type;

	public bool playSounds;

	public float spawnChance;

	public string runTimeID;

	public void GenerateID(BackroomsLevelWorld world)
	{
		runTimeID = UnityEngine.Random.Range(0, 1000).ToString();

		while (world.CheckWorldForPropKey(type.ToString() + "-" + runTimeID))
		{
			runTimeID = UnityEngine.Random.Range(0, 1000).ToString();
		}

		gameObject.name = type.ToString() + "-" + runTimeID;

		Save();
	}

	public void Load(SaveableProp objectData)
	{
		saveableData = objectData;
		type = saveableData.type;
		runTimeID = saveableData.runTimeID;
		gameObject.name = type.ToString() + "-" + runTimeID;

		if (saveableData.parentName != "" && GameObject.Find(saveableData.parentName) != null)
		{
			transform.parent = GameObject.Find(saveableData.parentName).transform;
			transform.localPosition = Vector3.zero;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			transform.localRotation = Quaternion.identity;

			if (transform.parent.name == "HoldLocation" || transform.parent.name == "HandLocation")
			{
				Debug.Log("Reparenting To Hand...");
				GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetHolding((HoldableObject)this);
			}
		}
		else
		{
			transform.position = new Vector3(saveableData.location[0], saveableData.location[1], saveableData.location[2]);
			transform.rotation = Quaternion.Euler(saveableData.rotationEuler[0], saveableData.rotationEuler[1], saveableData.rotationEuler[2]);
		}

		saveableData.instance = this;
	}

	public SaveableProp Save()
	{
		saveableData = new SaveableProp
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
			parentName = ((transform.parent != null) ? transform.parent.name : "")
		};

		return saveableData;
	}

	private void Awake()
	{
		Init();
	}

	public void OnDestroy()
	{
	}

	public abstract void Init();

<<<<<<< Updated upstream
	public abstract void Throw(Vector3 force);
=======
	/// <summary>
	/// Calls when all variables have been pulled from this object
	/// </summary>
	public abstract void OnSaveFinished();

	/// <summary>
	/// Calls when all variables have been loaded to this object
	/// </summary>
	public abstract void OnLoadFinished();

	public virtual void Throw(Vector3 force)
    {

    }
	/// <summary>
	/// Runs when the USE button is pressed while item is on the ground, or the RMB is pressed when holding
	/// </summary>
	/// <param name="player"></param>
	/// <param name="LMB"></param>
>>>>>>> Stashed changes

	public abstract void Use(InteractionSystem player, bool LMB);

	public abstract void AddToInv(InteractionSystem player);

	public abstract void Hold(InteractionSystem player);
}

// OBJECT_TYPE
public enum OBJECT_TYPE
{
<<<<<<< Updated upstream
	KNIFE,
	AXE,
	CHAIR,
	ALMOND_WATER,
	SCREWDRIVER,
	SOUP,
	SODA,
	FLASHLIGHT,
	BOXCUTTER,
	TAPE,
	SHIV_BOXCUTTER,
	BIGSPOON,
	SPEAR,
	METAL_LADDER,
	ROPE_COIL,
	ROPE_TIED
=======
    KNIFE,
    AXE,
    CHAIR,
    ALMOND_WATER,
    SCREWDRIVER,
    SOUP,
    SODA,
    FLASHLIGHT,
    BOXCUTTER,
    TAPE,
    SHIV_BOXCUTTER,
    BIGSPOON,
    SPEAR,
    METAL_LADDER,
    ROPE_COIL,
    ROPE_TIED,
    CASSET_TAPE,
	CASSET_PLAYER,
	LOOT_BOX_CARDBOARD,
	ANOMOLY_CHAIR,
	GLOCK
>>>>>>> Stashed changes
}
