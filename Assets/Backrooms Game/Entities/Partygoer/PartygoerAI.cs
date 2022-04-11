using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : MonoBehaviour
{
    // The target marker.
    private Transform target;

    // Angular speed in radians per sec.
    public float speed = 1.0f;

    private IEnumerator attackFunc()
    {
        while (true)
        {

            target = GameSettings.Instance.Player.transform;

            Vector3 targetDirection = target.transform.position - transform.position;
            float singleStep = speed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

            yield return new WaitForSeconds(2);
        }
    }

    void Start()
    {
        StartCoroutine(attackFunc());
    }
}
