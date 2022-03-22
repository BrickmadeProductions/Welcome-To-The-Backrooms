using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public bool loaded;
    public int id;
    public float posX;
    public float posY;

    public static Chunk CreateComponent(GameObject where, int id, float posX, float posY)
    {
           
        Chunk chunk = where.AddComponent<Chunk>(); 
        chunk.loaded = true;
        chunk.id = id;
        chunk.posX = posX;
        chunk.posY = posY;
        return chunk;
    }

    internal void UnLoad()
    {
 
        loaded = false;

        gameObject.SetActive(false);

    }

    internal void Delete()
    {
        Destroy(gameObject);
    }

    internal void Load()
    {

        loaded = true;
        //Destroy(gameObject);

        //gameObject.SetActive(true);

    }
}
