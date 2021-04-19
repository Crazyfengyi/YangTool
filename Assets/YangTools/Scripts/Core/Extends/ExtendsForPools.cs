using System.Collections.Generic;
using UnityEngine;

namespace YangTools
{
    public partial class Extends
    {
        /// <summary>
        /// 用于对象池的名字绑定--把名字绑定在HashCode
        /// </summary>
        public static Dictionary<int, string> extendName = new Dictionary<int, string>();

        /// <summary>
        /// 绑定名字在HashCode上
        /// </summary>
        /// <param name="obj"></param>
        public static void SetExtendName(this GameObject obj)
        {
            int key = obj.GetHashCode();
            if (!extendName.ContainsKey(key))
            {
                extendName.Add(key, obj.name);
            }
        }

        /// <summary>
        /// 获得HashCode上的名字
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetExtendName(this GameObject obj)
        {
            int key = obj.GetHashCode();

            string result = null;
            if (extendName.ContainsKey(key))
            {
                result = extendName[key];
            }
            return result;
        }

        /// <summary>
        /// 判断物体是否在对象池中
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsInGamePool(this GameObject obj)
        {
            return extendName.ContainsKey(obj.GetHashCode());
        }
    }
}
