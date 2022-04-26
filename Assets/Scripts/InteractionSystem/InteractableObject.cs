using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class InteractableObject : MonoBehaviour
{

    public bool playSounds = false;
    public Scene parentScene;

    private void Awake()
    {
        parentScene = SceneManager.GetActiveScene();
    }

    public abstract void Throw(Vector3 force);

    public abstract void Use(InteractionSystem player, bool LMB);

    public abstract void Grab(InteractionSystem player);


}
