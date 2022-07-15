// InteractableDoor
using UnityEngine;

public class InteractableDoor : InteractableObject
{
	public override void Throw(Vector3 force)
	{
	}

	public override void Use(InteractionSystem player, bool LMB)
	{
		base.gameObject.GetComponent<Rigidbody>().AddForce(player.transform.forward * 2f, ForceMode.Impulse);
	}

	public override void AddToInv(InteractionSystem player)
	{
	}

	public override void Init()
	{
	}

    public override void Hold(InteractionSystem player)
    {
        
    }
}
