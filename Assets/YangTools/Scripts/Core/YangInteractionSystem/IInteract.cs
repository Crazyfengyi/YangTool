using System;

namespace YangObjectInteract
{
	public interface IInteract
	{
		/// <summary>
		/// 互动对象标识
		/// </summary>
		public Type InteractTag { get; } 
	}

	/// <summary>
	/// 互动:可聚焦接口
	/// </summary>
	public interface ICanFocus : IInteract
	{
		public bool EnableFocus { get; }
		/// <summary>
		/// 焦点进入
		/// </summary>
		public void OnFocus();
		/// <summary>
		/// 焦点离开
		/// </summary>
		public void EndFocus();
	}

	public interface ICanSelect : ICanFocus, IInteract
	{
		public bool EnableSelect { get; }
		/// <summary>
		/// 选择
		/// </summary>
		public void OnSelect();
		/// <summary>
		/// 取消选择
		/// </summary>
		public void EndSelect();
	}

	public interface ICanDrag : ICanFocus, IInteract
	{
		public bool EnableDrag { get; }
		/// <summary>
		/// 开始拖拽
		/// </summary>
		public void OnDrag();
		/// <summary>
		/// 拖拽中
		/// </summary>
		public void OnDragging();
		/// <summary>
		/// 结束拖拽
		/// </summary>
		public void EndDrag(ICanFocus target);
	}
}