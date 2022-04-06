using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenMap : MonoBehaviour
{

    //chunk data 
    public bool loaded;
    public int id;
    public float posX;
    public float posZ;

    //perlin seed
    public int seed;

    //tile types
    Dictionary<int, GameObject> tileset;
    //Dictionary<int, GameObject> tile_groups;

    public List<GameObject> Tiles;

    int chunk_width;
    int chunk_height;

    List<List<int>> noise_grid;

    List<List<GameObject>> tile_grid;

    //how often tiles spawn ajason to eachother
    public float magnification;

    //Size of the Tiles
    public float size;

    public int x_offset; // <- +>
    public int y_offset; // v- +^

    private int RoomId;

    public void SetChunkVariables(float posX, float posZ, int id, int width, int height)
    {
        loaded = true;
        this.id = id;
        this.posX = posX;
        this.posZ = posZ;
        this.chunk_width = width;
        this.chunk_height = height;
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

    internal void UnLoad()
    {

        loaded = false;

        gameObject.SetActive(false);

    }
    internal void Save()
    {

    }

    internal void Load()
    {

        loaded = true;

        gameObject.SetActive(true);

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

            for (int y = 0; y < chunk_height; y++)
            {
                int tile_id = GetIdUsingPerlin(x, y);
                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, y);
            }
        }
    }

    

    int GetIdUsingPerlin(int x, int y)
    {
        /** Using a grid coordinate input, generate a Perlin noise value to be
            converted into a tile ID code. Rescale the normalised Perlin value
            to the number of tiles available. **/
        System.Random prng = new System.Random(seed);
        float raw_perlin = Mathf.PerlinNoise(
            (prng.Next(-100000, 100000) + x - x_offset) / magnification,
            (prng.Next(-100000, 100000) + y - y_offset) / magnification
        );
        float clamp_perlin = Mathf.Clamp01(raw_perlin);
        float scaled_perlin = clamp_perlin * tileset.Count;

        // Replaced 4 with tileset.Count to make adding tiles easier
        if (scaled_perlin == tileset.Count)
        {
            scaled_perlin = (tileset.Count - 1);
        }
        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        /** Creates a new tile using the type id code, group it with common
            tiles, set it's position and store the gameobject. **/

        GameObject tile_prefab = tileset[tile_id];
        //GameObject tile_group = tile_groups[tile_id];
        //GameObject tile = Instantiate(tile_prefab, tile_group.transform);
        GameObject tile = Instantiate(tile_prefab, gameObject.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x * size, 0, y * size);

        tile_grid[x].Add(tile);
    }

}