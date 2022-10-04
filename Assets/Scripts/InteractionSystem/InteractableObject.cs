// InteractableObject
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct MetaData
{
	//variable name and value
	public Dictionary<string, string> data;
}

[Serializable]
public struct SaveableProp
{
	public string runTimeID;

	public float[] location;

	public float[] rotationEuler;

	public string parentName;

	public OBJECT_TYPE type;

	public MetaData metaData;

	[JsonIgnore]
	public InteractableObject instance;

}

public abstract class InteractableObject : MonoBehaviour
{
	public SaveableProp saveableData;

	public MetaData activeMetaData;

	public OBJECT_TYPE type;

	public bool playSounds;

	public string runTimeID;

	public string GetWorldID()
	{
		return type.ToString() + "-" + runTimeID;
	}

	public void SetMetaData(string field, string value)
    {
		if (activeMetaData.data.ContainsKey(field)) {

			activeMetaData.data[field] = value;
		}
        else
        {
			activeMetaData.data.Add(field, value);
		}
	}

	public void RemoveMetaData(string field)
	{
		if (activeMetaData.data.ContainsKey(field))
		{

			activeMetaData.data.Remove(field);
		}
	}

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

		if (saveableData.metaData.data.Count > 0)
        {
			foreach (KeyValuePair<string, string> data in saveableData.metaData.data)
			{

				FieldInfo metaDataVariable = GetType().GetField(data.Key);
	
				if (metaDataVariable != null)
				{
					Type t = Nullable.GetUnderlyingType(metaDataVariable.FieldType) ?? metaDataVariable.FieldType;
					object safeValue = (data.Value == null) ? null : Convert.ChangeType(data.Value, t);
					metaDataVariable.SetValue(this, safeValue);
					
					//metaDataVariable.SetValue(this, Convert.ChangeType(data.Value, metaDataVariable.PropertyType), null);


				}
                else
                {
					Debug.Log(data.Key + " WAS NOT FOUND AS A TYPE");
                }
				
				
			}
		}
		activeMetaData = saveableData.metaData;
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

			if (transform.parent.name == "HoldLocation" || transform.parent.name == "RHandLocation" || transform.parent.name == "LHandLocation")
			{
				Debug.Log("Reparenting To Hand...");

				if (transform.parent.name == "RHandLocation")
                {
					GameSettings.Instance.Player.GetComponent<InteractionSystem>().FinalizePickup((HoldableObject)this);
				}

			}

			//it was in an inventory
			else if (transform.parent.name.Contains("SLOT"))
            {
				transform.parent = BPUtil.FindGameObject(saveableData.parentName).transform;

				Quaternion.Euler(saveableData.rotationEuler[0], saveableData.rotationEuler[1], saveableData.rotationEuler[2]);

                BPUtil.FindGameObject(activeMetaData.data["INV_SLOT"]).GetComponent<InventorySlot>().AddItemToSlot((HoldableObject)this);
            }
		}
		else
		{
			transform.position = new Vector3(saveableData.location[0], saveableData.location[1], saveableData.location[2]);
			transform.rotation = Quaternion.Euler(saveableData.rotationEuler[0], saveableData.rotationEuler[1], saveableData.rotationEuler[2]);
		}

		saveableData.instance = this;

		OnLoadFinished();
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
			metaData = activeMetaData,
			parentName = ((transform.parent != null) ? transform.parent.name : "")
		};
		OnSaveFinished();
		return saveableData;
	}

	private void Awake()
	{
		activeMetaData = new MetaData()
		{
			data = new Dictionary<string, string>()
		};

		Init();
	}

	public void OnDestroy()
	{
	}

	/// <summary>
	/// Awake()
	/// </summary>
	public abstract void Init();

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
	public abstract void Use(InteractionSystem player, bool LMB);

	public abstract void Hold(InteractionSystem player, bool RightHand);

}

// All Savable Objects In The Game
public enum OBJECT_TYPE
{
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
	ANOMOLY_CHAIR
}
