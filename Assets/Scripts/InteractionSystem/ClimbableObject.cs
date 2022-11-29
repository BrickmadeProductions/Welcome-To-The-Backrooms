// HoldableObject
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClimbableObject : HoldableObject
{
	public Transform climbSnapLocation;

	//0 is left, 1 is right, 2 is left, 3 is right etc.
	public Transform[] IKArmLocations;
	public Transform[] IKLegLocations;

	public override void Init()
	{
		base.Init();
	}

    public override void Use(InteractionSystem player, bool LMB)
    {
        
    }

}
