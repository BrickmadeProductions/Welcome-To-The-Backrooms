using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItemOfType : MonoBehaviour
{
    public OBJECT_TYPE typeToSpawn;
    public void SpawnItem()
    {
       
        Vector3 chunkVector = GameSettings.Instance.worldInstance.GetChunkKeyAtWorldLocation(transform.position);
        string chunkKey = chunkVector.x + "," + chunkVector.y + "," + chunkVector.z;
        GameSettings.Instance.worldInstance.loadedChunks.TryGetValue(chunkKey, out Chunk chunk);

        if (!GameSettings.Instance.worldInstance.loadedChunks.ContainsKey(chunkKey))
        {
            InteractableObject objectToSpawn;

            GameSettings.Instance.PropDatabase.TryGetValue(typeToSpawn, out objectToSpawn);

            GameSettings.Instance.worldInstance.AddNewProp(transform.position, transform.localRotation, objectToSpawn, chunk);
        }
           
    }
}
