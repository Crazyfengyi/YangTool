using System;

namespace YangObjectInteract
{
	/// <summary>
	/// 物品互动标识
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
    public class ObjectInteractAttribute : Attribute
    {
        public readonly Type InteractSubject;
        public readonly Type InteractTarget;
        public readonly bool EnableExecuteOnLoad;

		/// <summary>
		/// 物品互动标识
		/// </summary>
		/// <param name="subject">互动主体类型</param>
		/// <param name="target">互动目标类型</param>
		/// <param name="enableExecuteOnLoad">默认开启执行</param>
		public ObjectInteractAttribute(Type subject, Type target, bool enableExecuteOnLoad = true)
        {
            InteractSubject = subject;
            InteractTarget = target;
            EnableExecuteOnLoad = enableExecuteOnLoad;
        }
    }
}
