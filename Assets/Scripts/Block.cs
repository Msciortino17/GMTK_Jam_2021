using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	private BlockBuilder builderRef;

	public bool Placing;
	public CardinalDirection MyDirection;

	public LayerMask BodyMask;
	public LayerMask PositiveMask;
	public LayerMask NegativeMask;

	public Collider bodyCollider;
	public SphereCollider overlapCollider;
	public List<Collider> positiveConnectors = new List<Collider>();
	public List<Collider> negativeConnectors = new List<Collider>();
	public List<GameObject> connectorIcons = new List<GameObject>();

	private List<SpriteRenderer> myRenderers;

	// Start is called before the first frame update
	void Awake()
	{
		MyDirection = CardinalDirection.Up;
		InitConnectors();
		myRenderers = new List<SpriteRenderer>();
		myRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void InitBlock(BlockBuilder _builder)
	{
		builderRef = _builder;
		ShowConnectorIcons(builderRef.ShowIcons);
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

		foreach (Collider positive in positiveConnectors)
		{
			connectorIcons.Add(positive.transform.Find("Icon").gameObject);
		}
		foreach (Collider negative in negativeConnectors)
		{
			connectorIcons.Add(negative.transform.Find("Icon").gameObject);
		}

		bodyCollider = transform.Find("Body").GetComponent<Collider>();
		overlapCollider = transform.Find("OverlapZone").GetComponent<SphereCollider>();
	}

	/// <summary>
	/// Handles snapping the block onto an existing one.
	/// </summary>
	public bool ConnectBlock()
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
		transform.Translate(connectionDelta, Space.World);
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
	/// Will attempt to place the block in the air
	/// </summary>
	/// <returns></returns>
	public bool PlaceBlockInAir()
	{
		// First, take note of all nearby other blocks. If any are found, break out early.
		List<Collider> otherBodies = new List<Collider>();
		otherBodies.AddRange(Physics.OverlapSphere(transform.position, overlapCollider.radius, BodyMask));
		otherBodies.Remove(bodyCollider);
		if (otherBodies.Count > 0)
			return false;

		// Align position along grid
		Vector3 position = transform.position;
		position.x = (position.x + (position.x > 0f ? 0.5f : -0.5f));
		position.y = (position.y + (position.y > 0f ? 0.5f : -0.5f));
		position.x = ((int)(position.x * 2f)) / 2f;
		position.y = ((int)(position.y * 2f)) / 2f;
		transform.position = position;

		// One last check to make sure no more overlaps.
		otherBodies.Clear();
		otherBodies.AddRange(Physics.OverlapSphere(transform.position, overlapCollider.radius, BodyMask));
		otherBodies.Remove(bodyCollider);
		if (otherBodies.Count > 0)
			return false;

		// Should be good at this point!
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

	/// <summary>
	/// Iterates through all icons and toggles their visibility based on setting.
	/// </summary>
	public void ShowConnectorIcons(bool _show)
	{
		foreach (GameObject icon in connectorIcons)
		{
			icon.SetActive(_show);
		}
	}

	/// <summary>
	/// Rotates either clockwise or counter clockwise, cycling from up right down to left.
	/// </summary>
	public void Rotate(bool _clockwise)
	{
		if (_clockwise)
		{
			MyDirection++;
			if (MyDirection > CardinalDirection.Left)
				MyDirection = CardinalDirection.Up;
		}
		else
		{
			MyDirection--;
			if (MyDirection < CardinalDirection.Up)
				MyDirection = CardinalDirection.Left;
		}

		switch (MyDirection)
		{
			case CardinalDirection.Up:
				transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case CardinalDirection.Right:
				transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case CardinalDirection.Down:
				transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case CardinalDirection.Left:
				transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Hard sets the rotation to the given value and handles alignment.
	/// </summary>
	public void SetRotation(CardinalDirection _direction)
	{
		MyDirection = _direction;

		switch (MyDirection)
		{
			case CardinalDirection.Up:
				transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case CardinalDirection.Right:
				transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case CardinalDirection.Down:
				transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case CardinalDirection.Left:
				transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			default:
				break;
		}
	}
}
