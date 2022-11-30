using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : Entity
{
    //0 agressive
    //1 deceiving
    public int partyGoerType;

    // Angular speed in radians per sec.

    bool strangling = false;
    public GameObject grabLocation;
    public GameObject ragDoll;

    [ColorUsage(true, true)]
    public Color emissionColor;
    public Material emission;
    public Renderer partyGoerRenderer;

    public GameObject partygoerBalloon;

    private float originalSpeed;


    void ResetFloorVector()
    {
        gameObject.transform.rotation = new Quaternion(0, gameObject.transform.rotation.y, 0, gameObject.transform.rotation.w);
        //correct floating
        RaycastHit[] hits;
        float distance = 25f;

        hits = Physics.RaycastAll(transform.position + new Vector3(0, 2f, 0), Vector3.down, distance, sightMask);

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                //only if floor
                if (hit.transform.gameObject.layer == 19)
                {
                    transform.position = hit.point;
                    break;
                }


            }

        }

    }

    public override void OnEventStart()
    {
        emission.EnableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", emissionColor);
        
        
    }
    public override void OnEventEnd()
    {
        emission.DisableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", Color.black);
        
    }

    public override void Init()
    {
        ResetFloorVector();

        originalSpeed = speed;

        emission = partyGoerRenderer.material;

        partyGoerType = GameSettings.Instance.ActiveScene != SCENE.LEVELFUN ? Random.Range(0, 2) : 0;

        SetMetaData("partyGoerType", ((int)partyGoerType).ToString());

        switch (partyGoerType)
        {
            case 0:
                partygoerBalloon.SetActive(false);
               
                break;

            case 1:
                partygoerBalloon.SetActive(true);
                
                break;
        }

        if (GameSettings.Instance.worldInstance.currentWorldEvent == GAMEPLAY_EVENT.LIGHTS_OUT)
        {
            emission.EnableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", emissionColor);
           
        }
        else
        {
            emission.DisableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", Color.black);
            
        }
        
    }

    public override IEnumerator AI()
    {
        while (true)
        {
            if (canAttack && GameSettings.Instance.GetComponent<CheatSheet>().AIEnabled)
            {
                if (currentTarget != null)
                {
                    switch (partyGoerType)
                    {
                        case 0:

                            Vector3 targetDirection = tempTarget != null ? tempTarget.position - transform.position : currentTarget.position - transform.position;

                            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, speed, 0.0f);

                            int twitchNoise = Random.Range(0, movementNoises.Length);

                            //only twitch towards target if farther than 4 units


                            if (Vector3.Distance(currentTarget.position, transform.position) > 4 && !strangling && !stunned && speed > 1f)
                            {
                                entityAnimator.SetBool("Idle", false);
                                entityAnimator.SetBool("Grab", false);

                                if (canSeeTarget)
                                {
                                    //attack start
                                    if (!attackNoiseSource.isPlaying)
                                    {
                                        attackNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                                        attackNoiseSource.Play();

                                    }
                                    //ambience stop
                                    if (GetComponent<AudioSource>().isPlaying)
                                        GetComponent<AudioSource>().Stop();

                                    //hit wall
                                    if (Physics.SphereCast(eyes.transform.position, 0.25f, gameObject.transform.forward, out var rayWallHit, 3f, sightMask, QueryTriggerInteraction.Ignore) && rayWallHit.collider.gameObject.layer != 11)
                                    {                                            

                                        float rightHitDistance = entityViewDistance - 5f;
                                        float leftHitDistance = entityViewDistance - 5f;

                                        tempTarget = Instantiate(new GameObject()).transform;

                                        //right hit
                                        if (Physics.SphereCast(gameObject.transform.position + (Vector3.up * 2), 0.05f, rayWallHit.transform.right, out var rightHit, entityViewDistance - 5f, sightMask, QueryTriggerInteraction.Ignore))
                                        {
                                            rightHitDistance = rightHit.distance;

                                            tempTarget.transform.position = rightHit.point;

                                        }

                                        //left hit
                                        if (Physics.SphereCast(gameObject.transform.position + (Vector3.up * 2), 0.05f, -rayWallHit.transform.right, out var leftHit, entityViewDistance - 5f, sightMask, QueryTriggerInteraction.Ignore))
                                        {
                                            leftHitDistance = leftHit.distance;

                                            tempTarget.transform.position = leftHit.point;

                                            
                                        }

                                        if (leftHit.distance > rightHit.distance)

                                            tempTarget = leftHit.transform;

                                        else

                                            tempTarget = rightHit.transform;
                                    }

                                    //move towards target
                                    transform.position = Vector3.MoveTowards(transform.position, tempTarget == null ? currentTarget.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)) : tempTarget.position, speed);
                                    
                                    transform.rotation = Quaternion.LookRotation(newDirection);

                                    ResetFloorVector();

                                    movementNoiseSource.clip = movementNoises[twitchNoise];
                                    movementNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                                    movementNoiseSource.Play();

                                    entityAnimator.SetBool("Grab", false);
                                    entityAnimator.SetBool("RunAttack", true);

                                    yield return new WaitForSeconds(0.25f);

                                    //ambient
                                    GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                                    //simulate getting tired
                                    speed -= 0.0015f;

                                    

                                   
                                }
                                //idle
                                else 
                                {
                                    //ambience
                                    if (!GetComponent<AudioSource>().isPlaying)
                                        GetComponent<AudioSource>().Play();

                                    if (attackNoiseSource.isPlaying)
                                        attackNoiseSource.Stop();

                                    entityAnimator.SetBool("Grab", false);
                                    entityAnimator.SetBool("RunAttack", false);
                                    entityAnimator.ResetTrigger("Attack");
                                    entityAnimator.SetBool("Idle", true);

                                    yield return new WaitForSecondsRealtime(0.25f);
                                    speed += 0.007f;
                                    speed = Mathf.Clamp(speed, 1f, originalSpeed);
                                }
                            }
                            //attack
                            else if (!stunned && !strangling)
                            {
                                transform.LookAt(currentTarget.transform.position, Vector3.up);

                                ResetFloorVector();

                                entityAnimator.SetBool("Grab", false);
                                entityAnimator.SetBool("Idle", false);
                                entityAnimator.SetBool("RunAttack", false);
                                entityAnimator.SetTrigger("Attack");

                                yield return new WaitForSeconds(0.7f);

                                if (Vector3.Distance(currentTarget.position, transform.position) >= 4 || !entityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
                                    continue;

                                GameSettings.GetLocalPlayer().playerHealth.TakeDamage(damage, sanityMultiplier, 1f, true, DAMAGE_TYPE.ENTITY);
                                

                                transform.LookAt(currentTarget.transform.position, Vector3.up);

                                ResetFloorVector();

                                //damage per second
                                yield return new WaitForSeconds(0.4f);

                                if (Vector3.Distance(currentTarget.position, transform.position) >= 4 || !entityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
                                    continue;

                                GameSettings.GetLocalPlayer().playerHealth.TakeDamage(damage, sanityMultiplier, 1f, true, DAMAGE_TYPE.ENTITY);
                                ;
                                yield return new WaitForSeconds(0.3f);

                            }
                            
                            //ambient
                            GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                            break;

                        case 1:

                            
                            if (playerCanSee && Vector3.Distance(currentTarget.position, transform.position) > 4 && !strangling && !stunned)
                            {
                                entityAnimator.SetBool("Idle", false);
                                entityAnimator.SetBool("Waving", true);
                                entityAnimator.SetBool("Grab", false);
                                transform.LookAt(currentTarget.transform.position, Vector3.up);
                                ResetFloorVector();
                            }

                            else
                            {
                                entityAnimator.SetBool("Idle", false);
                                entityAnimator.SetBool("Waving", false);
                                entityAnimator.SetBool("Grab", false);

                                //attack
                                if (!strangling && !stunned && Vector3.Distance(currentTarget.position, transform.position) < 4)
                                {

                                    if (GetComponent<AudioSource>().isPlaying)
                                        GetComponent<AudioSource>().Stop();

                                    //attack start
                                    if (!attackNoiseSource.isPlaying)
                                    {
                                        attackNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                                        attackNoiseSource.Play();

                                    }

                                    strangling = true;

                                    entityAnimator.SetBool("Grab", true);

                                    entityAnimator.SetBool("Waving", false);
                                    
                                    entityAnimator.SetBool("RunAttack", false);

                                    entityAnimator.ResetTrigger("Attack");

                                    entityAnimator.SetBool("Idle", false);

                                    yield return new WaitForSeconds(1.5f);

                                    if (Vector3.Distance(currentTarget.position, transform.position) > 4 && !strangling)
                                    {
                                        entityAnimator.SetBool("Knockout", false);
                                        entityAnimator.SetBool("Grab", false);
                                        continue;
                                    }
                                        

                                    entityAnimator.SetBool("Grab", false);

                                    entityAnimator.SetBool("Knockout", true);

                                    yield return new WaitForSeconds(1f);

                                    if (Vector3.Distance(currentTarget.position, transform.position) > 4 && !strangling)
                                    {
                                        entityAnimator.SetBool("Knockout", false);
                                        entityAnimator.SetBool("Grab", false);
                                        continue;
                                    }
                                       

                                    if (entityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Knockout") && strangling)
                                        GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.KNOCKED_OUT_PARTYGOER);
                                   
                                    entityAnimator.SetBool("Knockout", false);

                                }

                                else if (strangling)
                                {

                                    GameSettings.GetLocalPlayer().playerHealth.TakeDamage(damage, sanityMultiplier, 1f, false, DAMAGE_TYPE.ENTITY);

                                    //damage per second
                                    yield return new WaitForSeconds(0.4f);
                                }
                                //idle
                                else
                                {
                                    if (attackNoiseSource.isPlaying)
                                        attackNoiseSource.Stop();

                                    entityAnimator.SetBool("RunAttack", false);

                                    entityAnimator.ResetTrigger("Attack");

                                    entityAnimator.SetBool("Idle", true);
                                    entityAnimator.SetBool("Grab", false);
                                    entityAnimator.SetBool("Waving", false);

                                    yield return new WaitForSecondsRealtime(0.5f);

                                    speed += 0.05f;
                                    speed = Mathf.Clamp(speed, 1f, originalSpeed);
                                }
                            }

                            yield return new WaitForSeconds(0.25f);

                            break;
                    }

                    

                }
                //idle
                else
                {
                    
                    entityAnimator.SetBool("RunAttack", false);
                    entityAnimator.ResetTrigger("Attack");
                    entityAnimator.SetBool("Idle", true);
                    entityAnimator.SetBool("Grab", false);

                    yield return new WaitForSecondsRealtime(0.1f);

                    speed += 0.05f;
                    speed = Mathf.Clamp(speed, 1f, originalSpeed);
                }
            }
            //idle
            else
            {
                

                entityAnimator.SetBool("RunAttack", false);
                entityAnimator.ResetTrigger("Attack");
                entityAnimator.SetBool("Idle", true);
                entityAnimator.SetBool("Grab", false);

                yield return new WaitForSecondsRealtime(0.5f);

                speed += 0.05f;
                speed = Mathf.Clamp(speed, 1f, originalSpeed);
            }

            if (stunned)
            {
                entityAnimator.SetBool("RunAttack", false);

                entityAnimator.ResetTrigger("Attack");

                entityAnimator.SetBool("Idle", false);
                entityAnimator.SetBool("Grab", false);
                entityAnimator.SetBool("Waving", false);

                entityAnimator.SetTrigger("Stunned");

                yield return new WaitUntil(() => !stunned);
            }
            

        }
    }
    public override void Despawn()
    {
        partygoerBalloon.transform.parent = null;
        partygoerBalloon.transform.Find("StringArmature").Find("Bone.007").gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        GameSettings.GetLocalPlayer().bodyAnim.SetBool("Choking", false);
        GameSettings.GetLocalPlayer().playerHealth.canRun = true;
        GameSettings.GetLocalPlayer().playerHealth.canJump = true;
        GameSettings.GetLocalPlayer().playerHealth.canWalk = true;

        //Debug.Log("Despawned " + type.ToString() + "-" + runTimeID);
        GameObject ragDollInstance = Instantiate(ragDoll);

        ragDollInstance.transform.position = transform.position;
        ragDollInstance.transform.rotation = transform.rotation;

        GameSettings.Instance.worldInstance.RemoveEntity(type.ToString() + "-" + runTimeID);

    }

    public override void UpdateEntity()
    {
        if (tempTarget != null && currentTarget != null)
        {
            //can see target again
            if (Physics.Raycast(eyes.transform.position, currentTarget.position - eyes.transform.position, out var hitInfo, despawnDistance, sightMask))
            {
                tempTarget = null;
            }
        }

        if (stunned)
        {
            strangling = false;

        }
        else
        {
            entityAnimator.ResetTrigger("Stunned");
        }

        if (!GameSettings.GetLocalPlayer().dead)
        {
            if (strangling)
            {

                Vector3 targetDirection = GameSettings.GetLocalPlayer().head.transform.transform.position - eyes.transform.position;

                GameSettings.GetLocalPlayer().transform.position = grabLocation.transform.position - new Vector3(0f, 2.3f, 0f);
                GameSettings.GetLocalPlayer().playerHealth.canRun = false;
                GameSettings.GetLocalPlayer().playerHealth.canJump = false;
                GameSettings.GetLocalPlayer().playerHealth.canWalk = false;

                Vector3 attackDirection = Vector3.RotateTowards(transform.forward, targetDirection, 1f, 1f);
                eyes.transform.rotation = Quaternion.LookRotation(attackDirection);

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("Choking", true);


            }
            else if (!strangling)
            {
                GameSettings.GetLocalPlayer().playerHealth.canJump = true;
                GameSettings.GetLocalPlayer().playerHealth.canWalk = true;
                GameSettings.GetLocalPlayer().bodyAnim.SetBool("Choking", false);
            }
        }
       


    }

    public override void OnSaveFinished()
    {
        
    }

    public override void OnLoadFinished()
    {
        switch (partyGoerType)
        {
            case 0:
                partygoerBalloon.SetActive(false);

                break;

            case 1:
                partygoerBalloon.SetActive(true);

                break;
        }
    }
}
