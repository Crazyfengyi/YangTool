using System;
using UnityEngine;

namespace YangObjectInteract
{
	/// <summary>
	/// 拖拽物体到目标物体的交互情景
	/// </summary> 
	public abstract class DragTargetInteractBase : InteractBase
	{
		private bool isEnter;
		private bool isExit = true;
		public DragTargetInteractBase(Type subject, Type target) : base(subject, target)
		{
		}
		protected abstract void OnExecute(ICanDrag subject, ICanFocus target);
		public override bool Execute(ICanFocus currentFocus, ICanSelect currentSelect, ICanDrag currentDrag) 
		{
			//拖拽对象和目标对象都不能为空且对象类型匹配才能执行
			if ( currentDrag == null || currentFocus == null || currentFocus.InteractTag != Target || currentDrag.InteractTag != Subject)
			{
				if (isEnter)
				{
					isEnter = false;
					OnExit();
					isExit = true;
				}
				return false;
			}

			//触发互动
			if (isExit)
			{
				isExit = false;
				OnEnter(currentDrag, currentFocus);
				isEnter = true;
			}

			OnExecute(currentDrag, currentFocus);
			return true;
		}

		/// <summary>
		/// 进入交互情景
		/// </summary>
		protected virtual void OnEnter(ICanDrag subject, ICanFocus target)
		{
		}
		/// <summary>
		/// 退出交互情景
		/// </summary>
		protected virtual void OnExit()
		{
		}
		/// <summary>
		/// 是否结束拖拽
		/// </summary>
		protected virtual bool EndDrag => Input.GetMouseButtonUp(0);
	}
}
