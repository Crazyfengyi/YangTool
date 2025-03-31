using YangObjectInteract;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI对象
/// </summary>
public class UIItem : InteractUIBase,ICanDrag  
{
	public Image icon;
	public Type InteractTag => typeof(UIItem);
	public bool EnableFocus => true;
	public bool EnableDrag { get; private set; } = true;

	private void Update()
	{
		EnableDrag = icon.gameObject.activeSelf;
	}
	public void OnFocus()
	{
	}
	public void EndFocus()
	{
	}
	public void OnDrag()
	{
		if (!icon.gameObject.activeSelf) return;
		DragTempIcon.Instance.ShowGhostIcon(icon.sprite); 
		icon.gameObject.SetActive(false);
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
			icon.gameObject.SetActive(true);
		}
	}
}
