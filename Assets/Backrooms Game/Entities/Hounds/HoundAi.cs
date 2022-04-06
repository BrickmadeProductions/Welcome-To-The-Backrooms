using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HoundAi : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject player;
    public GameObject hitbox;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameSettings.Instance.Player;
        }
        agent.destination = player.transform.position;

        if (hitbox)
        {

        }
    }

}
