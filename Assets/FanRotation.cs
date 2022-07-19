using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanRotation : MonoBehaviour
{
    // Interpolates rotation between the rotations
    // of from and to.
    // (Choose from and to not to be the same as
    // the object you attach this script to)

    public float rotation;
    void Update()
    {
        if (rotation <= 200f)
            rotation += 0.05f;

        transform.Rotate(0, rotation * Time.deltaTime, 0);
    }
}