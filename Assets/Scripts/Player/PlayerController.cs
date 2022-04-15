using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{

    private static PlayerController playerInstance;

    public PlayerHealthSystem playerHealth;
    public DistanceChecker distance;

    //movement
    float walkingSpeed = 3f;
    float runningSpeed = 6f;
    float crouchingSpeed = 0.5f;
    float jumpSpeed = 6.0f;
    float gravity = 20.0f;

    public bool grabbed = false;

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

    //player parts
    public Camera playerCamera;
    public Camera animatorCamera;

    public GameObject head;
    public GameObject neck;
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


    //fov editingi
    private float _FOVOFFSET;
    
    public bool dead = false;

    //audioSources

    public AudioSource playerNoises;
    public AudioSource feetSource;

    bool playFootSteps = true;

    //postEdits

    //head movement
   
    float lookXLimitTop = 85.0f;
    float lookXLimitBottom = 90.0f;
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
        WATCH = 5,
        IMMOBILE = 6
    }

    public PLAYERSTATES currentPlayerState = PLAYERSTATES.IDLE;

    //animation
    public Animator bodyAnim;

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
            DontDestroyOnLoad(gameObject);

            if (playerInstance == null)
            {
                playerInstance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            //UI.transform.Find("PlayerID").GetComponent<Text>().text = "PlayerID: " + CreatePlayerData.playerID;
            //UI.transform.Find("PlayerName").GetComponent<Text>().text = "PlayerName: " + CreatePlayerData.playerName;
            //GetComponent<AudioSource>().playOnAwake = true;
        }

    void Start()
    {
       
        

        playerHealth = GetComponent<PlayerHealthSystem>();

        

        ogHeadTrans = head.transform;


        characterController = GetComponent<CharacterController>();


    }

    public void Crouch()
    {
        head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, ogHeadTrans.localPosition - new Vector3(0, 15, 0), Time.deltaTime);
    }

    public void UnCrouch()
    {
        head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, ogHeadTrans.localPosition, Time.deltaTime);

        head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, ogHeadTrans.localRotation, Time.deltaTime);

    }
    private void LateUpdate()
    {
        if (!dead && !GameSettings.Instance.PauseMenuOpen)
        {

            head.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);

            //animations
            if (currentPlayerState != PLAYERSTATES.IMMOBILE && characterController.isGrounded)
            {

                if (Input.GetButton("Jump"))

                    moveDirection.y = jumpSpeed;

                if (Input.GetButton("Crouch"))
                {

                    Crouch();
                }
                else
                {
                    UnCrouch();                    
                }


            }
        }


    }


    private void FixedUpdate()
    {
  

    }

    

    void Update()
    {
        if (playerHealth.health <= 0.0f || playerHealth.sanity <= 0.0f)
        {
            die();
        }

        // Player and Camera rotation
        if (!dead && Cursor.lockState != CursorLockMode.None && playerHealth.canMoveHead)
        {
            
            if (currentPlayerState != PLAYERSTATES.IMMOBILE)

                SpeedAndFovController();


            rotationX += -Input.GetAxis("Mouse Y") * GameSettings.Instance.Sensitivity / 2;
            rotationX = Mathf.Clamp(rotationX, -lookXLimitBottom, lookXLimitTop);

            rotationY += Input.GetAxis("Mouse X") * GameSettings.Instance.Sensitivity;

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * GameSettings.Instance.Sensitivity, 0);

            //player movement

            PlayerMovement();

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            // Press Left Shift to run


            float curSpeedX = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
            float curSpeedY = currentPlayerState != PLAYERSTATES.IMMOBILE ? (currentPlayerState == PLAYERSTATES.RUN ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
                
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
            if (!characterController.isGrounded && !grabbed)
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

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {

            if ((Input.GetButton("W") || Input.GetButton("A") || Input.GetButton("D")) && Input.GetButton("Run") && playerHealth.canRun)
            {
                bodyAnim.SetBool("isCrouching", false);
                bodyAnim.SetBool("isWalking", false);
                bodyAnim.SetBool("isRunning", true);
                bodyAnim.SetBool("isIdle", false);

                currentPlayerState = PLAYERSTATES.RUN;

            }

            else if ((Input.GetButton("W") || Input.GetButton("A") || Input.GetButton("S") || Input.GetButton("D")) && playerHealth.canWalk)
            {

                bodyAnim.SetBool("isWalking", true);
                bodyAnim.SetBool("isRunning", false);
                bodyAnim.SetBool("isIdle", false);

                currentPlayerState = PLAYERSTATES.WALK;
            

            }

            if (Input.GetButton("Crouch") && playerHealth.canWalk && currentPlayerState != PLAYERSTATES.RUN)
            {
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
            bodyAnim.SetBool("isCrouching", false);
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);
            bodyAnim.SetBool("Watch", false);
            bodyAnim.SetBool("isIdle", true);
            

            currentPlayerState = PLAYERSTATES.IDLE;

        }

        if (Input.GetButton("Watch"))
        {
            bodyAnim.SetBool("Watch", true);

        }
        if (Input.GetButtonUp("Watch"))
        {
            bodyAnim.SetBool("Watch", false);
        }

        if (grabbed)
        {
            currentPlayerState = PLAYERSTATES.IMMOBILE;
        }

        //controll player stamina volume indicator
        playerNoises.volume = 0.2f + ((100 - playerHealth.stamina) / 100) / 2;

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

                GetComponent<CharacterController>().height = 2.5f;
                GetComponent<CharacterController>().center = new Vector3(0, 0.2f, 0);

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
            GetComponent<CharacterController>().height = height;
            GetComponent<CharacterController>().center = new Vector3(0, center, 0);
        }
    }

    private void HandleHealthSystem()
    {
        //heart system
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
        if (playerHealth.heartRate >= 140)
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

    private void PlayerMovement()
    {
        HandleFootstep();
        HandlePlayerStates();
        HandleHealthSystem();
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
   

    public void die()
    {
        if (!dead)
        {

            bodyAnim.SetBool("isCrouching", false);
            bodyAnim.SetBool("isProning", false);
            bodyAnim.SetBool("isReloading", false);
            bodyAnim.SetBool("isAiming", false);
            bodyAnim.SetBool("isRunning", false);
            bodyAnim.SetBool("isWalking", false);
            bodyAnim.SetBool("isSwimming", false);

            dead = true;

            GameSettings.Instance.LoadScene("IntroSequence");

        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Enter1")
        {
            GameSettings.Instance.LoadScene("Level 1");
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
                yield return new WaitForSeconds(0.5f);
                feetSource.volume = UnityEngine.Random.Range(0.06f, 0.11f);
                break;

            case PLAYERSTATES.RUN:
                yield return new WaitForSeconds(0.3f);
                feetSource.volume = UnityEngine.Random.Range(0.11f, 0.24f);
                break;
            case PLAYERSTATES.JUMP:
                feetSource.Stop();
                break;
            case PLAYERSTATES.IDLE:
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
}