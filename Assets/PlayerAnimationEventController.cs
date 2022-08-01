using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventController : MonoBehaviour
{
    public InteractionSystem interactionSystem;

    public void ThrowHoldable_AnimEvent()
    {
        interactionSystem.SetThrow();
    }
}
