using YangObjectInteract;
using System;
using UnityEngine;

/// <summary>
/// 从UIItem拖拽到场景中的交互情景
/// </summary>
[ObjectInteract(typeof(UIItem), typeof(Table))]
public class DragToScene : DragTargetInteractBase 
{
	private UIItem uiItem;
	private Table table;
	public DragToScene(Type subject, Type target) : base(subject, target)
	{
	}

	protected override void OnEnter(ICanDrag subject, ICanFocus target) 
	{
		uiItem = (subject as UIItem);
		table = (target as Table);
		table?.tempItem.gameObject.SetActive(true);
	}

	protected override void OnExecute(ICanDrag subject, ICanFocus target)
	{
		if (EndDrag) 
		{
			GameObject.FindObjectOfType<SceneItem>(true).gameObject.SetActive(true);
		}
	}

	protected override void OnExit()
	{
		table.tempItem.gameObject.SetActive(false);
		uiItem.icon.gameObject.SetActive(false);
	}
}
