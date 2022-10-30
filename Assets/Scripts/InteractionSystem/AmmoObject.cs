using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoObject : HoldableObject
{
    public int maxAmount;

    public int amountLeft;
    
    public bool shouldDestroyWhenLoaded;

    public string ammo_StatName;

    public override void OnLoadFinished()
    {
        SetStat(ammo_StatName, amountLeft.ToString());

        SetMetaData("amountLeft", amountLeft.ToString());
    }
    public override void Init()
    {
        base.Init();

        SetStat(ammo_StatName, amountLeft.ToString());
    }

    public void RemoveAmount(int amount)
    {
        amountLeft -= amount;

        SetStat(ammo_StatName, amountLeft.ToString());

        SetMetaData("amountLeft", amountLeft.ToString());
    }
}
