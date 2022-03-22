using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectChunkPooler : MonoBehaviour
{
    public class ChunkPool
    {
        public string tag;
        public List<GameObject> chunks;
        public int size;
    }

    public List<ChunkPool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;


    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (ChunkPool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                //load all chunks in that pool (loaded chunks)
                for (int z = 0; z < pool.chunks.Count; z++)
                {
                    GameObject obj = Instantiate(pool.chunks[z]);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                
            }

            poolDictionary.Add(pool.tag, objectPool);

        }

    }

    public GameObject SpawnFromChunkPool(string tag, Vector2 position)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool With Tag" + tag + " doesnt Exist....");
            return null;
        }
        GameObject chunkToSpawn = poolDictionary[tag].Dequeue();

        chunkToSpawn.SetActive(true);
        chunkToSpawn.transform.position = new Vector3(position.x, 0, position.y);

        poolDictionary[tag].Enqueue(chunkToSpawn);

        return chunkToSpawn;
      

    }

}
