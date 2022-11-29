using Lowscope.Saving;
using Lowscope.Saving.Components;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum DAMAGE_TYPE
{
    ENTITY,
    UNKNOWN,
    PLAYER,
    FALL_DAMAGE
}
[Serializable]
public struct PlayerSaveData
{
    public Dictionary<SKILL_TYPE, SKILL_LINE> skillLineSavedData;

    public SavedPlayerInventory savedPlayerInventory;

    public float healthSaved;
    public float hungerSaved;
    public float thirstSaved;

    public float staminaSaved;
    public float sanitySaved;
    public float heartRateSaved;
    public float bodyTemperatureSaved;

    public bool canRunSaved;

    public bool canWalkSaved;

    public bool canJumpSaved;

    public bool canCrouchSaved;

    public float distanceTraveledSaved;

    //notifications

    public bool hasGivenDrinkingNotificationSaved;
    public bool hasGivenSanityNotificationSaved;
    public bool hasGivenHeartRateNotificationSaved;
    public bool hasGivenStaminaNotificationSaved;
    public bool hasGivenCraftingNotificationSaved;
    public bool hasGivenItemBreakingNotificationSaved;

    public bool hasGivenChairCraftingNotificationSaved;
}
public class PlayerController : MonoBehaviour, ISaveable
{
    public TwoBoneIKConstraint offHandIK;
    public RigBuilder builder;

    Coroutine playerDamageOverTime = null;

    //notifications
    public bool hasGivenDrinkingNotification = false;
    public bool hasGivenSanityNotification = false;
    public bool hasGivenHeartRateNotification = false;
    public bool hasGivenStaminaNotification = false;
    public bool hasGivenCraftingNotification = false;
    public bool hasGivenItemBreakingNotification = false;

    public bool hasGivenChairCraftingNotification = false;

    PlayerSaveData playerSaveData;
    public string OnSave()
    {

        //save skill data
        playerSaveData.skillLineSavedData = skillSetSystem.skillDictionary;
        playerSaveData.healthSaved = playerHealth.health;
        playerSaveData.hungerSaved = playerHealth.hunger;
        playerSaveData.thirstSaved = playerHealth.thirst;

        playerSaveData.staminaSaved = playerHealth.stamina;
        playerSaveData.sanitySaved = playerHealth.sanity;
        playerSaveData.bodyTemperatureSaved = playerHealth.bodyTemperature;

        playerSaveData.canRunSaved = playerHealth.canRun;
        playerSaveData.canCrouchSaved = playerHealth.canCrouch;
        playerSaveData.canJumpSaved = playerHealth.canJump;
        playerSaveData.canWalkSaved = playerHealth.canWalk;

        playerSaveData.hasGivenStaminaNotificationSaved = hasGivenStaminaNotification;
        playerSaveData.hasGivenHeartRateNotificationSaved = hasGivenHeartRateNotification;
        playerSaveData.hasGivenSanityNotificationSaved = hasGivenSanityNotification;
        playerSaveData.hasGivenDrinkingNotificationSaved = hasGivenDrinkingNotification;
        playerSaveData.hasGivenCraftingNotificationSaved = hasGivenCraftingNotification;
        playerSaveData.hasGivenItemBreakingNotificationSaved = hasGivenItemBreakingNotification;

        playerSaveData.hasGivenChairCraftingNotificationSaved = hasGivenChairCraftingNotification;

        playerSaveData.distanceTraveledSaved = distance.distanceTraveled;

        foreach (InventorySlot slot in GetComponent<InventorySystem>().GetAllInvSlots())
        {
            if (slot.itemsInSlot.Count > 0)

                GetComponent<InventorySystem>().SetSlotSaveData(slot.name, slot.itemsInSlot[0].connectedObject.GetWorldID());
            
            else 
                GetComponent<InventorySystem>().RemoveSlotSaveData(slot.name);
        }

        playerSaveData.savedPlayerInventory = GetComponent<InventorySystem>().currentPlayerInventorySave;

        return JsonConvert.SerializeObject(playerSaveData);
    }

    public void OnLoad(string data)
    {
        playerSaveData = JsonConvert.DeserializeObject<PlayerSaveData>(data);

        skillSetSystem.LoadInData(playerSaveData);

        playerHealth.LoadInData(playerSaveData);

        distance.LoadInData(playerSaveData);

        GetComponent<InventorySystem>().LoadInData(playerSaveData);

        hasGivenDrinkingNotification = playerSaveData.hasGivenDrinkingNotificationSaved;
        hasGivenHeartRateNotification = playerSaveData.hasGivenHeartRateNotificationSaved;
        hasGivenSanityNotification = playerSaveData.hasGivenSanityNotificationSaved;
        hasGivenStaminaNotification = playerSaveData.hasGivenStaminaNotificationSaved;
        hasGivenCraftingNotification = playerSaveData.hasGivenCraftingNotificationSaved;
        hasGivenItemBreakingNotification = playerSaveData.hasGivenItemBreakingNotificationSaved;

        hasGivenChairCraftingNotification = playerSaveData.hasGivenChairCraftingNotificationSaved;

        rb.velocity = Vector3.zero;

    }

    public void OnLoadNoData()
    {
        rb.velocity = Vector3.zero;
        return;
    }

    public bool OnSaveCondition()
    {
        return true;
    }

    public Rigidbody rb;

    public ReflectionProbe probe;
    public GameObject darkShield;
    public GameObject headTarget;

    public bool isNoClipping = false;


