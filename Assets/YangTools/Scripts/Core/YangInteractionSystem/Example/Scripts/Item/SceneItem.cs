using YangObjectInteract;
using System;
using UnityEngine;
using YangInteractionSystem;

/// <summary>
/// 场景对象
/// </summary>
public class SceneItem : MonoBehaviour, ICanDrag 
{
	public Sprite iconSprite;
	public Type InteractTag => typeof(SceneItem);
	public bool EnableDrag => true;
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
	public void OnDrag()
	{
		DragTempIcon.Instance.ShowGhostIcon(iconSprite); 
		gameObject.SetActive(false);
	}
	public void OnDragging()
	{
	}
	public void EndDrag(ICanFocus target) 
	{
		DragTempIcon.Instance.HideGhostIcon();
		//判断是否拖拽到了目标 且当前对象和目标对象是否属于同一个交互对象
		if (target == null || !this.IsBelongToSameCase(target))
		{
			gameObject.SetActive(true);
		}
	}
}
