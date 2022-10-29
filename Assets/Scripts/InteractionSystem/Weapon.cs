using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public CraftedWeapon connetedObject;
    public Renderer WeaponBloodRenderer;
    public float bloodAmount = 0;
    public bool onlyHitOneLimb = true;

    private void Start()
    {
        //fix
        SearchForConnectedItem();


    }
    public void SearchForConnectedItem()
    {
        if (GetComponentInParent<CraftedWeapon>() != null)
        {
            connetedObject = GetComponentInParent<CraftedWeapon>();
            connetedObject.SetStat("Damage", damage.ToString());
        }
    }
    //hand to hand combat
    public void OnTriggerEnter(Collider hit)
    {
        //make sure that an entity isnt holding this.
        if (gameObject.GetComponentInParent<Entity>() == null)
        {
            Vector3 collisionPoint = hit.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            /*
             * Stabable Layer - 18
             */
            //turn down durability no matter what as long as its not the player
            //weapon is in players hand
            if (gameObject.layer == 23)
            {
                if (connetedObject != null)
                {
                    connetedObject = GetComponentInParent<CraftedWeapon>();
                }
                if (hit.gameObject != gameObject)
                {
                    //not player
                    if (hit.gameObject.layer != 11)
                    {
                        //hit a breakable holdable object sitting on the floor
                        if (hit.gameObject.layer == 9 && connetedObject.animationPlaying)
                        {
                            Debug.Log("Damage");
                            hit.GetComponentInParent<HoldableObject>().TakeDamage(damage);
                        }
                        //hit a wall, also check if 
                        else if (hit.gameObject.layer != 9 && hit.gameObject.layer == 18 && connetedObject != null ? connetedObject.animationPlaying : true && onlyHitOneLimb)
                        {
                            GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetTrigger("WeaponHitObject");

                            onlyHitOneLimb = false;
                            AttackableEntityLimb limb = hit.GetComponent<AttackableEntityLimb>();

                            if (limb != null)
                            {
                                if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.2f)
                                {
                                    StartCoroutine(limb.attachedEntity.StunTimer());
                                }

                                limb.attachedEntity.health -= (damage * limb.damageMultiplier);
                                limb.Stabbed(collisionPoint);

                                //GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.speed = 0;

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
            else if (connetedObject != null)
            {
                if (connetedObject.currentBlade != null && hit.gameObject != gameObject)
                {

                    if (hit.gameObject.layer != 13 && hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && !connetedObject.stuckInWall && connetedObject.Flying)
                    {
                        AudioSource.PlayClipAtPoint(connetedObject.stuckSounds[UnityEngine.Random.Range(0, connetedObject.stuckSounds.Length)], hit.transform.position);

                        /*if (connetedObject.ObjectConnectionJoint == null && hit.transform.GetComponentInParent<Rigidbody>() != null)
                        {
                            //conects the joint to the other object

                            connetedObject.ObjectConnectionJoint = GetComponentInParent<HoldableObject>().gameObject.AddComponent<FixedJoint>();
                            // sets joint position to point of contact
                            connetedObject.ObjectConnectionJoint.anchor = GetComponentInChildren<Weapon>().GetComponent<Collider>().ClosestPoint(transform.position);

                            connetedObject.ObjectConnectionJoint.connectedBody = hit.GetComponentInParent<Rigidbody>();
                            // Stops objects from continuing to collide and creating more joints
                            connetedObject.ObjectConnectionJoint.enableCollision = false;

                        }*/
                        if (hit.transform.GetComponentInParent<Rigidbody>() == null)
                        {
                            connetedObject.rb.constraints = RigidbodyConstraints.FreezeAll;
                        }

                        connetedObject.stuckInWall = true;
                        connetedObject.Flying = false;


                    }

                    //hit entity limb
                    if (hit.gameObject.layer != 9 && hit.gameObject.layer == 18 && (connetedObject.GetComponent<CraftedWeapon>().Flying || connetedObject.GetComponent<CraftedWeapon>().stuckInWall))
                    {
                        //Debug.Log("Player Attack");
                        AttackableEntityLimb limb = hit.GetComponent<AttackableEntityLimb>();

                        if (limb != null)
                        {
                            if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.3f)
                            {
                                StartCoroutine(limb.attachedEntity.StunTimer());
                            }
                            limb.attachedEntity.health -= (damage * limb.damageMultiplier);
                            limb.Stabbed(collisionPoint);

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
            if (connetedObject != null)
            {
                //reduce durability
                if (hit.gameObject.layer != 11 && connetedObject.stuckInWall ? true : connetedObject.animationPlaying)
                {
                    if (connetedObject.currentFasten != null)
                    {
                        connetedObject.currentFasten.durabilityLeft -= 1;

                        connetedObject.SetStat("Fasten Durability", connetedObject.currentFasten.durabilityLeft.ToString());

                        //set object stat
                        GetComponentInParent<HoldableObject>().SetStat("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());
                        GetComponentInParent<HoldableObject>().SetMetaData("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());


                        if (connetedObject.currentFasten.durabilityLeft <= 0)
                        {
                            connetedObject.BreakPiece(connetedObject.currentFasten, connetedObject.connectedInvItem);

                            if (connetedObject.currentBlade != null)
                                connetedObject.BreakPiece(connetedObject.currentBlade, connetedObject.connectedInvItem);
                        }


                    }
                    else
                    {
                        if (connetedObject.currentBlade != null)
                        {
                            connetedObject.currentBlade.durabilityLeft -= 1;

                            if (connetedObject.currentBlade.durabilityLeft <= 1)
                                connetedObject.currentBlade.durabilityLeft = 1;

                            connetedObject.SetStat("Blade Durability", connetedObject.currentBlade.durabilityLeft.ToString());

                            GetComponentInParent<HoldableObject>().SetStat("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());
                            GetComponentInParent<HoldableObject>().SetMetaData("Durability", GetComponentInParent<WeaponPiece>().durabilityLeft.ToString());

                            if (connetedObject.currentBlade.durabilityLeft <= 1)
                            {
                                if (connetedObject.currentFasten == null)
                                {
                                    connetedObject.BreakPiece(connetedObject.currentBlade, connetedObject.connectedInvItem);
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
        
        if (connetedObject != null)
        {
            //weapon is in players hand
            if (gameObject.layer == 23)
            {
                if (connetedObject.ObjectConnectionJoint != null)
                {
                    Destroy(GetComponentInParent<HoldableObject>().gameObject.GetComponent<FixedJoint>());
                    Destroy(GetComponentInParent<HoldableObject>().gameObject.GetComponent<Rigidbody>());
                    connetedObject.ObjectConnectionJoint = null;
                }

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.ResetTrigger("WeaponHitObject");
            }
            

            connetedObject.stuckInWall = false;

            
        }

        onlyHitOneLimb = true;


        //GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.speed = 1;
        //Debug.Log("TriggerExit");


    }

}
