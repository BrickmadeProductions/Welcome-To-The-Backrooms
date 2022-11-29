using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    private void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * 20f);
    }
}
