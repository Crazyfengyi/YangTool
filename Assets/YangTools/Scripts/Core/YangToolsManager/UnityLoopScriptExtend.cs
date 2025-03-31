using YangTools.Scripts.Core;

namespace YangObjectInteract
{
	/// <summary>
	/// 使用游戏Mono生命周期
	/// </summary>
	public interface IUnityLoopScriptExtend
    {
        public void FixedUpdate();
        public void Update();
        public void LateUpdate();
        public bool InUpdate { get; set; }
    }

	/// <summary>
	/// 游戏MonoUpdater扩展
	/// </summary>
	public static class UnityLoopScriptExtend
    {
	    // ReSharper disable Unity.PerformanceAnalysis
	    /// <summary>
		/// 启用MonoUpdate
		/// </summary>
		public static void EnableMonoUpdater(this IUnityLoopScriptExtend self)
        {
            if (self.InUpdate) return;
            UnityLoopScript.AddUpdateAction(self.Update);
            UnityLoopScript.AddFixedUpdateAction(self.FixedUpdate);
            UnityLoopScript.AddLateUpdateAction(self.LateUpdate);
            self.InUpdate = true;
        }
		/// <summary>
		/// 禁用MonoUpdate
		/// </summary>
		public static void DisableMonoUpdater(this IUnityLoopScriptExtend self)
        {
            UnityLoopScript.RemoveUpdateAction(self.Update);
            UnityLoopScript.RemoveFixedUpdateAction(self.FixedUpdate);
            UnityLoopScript.RemoveLateUpdateAction(self.LateUpdate);
            self.InUpdate = false;
        }
    }
}