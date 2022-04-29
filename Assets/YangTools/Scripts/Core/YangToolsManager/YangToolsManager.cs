using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YangTools.Timer;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        #region 属性
        /// <summary>
        /// 是否初始化
        /// </summary>
        private static bool isInit = false;
        /// <summary>
        /// 获得工具类的Unity物体(不可删除)
        /// </summary>
        public static GameObject DontDestoryObject { get; private set; }
        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数，为保证顺序必须有
        /// </summary>
        static YangToolsManager()
        {
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (isInit) return;
            isInit = true;

            InitDontDestoryObject();
            //TODO需要相关文件夹自动生成
            DataCore.Init();
        }
        /// <summary>
        /// 在Unity中创建一个不能删除的物体
        /// </summary>
        private static void InitDontDestoryObject()
        {
            DontDestoryObject = new GameObject("YangToolsObject");
            DontDestoryObject.AddComponent<UnityLoopScript>();
            //DontDestoryObject.hideFlags = HideFlags.HideInHierarchy; //在层级面板隐藏
            UnityEngine.Object.DontDestroyOnLoad(DontDestoryObject);
        }
        #endregion

        #region 生命周期
        /// <summary>
        /// 所有游戏模块轮询。
        /// </summary>
        /// <param name="deltaTime">逻辑流逝时间，以秒为单位。</param>
        /// <param name="unscaledDeltaTime">真实流逝时间，以秒为单位。</param>
        public static void Update(float deltaTime, float unscaledDeltaTime)
        {
            foreach (GameModuleManager module in GameModulesList)
            {
                module.Update(deltaTime, unscaledDeltaTime);
            }
        }
        /// <summary>
        /// 游戏退出时
        /// </summary>
        public static void OnApplicationQuit()
        {

        }
        #endregion

        #region 模块管理
        /// <summary>
        /// 游戏管理器链表
        /// </summary>
        private static readonly LinkedList<GameModuleManager> GameModulesList = new LinkedList<GameModuleManager>();
        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void CloseAllModule()
        {
            for (LinkedListNode<GameModuleManager> current = GameModulesList.Last; current != null; current = current.Previous)
            {
                current.Value.CloseModule();
            }
            GameModulesList.Clear();
        }
        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏模块类型.</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            Type typeInfo = typeof(T);
            string moduleName = String.Format("{0}.{1}", typeInfo.Namespace, typeInfo.Name);
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new Exception(string.Format("Can not find Game Module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }
        /// <summary>
        /// 获取游戏框架模块
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型.</param>
        /// <returns>要获取的游戏框架模块.</returns>
        /// <remarks>如果要获取的游戏框架模块不存在,则自动创建该游戏框架模块.</remarks>
        private static GameModuleManager GetModule(Type moduleType)
        {
            foreach (GameModuleManager module in GameModulesList)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }
            return CreateModule(moduleType);
        }
        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型.</param>
        /// <returns>要创建的游戏框架模块.</returns>
        private static GameModuleManager CreateModule(Type moduleType)
        {
            GameModuleManager module = (GameModuleManager)Activator.CreateInstance(moduleType);
            module.Init();

            if (module == null)
            {
                throw new Exception(string.Format("Can not create module '{0}'.", moduleType.FullName));
            }

            LinkedListNode<GameModuleManager> current = GameModulesList.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                GameModulesList.AddBefore(current, module);
            }
            else
            {
                GameModulesList.AddLast(module);
            }

            return module;
        }
        #endregion
    }
    /// <summary>
    /// 游戏模块父类
    /// </summary>
    internal abstract class GameModuleManager
    {
        /// <summary>
        /// 获取游戏模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行.</remarks>
        internal virtual int Priority
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        internal abstract void Init();
        /// <summary>
        /// 游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间,以秒为单位.(Time.delaTime)</param>
        /// <param name="realElapseSeconds">真实流逝时间,以秒为单位.(Time.unscaleDelaTime))</param>
        internal abstract void Update(float elapseSeconds, float realElapseSeconds);
        /// <summary>
        /// 关闭并清理游戏框架模块。
        /// </summary>
        internal abstract void CloseModule();
    }
}
