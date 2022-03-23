using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenMap : MonoBehaviour
{
    //point to loat chuncks around
    public GameObject RenderPoint;

    //perlin seed
    public float seed;

    //tile types
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups;
    public GameObject prefab_North;
    public GameObject prefab_South;
    public GameObject prefab_East;
    public GameObject prefab_West;
    public GameObject prefab_None;
    public GameObject prefab_North_Small;
    public GameObject prefab_South_Small;
    public GameObject prefab_East_Small;
    public GameObject prefab_West_Small;


    public int map_width;
    public int map_height;

    List<List<int>> noise_grid = new List<List<int>>();
    List<List<GameObject>> tile_grid = new List<List<GameObject>>();

    //how often tiles spawn ajason to eachother
    public float magnification;

    //Size of the Tiles
    public float size;

    public int x_offset; // <- +>
    public int y_offset; // v- +^

    void Start()
    {
        CreateTileset();
        CreateTileGroups();
        GenerateMap();
    }

    void CreateTileset()
    {
        /** Collect and assign ID codes to the tile prefabs, for ease of access.
            Best ordered to match land elevation. **/

        tileset = new Dictionary<int, GameObject>();
        //Repeat tiles to spawn more
        tileset.Add(0, prefab_None);
        tileset.Add(1, prefab_North);
        tileset.Add(2, prefab_South);
        tileset.Add(3, prefab_East);
        tileset.Add(4, prefab_West);
        tileset.Add(5, prefab_North_Small);
        tileset.Add(6, prefab_South_Small);
        tileset.Add(7, prefab_East_Small);
        tileset.Add(8, prefab_West_Small);
    }

    void CreateTileGroups()
    {
        /** Create empty gameobjects for grouping tiles of the same type, ie
            South tiles **/

        tile_groups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
    }

    void GenerateMap()
    {
        /** Generate a 2D grid using the Perlin noise fuction, storing it as
            both raw ID values and tile gameobjects **/

        for (int x = 0; x < map_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());

            for (int y = 0; y < map_height; y++)
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

        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
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
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x * size, 0, y * size);

        tile_grid[x].Add(tile);
    }
}