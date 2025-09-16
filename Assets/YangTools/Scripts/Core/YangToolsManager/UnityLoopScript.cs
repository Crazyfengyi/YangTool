/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-18
 */

using System;
using UnityEngine;
using YangTools.Scripts.Core.YangCoroutine;

namespace YangTools.Scripts.Core
{
    /// <summary>
    /// Unity生命周期脚本
    /// </summary>
    public class UnityLoopScript : MonoBehaviour
    {
        private static UnityLoopScript instance = null;

        public static UnityLoopScript Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("[UnityLoopScript]");
                    instance = obj.AddComponent<UnityLoopScript>();
                    DontDestroyOnLoad(obj);
                }

                return instance;
            }
        }

        public event Action UpdateAction;
        public event Action FixedUpdateAction;
        public event Action LateUpdateAction;
        
        /// <summary>
        /// 添加Update事件
        /// </summary>
        public static void AddUpdateAction(Action action) => Instance.UpdateAction += action;
        /// <summary>
        /// 移除Update事件
        /// </summary>
        public static void RemoveUpdateAction(Action action) => Instance.UpdateAction -= action;
        /// <summary>
        /// 添加FixedUpdate事件
        /// </summary>
        public static void AddFixedUpdateAction(Action action) => Instance.FixedUpdateAction += action;
        /// <summary>
        /// 移除FixedUpdate事件
        /// </summary>
        public static void RemoveFixedUpdateAction(Action action) => Instance.FixedUpdateAction -= action;
        /// <summary>
        /// 添加LateUpdate事件
        /// </summary>
        public static void AddLateUpdateAction(Action action) => Instance.LateUpdateAction += action;
        /// <summary>
        /// 移除LateUpdate事件
        /// </summary>
        public static void RemoveLateUpdateAction(Action action) => Instance.LateUpdateAction -= action;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            TimeTool.Init();
        }

        void Update()
        {
            UpdateAction?.Invoke();
            YangToolsManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            YangCoroutineManager.Instance.UpdateCoroutine();
            TimeTool.Update();
        }

        private void FixedUpdate()
        {
            FixedUpdateAction?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdateAction?.Invoke();
        }
        
        private void OnApplicationQuit()
        {
            YangToolsManager.OnApplicationQuit();
        }
    }
}