﻿
using System.Collections.Generic;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        #region 自动保存类型
        /// <summary>
        /// 可以自动存档读档的值（不支持容器类型）
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        public class AutoSaveValue<TValue>
        {
            /// <summary>
            /// 键
            /// </summary>
            public string Key { get; }
            /// <summary>
            /// 值
            /// </summary>
            private TValue value;
            /// <summary>
            /// 是否自动存档
            /// </summary>
            public bool isAutoSave = true;
            /// <summary>
            /// 是否加载过存档
            /// </summary>
            private bool hasLoadArchive;

            /// <summary>
            /// 初始化构建
            /// </summary>
            /// <param name="key"> 存档键</param>
            public AutoSaveValue(string key)
            {
                Key = key;
            }

            /// <summary>
            /// 初始化构建
            /// </summary>
            /// <param name="key">存档键</param>
            /// <param name="defaultValue">默认值</param>
            public AutoSaveValue(string key, TValue defaultValue)
            {
                Key = key;
                value = defaultValue;
            }

            /// <summary>
            /// 获得值（首次获取默认会从存档读取一次,"="赋值将自动存档,其它更改需手动Save）
            /// </summary>
            public TValue Value
            {
                get
                {
                    AutoLoad();
                    return value;
                }

                set
                {
                    this.value = value;
                    hasLoadArchive = true;
                    if (isAutoSave)
                    {
                        Save();
                    }
                }
            }

            /// <summary>
            /// 存档
            /// </summary>
            public void Save()
            {
                AutoLoad();
                if (value == null)
                {
                    DataCore.RemoveKey(Key);
                }
                else
                {
                    DataCore.SaveValue(Key, value);
                }
            }

            /// <summary>
            /// 自动读档
            /// </summary>
            private void AutoLoad()
            {
                if (!hasLoadArchive)
                {
                    TValue tValue;
                    if (DataCore.LoadValue(Key, out tValue))
                    {
                        value = tValue;
                    }
                    hasLoadArchive = true;
                }
            }

            /// <summary>
            /// 隐式转化
            /// </summary>
            public static implicit operator TValue(AutoSaveValue<TValue> value)
            {
                return value.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        /// <summary>
        /// 可以自动存档读档的列表
        /// </summary>
        /// <typeparam name="V">列表的子类型</typeparam>
        public class AutoSaveList<V>
        {
            /// <summary>
            /// 通过储存键来构造
            /// </summary>
            public AutoSaveList(string key)
            {
                Key = key;
            }
            /// <summary>
            /// 通过储存键和默认值来构造（如果有存档，将不使用默认值）
            /// </summary>
            public AutoSaveList(string key, List<V> defaultValue)
            {
                Key = key;
                _value = defaultValue;
            }
            /// <summary>
            /// 获得储存键
            /// </summary>
            public string Key { get; private set; }
            /// <summary>
            /// 是否自动存档
            /// </summary>
            public bool isAutoSave = true;
            private List<V> _value;
            private bool _hasInit;

            /// <summary>
            /// 获得值（该值初始化时会自动从存档读取一次，设置时也会自动存档，除非你将值保存出去之后自己更改，如果这样，你需要调用手动存档Save）
            /// </summary>
            public List<V> Value
            {
                get
                {
                    AutoLoad();
                    return _value;
                }

                set
                {
                    _value = value;
                    _hasInit = true;
                    if (isAutoSave) Save();
                }
            }

            /// <summary>
            /// 手动储存（当你对Value的内部属性进行手动更改后，你需要调用手动储存，否则不会储存）
            /// </summary>
            public void Save()
            {
                AutoLoad();
                if (_value == null)
                {
                    DataCore.RemoveKey(Key);
                }
                else
                {
                    DataCore.SaveValue(Key, _value);
                }
            }

            /// <summary>
            /// 自动读档（内部方法）
            /// </summary>
            public void AutoLoad()
            {
                if (!_hasInit)
                {
                    List<V> tValue;
                    if (DataCore.LoadValue(Key, out tValue))
                    {
                        _value = tValue;
                    }
                    _hasInit = true;
                }
            }

            /// <summary>
            /// 隐式转化
            /// </summary>
            public static implicit operator List<V>(AutoSaveList<V> value)
            {
                return value.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        /// <summary>
        /// 可以自动存档读档的设定集
        /// </summary>
        /// <typeparam name="V">设定集的子类型</typeparam>
        public class AutoSaveHashSet<V>
        {
            /// <summary>
            /// 通过储存键来构造
            /// </summary>
            public AutoSaveHashSet(string key)
            {
                Key = key;
            }
            /// <summary>
            /// 通过储存键和默认值来构造（如果有存档，将不使用默认值）
            /// </summary>
            public AutoSaveHashSet(string key, HashSet<V> defaultValue)
            {
                Key = key;
                _value = defaultValue;
            }
            /// <summary>
            /// 获得储存键
            /// </summary>
            public string Key { get; private set; }
            /// <summary>
            /// 是否自动存档
            /// </summary>
            public bool isAutoSave = true;
            private HashSet<V> _value;
            private bool _hasInit;

            /// <summary>
            /// 获得值（该值初始化时会自动从存档读取一次，设置时也会自动存档，除非你将值保存出去之后自己更改，如果这样，你需要调用手动存档Save）
            /// </summary>
            public HashSet<V> Value
            {
                get
                {
                    AutoLoad();
                    return _value;
                }

                set
                {
                    _value = value;
                    _hasInit = true;
                    if (isAutoSave) Save();
                }
            }

            /// <summary>
            /// 手动储存（当你对Value的内部属性进行手动更改后，你需要调用手动储存，否则不会储存）
            /// </summary>
            public void Save()
            {
                AutoLoad();
                if (_value == null)
                {
                    DataCore.RemoveKey(Key);
                }
                else
                {
                    DataCore.SaveValue(Key, _value);
                }
            }

            /// <summary>
            /// 自动读档（内部方法）
            /// </summary>
            public void AutoLoad()
            {
                if (!_hasInit)
                {
                    HashSet<V> tValue;
                    if (DataCore.LoadValue(Key, out tValue))
                    {
                        _value = tValue;
                    }
                    _hasInit = true;
                }
            }

            /// <summary>
            /// 隐式转化
            /// </summary>
            public static implicit operator HashSet<V>(AutoSaveHashSet<V> value)
            {
                return value.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        /// <summary>
        /// 可以自动存档读档的字典
        /// </summary>
        public class AutoSaveDict<K, V>
        {
            /// <summary>
            /// 通过储存键来构造
            /// </summary>
            public AutoSaveDict(string key)
            {
                Key = key;
            }
            /// <summary>
            /// 通过储存键和默认值来构造（如果有存档，将不使用默认值）
            /// </summary>
            public AutoSaveDict(string key, Dictionary<K, V> defaultValue)
            {
                Key = key;
                _value = defaultValue;
            }
            /// <summary>
            /// 获得储存键
            /// </summary>
            public string Key { get; private set; }
            /// <summary>
            /// 是否自动存档
            /// </summary>
            public bool isAutoSave = true;
            private Dictionary<K, V> _value;
            private bool _hasInit;

            /// <summary>
            /// 获得值（该值初始化时会自动从存档读取一次，设置时也会自动存档，除非你将值保存出去之后自己更改，如果这样，你需要调用手动存档Save）
            /// </summary>
            public Dictionary<K, V> Value
            {
                get
                {
                    AutoLoad();
                    return _value;
                }

                set
                {
                    _value = value;
                    _hasInit = true;
                    if (isAutoSave) Save();
                }
            }

            /// <summary>
            /// 手动储存（当你对Value的内部属性进行手动更改后，你需要调用手动储存，否则不会储存）
            /// </summary>
            public void Save()
            {
                AutoLoad();
                if (_value == null)
                {
                    DataCore.RemoveKey(Key);
                }
                else
                {
                    DataCore.SaveValue(Key, _value);
                }
            }

            /// <summary>
            /// 自动读档（内部方法）
            /// </summary>
            public void AutoLoad()
            {
                if (!_hasInit)
                {
                    Dictionary<K, V> tValue;
                    if (DataCore.LoadValue(Key, out tValue))
                    {
                        _value = tValue;
                    }
                    _hasInit = true;
                }
            }

            /// <summary>
            /// 隐式转化
            /// </summary>
            public static implicit operator Dictionary<K, V>(AutoSaveDict<K, V> value)
            {
                return value.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        /// <summary>
        /// 可以自动存档读档的字典
        /// </summary>
        public class AutoSaveSortedDict<K, V>
        {
            /// <summary>
            /// 通过储存键来构造
            /// </summary>
            public AutoSaveSortedDict(string key)
            {
                Key = key;
            }
            /// <summary>
            /// 通过储存键和默认值来构造（如果有存档，将不使用默认值）
            /// </summary>
            public AutoSaveSortedDict(string key, SortedDictionary<K, V> defaultValue)
            {
                Key = key;
                _value = defaultValue;
            }
            /// <summary>
            /// 获得储存键
            /// </summary>
            public string Key { get; private set; }
            /// <summary>
            /// 是否自动存档
            /// </summary>
            public bool isAutoSave = true;
            private SortedDictionary<K, V> _value;
            private bool _hasInit;

            /// <summary>
            /// 获得值（该值初始化时会自动从存档读取一次，设置时也会自动存档，除非你将值保存出去之后自己更改，如果这样，你需要调用手动存档Save）
            /// </summary>
            public SortedDictionary<K, V> Value
            {
                get
                {
                    AutoLoad();
                    return _value;
                }

                set
                {
                    _value = value;
                    _hasInit = true;
                    if (isAutoSave) Save();
                }
            }

            /// <summary>
            /// 手动储存（当你对Value的内部属性进行手动更改后，你需要调用手动储存，否则不会储存）
            /// </summary>
            public void Save()
            {
                AutoLoad();
                if (_value == null)
                {
                    DataCore.RemoveKey(Key);
                }
                else
                {
                    DataCore.SaveValue(Key, _value);
                }
            }

            /// <summary>
            /// 自动读档（内部方法）
            /// </summary>
            public void AutoLoad()
            {
                if (!_hasInit)
                {
                    SortedDictionary<K, V> tValue;
                    if (DataCore.LoadValue(Key, out tValue))
                    {
                        _value = tValue;
                    }
                    _hasInit = true;
                }
            }

            /// <summary>
            /// 隐式转化
            /// </summary>
            public static implicit operator SortedDictionary<K, V>(AutoSaveSortedDict<K, V> value)
            {
                return value.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        #endregion
    }
}