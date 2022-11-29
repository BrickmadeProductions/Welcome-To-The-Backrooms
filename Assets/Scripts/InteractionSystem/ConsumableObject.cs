using System.Collections;
using UnityEngine;



public class ConsumableObject : HoldableObject
{

    private float maxUsesInternal;

    public float currentUsesLeft;

    public int maxUsesTotal;


    public Renderer waterRenderer;

    public bool destroyWhenFinished;

    public float thirstQuenchPerDrink;
    public float sanityQuenchPerDrink;
    public float healthQuenchPerDrink;
    public float hungerQueenchPerConsume;
    public override void OnLoadFinished()
    {
        if (waterRenderer != null)
            waterRenderer.material.SetFloat("Vector1_411af52d3c8b49c6869ec3c5f0df3389", (float)currentUsesLeft);

        SetStat("Current Uses Left", GetConsumableUsesLeft().ToString());
        SetMetaData("currentUsesLeft", currentUsesLeft.ToString());

        if (currentUsesLeft <= 0 && !destroyWhenFinished)

            canBeUsed = false;

    }
    public override void Init()
    {
        base.Init();

        maxUsesInternal = currentUsesLeft;

        SetStat("Current Uses Left", GetConsumableUsesLeft().ToString());
        SetMetaData("currentUsesLeft", currentUsesLeft.ToString());
    }

    public int GetConsumableUsesLeft()
    {
        float usesLeft = currentUsesLeft;
        int usesLeftActual = 0;

        while (usesLeft > 0)
        {
            usesLeft -= maxUsesInternal / maxUsesTotal;
            usesLeftActual++;
        }
        return usesLeftActual;

    }

    public void ConsumeOne()
    {
        if (currentUsesLeft > 0)
        {
            GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeThirst(thirstQuenchPerDrink);
            GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeSanity(sanityQuenchPerDrink);
            GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeHealth(healthQuenchPerDrink);
            GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeHunger(hungerQueenchPerConsume);

            currentUsesLeft -= maxUsesInternal / maxUsesTotal;

            currentUsesLeft = Mathf.Round(currentUsesLeft * 100f) / 100f;

            if (waterRenderer != null)
                waterRenderer.material.SetFloat("Vector1_411af52d3c8b49c6869ec3c5f0df3389", (float)currentUsesLeft);

            SetMetaData("currentUsesLeft", currentUsesLeft.ToString());

            SetStat("Current Uses Left", GetConsumableUsesLeft().ToString());

            /*if (type == OBJECT_TYPE.ALMOND_WATER)
                StartCoroutine(GameSettings.GetLocalPlayer().playerHealth.ChangeStaminaOverTime(50f));*/

            if (currentUsesLeft <= 0)
            {
                currentUsesLeft = 0;
                canBeUsed = false;
            }
                
        }
        else if (destroyWhenFinished)
        {
            canBeUsed = false;
            StartCoroutine(WaitToDestroy());
            
        }

        
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitUntil(() => !animationPlaying);
        GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().SetDrop(GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().rHand);
        GameSettings.Instance.worldInstance.RemoveProp(GetWorldID(), true);
    }

    public void Refil(float amount)
    {
        currentUsesLeft += amount;
    }


}
