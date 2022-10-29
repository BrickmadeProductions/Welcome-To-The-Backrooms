using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public int damageAmount;
    public float sanityMultiplier;
    public int thirstAmount;
    public int hungerAmount;

    public void Damage(PlayerHealthSystem player)
    {
        player.TakeDamage(damageAmount, sanityMultiplier, 5f);
        player.ChangeThirst(-thirstAmount);
        player.ChangeHunger(-hungerAmount);
    }
    public void Damage(Entity entity)
    {
        entity.health -= damageAmount;
        entity.hunger -= hungerAmount;
    }
}
