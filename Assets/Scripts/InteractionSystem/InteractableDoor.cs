// InteractableDoor
using UnityEngine;

public class InteractableDoor : InteractableObject
{
	public override void Throw(Vector3 force)
	{
	}

    public override void Hold(InteractionSystem player, bool RightHand)
    {
    }

    public override void Use(InteractionSystem player, bool LMB)
	{
		base.gameObject.transform.GetChild(0).GetComponent<Rigidbody>().AddForce(player.transform.forward * 2f, ForceMode.Impulse);
	}


	public override void Init()
	{
	}
}
