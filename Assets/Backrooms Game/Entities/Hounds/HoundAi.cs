using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HoundAi : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject goal;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (goal == null)
        {
            goal = GameSettings.Instance.Player;
        }
        agent.destination = goal.transform.position;
    }
}
