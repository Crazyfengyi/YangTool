using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

        #region 深拷贝

        /// <summary>
        /// 深度拷贝-支持循环引用
        /// </summary>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            //用于跟踪已复制的对象，防止循环引用导致的无限递归
            var trackedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return DeepCopyInternal(obj, trackedObjects);
        }

        private static T DeepCopyInternal<T>(T obj, Dictionary<object, object> trackedObjects)
        {
            // 处理null值
            if (obj == null)
                return default(T);

            // 检查是否已经复制过该对象
            if (trackedObjects.TryGetValue(obj, out var existingCopy))
                return (T) existingCopy;

            // 处理值类型和字符串（字符串虽然是引用类型，但它是不可变的）
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
                return obj;

            // 处理数组
            if (type.IsArray)
            {
                var array = (Array) (object) obj;
                var newArray = (Array) Array.CreateInstance(type.GetElementType(), array.Length);
                trackedObjects[obj] = newArray;

                for (int i = 0; i < array.Length; i++)
                {
                    var element = array.GetValue(i);
                    var newElement = DeepCopyInternal(element, trackedObjects);
                    newArray.SetValue(newElement, i);
                }

                return (T) (object) newArray;
            }

            // 创建新实例
            object newObj;
            if (type.IsValueType)
            {
                newObj = obj;
            }
            else
            {
                // 尝试获取无参构造函数
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    newObj = constructor.Invoke(null);
                }
                else
                {
                    // 如果没有无参构造函数，使用FormatterServices创建未初始化的对象
                    newObj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                }
            }

            // 记录已复制的对象
            trackedObjects[obj] = newObj;

            // 复制所有字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // 跳过静态字段和只读字段
                if (field.IsStatic || field.IsInitOnly)
                    continue;

                var fieldValue = field.GetValue(obj);
                var newFieldValue = DeepCopyInternal(fieldValue, trackedObjects);
                field.SetValue(newObj, newFieldValue);
            }

            return (T) newObj;
        }

        /// <summary>
        /// 序列化克隆类对象
        /// </summary>
        public static T Clone<T>(T realObject)
        {
            using Stream objStream = new MemoryStream();
            //利用System.Runtime.Serialization序列化与反序列化完成引用对象的复制
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objStream, realObject);
            objStream.Seek(0, SeekOrigin.Begin);
            return (T) formatter.Deserialize(objStream);
        }

        /// <summary>
        /// 表达式树深拷贝
        /// 对象的相互引用,会导致方法循环调用
        /// 这个是最快的---其原理是反射和表达式树相结合,先用反射获取字段然后缓存起来,再用表达式树赋值 https://www.cnblogs.com/SF8588/p/16152078.html
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        public static class TransExpCopy<TIn, TOut>
        {
            private static readonly Func<TIn, TOut> cache = GetFunc();

            private static Func<TIn, TOut> GetFunc()
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
                List<MemberBinding> memberBindingList = new List<MemberBinding>();

                foreach (var item in typeof(TOut).GetProperties())
                {
                    if (!item.CanWrite) continue;
                    MemberExpression property =
                        Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }

                MemberInitExpression memberInitExpression =
                    Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
                Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression,
                    new ParameterExpression[] {parameterExpression});

                return lambda.Compile();
            }

            public static TOut Trans(TIn tIn)
            {
                return cache(tIn);
            }
        }

        #endregion

        #region 数据处理

        /// <summary>
        /// 编码分隔符
        /// </summary>
        private const string EncodeSeparator = ".";

        /// <summary>
        /// string 转 byte string  中文转byte存储
        /// </summary>
        private static string EncodeStringToByte(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return string.Join(EncodeSeparator, Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// byte string 转 string
        /// </summary>
        public static string DecodeByteToString(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            string[] encodedArray = str.Split(new string[] {EncodeSeparator}, StringSplitOptions.RemoveEmptyEntries);
            return Encoding.UTF8.GetString(Array.ConvertAll(encodedArray, s => byte.Parse(s)));
        }

        // encodeChinese ()  bytes[]  .join(".")  "111.255.133.132.321.3.545.656.656.67.434"
        // decode  "111.255.133.132.321.3.545.656.656.67.434" -> split()   ["111","255"]  => byte.pare()=> byte[]  => convert=>  string 

        #endregion

        #region 自动保存类

        /// <summary>
        /// 自动保存类(用的ES3.Save)
        /// </summary>
        /// <typeparam name="T">要保存的类型</typeparam>
        public class AutoSave<T>
        {
            private bool isLoad;

            /// <summary>
            /// 自动保存用的Key
            /// </summary>
            public string SaveKey { get; }

            /// <summary>
            /// 是否开启自动保存--重复连续更改Value请暂时关闭
            /// </summary>
            public bool IsOpenAutoSave { get; }

            private T value;

            public T Value
            {
                get
                {
                    if (isLoad == false)
                    {
                        Load();
                    }

                    return value;
                }
                set
                {
                    this.value = value;
                    if (IsOpenAutoSave)
                    {
                        Save();
                    }
                }
            }

            /// <summary>
            /// 默认使用的key:调用方的方法名+文件路径
            /// </summary>
            /// <param name="saveKey">保存用的Key</param>
            /// <param name="callName">key1:默认为调用方的方法名/字段名</param>
            /// <param name="path">key2:默认为调用方文件路径名</param>
            public AutoSave(string saveKey = "", [CallerMemberName] string callName = "",
                [CallerFilePath] string path = "")
            {
                if (string.IsNullOrEmpty(saveKey))
                {
                    SaveKey = callName + "_" + path.GetHashCode();
                }
                else
                {
                    SaveKey = saveKey;
                }

                IsOpenAutoSave = true;
            }

            public void Save()
            {
                ES3.Save<T>(SaveKey, value);
            }

            public void Load()
            {
                isLoad = true;
                if (ES3.KeyExists(SaveKey))
                {
                    T loadValue = ES3.Load<T>(SaveKey);
                    value = loadValue;
                }
            }
        }

        #endregion
    }

    #region 深拷贝相关

    //用于引用比较的特殊比较器
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        public new bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    /*
     * /创建循环引用
    var node1 = new Node("Node1");
    var node2 = new Node("Node2");
    node1.Next = node2;
    node2.Next = node1;  // 创建循环引用
    // 执行深拷贝
    var copiedNode1 = DeepCopyHelper.DeepCopy(node1);

    // 验证拷贝结果
    Console.WriteLine(copiedNode1.Name);  // 输出: Node1
    Console.WriteLine(copiedNode1.Next.Name);  // 输出: Node2
    Console.WriteLine(copiedNode1.Next.Next.Name);  // 输出: Node1
    Console.WriteLine(ReferenceEquals(node1, copiedNode1));  // 输出: False
    Console.WriteLine(ReferenceEquals(node1.Next, copiedNode1.Next));  // 输出: False
    Console.WriteLine(ReferenceEquals(copiedNode1, copiedNode1.Next.Next));  // 输出: True
    */

    #endregion
}