using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	public bool Placing;

	public LayerMask BodyMask;
	public LayerMask PositiveMask;
	public LayerMask NegativeMask;

	public Collider bodyCollider;
	public SphereCollider overlapCollider;
	public List<Collider> positiveConnectors;
	public List<Collider> negativeConnectors;

	private List<SpriteRenderer> myRenderers;

	// Start is called before the first frame update
	void Awake()
	{
		InitConnectors();
		myRenderers = new List<SpriteRenderer>();
		myRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// Wire up references to connectors along with other main colliders
	/// </summary>
	private void InitConnectors()
	{
		Transform connectors = transform.Find("Connectors");
		Transform positives = connectors.Find("Positive");
		Transform negatives = connectors.Find("Negative");
		positiveConnectors.AddRange(positives.GetComponentsInChildren<Collider>());
		negativeConnectors.AddRange(negatives.GetComponentsInChildren<Collider>());

		bodyCollider = transform.Find("Body").GetComponent<Collider>();
		overlapCollider = transform.Find("OverlapZone").GetComponent<SphereCollider>();
	}

	/// <summary>
	/// Handles snapping the block onto an existing one.
	/// </summary>
	public bool PlaceBlock()
	{
		// First, take note of all nearby other blocks. If none are found, break out early.
		List<Collider> otherBodies = new List<Collider>();
		otherBodies.AddRange(Physics.OverlapSphere(transform.position, overlapCollider.radius, BodyMask));
		otherBodies.Remove(bodyCollider);
		if (otherBodies.Count <= 0)
			return false;

		// Check through the other positive and negative connectors and find the first overlap possible.
		List<Block> otherBlocks = new List<Block>();
		List<Collider> otherPositives = new List<Collider>();
		List<Collider> otherNegatives = new List<Collider>();
		foreach (Collider body in otherBodies)
		{
			Block block = body.transform.parent.GetComponent<Block>();
			otherBlocks.Add(block);
			otherPositives.AddRange(block.positiveConnectors);
			otherNegatives.AddRange(block.negativeConnectors);
		}

		bool foundConnection = false;
		Collider myConnector = null;
		Collider otherConnector = null;

		for (int i = 0; i < positiveConnectors.Count && !foundConnection; i++)
		{
			Collider myPositive = positiveConnectors[i];
			for (int j = 0; j < otherNegatives.Count && !foundConnection; j++)
			{
				Collider otherNegative = otherNegatives[j];
				if (myPositive.bounds.Intersects(otherNegative.bounds))
				{
					foundConnection = true;
					myConnector = myPositive;
					otherConnector = otherNegative;
				}
			}
		}

		for (int i = 0; i < negativeConnectors.Count && !foundConnection; i++)
		{
			Collider myNegative = negativeConnectors[i];
			for (int j = 0; j < otherPositives.Count && !foundConnection; j++)
			{
				Collider otherPositive = otherPositives[j];
				if (myNegative.bounds.Intersects(otherPositive.bounds))
				{
					foundConnection = true;
					myConnector = myNegative;
					otherConnector = otherPositive;
				}
			}
		}

		if (!foundConnection)
		{
			return false;
		}

		// At this point, some sort of valid connection should have been found. Now we have to move the block accordingly and verify we don't overlap in the new position.
		Vector3 connectionDelta = otherConnector.transform.position - myConnector.transform.position;
		transform.Translate(connectionDelta);
		foreach (Collider otherCollider in otherBodies)
		{
			if (bodyCollider.bounds.Intersects(otherCollider.bounds))
				return false;
		}

		// If we get this far, all should be well!
		Placing = false;
		return true;
	}

	/// <summary>
	/// Simply set the color of all renderers
	/// </summary>
	public void SetColor(Color _color)
	{
		foreach (SpriteRenderer renderer in myRenderers)
		{
			renderer.color = _color;
		}
	}
}
