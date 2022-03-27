using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1 : InfLevelGenerator
{
    //basic building blocks
    public List<GameObject> tiles;

    void Awake()
    {
        ScriptInit();
    }
    void Update()
    {
        UpdateChunks();
    }
    /*protected override void GenerateRoom(float x, float z, int roomNumber, Chunk parentChunk)
    {
        
        float roomX, roomY;

        roomX = x / roomSize;

        roomY = z / roomSize;

        GameObject roomContainer = new GameObject("Room" + " ( " + roomX + "," + roomY + " )");

        float value = Random.value;

        Vector3 location = new Vector3(x + roomSize, roomHeight / 2, z + roomSize);

        Quaternion[] rotations = new Quaternion[4];

        rotations[0] = new Quaternion(0, 0, 0, 0);
        rotations[1] = new Quaternion(0, 90, 0, 0);
        rotations[2] = new Quaternion(0, 180, 0, 0);
        rotations[3] = new Quaternion(0, 270, 0, 0);

        if (value <= 1f && value > 0.5f)
        {
            Instantiate(tiles[0], location, Quaternion.identity, roomContainer.transform);
        }
        else if (value <= 0.5f && value > 0.5f)
        {
            Instantiate(tiles[1], location, Quaternion.identity, roomContainer.transform);
        }
        else
        {
            Instantiate(tiles[2], location, rotations[UnityEngine.Random.Range(0, 4)], roomContainer.transform);
        }


        roomContainer.transform.position = new Vector3(x, 0, z);

        roomContainer.transform.parent = parentChunk.transform;

        Room.CreateComponent(roomContainer, roomNumber, roomX, roomY);

        

    }

    protected override bool GenerateChunk(int chunkX, int chunkZ, int chunkIndex)
    {
        
        if (!IsChunkGeneratedAtPosition(chunkX, chunkZ))
        {
            //Debug.Log("Generating Chunk At" + "( " + chunkX + "," + chunkZ + " )");

            GameObject chunkContainer = new GameObject("Chunk" + " ( " + chunkX + "," + chunkZ + " )");

            chunkContainer.transform.position = new Vector3(chunkX, 0, chunkZ);

            Chunk chunk = Chunk.CreateComponent(chunkContainer, chunkIndex, chunkX, chunkZ);

            for (int x = 0; x < chunkDimensions; x++)
            {

                for (int z = 0; z < chunkDimensions; z++)
                {


                    GenerateRoom(((roomSize * x) + (ChunkSize() / 2 * chunkX)), ((roomSize * z) + (ChunkSize() / 2 * chunkZ)), currentRoomNumber, chunk);


                    currentRoomNumber++;

                }


            }
            
            chunks.Add(chunk);

            return true;
        }
        else
        {
            return false;
        }

       
    }*/

}
