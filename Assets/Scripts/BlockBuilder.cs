using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBuilder : MonoBehaviour
{
	public LayerMask BodyMask;
	public LayerMask PositiveMask;
	public LayerMask NegativeMask;

	public List<GameObject> BlockPrefabs = new List<GameObject>();
	private Block BlockGhost;
	public List<Color> PossibleColors;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		UpdatePlaceBlocks();
	}

	/// <summary>
	/// Handles all logic for the placement and setting of blocks in the world.
	/// </summary>
	private void UpdatePlaceBlocks()
	{
		if (BlockGhost == null)
		{
			// Right click to create a new block (random for now)
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				GameObject prefab = BlockPrefabs[Random.Range(0, BlockPrefabs.Count)];
				BlockGhost = Instantiate(prefab, transform).GetComponent<Block>();
				BlockGhost.SetColor(PossibleColors[Random.Range(0, PossibleColors.Count)]);
			}

			// Left click to pickup existing blocks
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				Collider[] hits = Physics.OverlapSphere(GetMouseWorld(), 0.1f, BodyMask);
				if (hits.Length == 0)
					return;

				BlockGhost = hits[0].transform.parent.GetComponent<Block>();
				BlockGhost.Placing = true;
			}
		}
		else
		{
			// Always have the ghost follow the mouse
			BlockGhost.transform.position = GetMouseWorld();

			// Right click to cancel and destroy the ghost
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				Destroy(BlockGhost.gameObject);
				BlockGhost = null;
			}

			// Left click to place
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (BlockGhost.PlaceBlock())
				{
					BlockGhost = null;
				}
			}
		}
	}

	/// <summary>
	/// Returns world coordinates of mouse
	/// </summary>
	public Vector3 GetMouseWorld()
	{
		Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseWorld.z = 0f;
		return mouseWorld;
	}
}
