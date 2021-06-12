using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
	public BlockBuilder BlockBuilderRef;
	public SoundEffects SoundEffectsRef;

	private CanvasGroup myCanvasGroup;
	private RectTransform myRectTransform;

	public MenuRadial RootRadial;
	public Stack<MenuRadial> RadialStack = new Stack<MenuRadial>();
	public List<Button> RadialButtons = new List<Button>();

	private GameObject lastChosenPrefab;

	public bool IsActive
	{
		get { return myCanvasGroup.interactable; }
	}

	// Start is called before the first frame update
	void Start()
	{
		myCanvasGroup = GetComponent<CanvasGroup>();
		myRectTransform = GetComponent<RectTransform>();
		SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		if (!IsActive)
		{
			// Right click to open the menu
			if (Input.GetKeyDown(KeyCode.Mouse1) && !BlockBuilderRef.HasBlockSelected)
			{
				SoundEffectsRef.PlayClick();
				SetActive(true);
				PushRadial(RootRadial);
				myRectTransform.position = Input.mousePosition;
			}
		}
		else
		{
			// Fade in
			myCanvasGroup.alpha += Time.deltaTime * 2f;
			if (myCanvasGroup.alpha > 1f)
				myCanvasGroup.alpha = 1f;
		}
	}

	/// <summary>
	/// Applies common settings for if active or not.
	/// </summary>
	public void SetActive(bool _active)
	{
		myCanvasGroup.interactable = _active;
		myCanvasGroup.blocksRaycasts = _active;
		if (!_active)
		{
			myCanvasGroup.alpha = 0f;
		}
	}

	/// <summary>
	/// Will pop off the queue each time pressed, or close the menu at the root.
	/// </summary>
	public void BackButton()
	{
		SoundEffectsRef.PlayClick();
		RadialStack.Pop();

		if (RadialStack.Count == 0)
		{
			SetActive(false);
		}
		else
		{
			RefreshButtons();
		}
	}

	/// <summary>
	/// Pushes the given menu radial onto the stack and then refreshes
	/// </summary>
	public void PushRadial(MenuRadial _menuRadial)
	{
		RadialStack.Push(_menuRadial);
		RefreshButtons();
	}

	/// <summary>
	/// Loads the proper information onto the buttons, disabling unused ones.
	/// </summary>
	private void RefreshButtons()
	{
		MenuRadial menuRadial = RadialStack.Peek();
		for (int i = 0; i < 8; i++)
		{
			if (i >= menuRadial.Options.Count)
			{
				RadialButtons[i].gameObject.SetActive(false);
				continue;
			}
			RadialButtons[i].gameObject.SetActive(true);
			string name = menuRadial.Options[i].name;
			name.Replace('_', ' ');
			RadialButtons[i].GetComponentInChildren<Text>().text = name;
		}
	}

	/// <summary>
	/// Callback for pressing an option on the radial menu.
	/// Should really only one of two things, either select a brick or open a new menu.
	/// </summary>
	public void PressRadialButton(int _buttonIndex)
	{
		SoundEffectsRef.PlayClick();
		MenuRadial menuRadial = RadialStack.Peek();
		Object radialChoice = menuRadial.Options[_buttonIndex];

		// First check if it's a menu radial
		if (radialChoice is MenuRadial)
		{
			PushRadial(radialChoice as MenuRadial);
			return;
		}

		// Check for any special commands
		if (radialChoice is SpecialCommand)
		{
			SpecialCommandOption option = (radialChoice as SpecialCommand).MyOption;
			switch (option)
			{
				case SpecialCommandOption.CreateLastBlock:
					if (lastChosenPrefab == null)
						return;
					BlockBuilderRef.CreateBlockGhost(lastChosenPrefab);
					RadialStack.Clear();
					SetActive(false);
					return;
				default:
					return;
			}
		}

		// At this point it should be a chosen block type
		lastChosenPrefab = radialChoice as GameObject;
		BlockBuilderRef.CreateBlockGhost(lastChosenPrefab);
		RadialStack.Clear();
		SetActive(false);
	}
}
