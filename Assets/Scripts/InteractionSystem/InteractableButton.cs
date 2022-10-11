// InteractableButton
using UnityEngine;

public class InteractableButton : InteractableObject
{
	public bool pressed;

	private bool justPressed;

	public override void Pickup(InteractionSystem player, bool RightHand)
	{
	}

	public override void Drop(Vector3 force)
	{
	}

	public override void Use(InteractionSystem player, bool LMB)
	{
		justPressed = true;
		pressed = true;
	}

	public void Update()
	{
		if (justPressed)
		{
			pressed = false;
			justPressed = false;
		}
	}

	public override void Init()
	{
	}

    public override void OnSaveFinished()
    {
        
    }

    public override void OnLoadFinished()
    {
        
    }
}
