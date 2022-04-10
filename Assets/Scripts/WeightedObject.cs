using System;
using UnityEngine;

[Serializable]
public class WeightedObject : MonoBehaviour
{
    public GameObject prefab;
    public int weight;

    public WeightedObject(GameObject gameObject)
    {
        this.prefab = gameObject;
    }
}

