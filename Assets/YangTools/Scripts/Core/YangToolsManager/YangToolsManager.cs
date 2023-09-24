using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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
            //手动创建模块(顺序问题)
            YangToolsManager.GetModule<YangUIManager>();
            YangToolsManager.GetModule<YangAudioManager>();
            //初始化未手动创建的模块
            for (int i = 0; i < allGameModule.Count; i++)
            {
                TypeInfo item = allGameModule[i];
                //还没有创建过
                if (!allCreateModuleName.Contains(item.FullName))
                {
                    GameModuleBase baseScript = item.Assembly.CreateInstance(item.FullName) as GameModuleBase;
                    AddModuleToLinkList(baseScript);
                }
            }
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

        //所有游戏模块
        private readonly static List<TypeInfo> allGameModule = GetAllGameModule();
        //已经创建过的模块全名
        private readonly static List<string> allCreateModuleName = new List<string>();
        //获得所有游戏模块
        private static List<TypeInfo> GetAllGameModule()
        {
            List<TypeInfo> tempList = new List<TypeInfo>();
            Type baseType = typeof(GameModuleBase);
            //获取执行程序集,定义的类型
            foreach (var typeInfo in System.Reflection.Assembly.GetExecutingAssembly().DefinedTypes)
            {
                //是class&&是子类
                if (typeInfo.IsClass && typeInfo.IsSubclassOf(baseType))
                {
                    tempList.Add(typeInfo);
                }
            }
            return tempList;
        }
        #endregion

        #region 模块管理
        /// <summary>
        /// 游戏模块链表
        /// </summary>
        private static readonly LinkedList<GameModuleBase> GameModulesList = new LinkedList<GameModuleBase>();
        /// <summary>
        /// 获得模块
        /// </summary>
        /// <remarks>模块不存在,自动创建模块</remarks>
        public static T GetModule<T>() where T : GameModuleBase
        {
            Type typeInfo = typeof(T);
            if (typeInfo == null)
            {
                throw new Exception($"不能找到模块{typeInfo.Name}");
            }

            return GetModule(typeInfo) as T;
        }
        /// <summary>
        /// 获得模块
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
        /// 创建模块
        /// </summary>
        private static GameModuleBase CreateModule(Type moduleType)
        {
            //创建并初始化
            GameModuleBase newModule = (GameModuleBase)Activator.CreateInstance(moduleType);
            if (newModule == null) throw new Exception($"不能创建模块:{moduleType.FullName}");
            AddModuleToLinkList(newModule);
            return newModule;
        }
        /// <summary>
        /// 添加模块到链表
        /// </summary>
        public static void AddModuleToLinkList(GameModuleBase newModule)
        {
            newModule.InitModule();
            allCreateModuleName.Add(newModule.GetType().FullName);
            //按优先级添加进链表
            LinkedListNode<GameModuleBase> currentNode = GameModulesList.First;
            while (currentNode != null)
            {
                if (newModule.Priority > currentNode.Value.Priority)
                {
                    break;
                }

                currentNode = currentNode.Next;
            }
            if (currentNode != null)
            {
                GameModulesList.AddBefore(currentNode, newModule);
            }
            else
            {
                GameModulesList.AddLast(newModule);
            }

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
    }

    /// <summary>
    /// 游戏模块基类
    /// </summary>
    public abstract class GameModuleBase
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
