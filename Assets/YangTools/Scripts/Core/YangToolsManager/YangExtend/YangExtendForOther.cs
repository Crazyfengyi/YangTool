using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.AI;
using System.Text.RegularExpressions;

namespace YangTools.Extend
{
    /// <summary>
    /// 扩展工具类
    /// </summary>
    public partial class YangExtend
    {
        /// <summary>
        /// 删除所有子节点
        /// </summary>
        /// <param name="content"></param>
        public static void DestoryAllChild(this Transform content)
        {
            foreach (Transform item in content)
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// 只更改透明度
        /// </summary>
        /// <param name="image"></param>
        public static void ChangeAlpha(this Image image, float alpha)
        {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        /// <summary>
        /// 只更改透明度
        /// </summary>
        /// <param name="text"></param>
        public static void ChangeAlpha(this Text text, float alpha)
        {
            Color tempColor = text.color;
            tempColor.a = alpha;
            text.color = tempColor;
        }

        /// <summary>
        /// 只更局部坐标X
        /// </summary>
        /// <param name="text"></param>
        public static void ChangeLocalPosX(this Transform transform, float x)
        {
            Vector3 oldPos = transform.localPosition;
            oldPos.x = x;
            transform.localPosition = oldPos;
        }

        /// <summary>
        /// 只更局部坐标Y
        /// </summary>
        /// <param name="text"></param>
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
                        Type t = typeof(T);//获取类型

                        Type[] types = new Type[] { typeof(string), type.MakeByRefType() };
                        ParameterModifier[] modifiers = new ParameterModifier[] { new ParameterModifier(2) };
                        MethodInfo methodInfo = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder, types, modifiers); //获取方法名

                        T obj = (T)t.Assembly.CreateInstance(t.FullName);//创建类实例
                        object[] parmsObj = new object[] { str, obj };
                        object isSucceed = methodInfo.Invoke(null, parmsObj);

                        if ((bool)isSucceed)
                        {
                            return (T)parmsObj[1];
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
                            Type t = typeof(BoolBridge);//获取类型
                            BoolBridge obj = (BoolBridge)t.Assembly.CreateInstance(t.FullName);//创建bool桥类实例
                                                                                               //反射设置属性值
                            FieldInfo info = t.GetField("value");
                            info.SetValue(obj, result == 0 ? false : true);

                            return (T)(object)(obj.value);
                        }
                        else
                        {
                            if (bool.TryParse(str, out bool booResult))
                            {
                                return (T)(object)booResult;
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
                        ConstructorInfo[] constructors = typeof(T).GetConstructors();//获得所有构造函数
                        char[] charArray = str.ToCharArray();//转成char数组,string构造函数只支持传char数组
                        List<object> o = new List<object>() { (object)charArray };
                        object obj = constructors[6].Invoke(o.ToArray());//调用第7的个构造函数

                        return (T)obj;
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
        public static bool GetKeyDown(KeyCode _code)
        {
#if UNITY_EDITOR
            return Input.GetKeyDown(_code);
#endif
            return false;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
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
            var pointCount = 1;//要生成点的数量
            Vector2 result = centerPos; //结果

            System.Random random = new System.Random();
            for (int i = 0; i < pointCount; i++)
            {
                //r和theta的生成要分别生成随机数，公式概念中明确说明，r和theta要互不相干
                //半径
                double randomValue1 = random.NextDouble();//0-1的随机值
                float r = (float)Math.Sqrt(randomValue1) * radius;
                //角度
                double randomValue2 = random.NextDouble();//0-1的随机值
                float theta = (float)(2 * Math.PI * randomValue2);

                //生成x，y坐标
                float xPos = r * Mathf.Cos(theta);
                float yPos = r * Mathf.Sin(theta);

                result.x += xPos;
                result.y += yPos;//* 0.5; 若要变成椭圆，将X和Y结果值乘上你想要的比例系数即可
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
    }

    //跟资源有关---张艺洪那边移过来
    public partial class YangExtend
    {
        /// <summary>
        /// 查找子节点--使用递归
        /// </summary>
        /// <param name="goParent">父对象</param>
        /// <param name="childName">查找的子对象</param>
        /// <returns></returns>
        public static Transform FindTheChildNode(GameObject goParent, string childName)
        {
            Transform searchTrans = null;

            searchTrans = goParent.transform.Find(childName);
            if (searchTrans == null)
            {
                foreach (Transform trans in goParent.transform)
                {
                    searchTrans = FindTheChildNode(trans.gameObject, childName);
                    if (searchTrans != null)
                    {
                        return searchTrans;
                    }
                }
            }
            return searchTrans;
        }

        /// <summary>
        /// 计算文字的长度
        /// </summary>
        public static int CalculateLengthOfText(Text tex, string _defalut = default)
        {
            int totalLength = 0;
            string message = tex.text;

            if (_defalut != default)
            {
                message = _defalut;
            }

            Font myFont = tex.font;
            myFont.RequestCharactersInTexture(message, tex.fontSize, tex.fontStyle);

            CharacterInfo characterInfo = new CharacterInfo();

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
                image.material.SetColor("_Color", color);
            }
        }

        /// <summary>
        /// 图片材质还原(材质置空)
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
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