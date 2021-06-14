using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialCommand", menuName = "ScriptableObjects/SpecialCommand", order = 1)]
public class SpecialCommand : ScriptableObject
{
	public SpecialCommandOption MyOption;
}

public enum SpecialCommandOption
{
	CreateLastBlock,
	SetColor,
}
