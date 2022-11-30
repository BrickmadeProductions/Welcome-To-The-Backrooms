using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_AnimEvents : MonoBehaviour
{
    void Close()
    {
        GetComponentInParent<InteractableDoor>().SetOpened(false);
    }
    void Open()
    {
        GetComponentInParent<InteractableDoor>().SetOpened(true);
    }
}
