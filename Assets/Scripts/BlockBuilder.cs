using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BlockBuilder : MonoBehaviour
{
	public LayerMask BodyMask;
	public LayerMask PositiveMask;
	public LayerMask NegativeMask;

	public List<GameObject> BlockPrefabs = new List<GameObject>();
	private Block BlockGhost;
	public List<Block> ExistingBlocks = new List<Block>();
	public List<Color> PossibleColors;

	public bool ShowIcons;
	public int CurrentBlock;
	public int CurrentColor;

	public SoundEffects MySoundEffects;

	[Header("Camera")]
	public float MinCameraZoom;
	public float MaxCameraZoom;
	private Camera mainCamera;
	public CinemachineVirtualCamera myVirtualCamera;
	public CinemachineBasicMultiChannelPerlin myCameraShake;
	private bool disableZoom;
	public float PanningSpeed;
	private Vector3 mouseDelta;
	private Vector3 prevMouse;
	public float shakeTimer = 0f;

	public bool HasBlockSelected
	{
		get { return BlockGhost != null; }
	}

	// Start is called before the first frame update
	void Start()
	{
		ExistingBlocks.AddRange(transform.GetComponentsInChildren<Block>());
		mainCamera = Camera.main;
		myCameraShake = myVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	// Update is called once per frame
	void Update()
	{
		disableZoom = false;
		UpdatePlaceBlocks();
		UpdateCamera();

		// I toggles showing icons.
		if (Input.GetKeyDown(KeyCode.I))
		{
			ShowIcons = !ShowIcons;
			foreach (Block block in ExistingBlocks)
			{
				block.ShowConnectorIcons(ShowIcons);
			}
		}

		mouseDelta = Input.mousePosition - prevMouse;
		prevMouse = Input.mousePosition;
	}

	/// <summary>
	/// Handles all logic for the placement and setting of blocks in the world.
	/// </summary>
	private void UpdatePlaceBlocks()
	{
		if (!HasBlockSelected)
		{
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

			// The mouse wheel can be used to change color and currently selected block
			float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
			bool changeColor = Input.GetKey(KeyCode.C);
			bool changeBlock = Input.GetKey(KeyCode.B);
			disableZoom = changeColor || changeBlock;
			if (mouseWheel > 0.01f)
			{
				if (changeColor)
				{
					CurrentColor++;
					if (CurrentColor >= PossibleColors.Count)
						CurrentColor = 0;
					BlockGhost.SetColor(PossibleColors[CurrentColor]);
				}
				else if (changeBlock)
				{
					//CurrentBlock++;
					//if (CurrentBlock >= BlockPrefabs.Count)
					//	CurrentBlock = 0;
					//RefreshBlockObject();
				}
			}
			else if (mouseWheel < -0.01f)
			{
				if (changeColor)
				{
					CurrentColor--;
					if (CurrentColor < 0)
						CurrentColor = PossibleColors.Count - 1;
					BlockGhost.SetColor(PossibleColors[CurrentColor]);
				}
				else if (changeBlock)
				{
					//CurrentBlock--;
					//if (CurrentBlock < 0)
					//	CurrentBlock = BlockPrefabs.Count - 1;
					//RefreshBlockObject();
				}
			}

			// Right click to cancel and destroy the ghost
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				ExistingBlocks.Remove(BlockGhost);
				Destroy(BlockGhost.gameObject);
				BlockGhost = null;
			}

			// Left click to place
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (BlockGhost.ConnectBlock())
				{
					BlockGhost = null;
					TriggerShake(0.25f, 0.5f, 0.25f);
					MySoundEffects.PlayClick();
				}
				else if (BlockGhost.PlaceBlockInAir())
				{
					BlockGhost = null;
					MySoundEffects.PlayClick();
				}
			}

			// R to rotate
			if (Input.GetKeyDown(KeyCode.R))
			{
				BlockGhost.Rotate(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			}
		}
	}

	/// <summary>
	/// Sets up the block ghost with the given prefab.
	/// </summary>
	public void CreateBlockGhost(GameObject _prefab)
	{
		BlockGhost = Instantiate(_prefab, GetMouseWorld(), Quaternion.identity, transform).GetComponent<Block>();
		BlockGhost.InitBlock(this);
		BlockGhost.SetColor(PossibleColors[CurrentColor]);
		ExistingBlocks.Add(BlockGhost);
	}

	/// <summary>
	/// Handles logic for camera controls
	/// </summary>
	private void UpdateCamera()
	{
		// Camera zoom
		if (!disableZoom)
		{
			float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
			if (mouseWheel > 0.01f)
			{
				myVirtualCamera.m_Lens.OrthographicSize--;
				if (myVirtualCamera.m_Lens.OrthographicSize < MinCameraZoom)
					myVirtualCamera.m_Lens.OrthographicSize = MinCameraZoom;
			}
			else if (mouseWheel < -0.01f)
			{
				myVirtualCamera.m_Lens.OrthographicSize++;
				if (myVirtualCamera.m_Lens.OrthographicSize > MaxCameraZoom)
					myVirtualCamera.m_Lens.OrthographicSize = MaxCameraZoom;
			}
		}

		// Camera pan
		bool panning = Input.GetKey(KeyCode.Mouse2);
		Cursor.visible = !panning;
		if (panning)
		{
			myVirtualCamera.transform.Translate(-mouseDelta * PanningSpeed * mainCamera.orthographicSize * Time.deltaTime);
		}

		// Recenter camera
		if (Input.GetKeyDown(KeyCode.Z))
		{
			myVirtualCamera.transform.position = new Vector3(0f, 0f, -10f);
		}

		UpdateCameraShake();
	}

	/// <summary>
	/// Triggers a camera shake based on the given parameters.
	/// </summary>
	public void TriggerShake(float amp, float freq, float duration)
	{
		myCameraShake.m_AmplitudeGain = amp;
		myCameraShake.m_FrequencyGain = freq;
		shakeTimer = duration;
	}

	/// <summary>
	/// Update the timer and logic for camera shaking.
	/// </summary>
	private void UpdateCameraShake()
	{
		if (shakeTimer > 0f)
		{
			shakeTimer -= Time.deltaTime;
			if (shakeTimer <= 0f)
			{
				myCameraShake.m_AmplitudeGain = 0f;
				myCameraShake.m_FrequencyGain = 0f;
			}
		}
	}

	/// <summary>
	/// Destroy the current block and replace it with a new one.
	/// </summary>
	private void RefreshBlockObject()
	{
		ExistingBlocks.Remove(BlockGhost);
		Destroy(BlockGhost.gameObject);
		GameObject prefab = BlockPrefabs[CurrentBlock];
		BlockGhost = Instantiate(prefab, GetMouseWorld(), Quaternion.identity, transform).GetComponent<Block>();
		BlockGhost.InitBlock(this);
		BlockGhost.SetColor(PossibleColors[CurrentColor]);
		ExistingBlocks.Add(BlockGhost);
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
