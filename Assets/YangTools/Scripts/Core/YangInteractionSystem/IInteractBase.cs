using System;

namespace YangObjectInteract
{
	/// <summary>
	/// 交互具体情景--类似ECS system处理关联类的交互
	/// </summary>
	public interface IInteractBase
	{
		public Type Subject { get; }
		public Type Target { get; }
		public bool Enable { get; set; }
		/// <summary>
		/// 执行交互
		/// </summary>
		/// <param name="currentFocus">当前聚焦对象</param>
		/// <param name="currentSelect">当前选择对象</param>
		/// <param name="currentDrag">当前拖拽对象</param>
		/// <returns>当前情景是否被激活</returns>
		public bool Execute(ICanFocus currentFocus, ICanSelect currentSelect, ICanDrag currentDrag);
	}

	/// <summary>
	/// 具体交互情景基类
	/// </summary>
	public abstract class InteractBase : IInteractBase
	{
		public Type Subject { get; }
		public Type Target { get; }
		/// <summary>
		/// 是否开启
		/// </summary>
		public bool Enable { get; set; }
		public InteractBase(Type subject, Type target)
		{
			Subject = subject;
			Target = target;
		}
		/// <summary>
		/// 执行交互
		/// </summary>
		public abstract bool Execute(ICanFocus currentFocus, ICanSelect currentSelect, ICanDrag currentDrag);
	}
}
