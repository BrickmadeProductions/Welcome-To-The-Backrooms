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
        defaultPosY = camera.localPosition.y;
        defaultPosX = camera.localPosition.x;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (Cursor.lockState != CursorLockMode.None && controller.currentPlayerState != PlayerController.PLAYERSTATES.IMMOBILE && controller.playerHealth.canWalk && !GameSettings.Instance.IsCutScene && !controller.playerHealth.isBeingDamaged)
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

            camera.localRotation = new Quaternion(camera.localRotation.x, 0, Mathf.Clamp(camera.localRotation.z, Mathf.Deg2Rad * -1f, Mathf.Deg2Rad * 1f) + Mathf.Deg2Rad * strafeRotation, camera.localRotation.w);
            
            prevRotZ = camera.localRotation.z;

            //head bobbing
            if (Mathf.Abs(controller.bodyAnim.GetFloat("xWalk")) > 0.1f || Mathf.Abs(controller.bodyAnim.GetFloat("YWalk")) > 0.1f && controller.GetComponent<CharacterController>().isGrounded)
            {
                if (controller.currentPlayerState == PlayerController.PLAYERSTATES.RUN && controller.currentPlayerState != PlayerController.PLAYERSTATES.JUMP)
                {
                    //Player is moving
                    timer += Time.deltaTime * runningBobbingSpeed;
                    camera.localPosition = new Vector3(Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 2) * horizontalBobbingAmount * 2, Time.deltaTime * 10), defaultPosY + Mathf.Sin(timer) * bobbingAmount, camera.localPosition.z);

                    camera.localRotation = new Quaternion(Mathf.Lerp(prevRotX, Mathf.Sin(timer) * horizontalBobbingAmount / 4, Time.deltaTime * 10), 0, Mathf.Lerp(prevRotZ, Mathf.Sin(timer / 2) * horizontalBobbingAmount / 8, Time.deltaTime * 10), camera.localRotation.w);

                    prevPosX = camera.localPosition.x;

                    prevRotX = camera.localRotation.x;
                }

                else if (controller.currentPlayerState == PlayerController.PLAYERSTATES.CROUCH && controller.currentPlayerState != PlayerController.PLAYERSTATES.JUMP)
                {
                    timer += Time.deltaTime * walkingBobbingSpeed;
                    //camera.localPosition = new Vector3(0, camera.localPosition.y, camera.localPosition.z);
                    camera.localPosition = new Vector3(0, Mathf.Lerp(camera.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), camera.localPosition.z);
                    prevPosX = camera.localPosition.x;

                    prevRotX = camera.localPosition.x;
                }

                else
                {
                    //Player is moving
                    timer += Time.deltaTime * walkingBobbingSpeed;
                    camera.localPosition = new Vector3(Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 2) * horizontalBobbingAmount, Time.deltaTime * 10), Mathf.Lerp(camera.localPosition.y, defaultPosY + Mathf.Sin(timer) * bobbingAmount, Time.deltaTime * walkingBobbingSpeed), camera.localPosition.z);

                    camera.localRotation = new Quaternion(Mathf.Lerp(prevRotX, Mathf.Sin(timer / 2.2f) * horizontalBobbingAmount / 10, Time.deltaTime * 10), camera.localRotation.y, camera.localRotation.z, camera.localRotation.w);

                    prevPosX = camera.localPosition.x;

                    prevRotX = camera.localRotation.x;
                }

            }
            else
            {
                if (controller.playerHealth.canMoveHead)
                {
                    //Idle
                    timer += Time.deltaTime * walkingBobbingSpeed;


                    if (!Input.GetButton("LeanLeft") && !Input.GetButton("LeanRight"))
                        camera.localPosition = new Vector3(camera.localRotation.x, Mathf.Lerp(camera.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), Mathf.Lerp(prevPosX, defaultPosX + Mathf.Sin(timer / 10) * horizontalBobbingAmount, Time.deltaTime * 10));

                    prevPosX = camera.localPosition.x;

                }

            }

            
        }

        if (camera.localRotation != Quaternion.identity)
        {
            camera.localRotation = Quaternion.Lerp(camera.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
        
    }
}