using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMenu : MonoBehaviour
{
    public bool menuOpen;
    public GameObject menuObject;
    public AudioSource audioObject;

    public string menuOpenKey;
    public bool canOpen = true;
    public AudioClip openSound;
    public AudioClip closeSound;

    private void Awake()
    {
        GameSettings.Instance.GameplayMenuDataBase.Add(this);
        Awake_Init();
    }
    public abstract void Awake_Init();
    public void ToggleMenu()
    {

        if (canOpen && !GameSettings.Instance.IsCutScene && !GameSettings.Instance.PauseMenuOpen && !GameSettings.isLoadingScene)
        {

            menuOpen = !menuOpen;

            //GetComponent<PlayerController>().bodyAnim.SetBool(type, inventoryOpened);

            if (!menuOpen)
            {
                if (closeSound != null)
                {
                    audioObject.clip = closeSound;
                    audioObject.Play();
                }
                    

                GameSettings.GetLocalPlayer().playerHealth.canMoveHead = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                menuObject.SetActive(false);

                GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);

            }

            else
            {
                if (closeSound != null)
                {
                    audioObject.clip = openSound;
                    audioObject.Play();
                }

                GameSettings.GetLocalPlayer().playerHealth.canMoveHead = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuObject.SetActive(true);
                GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(false);
            }
        }
        


    }
    public abstract void Update_ExtraInputs();
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

        Update_ExtraInputs();
    }
}
