using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEdgeHandler : MonoBehaviour
{
	public GameObject[] edges;

	void EdgeHandling()
    {
		Chunk chunk = GetComponentInParent<Chunk>();

		BIOME_ID leftBiomeID = chunk.parentGenerator.GetCurrentBiomeAtChunkPosition(new Vector3Int(chunk.chunkPosX, chunk.chunkPosY, chunk.chunkPosZ + 1));
		BIOME_ID rightBiomeID = chunk.parentGenerator.GetCurrentBiomeAtChunkPosition(new Vector3Int(chunk.chunkPosX - 1, chunk.chunkPosY, chunk.chunkPosZ));
		BIOME_ID forwardBiomeID = chunk.parentGenerator.GetCurrentBiomeAtChunkPosition(new Vector3Int(chunk.chunkPosX, chunk.chunkPosY, chunk.chunkPosZ - 1));
		BIOME_ID backBiomeID = chunk.parentGenerator.GetCurrentBiomeAtChunkPosition(new Vector3Int(chunk.chunkPosX + 1, chunk.chunkPosY, chunk.chunkPosZ));

		Debug.Log(leftBiomeID);
		Debug.Log(rightBiomeID);
		Debug.Log(forwardBiomeID);
		Debug.Log(backBiomeID);

		if (leftBiomeID != BIOME_ID.LEVEL_0_TALL_ROOMS)
		{
			edges[0].SetActive(true);
		}
		if (rightBiomeID != BIOME_ID.LEVEL_0_TALL_ROOMS)
		{
			edges[1].SetActive(true);
		}
		if (forwardBiomeID != BIOME_ID.LEVEL_0_TALL_ROOMS)
		{
			edges[2].SetActive(true);
		}
		if (backBiomeID != BIOME_ID.LEVEL_0_TALL_ROOMS)
		{
			edges[3].SetActive(true);
		}
	}
    // Start is called before the first frame update
    void Awake()
    {
		GetComponent<Tile>().biomeEdgeHandler += EdgeHandling;

	}
}
