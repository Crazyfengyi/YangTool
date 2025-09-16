using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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

                    Type[] types = new Type[] { typeof(string), type.MakeByRefType() };
                    ParameterModifier[] modifiers = new ParameterModifier[] { new ParameterModifier(2) };
                    MethodInfo methodInfo = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static,
                        Type.DefaultBinder, types, modifiers); //获取方法名

                    T obj = (T)t.Assembly.CreateInstance(t.FullName); //创建类实例
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
                        Type t = typeof(BoolBridge); //获取类型
                        BoolBridge obj = (BoolBridge)t.Assembly.CreateInstance(t.FullName); //创建bool桥类实例
                        //反射设置属性值
                        FieldInfo info = t.GetField("value");
                        info.SetValue(obj, result != 0);

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
                    ConstructorInfo[] constructors = typeof(T).GetConstructors(); //获得所有构造函数
                    char[] charArray = str.ToCharArray(); //转成char数组,string构造函数只支持传char数组
                    List<object> o = new List<object>() { (object)charArray };
                    object obj = constructors[6].Invoke(o.ToArray()); //调用第7的个构造函数

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
                float r = (float)Math.Sqrt(randomValue1) * radius;
                //角度
                double randomValue2 = random.NextDouble(); //0-1的随机值
                float theta = (float)(2 * Math.PI * randomValue2);

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

    /* 多语言参考写法--自动回调
     *
     * public static void AutoToText(this TextMeshPro text, string textId)
        {
            var id = text.gameObject.GetInstanceID();
            System.Action act = () =>
            {
                text.text = textId.ToText();
            };
            if (PlayerSettingManager.ChangeleLanguageTextDic.ContainsKey(id))
            {
                PlayerSettingManager.ChangeleLanguageTextDic[id] = act;
            }
            else
            {
                PlayerSettingManager.ChangeleLanguageTextDic.Add(id, act);
            }
            act();
        }
     */

    /*
        ---------参考编辑器update ----
     *	/// <summary>
/// 查找引用
/// </summary>
[MenuItem("SceneTools/删除未引用资源")]
static private void FindAndDelete()
{
    Dictionary<string, string> guidAndPaths = new Dictionary<string, string>();
    List<string> deletePaths = new List<string>();
    var obj = Selection.objects[0];
    string dire = AssetDatabase.GetAssetPath(obj);
    DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/" + dire.Replace("Assets/", ""));
    DirectoryInfo[] dirs = directory.GetDirectories();
    for (int i = 0; i < dirs.Length; i++)
    {
        if (dirs[i].FullName.Contains("Spine")) continue;//Spine不做处理
        FileInfo[] dFiles = dirs[i].GetFiles("*.png");
        for (int j = 0; j < dFiles.Length; j++)
        {
            string filePath = dFiles[j].FullName.Substring(dFiles[j].FullName.IndexOf("Assets")).Replace("\\", "/");
            if (string.IsNullOrEmpty(filePath)) continue;
            guidAndPaths.Add(AssetDatabase.AssetPathToGUID(filePath), filePath);
            deletePaths.Add(filePath);
        }
        dFiles = dirs[i].GetFiles("*.tga");
        for (int j = 0; j < dFiles.Length; j++)
        {
            string filePath = dFiles[j].FullName.Substring(dFiles[j].FullName.IndexOf("Assets")).Replace("\\", "/");
            if (string.IsNullOrEmpty(filePath)) continue;
            guidAndPaths.Add(AssetDatabase.AssetPathToGUID(filePath), filePath);
            deletePaths.Add(filePath);
        }
        dFiles = dirs[i].GetFiles("*.jpg");
        for (int j = 0; j < dFiles.Length; j++)
        {
            string filePath = dFiles[j].FullName.Substring(dFiles[j].FullName.IndexOf("Assets")).Replace("\\", "/");
            if (string.IsNullOrEmpty(filePath)) continue;
            guidAndPaths.Add(AssetDatabase.AssetPathToGUID(filePath), filePath);
            deletePaths.Add(filePath);
        }
    }
    List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
    string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
        .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
    int startIndex = 0;

    EditorApplication.update = delegate ()
    {
        string file = files[startIndex];

        bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
        foreach (var guid in guidAndPaths.Keys)
        {
            if (Regex.IsMatch(File.ReadAllText(file), guid))//找到引用则不删除
            {
                deletePaths.Remove(guidAndPaths[guid]);
            }
        }
        startIndex++;
        if (isCancel || startIndex >= files.Length)
        {
            EditorUtility.ClearProgressBar();
            EditorApplication.update = null;
            startIndex = 0;
            for (int i = 0; i < deletePaths.Count; i++)
            {
                AssetDatabase.DeleteAsset(deletePaths[i]);
                Debug.Log("删除了" + deletePaths[i]);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"删除了{deletePaths.Count}个文件");
        }
    };
}
     *
     *
     *
     */

    /*---参考移除动画事件-----
     *	[MenuItem("SceneTools/删除所有动画事件")]
public static void RemoveAllAnimationEvent()
{
    var obj = Selection.activeObject;
    string dire = AssetDatabase.GetAssetPath(obj);
    DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/" + dire.Replace("Assets/",""));
    DirectoryInfo[] dirs = directory.GetDirectories();
    for (int i = 0; i < dirs.Length; i++)
    {
        FileInfo[] files = dirs[i].GetFiles("*.fbx");
        for (int j = 0; j < files.Length; j++)
        {
            string modelPath = files[j].FullName.Substring(files[j].FullName.IndexOf("Assets")).Replace("\\","/");
            ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            ModelImporterClipAnimation[] tempModelClips = new ModelImporterClipAnimation[modelImporter.clipAnimations.Length];
            for (int k = 0; k < modelImporter.clipAnimations.Length; k++)
            {
                tempModelClips[k] = modelImporter.clipAnimations[k];
                tempModelClips[k].events = new AnimationEvent[0];
            }
            modelImporter.clipAnimations = tempModelClips;
            modelImporter.SaveAndReimport();
        }
    }
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Debug.Log("删除动画事件成功!");
}
     *
     *
     *
     *
     */

    /*
     ---------参考子弹对称发射--------
     *int bulletCount = 2;//子弹数量
    int atkCount = 2;//攻击次数

    if (Random.Range(0f, 1f) < finnalValue[GameAttribute.散射箭概率])
    {
        //触发散射
        bulletCount += 1;
    }

    if (Random.Range(0f, 1f) < finnalValue[GameAttribute.连射箭概率])
    {
        //连射箭
        atkCount += 1;
    }

    if (Random.Range(0f, 1f) < finnalValue[GameAttribute.贯穿箭概率])
    {
        //贯穿箭
        AddBattleTempAttribute(-1, (int)GameAttribute.子弹穿透障碍物, 1, 0);
    }

    for (int i = 0; i < atkCount; i++)
    {
        //Wait(-1, 0.2f * i);
        //连射间隔
        skillTimerId.AddTimePot(0.2f * i, () =>
         {
             //偶数
             if (bulletCount % 2 == 0)
             {
                 var lastOffSet = 0;//上一根箭位置偏移

                 for (int j = 0; j < bulletCount; j++)
                 {
                     int OffSet = 30 * j  * (j % 2 == 0 ? 1 : -1);//临时偏移  (j % 2 == 1 ? 1 : -1)是区分上下的

                     //第一根特殊处理--向上偏移15度，其他按规则来
                     if (j == 0)
                     {
                         OffSet = 15;
                     }

                     int realityOffset = OffSet + lastOffSet;//实际偏移

                     Shot(-1, "Bullet_101_1", true, euler: realityOffset);

                     lastOffSet = realityOffset;//记录偏移
                 }


             }
             else//奇数
             {
                 var lastOffSet = 0;//上一根箭位置偏移
                 for (int j = 0; j < bulletCount; j++)
                 {
                     int OffSet = 30 * j * (j % 2 == 1 ? 1 : -1);//临时偏移  (j % 2 == 1 ? 1 : -1)是区分上下的
                     int realityOffset = OffSet + lastOffSet;//实际偏移

                     Shot(-1, "Bullet_101_1", true, euler: realityOffset);

                     lastOffSet = realityOffset;//记录偏移
                 }
             }
         });
    }
     *
     *
     *
     *
     */
}