using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMenu : MonoBehaviour
{
    public bool menuOpen = false;
    public GameObject menuObject;

    public string menuOpenKey;
    public bool canOpen = true;
    public void OpenMenu()
    {
        if (canOpen)
        {
            menuOpen = !menuOpen;

            //GetComponent<PlayerController>().bodyAnim.SetBool(type, inventoryOpened);

            if (!menuOpen)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canMoveHead = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                menuObject.SetActive(false);

            }

            else
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canMoveHead = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuObject.SetActive(true);

            }
        }
        


    }

    void Update()
    {
        if (!GameSettings.Instance.PauseMenuOpen)
        {

            if (Input.GetButtonDown(menuOpenKey))
            {
                OpenMenu();
            }
        }
    }
}
