using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSway : MonoBehaviour
{

    public float amount;
    public float maxAmount;
    public float smoothingAmount;

    private Vector3 initialPosition;


    // Start is called before the first frame update
    void Start()
    {

        initialPosition = transform.localPosition;

    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (GameSettings.Instance.Player.GetComponent<PlayerController>().currentPlayerState != PlayerController.PLAYERSTATES.IMMOBILE 
            && GameSettings.Instance.Player.GetComponent<PlayerController>().holding != null)
        {
            float movementX = -Input.GetAxis("Mouse X") / 4 * amount;
            float movementY = -Input.GetAxis("Mouse Y") * amount;
            movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
            movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);


            Vector3 finalPosition = new Vector3(movementX, movementY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothingAmount);
        }
        else
        {
            transform.localPosition = initialPosition;
        }


    }
}
