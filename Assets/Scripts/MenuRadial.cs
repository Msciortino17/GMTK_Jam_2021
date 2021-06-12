using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuRadial", menuName = "ScriptableObjects/MenuRadial", order = 1)]
public class MenuRadial : ScriptableObject
{
	public List<Object> Options = new List<Object>();
}
