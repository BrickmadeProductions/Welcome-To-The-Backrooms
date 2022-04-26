using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    //Health
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

    public int maxAllowed; //total allowed in the game at one time
    public int despawnDistance;
    public int entityViewDistance = 500;
    public int memoryOfPlayerLocationInSeconds;

    Coroutine rememberPlayerLocation = null;

    public LayerMask sightMask;
    public bool canSeePlayer;
    public bool playerCanSee;

    public float spawnChance;

    public float speed;

    public bool stunned = false;
    public float stunTime;

    public Transform eyes;

    public SkinnedMeshRenderer entitySkin;

    bool isDespawned = false;

    public int ID;

    public GameObject bloodPrefab;



    // Start is called before the first frame update
    void Start()
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

        if ((Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > despawnDistance && !isDespawned) || health <= 0)
        {            
            Despawn();
            isDespawned = true;
        }
        else
        {
            Vector3 playerPos = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.position;

            RaycastHit hit;
            if (Vector3.Distance(eyes.transform.position, playerPos) < entityViewDistance)
            {
                //Debug.Log("Searching For Player");
                if (Physics.Raycast(eyes.transform.position, (playerPos - eyes.transform.position), out hit, despawnDistance, sightMask))
                {
                    if (hit.collider.gameObject.layer == 11)
                    {
                        canSeePlayer = true;

                    }
                    else if (rememberPlayerLocation == null)
                    {
                        rememberPlayerLocation = StartCoroutine(RememberPlayerLocation());
                    }


                }
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
        Debug.Log(GameSettings.Instance.GlobalEntityList.Count);
        Debug.Log("Despawned " + type);
        GameSettings.Instance.GlobalEntityList.Remove(ID);
        Debug.Log(GameSettings.Instance.GlobalEntityList.Count);
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
