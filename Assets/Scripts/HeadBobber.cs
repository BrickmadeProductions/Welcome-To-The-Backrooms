using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class HeadBobber : MonoBehaviour
{
    public float walkingBobbingSpeed = 14f;
    public float runningBobbingSpeed = 18f;
    public float bobbingAmount = 0.05f;
    public float horizontalBobbingAmount = 0.1f;
    public float rotationAmount = 0.5f;
    public Transform camera;
    public PlayerController controller;

    float defaultPosY;
    float defaultPosX;
    float prevPosX;

    float prevRotX;
    float prevRotZ;
    float timer = 0;


    // Start is called before the first frame update
    void Start()
    {
        defaultPosY = transform.localPosition.y;
        defaultPosX = transform.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {

        if (Cursor.lockState != CursorLockMode.None && controller.currentPlayerState != PlayerController.PLAYERSTATES.IMMOBILE && controller.playerHealth.canWalk && controller.playerHealth.canMoveHead && !controller.bodyAnim.GetBool("Watch"))
        {
            
            //head rotation
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f)
            {

                camera.localRotation = new Quaternion(camera.localRotation.x, 0, Mathf.Lerp(prevRotZ, - Input.GetAxis("Mouse X"), Time.deltaTime / 10), camera.localRotation.w);
            }

            float strafeRotation = 0f;

            if (Input.GetButton("A"))
                strafeRotation = Mathf.Lerp(prevRotZ, 2f, Time.deltaTime * 5);
            if (Input.GetButton("D"))
                strafeRotation = Mathf.Lerp(prevRotZ, -2f, Time.deltaTime * 5);

            camera.localRotation = new Quaternion(camera.localRotation.x, 0, Mathf.Lerp(camera.localRotation.z, 0, Time.deltaTime), camera.localRotation.w);

            camera.localRotation = new Quaternion(camera.localRotation.x, 0, Mathf.Clamp(camera.localRotation.z, Mathf.Deg2Rad * -1f, Mathf.Deg2Rad * 1f) + Mathf.Deg2Rad * strafeRotation, camera.localRotation.w);

            prevRotZ = camera.localRotation.z;
            

            //head bobbing
            if (Mathf.Abs(controller.moveDirection.x) > 0.1f || Mathf.Abs(controller.moveDirection.z) > 0.1f && controller.GetComponent<CharacterController>().isGrounded)
            {
                if (controller.currentPlayerState == PlayerController.PLAYERSTATES.RUN && controller.currentPlayerState != PlayerController.PLAYERSTATES.JUMP)
                {
                    //Player is moving
                    timer += Time.deltaTime * runningBobbingSpeed;
                    transform.localPosition = new Vector3(Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 2) * horizontalBobbingAmount * 2, Time.deltaTime * 10), defaultPosY + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);

                    camera.localRotation = new Quaternion(Mathf.Lerp(prevRotX, Mathf.Sin(timer) * horizontalBobbingAmount / 4, Time.deltaTime * 10), 0, Mathf.Lerp(prevRotZ, Mathf.Sin(timer / 2) * horizontalBobbingAmount / 8, Time.deltaTime * 10), camera.localRotation.w);

                    prevPosX = transform.localPosition.x;

                    prevRotX = camera.localRotation.x;
                }

                else if (controller.currentPlayerState == PlayerController.PLAYERSTATES.CROUCH && controller.currentPlayerState != PlayerController.PLAYERSTATES.JUMP)
                {
                    timer += Time.deltaTime * walkingBobbingSpeed;
                    //transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
                    transform.localPosition = new Vector3(0, Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);
                    prevPosX = transform.localPosition.x;
                }

                else
                {
                    //Player is moving
                    timer += Time.deltaTime * walkingBobbingSpeed;
                    transform.localPosition = new Vector3(Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 2) * horizontalBobbingAmount, Time.deltaTime * 10), Mathf.Lerp(transform.localPosition.y, defaultPosY + Mathf.Sin(timer) * bobbingAmount, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);

                    camera.localRotation = new Quaternion(Mathf.Lerp(prevRotX, Mathf.Sin(timer / 2.2f) * horizontalBobbingAmount / 10, Time.deltaTime * 10), camera.localRotation.y, camera.localRotation.z, camera.localRotation.w);

                    prevPosX = transform.localPosition.x;

                    prevRotX = camera.localRotation.x;
                }

            }
            else
            {
                if (controller.playerHealth.canMoveHead && !controller.bodyAnim.GetBool("Watch"))
                {
                    //Idle
                    timer += Time.deltaTime * walkingBobbingSpeed;


                    if (!Input.GetButton("LeanLeft") && !Input.GetButton("LeanRight"))
                        transform.localPosition = new Vector3(Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 10) * horizontalBobbingAmount, Time.deltaTime * 10), Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);

                    prevPosX = transform.localPosition.x;

                }

            }
        }
       
    }
}