using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMenu : MonoBehaviour
{
    public bool menuOpen = false;
    public GameObject menuObject;

    public string menuOpenKey;
    public bool canOpen = true;
    private void Awake()
    {
        GameSettings.Instance.GameplayMenuDataBase.Add(this);
        Awake_Init();
    }
    public abstract void Awake_Init();
    public void ToggleMenu()
    {

        if (canOpen && !GameSettings.Instance.IsCutScene && !GameSettings.Instance.PauseMenuOpen)
        {


            menuOpen = !menuOpen;

            //GetComponent<PlayerController>().bodyAnim.SetBool(type, inventoryOpened);

            if (!menuOpen)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canMoveHead = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                menuObject.SetActive(false);

                GameSettings.Instance.Player.GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);

            }

            else
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canMoveHead = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuObject.SetActive(true);
                GameSettings.Instance.Player.GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(false);
            }
        }
        


    }

    void Update()
    {
        if (!GameSettings.Instance.PauseMenuOpen)
        {

            if (Input.GetButtonDown(menuOpenKey))
            {
                foreach (GenericMenu menu in GameSettings.Instance.GameplayMenuDataBase)
                {
                    //toggle off the old menu
                    if (menu.menuOpen && menu != this)
                    {
                        menu.ToggleMenu();
                    }
                }

                ToggleMenu();
                
            }
        }
    }
}
