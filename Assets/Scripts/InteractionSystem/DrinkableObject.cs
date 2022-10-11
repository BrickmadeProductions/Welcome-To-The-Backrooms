using UnityEngine;



public class DrinkableObject : HoldableObject
{

    private readonly float maxFilledAmount = 0.2f;

    public float filledAmount = 0.2f;

    public int maxDrinks;


    public Renderer waterRenderer;

    public float thirstQuenchPerDrink;

    public override void OnLoadFinished()
    {
        waterRenderer.material.SetFloat("Vector1_411af52d3c8b49c6869ec3c5f0df3389", (float)filledAmount);
        SetMetaData("filledAmount", filledAmount.ToString());

        if (filledAmount <= 0)
            canBeUsed = false;
    }

    public override void Init()
    {
        base.Init();
        SetMetaData("filledAmount", filledAmount.ToString());
        waterRenderer.material.SetFloat("Vector1_411af52d3c8b49c6869ec3c5f0df3389", (float)filledAmount);
    }

    public void DrinkOneGulp()
    {
        if (filledAmount > 0)
        {
            GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().ChangeThirst(thirstQuenchPerDrink);

            filledAmount -= maxFilledAmount / maxDrinks;

            filledAmount = Mathf.Round(filledAmount * 100f) / 100f;

            waterRenderer.material.SetFloat("Vector1_411af52d3c8b49c6869ec3c5f0df3389", (float)filledAmount);

            SetMetaData("filledAmount", filledAmount.ToString());

            if (type == OBJECT_TYPE.ALMOND_WATER)
                StartCoroutine(GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.ChangeStaminaOverTime(50f));

            if (filledAmount <= 0)
            {
                filledAmount = 0;
                canBeUsed = false;
            }
                
        }

        
    }

    public void Refil(float amount)
    {
        filledAmount += amount;
    }


}