    public IEnumerator LoadInWorldPlayerData()
    {

        if (GameSettings.Instance.worldInstance != null)
        {
            

            yield return new WaitUntil(() => GameSettings.Instance.AmInSavableScene());

            rb.constraints = RigidbodyConstraints.FreezeAll;

            yield return new WaitUntil(() => GameSettings.SCENE_LOADED);

            SetPlayerPositionFromWorld(GameSettings.Instance.worldInstance);

            GameSettings.PLAYER_DATA_LOADED = true;

            yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
            yield return new WaitUntil(() => GameSettings.SPAWN_REGION_GENERATED);

            distance.lastPosition = transform.position;



        }

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Debug.Log("Player Data Finished Loading");
    }
    
    private void SetPlayerPositionFromWorld(BackroomsLevelWorld world)
    {
        if (world.playerLocationData.savedPosition.Length == 3)
        
            transform.position = new Vector3(world.playerLocationData.savedPosition[0], world.playerLocationData.savedPosition[1], world.playerLocationData.savedPosition[2]);
       
        if (world.playerLocationData.savedBodyRotationEuler.Length == 3)

            transform.rotation = Quaternion.Euler(world.playerLocationData.savedBodyRotationEuler[0], world.playerLocationData.savedBodyRotationEuler[1], world.playerLocationData.savedBodyRotationEuler[2]);
        
        if (world.playerLocationData.savedHeadRotationEuler.Length == 3)

            neck.transform.rotation = Quaternion.Euler(world.playerLocationData.savedHeadRotationEuler[0], world.playerLocationData.savedHeadRotationEuler[1], world.playerLocationData.savedHeadRotationEuler[2]);

        rotationX = world.playerLocationData.savedRotationX;
        rotationY = world.playerLocationData.savedRotationY;

        if (!PlayerHasRoom())
        {
            GetComponent<CapsuleCollider>().center = new Vector3(0f, 1.19f, 0f);
            GetComponent<CapsuleCollider>().height = 2.41f;

            maxSpeed = Crouchmaxspeed;

            bodyAnim.SetBool("isCrouching", true);
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);

            currentPlayerState = PLAYERSTATES.CROUCH;
            bufferStand = true;
        }



    }


    public GameObject playerRagDoll;

    public PlayerHealthSystem playerHealth;
    public DistanceChecker distance;
    public SkillSetSystem skillSetSystem;


    bool headReset = false;
    //movement

    //Movement
    public float moveSpeed = 1500;

    public float WalkmaxSpeed = 20;
    public float Crouchmaxspeed = 20;
    public float SprintmaxSpeed = 20;

    public float maxSpeed = 20;

    public bool isGrounded = true;

    float walkingSpeed = 4f;
    float runningSpeed = 6f;
    float crouchingMultiplier = 0.5f;
    float jumpSpeed = 7.0f;

    public float counterMovement = 0.175f;
    float threshold = 0.01f;
    public float maxSlopeAngle;
    float stepSmooth = 2f;

    public LayerMask whatIsGround;

    //Crouch & Slide
    [SerializeField]
    bool bufferStand;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public bool sliding;

    //used to adjust collider
    float OGcapsuleHight;
    Vector3 OGcapsuleCenter;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    public float AirmaxSpeed = 20;

    //Movement
    Vector3 groundNormal = Vector3.up;
    Vector3 currentMoveDirection = Vector3.zero;
    Vector3 slopeMoveDirection = Vector3.zero;

    Vector3 walkForceX = Vector3.zero;
    Vector3 walkForceZ = Vector3.zero;

    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    //Vaulting
    private GameObject VaultObject;
    private bool Vaulting = false;

    public Transform VaultCollider;

    public float adrenalineSpeedMultiplier;
    public float gravity = 50.0f;

    Coroutine run = null;
    Coroutine walk = null;
    Coroutine reviveHeartRate = null;
    Coroutine footStep = null;

    Coroutine rebuildStamina = null;
    Coroutine removeStamina = null;

    public GameObject RHandLocation;
    public GameObject LHandLocation;
    public GameObject holdLocation;

    public GameObject arms;
    public Renderer playerSkin;

    //player parts
    public Camera playerCamera;
    public Camera armsCamera;

    public GameObject neck;
    public GameObject head;
    public GameObject feet;
    public GameObject death;
    Transform ogHeadTrans;

    //mouth sounds
    public AudioClip breathingNormal;
    public AudioClip breathingRunning;
    public AudioClip[] hitSounds;

    //footstep noises
    public AudioClip carpetFootStep;
    public AudioClip cementFootStep;
    public AudioClip woodFootStep;
    public AudioClip metalFootStep;

    //swing noises
    public AudioClip[] swingSounds;

    //fov editing
    private float _FOVOFFSET;
    
    public bool dead = false;

    //audioSources

    public AudioSource playerNoises;
    public AudioSource feetSource;

    bool playFootSteps = false;

    //postEdits

    //head movement
    public float lookXLimitTop = 85.0f;
    public float lookXLimitBottom = 90.0f;
    public float rotationX = 0.0f;
    public float rotationY = 0.0f;

    //0 = walk, 1 = run, 2 = crouch, 3 = jumping
    public enum PLAYERSTATES : int
    {
        IDLE = 0,
        WALK = 1,
        CROUCH = 2,
        RUN = 3,
        JUMP = 4,
        PRONE = 5,
        SLIDE = 6,
        IMMOBILE = 7,
    }

    public PLAYERSTATES currentPlayerState = PLAYERSTATES.IDLE;

    //animation
    public Animator bodyAnim;
    public GameObject headAnimLocation;

   
    //ui
    public Canvas UI;


    public float pushMagnitude;

    /*private void OnControllerColliderHit(ControllerColliderHit collision)
    {

        Rigidbody rigidbody = collision.collider.attachedRigidbody;

        if (rigidbody != null)
        {
            Vector3 forceDirection = collision.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();

            //rigidbody.AddForceAtPosition(forceDirection * pushMagnitude, transform.position, ForceMode.Impulse);
        }
    }*/

    void Awake()
    {
        OGcapsuleHight = GetComponent<CapsuleCollider>().height;
        OGcapsuleCenter = GetComponent<CapsuleCollider>().center;

        playerHealth = GetComponent<PlayerHealthSystem>();
        rb = GetComponent<Rigidbody>();
        maxSpeed = WalkmaxSpeed;

    }

    void OnDestroy()
    {
        SaveMaster.RemoveListener(GetComponent<Saveable>());
        Destroy(GetComponent<Saveable>());
    }

    void Start()
    {
        gameObject.AddComponent<Saveable>();
        GetComponent<Saveable>().SaveIdentification = GameSettings.Instance.activeUser;
        GetComponent<Saveable>().AddSaveableComponent("PlayerData", this, true);       

        //if (GetComponent<Collider>().)
        isGrounded = false;

        ogHeadTrans = neck.transform;


        DontDestroyOnLoad(gameObject);


        /*Saveable component = gameObject.AddComponent<Saveable>();
        component.SaveIdentification = GameSettings.Instance.activeUser;
        component.AddSaveableComponent("PlayerData", this, true);

        SaveMaster.AddListener(component);
        SaveMaster.SyncLoad();*/

        /*AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener foundListener in audioListeners)
        {
            Debug.Log(foundListener.name);
        }*/

    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!isGrounded || currentPlayerState == PLAYERSTATES.JUMP) return;

        //Slow down sliding
        if (currentPlayerState == PLAYERSTATES.SLIDE)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// Find the velocity relative to where the player is looking
    /// Useful for vectors with movement and limiting movement
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// Handle ground detection
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            groundNormal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(groundNormal))
            {
                isGrounded = true;
                cancellingGrounded = false;
                normalVector = groundNormal;
                CancelInvoke(nameof(StopGrounded));

                break;               
                
            }
        }

        //ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        isGrounded = false;
    }

    bool isSlope()
    {
        if (groundNormal != Vector3.up)
        {
            return true;

            
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        bool isCurrentlyClimbing = GetComponent<InteractionSystem>().currentClimbable != null;

        if (!GameSettings.Instance.IsCutScene && !dead)
        {
            if (playerHealth.sanity <= 50f && !hasGivenSanityNotification)
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("BE SURE TO WATCH YOUR SANITY, WEIRD THINGS HAPPEN WHEN IT GETS LOW (HOLD [H])");
                hasGivenSanityNotification = true;
            }
                
            
            if (playerHealth.thirst <= 50f && !hasGivenDrinkingNotification)
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("BE SURE TO STAY HYDRATED, OR YOU COULD END UP LIKE THE REST OF THEM.... (HOLD [H])");
                hasGivenDrinkingNotification = true;
            }
               
            
            if (playerHealth.stamina <= 50f && !hasGivenStaminaNotification)
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU CAN ONLY RUN FOR A LIMITED AMOUNT OF TIME BEFORE YOU RUN OUT OF BREATH, BE CAREFUL TO CONSERVE YOUR ENERGY...");
                hasGivenStaminaNotification = true;
            }
               
            
            if (playerHealth.heartRate >= 105f && !hasGivenHeartRateNotification)
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("KEEPING YOUR HEARTRATE AROUND 90BPM WILL KEEP YOUR SANITY LOW, BE SURE TO WATCH YOUR BPM (HOLD [H])");
                hasGivenHeartRateNotification = true;
            }

            if (GameSettings.Instance.worldInstance != null)

                if (GameSettings.Instance.worldInstance.timeInSecondsSinceWorldFirstLoaded >= 150 && GameSettings.Instance.ActiveScene == SCENE.LEVEL0 && !hasGivenChairCraftingNotification)
                {
                    GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("MAYBE I SHOULD LOOK FOR SOMETHING SHARP TO CUT THESE CHAIR LEGS WITH AND CRAFT SOME WEAPONS, WHO KNOWS WHATS LURKING OUT THERE...");
                    hasGivenChairCraftingNotification = true;
                }


            if (!isCurrentlyClimbing)
            {
                if (PlayerHasRoom() && bufferStand) {
                    
                    GetComponent<CapsuleCollider>().center = OGcapsuleCenter;
                    GetComponent<CapsuleCollider>().height = OGcapsuleHight;

                    bodyAnim.SetBool("isCrouching", false);

                    bufferStand = false;
                }

                //base magnitude off of direction faced
                Vector2 mag = FindVelRelativeToLook();
                float xMag = mag.x, yMag = mag.y;

                currentMoveDirection.x = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxisRaw("Horizontal") * adrenalineSpeedMultiplier : 0;
                currentMoveDirection.y = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxisRaw("Vertical") * adrenalineSpeedMultiplier : 0;

                //CounterMovement(curSpeedX, curSpeedY, mag);

                //Set max speed
                float maxSpeed = this.maxSpeed;

                //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
                if (currentMoveDirection.x > 0 && xMag > maxSpeed) currentMoveDirection.x = 0;
                if (currentMoveDirection.x < 0 && xMag < -maxSpeed) currentMoveDirection.x = 0;
                if (currentMoveDirection.y > 0 && yMag > maxSpeed) currentMoveDirection.y = 0;
                if (currentMoveDirection.y < 0 && yMag < -maxSpeed) currentMoveDirection.y = 0;

                //Some movement multipliers
                float multiplier = 1f, multiplierV = 1f;

                // Movement while sliding
                if (currentPlayerState == PLAYERSTATES.SLIDE)
                {
                    multiplier = 2f;
                    multiplierV = 2f;
                }

                // Movement in air
                if (!isGrounded)
                {
                    multiplier = 0.25f;
                    multiplierV = 0.25f;
                }


                if (GetComponent<InteractionSystem>().GetObjectInRightHand() != null)
                {
                    multiplier *= GetComponent<InteractionSystem>().GetObjectInRightHand().large ? 0.80f : 1f;
                    multiplier *= GetComponent<InteractionSystem>().GetObjectInRightHand().large ? 0.80f : 1f;
                }

                Debug.Log("INPUT");
                walkForceX = transform.forward * currentMoveDirection.y * moveSpeed * Time.deltaTime * multiplier * multiplierV;
                walkForceZ = transform.right * currentMoveDirection.x * moveSpeed * Time.deltaTime * multiplier;


                float rbVelX = transform.InverseTransformDirection(rb.velocity).z;
                float rbVelY = transform.InverseTransformDirection(rb.velocity).x;

                bodyAnim.SetFloat("xWalk", rbVelX);
                bodyAnim.SetFloat("yWalk", rbVelY);


                rb.AddForce(walkForceX);
                rb.AddForce(walkForceZ);

                rb.AddForce(-transform.up * Time.deltaTime * gravity);

                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.25f && Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.25f && isGrounded && currentPlayerState != PLAYERSTATES.SLIDE && !Input.GetButton("Jump"))
                {
                    rb.drag = 100f;
                }
                else if (isGrounded)
                {
                    rb.drag = 2f;
                }
                else
                {
                    rb.drag = 0f;
                }

                //If holding jump && ready to jump, then jump
                if (Input.GetButtonDown("Jump") && isGrounded && playerHealth.canJump) Jump();
                //Debug.Log(headTarget.transform.position);

                /*if (holding != null)
                Debug.Log(holding.animationPlaying);*/
            }


            // Player and Camera rotation
            if (!dead)
            {

                if (currentPlayerState != PLAYERSTATES.IMMOBILE)

                    SpeedAndFovController();

                transform.rotation *= Quaternion.Euler(0, rotationY, 0);
                //player movement

                PlayerDataLoop();

                // Press Left Shift to run

                _FOVOFFSET = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? Mathf.Lerp(_FOVOFFSET, 10f, 10f * Time.deltaTime) : Mathf.Lerp(_FOVOFFSET, 0f, 10f * Time.deltaTime)) : 0;

                // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
                // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
                // as an acceleration (ms^-2)



                /*TODO*/
                // Move the controller
                //characterController.Move(new Vector3(moveDirection.x, moveDirection.y, moveDirection.z) * Time.deltaTime);

                /*  float smoothedSpeedX = Mathf.Lerp(0, curSpeedX, 2 * Time.deltaTime);
                  float smoothedSpeedY = Mathf.Lerp(0, curSpeedY, 2 * Time.deltaTime);*/

            }

            if (!dead && Cursor.lockState != CursorLockMode.None && playerHealth.canMoveHead)
            {

                rotationX += -Input.GetAxisRaw("Mouse Y") * GameSettings.Instance.Sensitivity / 2;
                rotationX = Mathf.Clamp(rotationX, -lookXLimitBottom, lookXLimitTop);

                rotationX = Mathf.Clamp(rotationX, -lookXLimitBottom + 4, lookXLimitTop - 4);

                rotationY = Input.GetAxisRaw("Mouse X") * GameSettings.Instance.Sensitivity;

            }
            else
            {
                rotationY = 0f;
            }

            

        }
        

    }

    void FixedUpdate()
    {
        //apply slope velocity in fixed
        if (!GameSettings.Instance.IsCutScene && !dead)
        {
            //Apply forces to move player
            if (isSlope() && isGrounded)
            {
                slopeMoveDirection = Vector3.ProjectOnPlane(currentMoveDirection, groundNormal);
                Debug.Log(slopeMoveDirection);
                rb.AddForce(slopeMoveDirection * 100f, ForceMode.Force);

            }
        }
    }
    void LateUpdate()
    {
        
        if (!GameSettings.Instance.IsCutScene)
        {
            //head.transform.rotation = Quaternion.Euler(bodyAnim.GetBool("isProne") ? rotationX : rotationX, head.transform.rotation.eulerAngles.y, 0);
            head.transform.localRotation = Quaternion.Euler(bodyAnim.GetBool("isProne") ? rotationX : rotationX, 0, 0);
        }
            

        else
        {
            head.transform.rotation = headTarget.transform.rotation;
        }
        //neck.transform.rotation = Quaternion.Euler(neck.transform.rotation.eulerAngles.x, neck.transform.rotation.eulerAngles.y, (bodyAnim.GetBool("isProne") ? rotationX - 90 : rotationX - 90));

        head.transform.position = Vector3.Lerp(head.transform.position, headTarget.transform.position, 75f * Time.deltaTime);
    }
    private void HandleFootstep()
    {
        //footstep
        RaycastHit hit;

        if (Physics.Raycast(new Ray(feet.transform.position, Vector3.down), out hit, 3f))

            if (hit.collider.gameObject.tag != "Player" && isGrounded && !playFootSteps && rb.velocity.magnitude > 0.005f)
            {

                playFootSteps = true;
                footStep = StartCoroutine(PlayFootstep(hit.collider.tag));
                //Debug.Log(hit.collider.tag);

            }

            if (!isGrounded && playFootSteps)
            {

                playFootSteps = false;

                if (footStep != null)
                    StopCoroutine(footStep);
            }

            //Debug.Log(hit.collider.gameObject.tag);

            

    }

    //Vaults if you can otherwise jump
    private void Jump()
    {
        //Jump and vault
        if (!CanVault() & isGrounded)
        {
            maxSpeed = AirmaxSpeed;

            //Add jump forces
             rb.AddForce(Vector3.up * jumpForce * 1.5f);
             rb.AddForce(-rb.velocity.normalized * 1000f * rb.velocity.magnitude);
             rb.AddForce(normalVector * jumpForce * 0.1f);

            currentPlayerState = PLAYERSTATES.JUMP;

            //rb.velocity += Vector3.up * jumpForce * 1.5f;

            //If jumping while falling, reset y velocity.

            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
        }
    }
    private bool CanVault()
    {
       /* Debug.Log("CanVault");*/

        RaycastHit VaultRayHit; // cast down to find objects
        RaycastHit VaultFinalRayHit; // cast forward to stop clipping
        RaycastHit VaultSizeRayHit; // cast the players size to test if he can be there

        //Cast down to find object
        if (!Vaulting & currentPlayerState != PLAYERSTATES.CROUCH & Physics.SphereCast(VaultCollider.position, 0.125f, -VaultCollider.up, out VaultRayHit, 3.5f, whatIsGround))
        {
           /* Debug.Log("Checking Forward Ray..");
            Debug.DrawRay(VaultCollider.position, -VaultCollider.up, Color.red, 3, false);*/
            
            

            // Cast forward then check if the player can fit where they tried to climb
            if (!Physics.SphereCast(new Vector3(rb.position.x, VaultCollider.position.y - (VaultRayHit.distance - 0.135f), rb.position.z), 0.125f, transform.forward, out VaultFinalRayHit, 0.475f, whatIsGround))
            {
                /*Debug.DrawRay(new Vector3(rb.position.x, VaultCollider.position.y - VaultRayHit.distance, rb.position.z), transform.forward, Color.blue, 3, false);*/

                if (!Physics.SphereCast(VaultRayHit.point + Vector3.up * (GetComponent<CapsuleCollider>().radius + 0.01f), GetComponent<CapsuleCollider>().radius, Vector3.up, out VaultSizeRayHit, GetComponent<CapsuleCollider>().height - (GetComponent<CapsuleCollider>().radius * 2) - 0.01f, whatIsGround))
                {
                    /*Debug.DrawRay(VaultRayHit.point + Vector3.up * (GetComponent<CapsuleCollider>().radius + 0.01f), Vector3.up, Color.green, 3, false);
                    Debug.Log("CastForward: " + true);*/
                    
                    //move the player

                    Vault(VaultRayHit);

                    //Return so we dont jump
                    return true;
                }
                else { return false; }

                

            }
            else { return false; }
        }
        else { return false; }
    }

    private void Vault(RaycastHit Hit)
    {

        //this is on animation start {}
        Vaulting = true;
        rb.isKinematic = true; //stops the player from moving
       /* jumping = false;*/

        //this is on animation end {}
        rb.position = Hit.point + (Vector3.up / 1.5f); // moves the player after animation
        Vaulting = false;
        rb.isKinematic = false;

    }

    private void HandlePlayerStates()
    {

        if (isGrounded && Mathf.Abs(rb.velocity.magnitude) >= 0.1f)
        {

            if (!bufferStand && Input.GetButton("Run") && playerHealth.canRun && !bodyAnim.GetBool("Prone") && !GetComponent<InventorySystem>().menuOpen && isGrounded)
            {
                bool shouldRun = true;


                if (GetComponent<InteractionSystem>().GetObjectInRightHand() != null)
                {
                    if (GetComponent<InteractionSystem>().GetObjectInRightHand().large)

                        shouldRun = false;
                }

                if (shouldRun)
                {
                    if (!Input.GetMouseButton(0))
                        bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 0, Time.deltaTime * 10));
                    else
                        bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 1, Time.deltaTime * 10));
                    
                    bodyAnim.SetBool("isCrouching", false);
                    bodyAnim.SetBool("isWalking", false);
                    bodyAnim.SetBool("isRunning", true);

                    maxSpeed = SprintmaxSpeed;

                    currentPlayerState = PLAYERSTATES.RUN;
                }


               

            }

            else if (!bufferStand && playerHealth.canWalk && isGrounded)
            {
                bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 1, Time.deltaTime * 10));
                bodyAnim.SetBool("isWalking", true);
                bodyAnim.SetBool("isRunning", false);

                maxSpeed = WalkmaxSpeed;

                currentPlayerState = PLAYERSTATES.WALK;
            

            }
            else
            {
                bodyAnim.SetBool("isRunning", false);
                bodyAnim.SetBool("isWalking", false);
            }

            /*if (Input.GetButtonDown("Crouch") && currentPlayerState != PLAYERSTATES.JUMP && isGrounded)
            {
                //slide

                if (isGrounded && Mathf.Abs(rb.velocity.magnitude) <= 2f)
                {
                    bodyAnim.SetBool("isCrouching", true);
                    bodyAnim.SetBool("isSliding", true);
                    bodyAnim.SetBool("isRunning", false);
                    bodyAnim.SetBool("isWalking", false);

                    Debug.Log("SLIDE");

                    currentPlayerState = PLAYERSTATES.SLIDE;

                    rb.AddForce(transform.forward * slideForce);

                }

            }*/

        }
        //idle
        else if (!bufferStand)
        {
            bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 1, Time.deltaTime * 10));
               
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);




            currentPlayerState = PLAYERSTATES.IDLE;

        }

        if (!bufferStand && Input.GetButton("Crouch") && currentPlayerState != PLAYERSTATES.JUMP && currentPlayerState != PLAYERSTATES.SLIDE && playerHealth.canWalk)
        {
            
            
            GetComponent<CapsuleCollider>().center = new Vector3(0f, 1.19f, 0f);
            GetComponent<CapsuleCollider>().height = 2.41f;

            bodyAnim.SetBool("isCrouching", true);
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);

            currentPlayerState = PLAYERSTATES.CROUCH;


            

        }

        if (Input.GetButtonUp("Crouch"))
        {
            if (PlayerHasRoom())
            {
                GetComponent<CapsuleCollider>().center = OGcapsuleCenter;
                GetComponent<CapsuleCollider>().height = OGcapsuleHight;

                bodyAnim.SetBool("isCrouching", false);
            }
            else { bufferStand = true; }


        }


        if (!bufferStand && Input.GetButtonDown("Jump") && currentPlayerState != PLAYERSTATES.CROUCH && currentPlayerState != PLAYERSTATES.PRONE && isGrounded && playerHealth.canJump)
        {

            if (CanVault())
            {
                bodyAnim.SetTrigger("isVaulting");

            }
            else
            {
                bodyAnim.SetTrigger("isJumping");
            }

            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);

            currentPlayerState = PLAYERSTATES.JUMP;


        }

        
        

        
        /* if (Input.GetButtonDown("Prone") && currentPlayerState != PLAYERSTATES.JUMP)
         {
             if (!bodyAnim.GetBool("isProne"))
             {
                 bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 0, Time.deltaTime * 10));
                 bodyAnim.SetBool("isProne", true);
                 ///bodyAnim.SetBool("isCrouching", false);
                 bodyAnim.SetBool("isWalking", false);
                 bodyAnim.SetBool("isRunning", false);
             }
             else
             {
                 bodyAnim.SetBool("isProne", false);
             }

         }*/


        
        if (Input.GetButton("Watch"))
        {
            bool canOpenWatch = true;

            if (GetComponent<InteractionSystem>().GetObjectInRightHand() != null)
            {
                if (GetComponent<InteractionSystem>().GetObjectInRightHand().large)
                    canOpenWatch = false;

                if (GetComponent<InteractionSystem>().GetObjectInRightHand().StartUseAnimation != null) bodyAnim.SetBool(GetComponent<InteractionSystem>().GetObjectInRightHand().StartUseAnimation, false);
                
                if (GetComponent<InteractionSystem>().GetObjectInRightHand().UseAnimations.Count > 0) bodyAnim.SetBool(GetComponent<InteractionSystem>().GetObjectInRightHand().UseAnimations[GetComponent<InteractionSystem>().GetObjectInRightHand().currentAnimChoice], false);
                
                GetComponent<InteractionSystem>().GetObjectInRightHand().animationPlaying = false;

                
            }

            bodyAnim.SetBool("Watch", canOpenWatch);


        }
        if (Input.GetButtonUp("Watch"))
        {                
            bodyAnim.SetBool("Watch", false);
        }
        
           

        //controll player stamina volume indicator
        playerNoises.volume = 0.025f + ((100 - playerHealth.stamina) / 100) / 2;

        switch (currentPlayerState)
        {
            case PLAYERSTATES.IDLE:

                if (removeStamina != null)
                    StopCoroutine(removeStamina);


                if (run != null)
                    StopCoroutine(run);

                if (walk != null)
                    StopCoroutine(walk);

                run = null;
                walk = null;
                removeStamina = null;

                if (!playerNoises.isPlaying || playerNoises.clip == breathingRunning)
                {
                    //playerNoises.time = UnityEngine.Random.Range(0f, playerNoises.clip.length);
                    playerNoises.Stop();
                    playerNoises.clip = breathingNormal;
                    playerNoises.Play();

                }

                break;

            case PLAYERSTATES.WALK:

                
                //if (removeStamina == null)
                //removeStamina = StartCoroutine(playerHealth.ChangeStamina(UnityEngine.Random.Range(-2, -5)));

                if (removeStamina != null)
                    StopCoroutine(removeStamina);


                if (run != null)
                
                    StopCoroutine(run);

                run = null;
                removeStamina = null;

                if (walk == null)
                    walk = StartCoroutine(playerHealth.Walk());

                if (!playerNoises.isPlaying || playerNoises.clip == breathingRunning)
                {
                    //playerNoises.time = UnityEngine.Random.Range(0f, playerNoises.clip.length);
                    playerNoises.Stop();
                    playerNoises.clip = breathingNormal;
                    playerNoises.Play();
    
                }

                break;

            case PLAYERSTATES.RUN:
                
                if (removeStamina == null)

                    removeStamina = StartCoroutine(playerHealth.ChangeStaminaOverTime(UnityEngine.Random.Range(-1, -2)));

                if (run == null)

                    run = StartCoroutine(playerHealth.Run());

                if (reviveHeartRate != null)
                
                    StopCoroutine(reviveHeartRate);
                    
                if (rebuildStamina != null)
               
                    StopCoroutine(rebuildStamina);

                reviveHeartRate = null;
                rebuildStamina = null;

                if (!playerNoises.isPlaying || playerNoises.clip == breathingNormal)
                {
                    //playerNoises.time = UnityEngine.Random.Range(0f, playerNoises.clip.length);

                    playerNoises.Stop();
                    playerNoises.clip = breathingRunning;
                    playerNoises.Play();
                }


                break;

            case PLAYERSTATES.CROUCH:

                maxSpeed = Input.GetButton("Run") ? WalkmaxSpeed : Crouchmaxspeed;

                if (run != null)
                    StopCoroutine(run);

                run = null;

                if (removeStamina != null)
                    StopCoroutine(removeStamina);

                removeStamina = null;

                //GetComponent<CharacterController>().height = 2.5f;
                //GetComponent<CharacterController>().center = new Vector3(0, 0.2f, 0);

                if (!playerNoises.isPlaying || playerNoises.clip == breathingRunning)
                {
                    //playerNoises.time = UnityEngine.Random.Range(0f, playerNoises.clip.length);
                    playerNoises.Stop();
                    playerNoises.clip = breathingNormal;
                    playerNoises.Play();

                }

                break;

            case PLAYERSTATES.JUMP:

                playerHealth.ChangeHeartRate(UnityEngine.Random.Range(2, 4f));
                playerHealth.ChangeStamina(UnityEngine.Random.Range(-5, -10f));

                break;

        }
    }
    private bool PlayerHasRoom()
    {
        
        RaycastHit SneakRayHit;
        
/*      Debug.DrawRay(transform.position + Vector3.up * (OGcapsuleHight + 0.01f), Vector3.up / 4f, Color.cyan, 3, false);*/



        if (!Physics.SphereCast(transform.position, GetComponent<CapsuleCollider>().radius - 0.01f, Vector3.up, out SneakRayHit, (OGcapsuleHight - 0.01f) - GetComponent<CapsuleCollider>().radius - 0.01f, whatIsGround))
        {
            Debug.Log("Player Has room");
            return true;
        }
        else { Debug.Log(SneakRayHit.transform.gameObject.name);  return false; }
    }
    private void HandleHealthSystem()
    {
        //adrenaline (100+bpm)
        if (Input.GetButtonDown("Adrenaline") && playerHealth.heartRate >= 100 && !playerHealth.adrenalineActive && playerHealth.canUseAdrenaline && !playerHealth.adrenalineCoolDownActive)
        {
            StartCoroutine(playerHealth.ActivateAdrenaline());
        }     

        if (playerHealth.adrenalineActive == false && !playerHealth.canUseAdrenaline && playerHealth.adrenalineCoolDownActive)
        {
            StartCoroutine(playerHealth.AdrenalineCooldown());
            playerHealth.adrenalineCoolDownActive = false;
        }
        //heart system (revive and drop)
        if (!playerHealth.adrenalineActive)
        {
            if (playerHealth.heartRate > 90 && currentPlayerState != PLAYERSTATES.RUN && reviveHeartRate == null)
            {
                reviveHeartRate = StartCoroutine(playerHealth.ReviveHeartRate());
            }
            if (playerHealth.heartRate <= 90 && reviveHeartRate != null)
            {
                if (reviveHeartRate != null)
                {
                    StopCoroutine(reviveHeartRate);
                    reviveHeartRate = null;
                }
            }
            //heartrate
            if (playerHealth.heartRate >= 120)
            {
                currentPlayerState = PLAYERSTATES.WALK;

                playerHealth.canJump = true;
                playerHealth.canWalk = true;
                playerHealth.canRun = false;
            }
            if (playerHealth.heartRate >= 160)
            {

                currentPlayerState = PLAYERSTATES.IMMOBILE;

                playerHealth.canJump = false;
                playerHealth.canWalk = false;
                playerHealth.canRun = false;

            }
            if (playerHealth.stamina >= 50 && playerHealth.heartRate < 100)
            {
                playerHealth.canJump = true;
                playerHealth.canRun = true;
                playerHealth.canWalk = true;

            }

            //stamina
            if (currentPlayerState != PLAYERSTATES.RUN)
            {
                if (rebuildStamina == null)
                    rebuildStamina = StartCoroutine(playerHealth.ChangeStaminaOverTime(UnityEngine.Random.Range(1, 3)));

            }

            if (playerHealth.stamina >= 100)
            {
                playerHealth.stamina = 100;

                if (rebuildStamina != null)
                    StopCoroutine(rebuildStamina);

                rebuildStamina = null;

            }

            else if (playerHealth.stamina <= 0)
            {
                playerHealth.canRun = false;
                playerHealth.canWalk = true;
                playerHealth.canJump = false;

                currentPlayerState = PLAYERSTATES.WALK;
                playerHealth.stamina = 0;

            }
        }

    }

    private void PlayerDataLoop()
    {
        if (!GameSettings.Instance.cheatSheet.noClip)
        {
            if (playerHealth.canWalk && currentPlayerState != PLAYERSTATES.IDLE)

                HandleFootstep();

            HandlePlayerStates();
        }
        

        if (!GameSettings.Instance.cheatSheet.invincible)

            HandleHealthSystem();

        else
        {
            playerHealth.health = 100;
            playerHealth.sanity = 100;
            playerHealth.thirst = 100;
            playerHealth.heartRate = 80;
            playerHealth.hunger = 100;
            playerHealth.bodyTemperature = 98.6f;
        }
    }

    private void SpeedAndFovController()
    {

        if (bodyAnim.GetBool("isProning"))
        {

            walkingSpeed = 0.5f;
        }

        else
        {

            playerCamera.fieldOfView = GameSettings.Instance.FOV + _FOVOFFSET;
            walkingSpeed = 3f;
        }
    }


    public IEnumerator Die()
    {

        dead = true;
        playerSkin.enabled = false;

        GameObject ragDoll = Instantiate(playerRagDoll);

        ragDoll.transform.position = transform.position;
        ragDoll.transform.rotation = transform.rotation;

        bodyAnim.SetBool("isCrouching", value: false);
        bodyAnim.SetBool("isProning", value: false);
        bodyAnim.SetBool("isReloading", value: false);
        bodyAnim.SetBool("isAiming", value: false);
        bodyAnim.SetBool("isRunning", value: false);
        bodyAnim.SetBool("isWalking", value: false);
        bodyAnim.SetBool("isSwimming", value: false);

        
        playerCamera.enabled = false;
        armsCamera.enabled = false;

        float pass = 20000f;
        bool dying = true;

        while (dying && pass > 2000f)
        {
            pass -= 1500f;
            GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", pass);
            yield return new WaitForSecondsRealtime(0.25f);
        }

        GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
        yield return new WaitForSecondsRealtime(4f);
        GameSettings.Instance.ResetGame();
        GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", 20000f);
        
        
    }
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Arm_EventTrigger")
        {
            StartCoroutine(EnableArmsOverTime(col.transform.GetChild(0), false));
        }

        if (playerDamageOverTime != null)
        {
            StopCoroutine(playerDamageOverTime);
            playerDamageOverTime = null;
        }

       /* if (col.tag == "NoClip")
        {
            isNoClipping = false;
        }*/
    }

    void OnTriggerEnter(Collider col)
    {
        //first entrance
        if (col.tag == "NoClip" && !isNoClipping)
        {
            isNoClipping = true;

            //push the player back out of it when they return
            if (col.gameObject.transform.childCount > 0)
            { 
                GameSettings.Instance.worldInstance.playerLocationData.savedPosition = new float[] { col.gameObject.transform.GetChild(0).transform.position.x, col.gameObject.transform.GetChild(0).transform.position.y, col.gameObject.transform.GetChild(0).transform.position.z};
            }
                


            GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_SUCCESS);

            

            //Debug.Log(col.gameObject.name);

            /*if (randomEntry < 0.9f)
                GameSettings.Instance.LoadScene(SCENE.LEVEL0);
            else if (randomEntry >= 0.9f)
                GameSettings.Instance.LoadScene(SCENE.LEVEL1);*/
            /* else if (randomEntry >= 0.95f)
                 GameSettings.Instance.LoadScene(SCENE.LEVEL2);*/
        }

        if (col.tag == "Kill_Instant")
        {
            GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().TakeDamage(500f, 0f, 10000f, false, DAMAGE_TYPE.UNKNOWN);
        }

        if (col.tag == "Arm_EventTrigger" && playerHealth.sanity < 25f)
        {
            Steam.AddAchievment("HALLUCINATION");
            StartCoroutine(EnableArmsOverTime(col.transform.GetChild(0), true));
        }
        if (col.tag == "DamagePlayer")
        {
            if (playerDamageOverTime == null)
               playerDamageOverTime = StartCoroutine(HurtPlayerOverSeconds(col.gameObject.GetComponent<DamageCollider>()));
        }
        if (col.tag == "JAS_Tile")
        {
            GameSettings.Instance.worldInstance.storyTilesFoundInThisWorld[STORY_TILE.JAS] = true;
        }

    }
    IEnumerator HurtPlayerOverSeconds(DamageCollider damageLocation)
    {
        while (true)
        {
            damageLocation.Damage(playerHealth);
            yield return new WaitForSecondsRealtime(1f);
        }
    }
    IEnumerator EnableArmsOverTime(Transform top, bool io)
    {
        top.gameObject.SetActive(io);

        foreach (Transform child in top)
        {
            child.gameObject.SetActive(io);
            yield return new WaitForSecondsRealtime(0.2f);
        }
            
    }
    IEnumerator PlayFootstep(string sound)
    {

        switch (currentPlayerState)
        {
            case PLAYERSTATES.CROUCH:
                yield return new WaitForSecondsRealtime(1f);
                feetSource.volume = UnityEngine.Random.Range(0.03f, 0.07f);
                break;

            case PLAYERSTATES.WALK:
                yield return new WaitForSecondsRealtime(0.4f);
                feetSource.volume = UnityEngine.Random.Range(0.04f, 0.09f);
                break;

            case PLAYERSTATES.RUN:
                yield return new WaitForSecondsRealtime(0.28f);
                feetSource.volume = UnityEngine.Random.Range(0.09f, 0.17f);
                break;
            case PLAYERSTATES.JUMP:
                feetSource.Stop();
                break;
            case PLAYERSTATES.IDLE:
                feetSource.Stop();
                break;
            case PLAYERSTATES.IMMOBILE:
                feetSource.Stop();
                break;

        }

        
        feetSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);

        switch (sound)
        {
            case "Carpet":
                feetSource.clip = carpetFootStep;
                feetSource.Play();
                break;
            case "Cement":
                feetSource.clip =  cementFootStep;
                feetSource.Play();
                break;
            case "Wood":
                feetSource.clip = woodFootStep;
                feetSource.Play();
                break;
            case "Metal":
                feetSource.clip = metalFootStep;
                feetSource.Play();
                break;
        }
        
        playFootSteps = false;

        
    }

   
}