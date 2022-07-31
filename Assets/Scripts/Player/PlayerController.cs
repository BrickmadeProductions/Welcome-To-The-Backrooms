using Lowscope.Saving;
using Lowscope.Saving.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DEATH_CASE
{
    UNKNOWN,
    PLAYER,
    ENTITY,
    FALL_DAMAGE
}

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour, ISaveable
{
    public ReflectionProbe probe;
    public GameObject darkShield;
    public GameObject headTarget;

    public DEATH_CASE deathCase;

    [Serializable]
    private struct PlayerSaveData
    {
        public float[] savedPosition;

        public float[] savedHeadRotationEuler;

        public float[] savedBodyRotationEuler;

        public float savedRotationX;

        public float savedRotationY;
    }

    public string OnSave()
    {
        float[] savedPosition = new float[3]
        {
            transform.position.x,
            transform.position.y,
            transform.position.z
        };
        float[] savedHeadRotationEuler = new float[3]
        {
            neck.transform.rotation.eulerAngles.x,
            neck.transform.rotation.eulerAngles.y,
            neck.transform.rotation.eulerAngles.z
        };
        float[] savedBodyRotationEuler = new float[3]
        {
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            transform.rotation.eulerAngles.z
        };
        PlayerSaveData playerSaveData = new PlayerSaveData()
        {
            savedPosition = savedPosition,
            savedHeadRotationEuler = savedHeadRotationEuler,
            savedBodyRotationEuler = savedBodyRotationEuler,
            savedRotationX = rotationX,
            savedRotationY = rotationY
        };
        

        return JsonUtility.ToJson(playerSaveData);
    }

    public void OnLoad(string data)
    {
        StartCoroutine(OnLoadAsync(data));
    }

    public void OnLoadNoData()
    {
        if (GameSettings.Instance.AmInSavableScene())
        {
            transform.position = new Vector3(0f, 1.25f, 0f);
        }
        StartCoroutine(OnLoadNoDataAsync());
    }

    private IEnumerator OnLoadNoDataAsync()
    {
        Debug.Log("Player Data Doesnt Exist");

        if (GameSettings.Instance.worldInstance != null)
        {
        
            yield return new WaitUntil(() => GameSettings.Instance.AmInSavableScene());

            characterController.enabled = false;

            yield return new WaitUntil(() => GameSettings.LEVEL_LOADED);

            GameSettings.PLAYER_DATA_LOADED_IN_SCENE = true;

            yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
            yield return new WaitUntil(() => GameSettings.LEVEL_GENERATED);
        
            
        }
        characterController.enabled = true;
        Debug.Log("Player Data Finished Loading");
    }

    private IEnumerator OnLoadAsync(string data)
    {

        if (GameSettings.Instance.worldInstance != null)
        {
            Debug.Log("Player Data Loading");

            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(data);

            yield return new WaitUntil(() => GameSettings.Instance.AmInSavableScene());

            characterController.enabled = false;

            yield return new WaitUntil(() => GameSettings.LEVEL_LOADED);

            transform.position = new Vector3(saveData.savedPosition[0], saveData.savedPosition[1], saveData.savedPosition[2]);
            transform.rotation = Quaternion.Euler(saveData.savedBodyRotationEuler[0], saveData.savedBodyRotationEuler[1], saveData.savedBodyRotationEuler[2]);
            neck.transform.rotation = Quaternion.Euler(saveData.savedHeadRotationEuler[0], saveData.savedHeadRotationEuler[1], saveData.savedHeadRotationEuler[2]);
            rotationX = saveData.savedRotationX;
            rotationY = saveData.savedRotationY;

            GameSettings.PLAYER_DATA_LOADED_IN_SCENE = true;

            yield return new WaitUntil(() => GameSettings.LEVEL_GENERATED);
            yield return new WaitUntil(() => GameSettings.LEVEL_SAVE_LOADED);
        }
        characterController.enabled = true;
        Debug.Log("Player Data Finished Loading");
    }

    public bool OnSaveCondition()
    {
        
        return GameSettings.Instance.AmInSavableScene();

    }

    public GameObject playerRagDoll;

    public PlayerHealthSystem playerHealth;
    public DistanceChecker distance;


    bool headReset = false;
    //movement
    float walkingSpeed = 3f;
    float runningSpeed = 6f;
    float crouchingSpeed = 0.5f;
    float jumpSpeed = 6.0f;

    public float adrenalineSpeedMultiplier;
    float gravity = 20.0f;

    Coroutine run = null;
    Coroutine walk = null;
    Coroutine reviveHeartRate = null;
    Coroutine footStep = null;

    Coroutine rebuildStamina = null;
    Coroutine removeStamina = null;

    public GameObject handLocation;
    public GameObject holdLocation;
    public HoldableObject holding;

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

    //breathing
    public AudioClip breathingNormal;
    public AudioClip breathingRunning;

    //footstep noises
    public AudioClip carpetFootStep;
    public AudioClip cementFootStep;
    public AudioClip woodFootStep;
    public AudioClip metalFootStep;

    //swing noises
    public AudioClip[] swingSounds;

    //fov editingi
    private float _FOVOFFSET;
    
    public bool dead = false;

    //audioSources

    public AudioSource playerNoises;
    public AudioSource feetSource;

    bool playFootSteps = true;

    //postEdits

    //head movement
    public float lookXLimitTop = 85.0f;
    public float lookXLimitBottom = 90.0f;
    public float rotationX = 0.0f;
    public float rotationY = 0.0f;
    float height = 3.35f;
    float center = 0.65f;

    //0 = walk, 1 = run, 2 = crouch, 3 = jumping
    public enum PLAYERSTATES : int
    {
        IDLE = 0,
        WALK = 1,
        CROUCH = 2,
        RUN = 3,
        JUMP = 4,
        PRONE = 5,
        IMMOBILE = 6
    }

    public PLAYERSTATES currentPlayerState = PLAYERSTATES.IDLE;

    //animation
    public Animator bodyAnim;
    public GameObject headAnimLocation;

    //playerMovement
    CharacterController characterController;
    public Vector3 moveDirection;
    public Vector3 velocity = Vector3.zero;
   
    

    //ui
    public Canvas UI;


    public float pushMagnitude;

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {

        Rigidbody rigidbody = collision.collider.attachedRigidbody;

        if (rigidbody != null)
        {
            Vector3 forceDirection = collision.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();

            //rigidbody.AddForceAtPosition(forceDirection * pushMagnitude, transform.position, ForceMode.Impulse);
        }
    }

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealthSystem>();
    }


    void Start()
    {
        ogHeadTrans = neck.transform;


        characterController = GetComponent<CharacterController>();

        DontDestroyOnLoad(gameObject);
        Saveable component = gameObject.AddComponent<Saveable>();
        component.SaveIdentification = GameSettings.Instance.activeUser;
        component.AddSaveableComponent("PlayerData", this, true);

        SaveMaster.AddListener(component);
        SaveMaster.SyncLoad();

        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener foundListener in audioListeners)
        {
            Debug.Log(foundListener.name);
        }
        
    }
    public void Crouch()
    {
        //head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, ogHeadTrans.localPosition - new Vector3(0, 15, 0), Time.deltaTime);
    }

    public void UnCrouch()
    {
        //head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, ogHeadTrans.localPosition, Time.deltaTime);

        //head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, ogHeadTrans.localRotation, Time.deltaTime);

    }
    private void LateUpdate()
    {
        if (characterController.isGrounded && !bodyAnim.GetBool("hitGround"))
        {
            bodyAnim.SetBool("hitGround", true);
        }
        else
        {
            bodyAnim.SetBool("hitGround", false);
        }

        if (!dead && Cursor.lockState != CursorLockMode.None && playerHealth.canMoveHead)
        {

            rotationX += -Input.GetAxis("Mouse Y") * GameSettings.Instance.Sensitivity / 2;
            rotationX = Mathf.Clamp(rotationX, -lookXLimitBottom, lookXLimitTop);

            rotationX = Mathf.Clamp(rotationX, -lookXLimitBottom + 4, lookXLimitTop - 4);

            rotationY = Input.GetAxis("Mouse X") * GameSettings.Instance.Sensitivity;

            if (holding != null)
            {
                neck.transform.rotation = Quaternion.Euler(neck.transform.rotation.eulerAngles.x, neck.transform.rotation.eulerAngles.y, bodyAnim.GetBool("isProne") ? rotationX - 90 : rotationX - 90);
                head.transform.rotation = Quaternion.Euler(bodyAnim.GetBool("isProne") ? rotationX : rotationX, head.transform.rotation.eulerAngles.y, 0);

                headReset = true;
            }
            else if (headReset)
            {
                head.transform.rotation = Quaternion.Euler(90, head.transform.rotation.eulerAngles.y, 0);

                headReset = false;
            }
            else
            {
                head.transform.rotation = Quaternion.Euler(rotationX, head.transform.rotation.eulerAngles.y, 0);

            }

            head.transform.position = Vector3.Lerp(head.transform.position, headTarget.transform.position, 75f * Time.deltaTime);
            //head.transform.rotation = Quaternion.Lerp(head.transform.rotation, headTarget.transform.rotation, 10f * Time.deltaTime);
        }

        if (!dead && !GameSettings.Instance.PauseMenuOpen)
        {
            //animations
            if (currentPlayerState != PLAYERSTATES.IMMOBILE && characterController.isGrounded)
            {

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                    bodyAnim.SetTrigger("isJumping");
                }

                    

                if (Input.GetButton("Crouch") && playerHealth.canCrouch)
                {

                    Crouch();
                }
                else if (playerHealth.canCrouch)
                {
                    UnCrouch();                    
                }
                else
                {
                    neck.transform.localPosition = ogHeadTrans.localPosition;

                    neck.transform.localRotation = ogHeadTrans.localRotation;
                }


            }
        }


    }


    

    void Update()
    {
        //Debug.Log(headTarget.transform.position);
        
        /*if (holding != null)
        Debug.Log(holding.animationPlaying);*/

        if (!dead && playerHealth.health <= 0.0f)
        {
            StartCoroutine(Die(deathCase));

        }

        // Player and Camera rotation
        if (!dead && Cursor.lockState != CursorLockMode.None && playerHealth.canMoveHead)
        {
            
            if (currentPlayerState != PLAYERSTATES.IMMOBILE)

                SpeedAndFovController();

            transform.rotation *= Quaternion.Euler(0, rotationY, 0);
            //player movement

            PlayerLoop();

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            // Press Left Shift to run


            float curSpeedX = currentPlayerState != PLAYERSTATES.IMMOBILE || holding.animationPlaying ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") * adrenalineSpeedMultiplier : 0;
            float curSpeedY = currentPlayerState != PLAYERSTATES.IMMOBILE || holding.animationPlaying ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") * adrenalineSpeedMultiplier : 0;

            bodyAnim.SetFloat("xWalk", curSpeedX / 2);
            bodyAnim.SetFloat("yWalk", curSpeedY / 2);

            if (currentPlayerState == PLAYERSTATES.CROUCH)
            {
                curSpeedX *= crouchingSpeed;
                curSpeedY *= crouchingSpeed;
            }
            float movementDirectionY = moveDirection.y;

            _FOVOFFSET = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? Mathf.Lerp(_FOVOFFSET, 10f, 10f * Time.deltaTime) : Mathf.Lerp(_FOVOFFSET, 0f, 10f * Time.deltaTime)) : 0;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
            moveDirection.y = movementDirectionY;

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            if (!characterController.isGrounded && GameSettings.LEVEL_LOADED)
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);

        }

    }

    private void HandleFootstep()
    {
        //footstep
        RaycastHit hit;

        if (Physics.Raycast(new Ray(feet.transform.position, Vector3.down), out hit, 3f))

            if (hit.collider.gameObject.tag != "Player" && characterController.isGrounded && !playFootSteps && characterController.velocity.magnitude > 0.005f)
            {

                playFootSteps = true;
                footStep = StartCoroutine(PlayFootstep(hit.collider.tag));

            }

            if (!characterController.isGrounded && playFootSteps)
            {

                playFootSteps = false;

                if (footStep != null)
                    StopCoroutine(footStep);
            }

            //Debug.Log(hit.collider.gameObject.tag);

            

    }

    private void HandlePlayerStates()
    {
        if (!characterController.isGrounded)
            currentPlayerState = PLAYERSTATES.JUMP;

        //all but prone
        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {

            if ((Input.GetButton("W") || Input.GetButton("A") || Input.GetButton("D")) && Input.GetButton("Run") && playerHealth.canRun && !bodyAnim.GetBool("Prone"))
            {
                bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 0, Time.deltaTime * 10));
                bodyAnim.SetBool("isCrouching", false);
                bodyAnim.SetBool("isWalking", false);
                bodyAnim.SetBool("isRunning", true);
                bodyAnim.SetBool("isIdle", false);

                currentPlayerState = PLAYERSTATES.RUN;

            }

            else if ((Input.GetButton("W") || Input.GetButton("A") || Input.GetButton("S") || Input.GetButton("D")) && playerHealth.canWalk)
            {
                bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 1, Time.deltaTime * 10));
                bodyAnim.SetBool("isWalking", true);
                bodyAnim.SetBool("isRunning", false);
                bodyAnim.SetBool("isIdle", false);

                currentPlayerState = PLAYERSTATES.WALK;
            

            }

            if (Input.GetButton("Crouch") && playerHealth.canWalk && currentPlayerState != PLAYERSTATES.RUN)
            {
                bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 0, Time.deltaTime * 10));
                bodyAnim.SetBool("isCrouching", true);
                bodyAnim.SetBool("isWalking", false);
                bodyAnim.SetBool("isRunning", false);
                bodyAnim.SetBool("isIdle", false);

                currentPlayerState = PLAYERSTATES.CROUCH;
                
            }


        }

        else if (Input.GetButton("Crouch") && currentPlayerState != PLAYERSTATES.JUMP)
        {

            bodyAnim.SetBool("isCrouching", true);
            bodyAnim.SetBool("isRunning", false);

            currentPlayerState = PLAYERSTATES.CROUCH;
       
        }

        //idle
        else
        {
            
            bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 1, Time.deltaTime * 10));
            bodyAnim.SetBool("isCrouching", false);
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);
            
            
 
            currentPlayerState = PLAYERSTATES.IDLE;

        }

        if (Input.GetButtonDown("Prone") && currentPlayerState != PLAYERSTATES.JUMP)
        {
            if (!bodyAnim.GetBool("isProne"))
            {
                bodyAnim.SetLayerWeight(1, Mathf.Lerp(bodyAnim.GetLayerWeight(1), 0, Time.deltaTime * 10));
                bodyAnim.SetBool("isProne", true);
                bodyAnim.SetBool("isCrouching", false);
                bodyAnim.SetBool("isWalking", false);
                bodyAnim.SetBool("isRunning", false);
            }
            else
            {
                bodyAnim.SetBool("isProne", false);
            }

        }

        if (Input.GetButton("Watch"))
        {
            bodyAnim.SetBool("Watch", true);

        }
        if (Input.GetButtonUp("Watch"))
        {
            bodyAnim.SetBool("Watch", false);
        }

        //controll player stamina volume indicator
        playerNoises.volume = 0.25f + ((100 - playerHealth.stamina) / 100) / 2;

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

                if (run != null)
                
                    StopCoroutine(run);

                run = null;

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

                    removeStamina = StartCoroutine(playerHealth.ChangeStamina(UnityEngine.Random.Range(-1, -2)));

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

          
                break;

        }

        if (currentPlayerState != PLAYERSTATES.CROUCH)
        {
            //GetComponent<CharacterController>().height = height;
            //GetComponent<CharacterController>().center = new Vector3(0, center, 0);
        }
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
                    rebuildStamina = StartCoroutine(playerHealth.ChangeStamina(UnityEngine.Random.Range(1, 3)));

                if (removeStamina != null)

                    StopCoroutine(removeStamina);





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

    private void PlayerLoop()
    {
        if (!GameSettings.Instance.cheatSheet.noClip)
        {
            if (playerHealth.canWalk)

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


    public IEnumerator Die(DEATH_CASE deathCase)
    {
        if (deathCase == DEATH_CASE.ENTITY)
        {
            Steam.AddAchievment("MEAT_CRAYON");
        }

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
            yield return new WaitForSeconds(0.25f);
        }

        GetComponent<Blinking>().eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
        yield return new WaitForSeconds(4f);
        GameSettings.Instance.ResetGame();
        GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", 20000f);
        
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Enter0")
        {
            GameSettings.Instance.LoadScene(GameSettings.SCENE.LEVEL0);
        }
        if (col.tag == "Enter1")
        {
            GameSettings.Instance.LoadScene(GameSettings.SCENE.LEVEL1);
        }
        
    }
    IEnumerator PlayFootstep(string sound)
    {

        switch (currentPlayerState)
        {
            case PLAYERSTATES.CROUCH:
                yield return new WaitForSeconds(1f);
                feetSource.volume = UnityEngine.Random.Range(0.04f, 0.06f);
                break;

            case PLAYERSTATES.WALK:
                yield return new WaitForSeconds(0.4f);
                feetSource.volume = UnityEngine.Random.Range(0.06f, 0.11f);
                break;

            case PLAYERSTATES.RUN:
                yield return new WaitForSeconds(0.28f);
                feetSource.volume = UnityEngine.Random.Range(0.11f, 0.24f);
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
                feetSource.PlayOneShot(carpetFootStep);
                break;
            case "Cement":
                feetSource.PlayOneShot(cementFootStep);
                break;
            case "Wood":
                feetSource.PlayOneShot(woodFootStep);
                break;
            case "Metal":
                feetSource.PlayOneShot(metalFootStep);
                break;
        }
        
        playFootSteps = false;

        
    }
    void OnDestroy()
    {
        SaveMaster.RemoveListener(GetComponent<Saveable>());
    }
    
}