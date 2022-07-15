using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placable : MonoBehaviour
{
    public bool PlaceActive;
    public HoldableObject connetedObject;
    public MeshRenderer NormalMeshRend;
    public MeshRenderer PlacingMeshRend;

    // Start is called before the first frame update
    public void IsPlacing()
    {
        NormalMeshRend.enabled = false;
        PlacingMeshRend.enabled = true;
    }
    public void Placed()
    {
        NormalMeshRend.enabled = true;
        PlacingMeshRend.enabled = false;
    }

}
