using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorSpawner : MonoBehaviour
{
    public Elevator elevator;
    public GameObject spawnLocation;
    public Chunk parentChunk;

    private void Awake()
    {
        parentChunk = spawnLocation.transform.parent.parent.parent.GetComponent<Chunk>();

        if (parentChunk.chunkPosY == 0)
        {
            GameObject spawned = Instantiate(elevator.gameObject);
            spawned.transform.position = spawnLocation.transform.position;
        }
        
    }
}
