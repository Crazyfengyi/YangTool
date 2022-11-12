using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YangTools.Timer;
using YangTools.UGUI;

namespace YangTools
{
    /// <summary>
    /// 工具总管理器
    /// </summary>
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
        /// <summary>
        /// 对象池父节点
        /// </summary>
        public static GameObject GamePoolObject { get; private set; }
        #endregion

        #region 初始化
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (isInit) return;
            isInit = true;
            InitDontDestoryObject();
            //保证模块创建
            YangToolsManager.GetModule<YangUIManager>();
            YangToolsManager.GetModule<YangAudioManager>();
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
            GamePoolObject = new GameObject("GamePoolsObject");
            GamePoolObject.transform.SetParent(DontDestoryObject.transform);
        }
        #endregion

        #region 生命周期
        /// <summary>
        /// 游戏模块轮询
        /// </summary>
        /// <param name="deltaTime">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTime">真实流逝时间,以秒为单位</param>
        public static void Update(float deltaTime, float unscaledDeltaTime)
        {
            foreach (GameModuleBase module in GameModulesList)
            {
                module.Update(deltaTime, unscaledDeltaTime);
            }
        }
        /// <summary>
        /// 游戏退出时
        /// </summary>
        public static void OnApplicationQuit()
        {
            CloseAllModule();
        }
        #endregion

        #region 模块管理
        /// <summary>
        /// 游戏管理器链表
        /// </summary>
        private static readonly LinkedList<GameModuleBase> GameModulesList = new LinkedList<GameModuleBase>();
        /// <summary>
        /// 获取游戏框架模块
        /// </summary>
        /// <typeparam name="T">要获取的游戏模块类型</typeparam>
        /// <returns>要获取的游戏框架模块</returns>
        /// <remarks>如果要获取的游戏框架模块不存在,则自动创建该游戏框架模块</remarks>
        public static T GetModule<T>() where T : class
        {
            Type typeInfo = typeof(T);
            string moduleName = String.Format("{0}.{1}", typeInfo.Namespace, typeInfo.Name);
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null) throw new Exception(string.Format("Can not find Game Module type '{0}'", moduleName));

            return GetModule(moduleType) as T;
        }
        /// <summary>
        /// 获取游戏模块
        /// </summary>
        private static GameModuleBase GetModule(Type moduleType)
        {
            foreach (GameModuleBase module in GameModulesList)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }
        /// <summary>
        /// 创建游戏模块
        /// </summary>
        private static GameModuleBase CreateModule(Type moduleType)
        {
            //创建并初始化
            GameModuleBase resultModule = (GameModuleBase)Activator.CreateInstance(moduleType);
            resultModule.InitModule();
            if (resultModule == null) throw new Exception(string.Format("Can not create module '{0}'", moduleType.FullName));

            //按优先级添加进链表
            LinkedListNode<GameModuleBase> currentNode = GameModulesList.First;
            while (currentNode != null)
            {
                if (resultModule.Priority > currentNode.Value.Priority)
                {
                    break;
                }

                currentNode = currentNode.Next;
            }
            if (currentNode != null)
            {
                GameModulesList.AddBefore(currentNode, resultModule);
            }
            else
            {
                GameModulesList.AddLast(resultModule);
            }

            return resultModule;
        }
        /// <summary>
        /// 关闭所有游戏框架框架模块
        /// </summary>
        private static void CloseAllModule()
        {
            //先关闭优先级低的
            for (LinkedListNode<GameModuleBase> current = GameModulesList.Last; current != null; current = current.Previous)
            {
                current.Value.CloseModule();
            }
            GameModulesList.Clear();
        }
        #endregion
    }

    /// <summary>
    /// 游戏模块
    /// </summary>
    internal abstract class GameModuleBase
    {
        /// <summary>
        /// 优先级
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询</remarks>
        internal virtual int Priority => 0;
        /// <summary>
        /// 初始化模块
        /// </summary>
        internal abstract void InitModule();
        /// <summary>
        /// 模块轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位(Time.delaTime)</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位(Time.unscaledDeltaTime)</param>
        internal abstract void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds);
        /// <summary>
        /// 关闭模块
        /// </summary>
        internal abstract void CloseModule();
    }
}
