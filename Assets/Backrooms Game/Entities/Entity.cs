using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public ENTITY_TYPE type;

    public Collider[] attackHitboxs;

    public bool canAttack;
    public Animator entityAnimator;
    public AudioClip[] noises;
    public AudioSource noiseSource;

    public int damage;
    public float sanityMultiplier;

    public int maxAllowed; //total allowed in the game at one time
    public int despawnDistance;

    public enum ENTITYSTATES : int
    {
        SEARCHING = 1,
        ATTACKING = 2
    }

    public ENTITYSTATES currentEntityState = ENTITYSTATES.SEARCHING;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AI());
    }

    private void Update()
    {
        if (Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > despawnDistance)
        {
            Despawn();
        }
    }

    public abstract IEnumerator AI();

    public abstract void Despawn();

    public abstract void OnDestroy();
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
