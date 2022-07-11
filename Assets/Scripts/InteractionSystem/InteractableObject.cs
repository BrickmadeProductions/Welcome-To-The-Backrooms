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
				GameSettings.Instance.Player.GetComponent<PlayerController>().holding = (HoldableObject)this;
				GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetPickup();
				GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetHolding();
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

	public abstract void Throw(Vector3 force);

	public abstract void Use(InteractionSystem player, bool LMB);

	public abstract void Grab(InteractionSystem player);
}

// OBJECT_TYPE
public enum OBJECT_TYPE
{
	KNIFE,
	CHAIR,
	ALMOND_WATER,
	SCREWDRIVER,
	SOUP,
	FLASHLIGHT
}
