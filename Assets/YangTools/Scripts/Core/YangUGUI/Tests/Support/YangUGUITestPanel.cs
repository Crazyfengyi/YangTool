#if UNITY_EDITOR
using System;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// 为编辑器内的 PlayMode 回归验证提供真实界面生命周期
    /// </summary>
    internal sealed class YangUGUITestPanel : UGUIPanelBase<DefaultUGUIDataBase>
    {
        internal static Action<Component> Created; //创建回调
        internal static Action<Component> Initialized; //初始化回调
        internal static Action<Component> Opened; //打开回调
        internal static Action<Component> Updated; //轮询回调

        /// <summary>
        /// 在实例创建阶段模拟管理器关闭
        /// </summary>
        private void Awake()
        {
            Created?.Invoke(this);
        }

        /// <summary>
        /// 使用真实基类初始化并执行测试回调
        /// </summary>
        public override void OnInit(DefaultUGUIDataBase data)
        {
            base.OnInit(data);
            Initialized?.Invoke(this);
        }

        /// <summary>
        /// 使用真实基类打开并执行测试回调
        /// </summary>
        public override void OnOpen(object data)
        {
            base.OnOpen(data);
            Opened?.Invoke(this);
        }

        /// <summary>
        /// 在管理器轮询中验证注册和取消行为
        /// </summary>
        public override void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            Updated?.Invoke(this);
        }
    }
}
#endif
