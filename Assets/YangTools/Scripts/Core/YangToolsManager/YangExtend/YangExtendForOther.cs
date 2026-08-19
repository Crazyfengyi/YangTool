using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YangTools.Scripts.Core.YangExtend
{
    /// <summary>
    /// 扩展工具类
    /// </summary>
    public partial class YangExtend
    {
        /// <summary>
        /// 只更改透明度
        /// </summary>
        public static void ChangeAlpha(this Image image, float alpha)
        {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        /// <summary>
        /// 只更改透明度
        /// </summary>
        public static void ChangeAlpha(this Text text, float alpha)
        {
            Color tempColor = text.color;
            tempColor.a = alpha;
            text.color = tempColor;
        }

        /// <summary>
        /// 只更局部坐标X
        /// </summary>
        public static void ChangeLocalPosX(this Transform transform, float x)
        {
            Vector3 oldPos = transform.localPosition;
            oldPos.x = x;
            transform.localPosition = oldPos;
        }

        /// <summary>
        /// 只更局部坐标Y
        /// </summary>
        public static void ChangeLocalPosY(this Transform transform, float y)
        {
            Vector3 oldPos = transform.localPosition;
            oldPos.y = y;
            transform.localPosition = oldPos;
        }

        /// <summary>
        /// 获得随机bool
        /// </summary>
        public static bool GetRandomBool()
        {
            return UnityEngine.Random.Range(0, 2) == 1;
        }

        /// <summary>
        /// 给字符串添加Text富文本颜色
        /// </summary>
        public static string AddColor(this string str, string color)
        {
            if (ColorUtility.TryParseHtmlString($"#{color}", out Color toColor))
            {
                return $"<color=#{color}>" + str + "</color>";
            }

            return str;
        }

        /// <summary>
        /// 倒叙刷新GameObject下的ContentSizeFitter组件适配，必须物体显示时才可以适配
        /// </summary>
        /// <param name="obj">最上层物体</param>
        public static void LayoutRefresh(this GameObject obj)
        {
            ContentSizeFitter[] tempArray = obj.transform.GetComponentsInChildren<ContentSizeFitter>(true);
            for (int i = tempArray.Length - 1; i >= 0; i--)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tempArray[i].GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// 倒叙刷新GameObject下所有的Horizonral LayoutGroup组件适配,必须物体显示时才可以适配
        /// </summary>
        public static void LayoutHGroupRefresh(this GameObject obj)
        {
            var temp = obj.GetComponentsInChildren<HorizontalLayoutGroup>(true);
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(temp[i].GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// 倒叙刷新GameObject下所有的VerticalLayoutGroup组件适配，必须物体显示时才可以适配
        /// </summary>
        public static void LayoutVGroupRefresh(this GameObject obj)
        {
            var temp = obj.GetComponentsInChildren<VerticalLayoutGroup>(true);
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(temp[i].GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// string转指定类型
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="str">待转字符串</param>
        /// <param name="defaultT">转失败后返回的默认类值</param>
        /// <returns></returns>
        public static T ToTryParse<T>(this string str, T defaultT = default)
        {
            Type type = typeof(T);
            TypeCode typeCode = Type.GetTypeCode(type);

            #region 反射，减少代码量

            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                {
                    //反射取方法调用
                    Type t = typeof(T); //获取类型

                    Type[] types = new Type[] {typeof(string), type.MakeByRefType()};
                    ParameterModifier[] modifiers = new ParameterModifier[] {new ParameterModifier(2)};
                    MethodInfo methodInfo = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static,
                        Type.DefaultBinder, types, modifiers); //获取方法名

                    T obj = (T) t.Assembly.CreateInstance(t.FullName); //创建类实例
                    object[] parmsObj = new object[] {str, obj};
                    object isSucceed = methodInfo.Invoke(null, parmsObj);

                    if ((bool) isSucceed)
                    {
                        return (T) parmsObj[1];
                    }
                    else
                    {
                        return defaultT;
                    }
                }
                case TypeCode.Boolean:
                {
                    if (int.TryParse(str, out int result))
                    {
                        Type t = typeof(BoolBridge); //获取类型
                        BoolBridge obj = (BoolBridge) t.Assembly.CreateInstance(t.FullName); //创建bool桥类实例
                        //反射设置属性值
                        FieldInfo info = t.GetField("value");
                        info.SetValue(obj, result != 0);

                        return (T) (object) (obj.value);
                    }
                    else
                    {
                        if (bool.TryParse(str, out bool booResult))
                        {
                            return (T) (object) booResult;
                        }
                        else
                        {
                            return defaultT;
                        }
                    }
                }
                case TypeCode.DBNull:
                case TypeCode.Empty:
                {
                    //空对象和未初始化值
                    Debug.LogError("ToTryParse：string不能转DBNull或者Empty");
                    return defaultT;
                }
                case TypeCode.String:
                {
                    Debug.LogError("ToTryParse：请不要string调用转string");
                    //反射手动调用构造函数创建string
                    ConstructorInfo[] constructors = typeof(T).GetConstructors(); //获得所有构造函数
                    char[] charArray = str.ToCharArray(); //转成char数组,string构造函数只支持传char数组
                    List<object> o = new List<object>() {(object) charArray};
                    object obj = constructors[6].Invoke(o.ToArray()); //调用第7的个构造函数

                    return (T) obj;
                }
                case TypeCode.Object:
                {
                    Debug.LogError("ToTryParse：string不能转Object");
                    return defaultT;
                }
                default:
                    Debug.LogError("ToTryParse：函数进入default");
                    break;
            }

            #endregion

            return defaultT;
        }

        /// <summary>
        /// 获得按键
        /// </summary>
        public static bool EditorGetKeyDown(KeyCode _code)
        {
#if UNITY_EDITOR
            return Input.GetKeyDown(_code);
#endif
            return false;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 在圆内获取随机点
        /// </summary>
        /// <param name="centerPos">圆心</param>
        /// <param name="radius">圆的半径</param>
        public static Vector2 GetRandomPointInCircle(Vector2 centerPos, float radius)
        {
            var pointCount = 1; //要生成点的数量
            Vector2 result = centerPos; //结果

            System.Random random = new System.Random();
            for (int i = 0; i < pointCount; i++)
            {
                //r和theta的生成要分别生成随机数，公式概念中明确说明，r和theta要互不相干
                //半径
                double randomValue1 = random.NextDouble(); //0-1的随机值
                float r = (float) Math.Sqrt(randomValue1) * radius;
                //角度
                double randomValue2 = random.NextDouble(); //0-1的随机值
                float theta = (float) (2 * Math.PI * randomValue2);

                //生成x，y坐标
                float xPos = r * Mathf.Cos(theta);
                float yPos = r * Mathf.Sin(theta);

                result.x += xPos;
                result.y += yPos; //* 0.5; 若要变成椭圆，将X和Y结果值乘上你想要的比例系数即可
            }

            return result;
        }

        /// <summary>
        /// 检测物体是否为空
        /// </summary>
        /// <returns>是否为空</returns>
        public static bool CheckUnityObjIsNull(this UnityEngine.Object obj)
        {
            if (obj == null || obj.Equals(null))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获得颜色
        /// </summary>
        /// <param name="htmlStr">16进制颜色值</param>
        /// <returns>颜色结果</returns>
        public static Color GetColor(this string htmlStr)
        {
            if (!htmlStr.StartsWith("#"))
            {
                htmlStr = "#" + htmlStr;
            }

            ColorUtility.TryParseHtmlString(htmlStr, out Color nowColor);
            return nowColor;
        }

        /// <summary>
        /// 获得颜色的string
        /// </summary>
        public static string GetHtmlColor(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        private static readonly int Color1 = Shader.PropertyToID("_Color");

        /// <summary>
        /// 计算文字的长度
        /// </summary>
        public static int CalculateLengthOfText(Text tex, string temp = null)
        {
            int totalLength = 0;
            string message = tex.text;

            if (temp != null)
            {
                message = temp;
            }

            Font myFont = tex.font;
            myFont.RequestCharactersInTexture(message, tex.fontSize, tex.fontStyle);

            CharacterInfo characterInfo;
            char[] arr = message.ToCharArray();
            foreach (char c in arr)
            {
                myFont.GetCharacterInfo(c, out characterInfo, tex.fontSize, tex.fontStyle);
                totalLength += characterInfo.advance;
            }

            return totalLength;
        }

        /// <summary>
        /// 图片置灰
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        public static void SetImageGrey(this Image image, Color color = default)
        {
            Shader shader = Shader.Find("UI/ImageGreyShader");
            image.material = new Material(shader);
            if (color != default)
            {
                image.material.SetColor(Color1, color);
            }
        }

        /// <summary>
        /// 图片材质还原(材质置空)
        /// </summary>
        public static void SetImageDefault(this Image image)
        {
            image.material = null;
        }

        /// <summary>
        /// 物体下所有图片置灰(包含显示为false的)
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="color"></param>
        public static void SetAllImageGrey(this GameObject gameObject, Color color = default)
        {
            var images = gameObject.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                images[i].SetImageGrey(color);
            }
        }

        /// <summary>
        /// 物体下所有图片材质还原(材质置空)(包含显示为false的)
        /// </summary>
        public static void SetAllImageDefault(this GameObject gameObject)
        {
            var images = gameObject.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                images[i].SetImageDefault();
            }
        }

        /// <summary>
        /// 当同一方向有多个箭时，箭的偏移
        /// </summary>
        public static float[] Offset(int count)
        {
            float scale = 0.65f;
            float[] offsetVec3 = new float[count];
            for (int i = 0; i < count; i++)
            {
                // 奇数根
                if (count % 2 != 0)
                {
                    // 第一根不偏移
                    if (i == 0)
                    {
                        offsetVec3[i] = 0;
                    }
                    else
                    {
                        if (i % 2 != 0)
                        {
                            offsetVec3[i] = (1f + (i - 1) / 2f) * scale;
                        }
                        else
                        {
                            offsetVec3[i] = -(1f + (i - 1) / 2f) * scale;
                        }
                    }
                }
                else
                {
                    // 偶数箭
                    if (i % 2 != 0)
                    {
                        offsetVec3[i] = (0.5f + (i / 2f)) * scale;
                    }
                    else
                    {
                        offsetVec3[i] = -(0.5f + (i / 2f)) * scale;
                    }
                }
            }

            return offsetVec3;
        }

        #region 多语言

        //更改回调字典
        private static Dictionary<ulong, Action> LanguageTextDic = new Dictionary<ulong, Action>();

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="textKey">多语言表Key</param>
        public static void AutoToText(this TextMeshPro text, string textKey)
        {
            ulong uuidKey = EntityId.ToULong(text.GetEntityId());
            Action changeAction = () => { text.text = LanguageManager.Instance.GetLanguage(textKey); };

            LanguageTextDic[uuidKey] = changeAction;
            changeAction?.Invoke();
        }

        #endregion

        #region UI添加点击事件

        /// <summary>
        /// 为UI对象添加点击事件监听器的扩展方法
        /// </summary>
        /// <param name="uiObject">要添加事件的UI对象的RectTransform组件</param>
        /// <param name="eventType">事件类型，如点击、悬停等</param>
        /// <param name="callBack">事件触发时的回调函数</param>
        /// <returns>返回创建的事件触发条目，可用于后续操作</returns>
        public static EventTrigger.Entry AddUIClickEventListener(this RectTransform uiObject,
            EventTriggerType eventType, Action<BaseEventData> callBack)
        {
            // 获取或添加EventTrigger组件
            EventTrigger eventTrigger = uiObject.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = uiObject.gameObject.AddComponent<EventTrigger>();
            }

            // 创建新的事件触发条目
            EventTrigger.Entry entry = new EventTrigger.Entry()
            {
                eventID = eventType
            };
            // 添加回调函数到事件触发条目
            entry.callback.AddListener(new UnityAction<BaseEventData>(callBack));
            // 将事件触发条目添加到事件触发器中
            eventTrigger.triggers.Add(entry);

            // 返回创建的事件触发条目
            return entry;
        }

        /// <summary>
        /// 扩展方法：用于移除UI对象的点击事件监听器
        /// </summary>
        /// <param name="uiObject">要移除事件监听器的UI对象的RectTransform组件</param>
        public static void RemoveUIClickEventListener(this RectTransform uiObject)
        {
            // 获取UI对象上的EventTrigger组件
            EventTrigger eventTrigger = uiObject.gameObject.GetComponent<EventTrigger>();
            // 如果EventTrigger组件存在
            if (eventTrigger != null)
            {
                // 清除所有触发器
                eventTrigger.triggers.Clear();
                // 销毁EventTrigger组件
                GameObject.Destroy(eventTrigger);
            }
        }

        #endregion

        #region 射线忽略

        /// <summary>
        /// 获得Game2移动点
        /// </summary>
        public static (List<RaycastHit2D> rayCastList, bool result, Vector2 targetWorldPos) GetRaycastPoint(Vector2 pos, Vector2 direction, List<Collider2D> ignoredList = null,float distance = 30)
        {
#if UNITY_EDITOR
            GizmosManager.Instance.GizmosDrawRay(pos, direction, distance);
#endif
            List<RaycastHit2D> resultList = new List<RaycastHit2D>();
            
            int layerId = LayerMask.GetMask("Animal");
            RaycastHit2D[] hits = Physics2D.RaycastAll(pos, direction, distance,layerId);
            //返回的数组是无序的,按距离start的远近,排下序
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            foreach (RaycastHit2D hit in hits)
            {
                //检测到的点不是起始点
                if (hit.point != pos)
                {
                    //检测到的物体没有在忽略名单里
                    if (!Contains(ignoredList, hit))
                    {
                        resultList.Add(hit);
                    }
                }
            }
                 
            if (resultList.Count > 0)
            {
                return (resultList, true,pos + direction * distance);
            }
            
            return (resultList, false,pos + direction * distance);
        }

        /// <summary>
        /// 等于或在Collider的内部
        /// </summary>
        private static bool Contains(List<Collider2D> ignoredColliders, RaycastHit2D hit)
        {
            foreach (var collider in ignoredColliders)
            {
                if (collider == hit.collider || collider.bounds.Contains(hit.point))
                {
                    return true;
                }
            }
            return false;
        }

        //是否在碰撞体里
        public static bool IsPointInsideCollider(Vector3 point, Collider collider)
        {
            Vector3 closestPoint = collider.ClosestPoint(point);
            var temp = Vector3.Distance(closestPoint, point) < 0.2f;
            //当点在Collider内时，最近点与原点相同
            return temp;
        }
        #endregion

        #region 栈管理显隐

        private static readonly Dictionary<string, Stack<bool>> s_VisibleStacks = new Dictionary<string, Stack<bool>>();
        /// <summary>
        /// 开始一个显示作用域。
        /// </summary>
        /// <param name="key">作用域唯一标识</param>
        /// <param name="isVisible">当前作用域是否需要显示</param>
        /// <returns>当前作用域最终是否可见</returns>
        public static bool BeginVisible(string key, bool isVisible)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("BeginVisible的key不能为空。");
                return false;
            }

            if (!s_VisibleStacks.TryGetValue(key, out Stack<bool> visibleStack))
            {
                visibleStack = new Stack<bool>(4);
                s_VisibleStacks.Add(key, visibleStack);
            }

            bool currentVisible = isVisible;
            visibleStack.Push(currentVisible);
            return currentVisible;
        }

        /// <summary>
        /// 结束显示作用域,返回恢复后的显示状态
        /// </summary>
        /// <param name="key">作用域唯一标识</param>
        /// <returns>结束后的显示状态</returns>
        public static bool EndVisible(string key)
        {
            if (string.IsNullOrEmpty(key) ||
                !s_VisibleStacks.TryGetValue(key, out Stack<bool> visibleStack) ||
                visibleStack.Count == 0)
            {
                Debug.LogError($"EndVisible调用次数多于BeginVisible，key：{key}");
                return false;
            }

            visibleStack.Pop();
            if (visibleStack.Count > 0)
            {
                return visibleStack.Peek();
            }
            s_VisibleStacks.Remove(key);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// bool值桥
    /// </summary>                             
    public class BoolBridge
    {
        public bool value;

        public BoolBridge()
        {
        }
    }
}