using System.Collections.Generic;
using UnityEngine;
namespace YangTools
{
    public class StaticInfo
    {
        public object id;

        //----------------------------------------
        /// <summary>
        /// 整型ID
        /// </summary>
        public int Id
        {
            get
            {
                return (int)id;
            }
            set
            {
                id = value;
            }
        }
        /// <summary>
        /// 字符串ID
        /// </summary>
        public string IdStr
        {
            get
            {
                return (string)id;
            }
            set
            {
                id = value;
            }
        }
        //----------------------------------------
        /// <summary>
        /// 当字段被加载时回调该函数
        /// [返回是否已经过你的手动赋值]
        /// </summary>
        public virtual bool OnFieldLoadProgress(FieldLoadProgressInfo progressInfo)
        {
            return false;
        }
    }

    public static class StaticGroup<T>
        where T : StaticInfo, new()
    {
        /// <summary>
        /// 是否曾经加载过该静态表
        /// </summary>
        public static bool HasReloaded { get; set; }
        /// <summary>
        /// 属性：是否是字符串ID
        /// </summary>
        public static bool IsStrId { get; private set; }
        /// <summary>
        /// 整型字典
        /// </summary>
        private static Dictionary<int, T> infos;
        /// <summary>
        /// 属性：整型键信息组
        /// </summary>
        public static Dictionary<int, T> Infos
        {
            get
            {
                if (infos == null)
                {
                    infos = new Dictionary<int, T>();
                }
                return infos;
            }

            private set
            {
                infos = value;
            }
        }
        /// <summary>
        /// 字符串字典
        /// </summary>
        private static Dictionary<string, T> strIdInfos;
        /// <summary>
        /// 属性：字符键信息组
        /// </summary>
        public static Dictionary<string, T> StrIdInfos
        {
            get
            {
                if (strIdInfos == null)
                {
                    strIdInfos = new Dictionary<string, T>();
                }
                return strIdInfos;
            }

            private set
            {
                strIdInfos = value;
            }
        }

        /// <summary>
        /// 从本地json文件读取到信息组（不检查云端热更新的表格文件）
        /// </summary>
        /// <param name="filePathWithNoSuffix">不带后缀名的json文件位置</param>
        /// <param name="asStrId">是否将id作为字符串进行加载</param>
        public static bool ReloadFromLocalJsonFile(string filePathWithNoSuffix, bool asStrId = false)
        {
            var jsonData = DataCore.GetJsonFromAssetsResources(filePathWithNoSuffix);
            if (jsonData == null) return false;

            HasReloaded = true;

            if (asStrId)
            {
                IsStrId = true;

                DataCore.ReloadStringDictFromJsonDataGroup(StrIdInfos, jsonData, (loadProgressInfo) => (loadProgressInfo.ObjectInstance as T).OnFieldLoadProgress(loadProgressInfo));
                return StrIdInfos.Count != 0;
            }
            else
            {
                IsStrId = false;
                DataCore.ReloadIntDictFromJsonDataGroup(Infos, jsonData, (loadProgressInfo) => (loadProgressInfo.ObjectInstance as T).OnFieldLoadProgress(loadProgressInfo));
                return Infos.Count != 0;
            }
        }

        /// <summary>
        /// 通过整型键获取一个静态信息
        /// </summary>
        public static T GetStaticInfoById(int id)
        {
            T ret;
            bool isFind = Infos.TryGetValue(id, out ret);
            return isFind ? ret : null;
        }

        /// <summary>
        /// 通过字符键获取一个静态信息
        /// </summary>
        public static T GetStaticInfoById(string strId)
        {
            T ret;
            bool isFind = StrIdInfos.TryGetValue(strId, out ret);
            return isFind ? ret : null;
        }
    }
}