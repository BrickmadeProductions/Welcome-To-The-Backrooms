using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public Vector2Int tilePos;

	public List<Transform> entitySpawnLocations;

	public List<Transform> randomPropSpawnLocations;

	public List<SpawnItemOfType> setPropSpawnLocations;

    public void SpawnPresetProps()
    {
        foreach (SpawnItemOfType spawnLocation in setPropSpawnLocations)
        {
            spawnLocation.SpawnItem();
        }
    }
}
