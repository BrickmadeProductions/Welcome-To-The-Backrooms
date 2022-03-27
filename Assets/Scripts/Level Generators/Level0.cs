using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level0 : InfLevelGenerator
{
    //basic building blocks
    public GameObject wall;
    public GameObject floor;
    public List<GameObject> ceilings;

    public GameObject exit;

    public List<GameObject> extraWalls;
    public List<GameObject> props;

    List<Vector3> tempWallLocations = new List<Vector3>();

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


        //floor
        GameObject newFloor = Instantiate(floor, new Vector3(x, 0.015f, z), floor.transform.rotation, roomContainer.transform);


        //ceiling
        GameObject newCeiling;
        float valueCeiling = Random.value;

        if (valueCeiling <= 1f && valueCeiling > 0.3f)

            newCeiling = Instantiate(ceilings[0], new Vector3(x, roomHeight, z), ceilings[0].transform.rotation, roomContainer.transform);

        else if (valueCeiling <= 0.3f && valueCeiling > 0.1f)
        {
            newCeiling = Instantiate(ceilings[1], new Vector3(x, roomHeight, z), ceilings[1].transform.rotation, roomContainer.transform);
        }
        else
        {
            newCeiling = Instantiate(ceilings[2], new Vector3(x, roomHeight, z), ceilings[1].transform.rotation, roomContainer.transform);
        }

        GameObject[] walls = new GameObject[4];

        //r = rotation number
        int wallsToBuild;

        wallsToBuild = parentChunk.posX % 2 == 0 && parentChunk.posY % 2 == 1 ? 2 : 3;

        wallsToBuild = parentChunk.posX % 4 == 0 && parentChunk.posY % 4 == 1 ? 1 : 3;

        wallsToBuild = parentChunk.posX % 8 == 0 && parentChunk.posY % 8 == 1 ? 0 : 3;

        for (int r = 0; r < wallsToBuild; r++)
        {

            bool placeWall;
            if (r < 4)
            {
                placeWall = randomBoolean();
            }
            else
            {
                placeWall = false;
            }

            //int direction = (int)UnityEngine.Random.Range(0, 4);

            //4 walls

            if (placeWall)
            {

                float xWall = 0;
                float zWall = 0;

                switch (r)
                {
                    case 0:
                        xWall = 0;
                        zWall = 1;
                        break;

                    case 1:
                        xWall = 1;
                        zWall = 0;
                        break;

                    case 2:
                        xWall = 0;
                        zWall = -1;
                        break;

                    case 3:
                        xWall = -1;
                        zWall = 0;
                        break;

                }

                GameObject newWall;
                float valueWall = UnityEngine.Random.value;

                Vector3 location = new Vector3(x + (xWall * roomSize), roomHeight / 2, z + (zWall * roomSize));


                if (valueWall <= 1f && valueWall > 0.6f)
                {
                    newWall = Instantiate(wall, location, wall.transform.rotation, roomContainer.transform);
                }
                else if (valueWall <= 0.6f && valueWall > 0.3f)
                {
                    newWall = Instantiate(extraWalls[0], location, wall.transform.rotation, roomContainer.transform);
                }
                else if (valueWall <= 0.3f && valueWall > 0.1f)
                {
                    newWall = Instantiate(extraWalls[1], location, wall.transform.rotation, roomContainer.transform);
                }
                else if (valueWall <= 0.1f && valueWall > 0.05f)
                {
                    newWall = Instantiate(extraWalls[2], location, wall.transform.rotation, roomContainer.transform);
                }
                else if (valueWall <= 0.05f && valueWall > 0.005f)
                {
                    newWall = Instantiate(extraWalls[3], location, wall.transform.rotation, roomContainer.transform);
                }

                else if (

                    GameSettings.Instance.Player.GetComponent<PlayerController>() != null
                    && Vector3.Distance(GameSettings.Instance.Player.transform.position, Vector3.zero) > 500
                    && GameSettings.Instance.Player.GetComponent<PlayerController>().distance.GetDistanceTraveled() > 250
                    && valueWall <= 0.005f

                    )
                {
                    newWall = Instantiate(exit, location, wall.transform.rotation, roomContainer.transform);

                }
                else
                {
                    newWall = Instantiate(extraWalls[3], location, wall.transform.rotation, roomContainer.transform);
                }


                if (GameSettings.Instance.Player.GetComponent<PlayerController>() != null) 
                newWall.transform.Rotate(0, 0, r * 90);
                walls[r] = newWall;



            }

        }

        roomContainer.transform.position = new Vector3(x, 0, z);

        roomContainer.transform.parent = parentChunk.transform;

        foreach (GameObject wall in walls)
        {
            if (wall != null)
            {
                if (!tempWallLocations.Contains(wall.transform.position))
                {
                    tempWallLocations.Add(wall.transform.position);
                }
                else
                {
                    Destroy(wall);
                }
            }
        }

        Room.CreateComponent(roomContainer, walls, props, newFloor, newCeiling, roomNumber, roomX, roomY);



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


            tempWallLocations.Clear();

            return true;
        }
        else
        {
            return false;
        }


    }*/
    

}
