using YangObjectInteract;
using System;
using System.Collections.Generic;

public static class IInteractableExtension
{
	/// <summary>
	/// 判断两个交互对象是否属于同一个交互情景
	/// </summary>
	public static bool IsBelongToSameCase(this IInteract self, IInteract other) 
	{
		return YangInteractSystem.Instance.InteractRelationMapper.TryGetValue(self.InteractTag, out List<Type> types) && types.Contains(other.InteractTag);
	}
}
