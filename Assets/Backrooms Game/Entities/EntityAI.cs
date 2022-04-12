using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityAI : MonoBehaviour
{    
    public Collider[] attackHitboxs;

    public bool canAttack;
    public Animator entityAnimator;
    public AudioClip[] noises;
    public AudioSource noiseSource;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(attackFunc());
    }

    public abstract IEnumerator attackFunc();

}
