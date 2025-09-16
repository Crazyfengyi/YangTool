using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using YangTools.Scripts.Core.YangObjectPool;

namespace YangTools.Scripts.Core
{
    public static partial class YangToolsManager
    {
        #region 判断
        /// <summary>
        /// 是否有泛型接口
        /// </summary>
        private static bool HaveInterface(Type type)
        {
            return Array.Exists(type.GetInterfaces(), t =>
            {
                //是泛型
                if (t.IsGenericType)
                {
                    return t.GetGenericTypeDefinition() == typeof(IPoolItem<>);
                }

                return false;
            });
        }

        /// <summary>
        /// 判断事件列表中是否存在这个委托
        /// </summary>
        public static bool EventHaveFunction(Action<int> onClick, Action<int> func)
        {
            Delegate[] list = onClick.GetInvocationList();

            return Convert.ToBoolean(Array.IndexOf(list, func));
        }
        #endregion
    }
}