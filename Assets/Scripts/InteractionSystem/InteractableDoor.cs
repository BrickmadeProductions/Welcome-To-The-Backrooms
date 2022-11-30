// InteractableDoor
using UnityEngine;

public class InteractableDoor : InteractableObject
{
    bool opened = false;
    bool isAnimating = false;

    public void SetOpened(bool io)
    {
        opened = io;
        isAnimating = false;
    }

	public override void Drop(Vector3 force)
	{
	}

    public override void Pickup(InteractionSystem player, bool RightHand)
    {
    }

    public override void Use(InteractionSystem player, bool LMB)
	{
        if (!isAnimating)
        {
            isAnimating = true;
            gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger(opened ? "Close" : "Open");
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
