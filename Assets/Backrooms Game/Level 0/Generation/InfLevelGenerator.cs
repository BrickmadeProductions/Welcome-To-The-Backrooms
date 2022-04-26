
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class InfLevelGenerator : MonoBehaviour
{
    public bool gen_enabled;

    public bool ThreeDimensional;

    //public Dictionary<Vector3, entityAttack> entitiesInScene;

    [HideInInspector]
    public bool isLoadingChunks = false;

    [HideInInspector]
    public int currentRoomNumber;

    [HideInInspector]
    public int currentChunkNumber;

    [HideInInspector]
    public Vector3 oldPlayerChunkLocation = Vector3.zero;

    public GameObject level_chunkGenerator;

    public int chunk_length;
    public int layers;

    //chunks can be dynamically added and removed therefore there is no point in making it a 2d array
    //chunks can be dynamically added and removed therefore there is no point in making it a 2d array
    public List<Chunk> chunks;

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
    
    public void ScriptInit()
    {
       
        
        if (gen_enabled)
        {
            
            chunks = new List<Chunk>();

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
                for (int y = 0; y < layers; y++)
                {
                    if (GenerateChunk(x, y, z, currentChunkNumber))
                        currentChunkNumber++;
                }
            }
            
        }

    }


    public void manageChunkLoading()
    {

        for (int i = chunks.Count - 1; i >= 0 ; i--)
        {
            Chunk chunk = chunks[i];

            if (Vector3.Distance(new Vector3(chunk.chunkPosX, chunk.chunkPosY * 3, chunk.chunkPosZ), new Vector3(GetChunkAtPlayerLocation().chunkPosX, GetChunkAtPlayerLocation().chunkPosY * 3, GetChunkAtPlayerLocation().chunkPosZ)) > viewDistance)
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

            if (chunk != null)
                chunk.UpdateChunkData();
        }



        
    }
    bool GenerateChunk(int chunkX, int chunkY, int chunkZ, int chunkIndex)
    {
        if (!IsChunkGeneratedAtPosition(chunkX, chunkY, chunkZ))
        {
            
            GameObject chunk = Instantiate(level_chunkGenerator);

            chunk.name = chunkX + ", " + chunkY + ","  + chunkZ;
            chunk.GetComponent<Chunk>().SetChunkVariables(chunkX, chunkY, chunkZ, chunkIndex, chunk_length, chunk_length);
            chunk.transform.position = new Vector3(chunkX * ChunkSize(), chunkY * ChunkHeight(), chunkZ * ChunkSize());

            chunks.Add(chunk.GetComponent<Chunk>());

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
        if (GetChunkAtPlayerLocation() != null)
        {
            Vector3 newChunkPlayerLocation = new Vector3(GetChunkAtPlayerLocation().chunkPosX, GetChunkAtPlayerLocation().chunkPosY, GetChunkAtPlayerLocation().chunkPosZ);

            if (newChunkPlayerLocation != oldPlayerChunkLocation && !isLoadingChunks)
            {
                isLoadingChunks = true;

                for (int x = (int)(GetChunkAtPlayerLocation().chunkPosX - viewDistance); x < (int)(GetChunkAtPlayerLocation().chunkPosX + viewDistance); x++)
                {
                    for (int z = (int)(GetChunkAtPlayerLocation().chunkPosZ - viewDistance); z < (int)(GetChunkAtPlayerLocation().chunkPosZ + viewDistance); z++)
                    {
                        if (layers > 1)
                        {
                            for (int y = (int)(GetChunkAtPlayerLocation().chunkPosY - layers); y < (int)(GetChunkAtPlayerLocation().chunkPosY + layers); y++)
                            {

                                if (GenerateChunk(x, y, z, currentChunkNumber))
                                    currentChunkNumber++;
                            }
                        }
                        else
                        {


                            if (GenerateChunk(x, 0, z, currentChunkNumber))
                                currentChunkNumber++;

                        }

                    }



                }

                manageChunkLoading();

                isLoadingChunks = false;

            }

            oldPlayerChunkLocation = new Vector3(GetChunkAtPlayerLocation().chunkPosX, GetChunkAtPlayerLocation().chunkPosY, GetChunkAtPlayerLocation().chunkPosZ);
        }
        
    }
    

    public bool randomBoolean()
    {
        
        if (UnityEngine.Random.value >= 0.7)
        {
            return true;
        }
        return false;
    }

    public bool IsChunkGeneratedAtPosition(int x, int y, int z)
    {
        foreach (Chunk c in chunks)
        {
            if ((int)c.chunkPosX == x && (int)c.chunkPosY == y && (int)c.chunkPosZ == z)
            {
                return true;
            }

        }

        return false;

            
    }
   
    
    
    
    public bool CheckPointIntersection(float x1, float y1, float z1, float x2, float y2, float z2, float x, float y, float z)
    {
        if (x > x1 && x < x2 && y > y1 && y < y2 && z > z1 && z < z2)

            return true;

        else
        {
            return false;
        }
        
    }
    public float ChunkSize()
    {
        return level_chunkGenerator.GetComponent<Chunk>().sizeLength * chunk_length;
    }

    public float ChunkHeight()
    {
        return level_chunkGenerator.GetComponent<Chunk>().sizeHeight * layers;
    }
    public Chunk GetChunkAtPlayerLocation()

    {
        

        foreach (Chunk c in chunks)
        {
            
            if (c != null)
                if (CheckPointIntersection(

                    (c.chunkPosX * ChunkSize()) - ChunkSize() / 2,
                    (c.chunkPosY * ChunkHeight()) - ChunkHeight() / 2,
                    (c.chunkPosZ * ChunkSize()) - ChunkSize() / 2,

                    (c.chunkPosX * ChunkSize()) + ChunkSize() / 2,
                    (c.chunkPosY * ChunkHeight()) + ChunkHeight() / 2,
                    (c.chunkPosZ * ChunkSize()) + ChunkSize() / 2,



                    GameSettings.Instance.Player.transform.position.x, GameSettings.Instance.Player.transform.position.y, GameSettings.Instance.Player.transform.position.z))
                {
                    //Debug.Log(c.posX + ", " + c.posZ);
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
