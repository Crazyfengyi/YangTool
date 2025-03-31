using System;
using UnityEngine;
using YangObjectInteract;
using YangInteractionSystem;

/// <summary>
/// 放置物体的台面
/// </summary>
public class Table : MonoBehaviour, ICanFocus  
{
	public GameObject tempItem;
	public Type InteractTag => typeof(Table);
	public bool EnableFocus => true;
	private Outline outline;
	private void Awake()
	{
		outline = GetComponent<Outline>();
	}
	public void OnFocus()
	{
		outline.enabled = true;
	}
	public void EndFocus()
	{
		outline.enabled = false;
	}
}
