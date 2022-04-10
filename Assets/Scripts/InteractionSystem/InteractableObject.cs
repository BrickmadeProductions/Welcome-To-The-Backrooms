using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{

    public float durability;
    public GameObject[] breakablePrefabs;
    public AudioClip[] hitClips;
    public AudioClip[] breakClips;
    public bool playSounds = false;

    public abstract void Throw(Vector3 force);

    public abstract void Use(InteractionSystem player);

    public abstract void Grab(InteractionSystem player);


}
