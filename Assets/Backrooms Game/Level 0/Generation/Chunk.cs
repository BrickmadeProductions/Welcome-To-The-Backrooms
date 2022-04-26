using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chunk : MonoBehaviour
{

    //chunk data 
    public bool loaded;
    public int id;
    public float chunkPosX;
    public float chunkPosZ;
    public float chunkPosY;

    //tile types
    Dictionary<int, GameObject> tileset;
    public GameObject exit;

    public List<GameObject> Tiles;

    //entity tiles (types that can be created)
    public List<Entity> entities;

    //specialTiles
    public List<GameObject> specialTiles;

    //regularly placed tiles
    public List<GameObject> regTiles;
    public int regTileSpace;


    int chunk_width;
    int chunk_length;

    List<List<int>> noise_grid;

    List<List<GameObject>> tile_grid;

    //how often tiles spawn ajason to eachother
    public float magnification;

    //Size of the Tiles
    public float sizeLength;
    public float sizeHeight;

    public int x_offset; // <- +>
    public int z_offset; // v- +^

    private int RoomId;

    private int seed;
    public void SetChunkVariables(float posX, float posY, float posZ, int id, int width, int length)
    {
        loaded = true;
        this.id = id;
        this.chunkPosX = posX;
        this.chunkPosY = posY;
        this.chunkPosZ = posZ;
        this.chunk_width = width;
        this.chunk_length = length;
    }

    void Start()
    {
        noise_grid = new List<List<int>>();

        tile_grid = new List<List<GameObject>>();

        seed = Random.Range(-2147483648, 2147483647);

        loaded = true;
        CreateTileset();
        CreateTileGroups();
        GenerateMap();

    }

    /*
    void Update()
    {
        
    }
    */
    internal void UpdateChunkData()
    {


    }
    internal void UnLoad()
    {

        loaded = false;

    }
    internal void Save()
    {

    }

    internal void Load()
    {

        loaded = true;

    }

    internal void Delete()
    {
        Destroy(gameObject);
    }

    void CreateTileset()
    {
        /** Collect and assign ID codes to the tile prefabs, Tiles first on list spawn more often. **/

        tileset = new Dictionary<int, GameObject>();
        //Repeat tiles to spawn more




        for (int Id = 0; Id < Tiles.Count;  Id++)
        {
            tileset.Add(Id, Tiles[Id]);
        }



    }

    void CreateTileGroups()
    {
        /** Create empty gameobjects for grouping tiles of the same type, ie
            South tiles **/

        //tile_groups = new Dictionary<int, GameObject>();

        /*foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);

            tile_groups.Add(prefab_pair.Key, tile_group);
        }*/
    }

    void GenerateMap()
    {
        /** Generate a 2D grid using the Perlin noise fuction, storing it as
            both raw ID values and tile gameobjects **/
        for (int x = 0; x < chunk_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());

            for (int z = 0; z < chunk_length; z++)
            {
                int tile_id = GetIdUsingPerlin(x, z);
                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, (int)chunkPosY, z);
            }
        }
    }

    

    int GetIdUsingPerlin(int x, int z)
    {
        /** Using a grid coordinate input, generate a Perlin noise value to be
            converted into a tile ID code. Rescale the normalised Perlin value
            to the number of tiles available. **/
        System.Random prng = new System.Random(seed);
        float raw_perlin = Mathf.PerlinNoise(
            (prng.Next(-100000, 100000) + x - x_offset) / magnification,
            (prng.Next(-100000, 100000) + z - z_offset) / magnification
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * tileset.Count;

        if (scaled_perlin == tileset.Count)
        {
            scaled_perlin = (tileset.Count - 1);
        }
        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y, int z)
    {
        /** Creates a new tile using the type id code, group it with common
            tiles, set it's position and store the gameobject. **/

        GameObject tile_prefab = tileset[tile_id];
        GameObject tile = null;

        if (SceneManager.GetActiveScene().name != "HomeScreen")
        {
            float specialChance = Random.Range(0f, 1f);

            if (specialChance > 0.997f)
            {

                tile = Instantiate(specialTiles[Random.Range(0, specialTiles.Count)], gameObject.transform);
            }

            if (x + chunkPosX % regTileSpace == 0 && z + chunkPosZ % regTileSpace == 0 && regTileSpace > 0)
            {

                tile = Instantiate(regTiles[Random.Range(0, regTiles.Count)], gameObject.transform);
            }

            if (GameSettings.Instance.Player.GetComponent<PlayerController>().distance.GetDistanceTraveled() > 750

            && Vector3.Distance(GameSettings.Instance.Player.transform.position, Vector3.zero) > 500 && exit != null)
            {
                float noClipWallChance = Random.Range(0f, 1f);

                if (noClipWallChance > 0.995f)

                    tile = Instantiate(exit, gameObject.transform);

                else
                {
                    tile = Instantiate(tile_prefab, gameObject.transform);
                }
            }

            //regular tile spawning
            if (tile == null)
            {
                tile = Instantiate(tile_prefab, gameObject.transform);
            }

            //entity spawning
            foreach (Entity entity in entities)
            {
                float entityChance = Random.Range(0f, 0.99f);

                if (entity.spawnChance > entityChance)
                {

                    if (tile.GetComponent<ExtraTileData>() != null)
                    {
                        GameSettings.Instance.AddEntity(tile.GetComponent<ExtraTileData>().entitySpawnLocations[Random.Range(0, tile.GetComponent<ExtraTileData>().entitySpawnLocations.Count)].position, entity);

                    }
                    else 
                    { 
                        GameSettings.Instance.AddEntity(new Vector3((x * sizeLength) + (chunkPosX * chunk_width * sizeLength), 0, (z * sizeLength) + (chunkPosZ * chunk_width * sizeLength)), entity);
                    }
                }

            }
            

            
        }
        else
        {
            tile = Instantiate(tile_prefab, gameObject.transform);

        }
        tile.name = string.Format("tile_x{0}_y{1}_z{2}", x + chunkPosX, y + chunkPosY, z + chunkPosZ);

        //set tiles location relative to layer
        tile.transform.localPosition = new Vector3(x * sizeLength, 0, z * sizeLength);

        tile_grid[x].Add(tile);

        if (chunkPosY > 0)
        {
            Elevator elevator = tile.GetComponentInChildren<Elevator>();
            if (elevator != null)
            {
                Destroy(elevator.gameObject);
            }
        }

    }

}