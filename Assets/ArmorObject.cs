using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorObject : HoldableObject
{
    public float armorAmount;
    public GameObject screenIcon;

    public override void Pickup(InteractionSystem player, bool RightHand)
    {
        base.Pickup(player, RightHand);
        player.GetComponent<PlayerHealthSystem>().armorReduction += armorAmount;
        screenIcon.SetActive(true);
    }

    public override void Drop(Vector3 force)
    {
        base.Drop(force);
        GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().armorReduction -= armorAmount;
        screenIcon.SetActive(false);
    }
}
