using YangObjectInteract;
using System;

/// <summary>
/// 拖拽物体到UI上的交互情景
/// </summary>
[ObjectInteract(typeof(SceneItem), typeof(UIItem))]
public class DragToUI : DragTargetInteractBase 
{
	private SceneItem sceneItem;
	private UIItem uiItem;
	public DragToUI(Type subject, Type target) : base(subject, target)
	{
	}
	protected override void OnEnter(ICanDrag subject, ICanFocus target)
	{
		sceneItem = (subject as SceneItem);
		uiItem = (target as UIItem);
	}
	protected override void OnExecute(ICanDrag subject, ICanFocus target) 
	{
		if (EndDrag)
		{
			uiItem.icon.gameObject.SetActive(true);
			uiItem.icon.sprite = sceneItem.iconSprite;
		}
	}
	protected override void OnExit()
	{
		sceneItem.gameObject.SetActive(false);
	}
}
