using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class InfLevel1Gen : MonoBehaviour
{
    public bool gen_enabled;

    public List<Entity> entities;
    public Dictionary<Vector3, Entity> entitiesInScene;

    [HideInInspector]
    public bool isLoadingChunks = false;

    [HideInInspector]
    public int currentRoomNumber;

    [HideInInspector]
    public int currentChunkNumber;

    [HideInInspector]
    public Vector2 oldPlayerChunkLocation = Vector2.zero;

    public GameObject level_chunkGenerator;

    public int chunk_width;
    public int chunk_height;

    //chunks can be dynamically added and removed therefore there is no point in making it a 2d array
    public List<NoiseGenMap> chunks;

    [Range(1.0f, 5.0f)]
    //in chunks
    public int viewDistance;

    void Awake()
    {
        ScriptInit();
    }

    void Update()
    {
        UpdateChunks();
    }


    //entity spawning logic
    IEnumerator trySpawnEntites()
    {
        while (true)
        {

            if (UnityEngine.Random.value >= 0.98f && entities != null)
                Instantiate(entities[0], GameSettings.Instance.Player.transform.position + new Vector3(UnityEngine.Random.Range(-50, 50) + UnityEngine.Random.Range(0, 15), 3, UnityEngine.Random.Range(-50, 50) + UnityEngine.Random.Range(0, 15)), Quaternion.identity);

            yield return new WaitForSeconds(0.5f);
            Debug.Log("Attempting to spawn entity");
        }

    }
    public void ScriptInit()
    {


        if (gen_enabled)
        {

            chunks = new List<NoiseGenMap>();

            currentRoomNumber = 0;
            currentChunkNumber = 0;

            StartCoroutine(Init());
        }


    }
    IEnumerator Init()
    {
        Debug.Log("Waiting For Level To Load");

        yield return new WaitUntil(() => GameSettings.LEVEL_LOADED == true);

        init();
    }

    void init()
    {
        //StartCoroutine(trySpawnEntites());

        Debug.Log("Init LEVEL_GEN");

        for (int x = -viewDistance; x < viewDistance; x++)
        {
            for (int z = -viewDistance; z < viewDistance; z++)
            {
                if (GenerateChunk(x, z, currentChunkNumber))
                    currentChunkNumber++;
            }
        }

    }


    public void manageChunkLoading()
    {

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            NoiseGenMap chunk = chunks[i];

            if (Vector2.Distance(new Vector2(chunk.posX, chunk.posZ), new Vector2(GetChunkAtPlayerLocation().posX, GetChunkAtPlayerLocation().posZ)) > viewDistance)
            {


                //unload from loaded chunks
                chunk.Save();
                chunk.UnLoad();
                chunks.Remove(chunk);
                chunk.Delete();


            }

            else
            {


                chunk.Load();


            }
        }




    }
    bool GenerateChunk(int chunkX, int chunkZ, int chunkIndex)
    {
        if (!IsChunkGeneratedAtPosition(chunkX, chunkZ))
        {

            GameObject chunk = Instantiate(level_chunkGenerator);

            chunk.name = chunkX + ", " + chunkZ;
            chunk.GetComponent<NoiseGenMap>().SetChunkVariables(chunkX, chunkZ, chunkIndex, chunk_width, chunk_height);
            chunk.transform.position = new Vector3(chunkX * ChunkSize(), 0, chunkZ * ChunkSize());

            chunks.Add(chunk.GetComponent<NoiseGenMap>());

            return true;


        }

        else
        {
            return false;
        }

    }

    //does checks to see if chunks are loaded as well
    void TryGenChunks()
    {

        Vector2 newChunkPlayerLocation = new Vector2(GetChunkAtPlayerLocation().posX, GetChunkAtPlayerLocation().posZ);

        if (newChunkPlayerLocation != oldPlayerChunkLocation && !isLoadingChunks)
        {
            isLoadingChunks = true;


            for (int x = (int)(GetChunkAtPlayerLocation().posX - viewDistance); x < (int)(GetChunkAtPlayerLocation().posX + viewDistance); x++)
            {
                for (int z = (int)(GetChunkAtPlayerLocation().posZ - viewDistance); z < (int)(GetChunkAtPlayerLocation().posZ + viewDistance); z++)
                {
                    if (GenerateChunk(x, z, currentChunkNumber))
                        currentChunkNumber++;
                }


            }

            manageChunkLoading();

            isLoadingChunks = false;
        }

        oldPlayerChunkLocation = new Vector2(GetChunkAtPlayerLocation().posX, GetChunkAtPlayerLocation().posZ);
    }


    public bool randomBoolean()
    {

        if (UnityEngine.Random.value >= 0.7)
        {
            return true;
        }
        return false;
    }

    public bool IsChunkGeneratedAtPosition(int x, int y)
    {
        foreach (NoiseGenMap c in chunks)
        {
            if ((int)c.posX == x && (int)c.posZ == y)
            {
                return true;
            }

        }

        return false;


    }




    public bool CheckPointIntersection(float x1, float y1, float x2, float y2, float x, float y)
    {
        if (x > x1 && x < x2 && y > y1 && y < y2)

            return true;

        else
        {
            return false;
        }

    }
    public float ChunkSize()
    {
        return level_chunkGenerator.GetComponent<NoiseGenMap>().size * chunk_width;
    }
    public NoiseGenMap GetChunkAtPlayerLocation()

    {


        foreach (NoiseGenMap c in chunks)
        {

            if (c != null)
                if (CheckPointIntersection(

                    (c.posX * ChunkSize()) - ChunkSize() / 2,
                    (c.posZ * ChunkSize()) - ChunkSize() / 2,

                    (c.posX * ChunkSize()) + ChunkSize() / 2,
                    (c.posZ * ChunkSize()) + ChunkSize() / 2,

                    GameSettings.Instance.Player.transform.position.x, GameSettings.Instance.Player.transform.position.z))
                {
                    Debug.Log(c.posX + ", " + c.posZ);
                    return c;

                }
        }

        return null;

    }

    NoiseGenMap GetChunkAtlLocation(int x, int z)

    {
        foreach (NoiseGenMap c in chunks)
        {

            if (c.posX == x && c.posZ == z)
            {
                return c;
            }
        }

        return null;

    }

    public void UpdateChunks()
    {

        if (!GameSettings.LEVEL_LOADED)

            Debug.Log("Waiting For Level To Load");

        else
        {
            //update Chunks
            if (gen_enabled && chunks.Count > 0)
                TryGenChunks();
        }



    }



    /*private void OnApplicationQuit()
    {
        DeleteFile();
    }*/

    //delete the temp world data
    /*void DeleteFile()
    {
        string destination = Application.persistentDataPath + "/currentWorld.dat";

        if (File.Exists(destination)) File.Delete(destination);
 
    }

    public void AddChunkToFile(Chunk chunk)
    {
        string destination = Application.persistentDataPath + "/currentWorld.dat";
        FileStream file;

        if (File.Exists(destination)) file = new FileStream(destination, FileMode.Append);
        else file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, chunk);
        file.Close();
    }

    public Chunk LoadChunkFromFile(Vector2 chunkPos)
    {
        using (var fileStream = new FileStream("C:\file.dat", FileMode.Open))
        {
            var bFormatter = new BinaryFormatter();
            while (fileStream.Position != fileStream.Length)
            {
                Chunk c = (Chunk)bFormatter.Deserialize(fileStream);
                if (c.posX == chunkPos.x && c.posZ == chunkPos.y)
                    return c;
            }
        }
        return null;
    }*/
}
