using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public CraftedWeapon connectedObject;
    public Renderer WeaponBloodRenderer;
    public float bloodAmount = 0;
    public bool onlyHitOneLimb = true;

    public bool hasHit = false;

    private void Start()
    {
        //fix
        SearchForConnectedItem();


    }
    public void SearchForConnectedItem()
    {
        if (GetComponentInParent<CraftedWeapon>() != null)
        {
            connectedObject = GetComponentInParent<CraftedWeapon>();
            connectedObject.SetStat("Damage", damage.ToString());
        }
    }
    //hand to hand combat
    public void OnTriggerEnter(Collider hit)
    {
        
        //make sure that an entity isnt holding this.
        if (gameObject.GetComponentInParent<Entity>() == null && !hasHit)
        {
            hasHit = true;
            Vector3 collisionPoint = hit.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            /*
             * Stabable Layer - 18
             */
            //turn down durability no matter what as long as its not the player
            //weapon is in players hand
            if (gameObject.layer == 23)
            {
               
                    connectedObject = GetComponentInParent<CraftedWeapon>();

                    if (hit.gameObject != gameObject)
                    {
                        //not player
                        if (hit.gameObject.layer != 11)
                        {
                            //hit a breakable holdable object sitting on the floor
                            if (hit.gameObject.layer == 9 && connectedObject.animationPlaying)
                            {

                                hit.GetComponentInParent<HoldableObject>().TakeDamage(damage);

                            }
                            //hit a wall, also check if 
                            else if (hit.gameObject.layer != 9 && hit.gameObject.layer == 18 && connectedObject != null ? connectedObject.animationPlaying : true && onlyHitOneLimb)
                            {
                                GameSettings.GetLocalPlayer().bodyAnim.SetTrigger("WeaponHitObject");

                                onlyHitOneLimb = false;
                                AttackableEntityLimb limb = hit.GetComponent<AttackableEntityLimb>();

                                if (limb != null)
                                {
                                    if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.1f)
                                    {
                                        StartCoroutine(limb.attachedEntity.StunTimer());
                                    }

                                    if (limb.attachedEntity.health - (damage * limb.damageMultiplier) <= 0)
                                    {
                                        Steam.AddAchievment("KILL_ENTITY");
                                    }

                                    
                                    limb.Hit(collisionPoint, damage);

                                    //GameSettings.GetLocalPlayer().bodyAnim.speed = 0;

                                }

                                if (bloodAmount < 1 && GameSettings.Instance.BloodAndGore)
                                {
                                    bloodAmount += 0.04f;

                                    if (WeaponBloodRenderer != null)
                                    {
                                        Debug.Log(hit.gameObject.layer);
                                        WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                                    }

                                }

                            }

                        }
                    }
                
                
            }
            //weapon was being thrown, and is connected to a weapon base
            else if (connectedObject != null)
            {
                if (connectedObject.currentBlade != null && hit.gameObject != gameObject)
                {

                    //dont check for entity hits because stabable objects are the only ones that can be hit
                    if (hit.gameObject.layer != 13 && hit.gameObject.layer != 12 && hit.gameObject.layer != 16 && hit.gameObject.layer != 11 && hit.gameObject.layer != 27 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && !connectedObject.stuckInWall && connectedObject.Flying)
                    {
                        AudioSource.PlayClipAtPoint(connectedObject.stuckSounds[Random.Range(0, connectedObject.stuckSounds.Length)], hit.transform.position);

                        /*if (connetedObject.ObjectConnectionJoint == null)
                        {
                            //conects the joint to the other object

                            connetedObject.ObjectConnectionJoint = connetedObject.gameObject.AddComponent<FixedJoint>();

                            // sets joint position to point of contact

                            connetedObject.ObjectConnectionJoint.anchor = GetComponentInChildren<Weapon>().GetComponent<Collider>().ClosestPoint(transform.position);

                            connetedObject.ObjectConnectionJoint.connectedBody = hit.GetComponentInParent<Rigidbody>();

                            // Stops objects from continuing to collide and creating more joints

                            connetedObject.ObjectConnectionJoint.enableCollision = false;

                        }*/
                        connectedObject.rb.isKinematic = true;

                        connectedObject.transform.parent = hit.transform;
                        

                        connectedObject.stuckInWall = true;
                        connectedObject.Flying = false;


                    }

                    //hit entity limb
                    if (hit.gameObject.layer != 9 && hit.gameObject.layer == 18 && (connectedObject.GetComponent<CraftedWeapon>().Flying || connectedObject.GetComponent<CraftedWeapon>().stuckInWall))
                    {
                        //Debug.Log("Player Attack");
                        AttackableEntityLimb limb = hit.GetComponent<AttackableEntityLimb>();

                        if (limb != null)
                        {
                            if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.1f)
                            {
                                StartCoroutine(limb.attachedEntity.StunTimer());
                            }
                            limb.Hit(collisionPoint, damage);

                            if (bloodAmount < 1 && GameSettings.Instance.BloodAndGore)
                            {
                                bloodAmount += 0.04f;

                                if (WeaponBloodRenderer != null)
                                {
                                    Debug.Log(hit.gameObject.layer);
                                    WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                                }
                            }
                        }
                    }

                }
            }
            if (connectedObject != null)
            {
                //reduce durability
                if (hit.gameObject.layer != 11 && connectedObject.stuckInWall ? true : connectedObject.animationPlaying)
                {
                    if (connectedObject.currentFasten != null)
                    {
                        connectedObject.currentFasten.durabilityLeft -= 1;

                        connectedObject.SetStat("Fasten Durability", connectedObject.currentFasten.durabilityLeft.ToString());

                        //set object stat
                        GetComponentInParent<HoldableObject>().SetStat("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());
                        GetComponentInParent<HoldableObject>().SetMetaData("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());


                        if (connectedObject.currentFasten.durabilityLeft <= 0)
                        {
                            connectedObject.BreakPiece(connectedObject.currentFasten, connectedObject.connectedInvItem);

                            if (connectedObject.currentBlade != null)
                                connectedObject.BreakPiece(connectedObject.currentBlade, connectedObject.connectedInvItem);
                        }


                    }
                    else
                    {
                        if (connectedObject.currentBlade != null)
                        {
                            connectedObject.currentBlade.durabilityLeft -= 1;

                            if (connectedObject.currentBlade.durabilityLeft <= 1)
                                connectedObject.currentBlade.durabilityLeft = 1;

                            connectedObject.SetStat("Blade Durability", connectedObject.currentBlade.durabilityLeft.ToString());

                            GetComponentInParent<HoldableObject>().SetStat("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());
                            GetComponentInParent<HoldableObject>().SetMetaData("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());

                            if (connectedObject.currentBlade.durabilityLeft <= 1)
                            {
                                if (connectedObject.currentFasten == null)
                                {
                                    connectedObject.BreakPiece(connectedObject.currentBlade, connectedObject.connectedInvItem);
                                }

                            }


                        }
                    }

                }
            }
        }
        
      

    }


    //fix blood renderer
    //fix joint
    //fix durability

    public void OnTriggerExit(Collider other)
    {
        hasHit = false;

        if (connectedObject != null)
        {

            //weapon is in players hand
            if (gameObject.layer == 23)
            {
                GameSettings.GetLocalPlayer().bodyAnim.ResetTrigger("WeaponHitObject");
            }
            

            connectedObject.stuckInWall = false;

            
        }

        onlyHitOneLimb = true;


        //GameSettings.GetLocalPlayer().bodyAnim.speed = 1;
        //Debug.Log("TriggerExit");


    }

}
