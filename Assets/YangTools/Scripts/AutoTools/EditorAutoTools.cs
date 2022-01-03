using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace YangTools
{
    #region 工具类--显示在unity面板上
    /// <summary>
    /// 工具类
    /// </summary>
    public static partial class EditorAutoTools
    {
        static string currentFloder = "Assets/Resources/Prefabs/Monster";
        static string targetFloder = "Assets/Resources/UI/Monster/New";
        /// <summary>
        /// 输出字符串
        /// </summary>
        public static ConsleString consleString = new ConsleString();

        #region 复制文件-移除脚本

        [MenuItem(SettingInfo.YongToolsFunctionPath + "CopyToFolder")]
        public static void CopyToTargetFolder()
        {
            //读id文件
            TextAsset idList = AssetDatabase.LoadAssetAtPath<TextAsset>($"{targetFloder}/ID.txt");
            string idListStr = idList.text;
            string[] tempList = idListStr.Split('\n', '\r');

            List<string> newList = new List<string>();

            for (int i = 0; i < tempList.Length; i++)
            {
                if (!string.IsNullOrEmpty(tempList[i]))
                {
                    newList.Add(tempList[i]);
                }
            }


            for (int i = 0; i < newList.Count; i++)
            {

                try
                {
                    string name = newList[i];

                    var ss = $"{currentFloder}/Monster_{name}";
                    // 加载资源
                    GameObject obj = AssetDatabase.LoadAssetAtPath($"{currentFloder}/Monster_{name}.prefab", typeof(GameObject)) as GameObject;
                    // 以模板创建
                    //Object obj2 = Object.Instantiate(obj);

                    UnityEngine.Object obj2 = PrefabUtility.InstantiatePrefab(obj);

                    // 创建资源
                    //AssetDatabase.CreateAsset(obj2, $"{targetFloder}/{name}.prefab");
                    PrefabUtility.SaveAsPrefabAsset((GameObject)obj2, $"{targetFloder}/{name}.prefab", out bool success);

                    if (success)
                    {
                        Debug.LogError($"{newList[i]}:复制成功");
                    }
                    else
                    {
                        Debug.LogError($"{newList[i]}:复制失败");
                    }

                    Resources.UnloadAsset(obj);
                    Resources.UnloadAsset(obj2);
                }
                catch (System.Exception)
                {
                    Debug.LogError($"{newList[i]}:复制失败");
                }
            }

            Resources.UnloadAsset(idList);

            // 刷新编辑器，使刚创建的资源立刻被导入，才能接下来立刻使用上该资源
            AssetDatabase.Refresh();
            Debug.LogError("Copy完成");
        }

        [MenuItem(SettingInfo.YongToolsFunctionPath + "RemoveScript")]
        public static void RemoveScript()
        {
            //读id文件
            TextAsset idList = AssetDatabase.LoadAssetAtPath<TextAsset>($"{targetFloder}/ID.txt");
            string idListStr = idList.text;
            string[] tempList = idListStr.Split('\n', '\r');

            List<string> newList = new List<string>();

            for (int i = 0; i < tempList.Length; i++)
            {
                if (!string.IsNullOrEmpty(tempList[i]))
                {
                    newList.Add(tempList[i]);
                }
            }

            for (int i = 0; i < newList.Count; i++)
            {

                try
                {
                    string name = newList[i];

                    var ss = $"{currentFloder}/Monster_{name}";
                    // 加载资源
                    GameObject obj = AssetDatabase.LoadAssetAtPath($"{targetFloder}/{name}.prefab", typeof(GameObject)) as GameObject;

                    //修改资源
                    var script1List = obj.GetComponents<MySample>();

                    for (int j = 0; j < script1List.Length; j++)
                    {
                        GameObject.DestroyImmediate(script1List[j], true);
                    }

                    var script2List = obj.GetComponents<MySample>();

                    for (int j = 0; j < script2List.Length; j++)
                    {
                        GameObject.DestroyImmediate(script2List[j], true);
                    }

                    // 通知编辑器有资源被修改了
                    EditorUtility.SetDirty(obj);

                    Resources.UnloadAsset(obj);
                }
                catch (System.Exception)
                {
                    Debug.LogError($"{newList[i]}:修改失败");
                }
            }

            Resources.UnloadAsset(idList);

            //保存所有修改
            AssetDatabase.SaveAssets();
            //// 刷新编辑器，使刚创建的资源立刻被导入，才能接下来立刻使用上该资源
            //AssetDatabase.Refresh();
            Debug.LogError("修改完成");
        }

        /// <summary>
        /// 占位用--工具脚本需要MonoBehaviour站位
        /// </summary>
        public class MySample : MonoBehaviour
        {

        }

        #endregion

        #region 测试用相关
        [MenuItem("YangTools/TestScript", priority = 100000)]
        public static void TestScrpit()
        {

        }

        /// <summary>
        /// 清空Log输出
        /// </summary>
        private static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        //[MenuItem("Tools/生成常用文件夹--GenerateFolders")]
        private static void GenerateFolder()
        {
            //string resPath = Application.dataPath + "/Resources/";//resources路径 
            //string path = Application.dataPath + "/";//路径 

            //Directory.CreateDirectory(resPath + "Audio");
            //Directory.CreateDirectory(resPath + "Materials");
            //Directory.CreateDirectory(resPath + "Prefabs");
            //Directory.CreateDirectory(resPath + "Shaders");
            //Directory.CreateDirectory(resPath + "Textures");

            //Directory.CreateDirectory(path + "Scenes");
            //Directory.CreateDirectory(path + "Editor");
            //Directory.CreateDirectory(path + "StreamingAssets");
            //Directory.CreateDirectory(path + "Scripts");

            //AssetDatabase.Refresh();

            //Debug.Log("Folders Created");
        }

        /// <summary>
        /// 保存字符串到Txt
        /// </summary>
        public static void SaveStringToTxt(string fileName, string text)
        {
            string dicPath = Application.dataPath + $"/YangTools/Editor/SaveData";
            string path = dicPath + $"/{fileName}.txt";

            if (!Directory.Exists(dicPath))
            {
                Directory.CreateDirectory(dicPath);
            }

            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            //定义存放文件信息的字节数组
            byte[] tempBytes = new byte[fileStream.Length];
            //读取文件信息
            fileStream.Read(tempBytes, 0, tempBytes.Length);
            //将得到的字节型数组重写编码为字符型数组
            char[] chars = Encoding.UTF8.GetChars(tempBytes);
            //转换成字符串
            string tempStr = new string(chars);
            //=========更改=========

            tempStr += text + "|";

            //======================
            //将字符串转换为字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(tempStr);
            //向文件中写入字节数组
            fileStream.Write(bytes, 0, bytes.Length);
            //刷新缓冲区
            fileStream.Flush();
            //关闭流
            fileStream.Close();

            AssetDatabase.Refresh();
        }
        #endregion

        #region 小功能
        [MenuItem(SettingInfo.YongToolsFunctionPath + "移除选中物体丢失的脚本")]
        public static void RemoveMissScript()
        {
            GameObject[] objs = Selection.gameObjects;

            for (int i = 0; i < objs.Length; i++)
            {
                GameObject obj = GameObject.Instantiate(objs[i]);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                PrefabUtility.SaveAsPrefabAsset(obj, AssetDatabase.GetAssetPath(objs[i]));
            }

            AssetDatabase.Refresh();

            Debug.LogError("移除丢失脚本成功!");
        }

        /// <summary>
        /// 删除所有动画事件
        /// </summary>
        /// <param name="modelPath"></param>
        [MenuItem(SettingInfo.YongToolsFunctionPath + "移除选中动画文件所有动画事件")]
        public static void RemoveAllAnimationEvent()
        {
            UnityEngine.Object obj = Selection.activeObject;
            string modelPath = AssetDatabase.GetAssetPath(obj);
            ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            ModelImporterClipAnimation[] tempModelClips = new ModelImporterClipAnimation[modelImporter.clipAnimations.Length];
            for (int k = 0; k < modelImporter.clipAnimations.Length; k++)
            {
                tempModelClips[k] = modelImporter.clipAnimations[k];
                tempModelClips[k].events = new AnimationEvent[0];
            }
            modelImporter.clipAnimations = tempModelClips;
            modelImporter.SaveAndReimport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.LogError("移除动画事件成功!");
        }

        //[MenuItem("YangTools/" + "/脱离寻路组件数据")]
        //public static void SetNavMeshDateNull()
        //{
        //    Transform obj = Selection.activeTransform;

        //    if (obj.GetComponentInChildren<NavMeshSurface>(true))
        //    {
        //        NavMeshSurface script = obj.GetComponentInChildren<NavMeshSurface>(true);
        //        script.navMeshData = null;
        //    }
        //}

        #endregion

        #region 大功能
        [MenuItem(SettingInfo.YongToolsFunctionPath + "自动切割动画")]
        /// <summary>
        /// 自动切割动画
        /// </summary>
        public static void AutoSplitAni()
        {
            SplitAni.OpenWindow();
        }
        #endregion
    }

    #endregion

    #region 窗口类

    #region 切割动画弹窗
    /// <summary>
    /// 切割动画弹窗
    /// </summary>
    public class SplitAni : EditorWindow
    {
        public class monsterInfo
        {
            public string id;
        }
        private static GameObject model;
        //动画文件
        public static UnityEngine.Object aniObj;
        //切割表
        public static UnityEngine.Object frameObj;
        //打开界面
        public static void OpenWindow()
        {
            GetWindowWithRect<SplitAni>(new Rect(500, 500, 360, 360), false, "自动切割动画", true);
        }
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            aniObj = EditorGUILayout.ObjectField("动画文件", aniObj, typeof(UnityEngine.Object), false);
            frameObj = EditorGUILayout.ObjectField("切分表", frameObj, typeof(UnityEngine.TextAsset), false);

            if (GUILayout.Button("开始切分动画"))
            {
                if (aniObj != null && frameObj != null)
                {
                    SplitClips(AssetDatabase.GetAssetPath(aniObj), LoadFrameTable(Application.dataPath + AssetDatabase.GetAssetPath(frameObj).Substring(6)));
                    base.Close();
                }
                else
                {
                    TipsShowWindow.OpenWindow("提示", "文件为空");
                }
            }

            GUILayout.EndVertical();
        }
        /// <summary>
        /// 从模型文件切分动画
        /// </summary>
        /// <param name="modelPath"></param>
        public static void SplitClips(string modelPath, List<(string name, int firstFrame, int endFrame)> frameInfos)
        {
            //clip列表
            List<ModelImporterClipAnimation> clipList = new List<ModelImporterClipAnimation>();
            //模型信息
            ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;

            for (int i = 0; i < frameInfos.Count; i++)
            {
                if (frameInfos[i].firstFrame >= frameInfos[i].endFrame)
                {
                    EditorAutoTools.consleString.Add($"{frameInfos[i].name}的首帧比尾帧大");
                    return;
                }
                if (frameInfos[i].firstFrame < 0 || frameInfos[i].endFrame < 0)
                {
                    EditorAutoTools.consleString.Add($"{frameInfos[i].name}的首尾帧有个小于0");
                    return;
                }

                //clip
                ModelImporterClipAnimation clip = new ModelImporterClipAnimation();
                clip.name = frameInfos[i].name;
                clip.firstFrame = frameInfos[i].firstFrame;
                clip.lastFrame = frameInfos[i].endFrame;
                clip.loopTime = frameInfos[i].name.Contains("Loop");

                if (frameInfos[i].name.Contains("Idle") || frameInfos[i].name.Contains("_Move") || frameInfos[i].name.Contains("_b"))
                {
                    clip.loopTime = true;
                }
                //List<AnimationEvent> evnets = new List<AnimationEvent>();
                //clip.events = evnets.ToArray();
                clipList.Add(clip);
            }

            //数组
            List<ModelImporterClipAnimation> tempList = modelImporter.clipAnimations.ToList();

            //已有同名的直接设置原clip
            for (int i = 0; i < tempList.Count; i++)
            {
                for (int j = 0; j < clipList.Count; j++)
                {
                    if (modelImporter.clipAnimations[i].name == clipList[j].name)
                    {
                        tempList[i].firstFrame = clipList[j].firstFrame;
                        tempList[i].lastFrame = clipList[j].lastFrame;
                        tempList[i].loopTime = clipList[j].loopTime;

                        clipList.RemoveAt(j);
                        break;
                    }
                }
            }

            for (int i = 0; i < clipList.Count; i++)
            {
                tempList.Add(clipList[i]);
            }
            modelImporter.clipAnimations = tempList.ToArray();
            EditorUtility.SetDirty(modelImporter);

            modelImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (EditorAutoTools.consleString.CheackHaveValue())
            {
                EditorAutoTools.consleString.OutPutToDeBug();
                TipsShowWindow.OpenWindow("动画切分结果", "下面是运行结果提示:");
            }
            else
            {
                TipsShowWindow.OpenWindow("动画切分结果", "<color=#00F5FF>切分完成,完美运行</color>");
            }
        }
        /// <summary>
        /// 读取切割文件并规范格式--(名称，首帧，尾帧)
        /// </summary>
        public static List<(string, int, int)> LoadFrameTable(string path)
        {
            EditorAutoTools.consleString.Clear();

            FileStream fileStream = File.Open(path, FileMode.Open);

            byte[] content = new byte[fileStream.Length];
            fileStream.Read(content, 0, (int)fileStream.Length);

            string frameTable = "";
            try
            {
                frameTable = System.Text.Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback()).GetString(content);
            }
            catch (DecoderFallbackException e)
            {
                frameTable = System.Text.Encoding.GetEncoding("gb2312").GetString(content);
            }

            fileStream.Close();

            //容错符号
            frameTable = frameTable.Replace("：", ":");
            frameTable = frameTable.Replace("：", ":");
            frameTable = frameTable.Replace("\r\n", "\n");

            string[] datas = frameTable.Split('\r', '\n');
            //帧数信息
            List<(string, int, int)> frameInfos = new List<(string, int, int)>();

            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = datas[i].Replace("\0", "");//字符串结束用\0占位表示结束
                if (string.IsNullOrEmpty(datas[i]))
                {
                    EditorAutoTools.consleString.Add($"第{i + 1}行是空行");
                    continue;
                }

                //名称，首帧，尾帧
                (string name, int firstFrame, int endFrame) info = (string.Empty, 0, 0);
                //分割 Q_Run:195-225 =>  Q_Run + 195-225
                string[] tempArray = datas[i].Split(':');

                if (tempArray.Length < 1)
                {
                    EditorAutoTools.consleString.Add($"第{i + 1}行分割有问题");
                    continue;
                }

                info.name = tempArray[0].Trim();

                MatchCollection matches = Regex.Matches(tempArray[1], @"[0-9]+");
                for (int j = 0; j < matches.Count; j++)
                {
                    switch (j)
                    {
                        case 0:
                            info.firstFrame = int.Parse(matches[j].Value);
                            break;
                        case 1:
                            info.endFrame = int.Parse(matches[j].Value);
                            break;
                    }
                }

                if (info.endFrame <= 0)
                {
                    EditorAutoTools.consleString.Add($"{info.name}的尾帧为小于等于0");
                    continue;
                }

                frameInfos.Add(info);
            }

            return frameInfos;
        }
    }
    #endregion

    #region 提示弹窗
    /// <summary>
    /// 提示弹窗
    /// </summary>
    public class TipsShowWindow : EditorWindow
    {
        private Vector2 scroll;
        ///搜索文字
        private static string search;
        //提示文字
        private static string ShowTips;
        //输出文字
        private static string ShowStr;

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="tips">提示</param>
        public static void OpenWindow(string title, string tips)
        {
            search = "";
            ShowTips = tips;
            ShowStr = EditorAutoTools.consleString.GetString();
            GetWindowWithRect<TipsShowWindow>(new Rect(500, 500, 300, 300), true, title, true);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("搜索框");

            //检查代码块中是否有任何控件被更改
            //EditorGUI.BeginChangeCheck();
            search = GUILayout.TextField(search);
            //EditorGUI.EndChangeCheck();

            GUILayout.Space(20);

            var temp = new GUIStyle();
            temp.richText = true;

            GUILayout.Label(ShowTips, temp);
            GUILayout.Space(20);

            scroll = GUILayout.BeginScrollView(scroll);
            string[] charArray = ShowStr.Split('\n', '\r');
            for (int i = 0; i < charArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    if (!charArray[i].Contains(search)) continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(charArray[i]);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button("关闭"))
            {
                Close();
            }
        }
    }
    #endregion

    #endregion

    #region 辅助类

    #region 字符串记录
    /// <summary>
    /// 面板输出字符串记录
    /// </summary>
    public class ConsleString
    {
        StringBuilder value = new StringBuilder();

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            value.Clear();
        }

        /// <summary>
        /// 添加-自动换行
        /// </summary>
        public void Add(string str)
        {
            value.Append(str + "\n");
        }

        /// <summary>
        /// 检测是否有值
        /// </summary>
        /// <returns></returns>
        public bool CheackHaveValue()
        {
            return value.Length > 0;
        }

        /// <summary>
        /// 获得字符串
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return value.ToString();
        }

        /// <summary>
        /// 输出到DeBug面板
        /// </summary>
        public void OutPutToDeBug()
        {
            Debug.LogError(value.ToString());
        }
    }
    #endregion

    #endregion

    #region 资源管理器监听
    /// <summary>
    /// 资源流程管理
    /// </summary>
    public class AssetChangeListener : AssetPostprocessor
    {
        #region 预制体更改
        /// <summary>
        /// 预制体修改监听
        /// </summary>
        [InitializeOnLoadMethod]
        private static void PrefabStageListen()
        {
            // 打开Prefab编辑界面回调
            UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabOpened;
            // Prefab被保存之前回调
            UnityEditor.SceneManagement.PrefabStage.prefabSaving += OnPrefabSaving;
            // Prefab被保存之后回调
            UnityEditor.SceneManagement.PrefabStage.prefabSaved += OnPrefabSaved;
            // 关闭Prefab编辑界面回调
            UnityEditor.SceneManagement.PrefabStage.prefabStageClosing += OnPrefabClosing;
        }

        private static void OnPrefabOpened(UnityEditor.SceneManagement.PrefabStage prefabStage)
        {
        }
        private static void OnPrefabSaving(GameObject prefab)
        {
        }
        private static void OnPrefabSaved(GameObject prefab)
        {
        }
        private static void OnPrefabClosing(UnityEditor.SceneManagement.PrefabStage prefabStage)
        {
        }
        #endregion

        #region 资源导入+更改

        //所有的资源的导入，删除，移动，都会调用此方法
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                //导入
            }
            foreach (string str in deletedAssets)
            {
                //删除
            }
            foreach (string str in movedAssets)
            {
                //移动结束位置
            }
            foreach (string str in movedFromAssetPaths)
            {
                //移动起始位置
            }
        }

        //模型导入之前调用
        public void OnPreprocessModel()
        {
            //该方法有问题,每次更改模型都会调用
            return;
            ModelImporter modelImporter = this.assetImporter as ModelImporter;
            modelImporter.isReadable = false;
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;

            //带动画的FBX资源
            if (/*assetPath.Contains("Arts") &&*/ assetPath.EndsWith("_anim.fbx"))
            {
                modelImporter.importAnimation = true;
                modelImporter.importBlendShapes = true;
                modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
            }
            //蒙皮模型
            else if (/*assetPath.Contains("Arts") &&*/ assetPath.EndsWith("_skin.fbx"))
            {
                modelImporter.importAnimation = false;
                modelImporter.importBlendShapes = false;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                modelImporter.meshCompression = ModelImporterMeshCompression.Medium;
                modelImporter.importCameras = false;
                modelImporter.importLights = false;
            }
        }

        //模型导入之后调用
        public void OnPostprocessModel(GameObject go)
        {
        }
        // 正则表达式匹配--示例："宝箱&1204"名字
        private string RegexTextureMaxSize = @"[&]\d{2,4}";
        //纹理导入之后调用
        public void OnPostprocessTexture(Texture2D tex)
        {
            return;
            //判断导入资源的路径名中,是否含有sprites文件夹,如果有则该图片自动设置Sprite,并做一些初始化。
            if (assetPath.ToUpper().Contains("/UI/"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;

                string fileName = System.IO.Path.GetFileName(assetPath);
                // 设置MaxSize尺寸
                Regex tempRegex = new Regex(RegexTextureMaxSize);
                if (tempRegex.IsMatch(fileName))
                {
                    string maxSizeStr = tempRegex.Match(fileName).Value.Replace("&", "");
                    int maxSize = Convert.ToInt32(maxSizeStr);

                    switch (maxSize)
                    {
                        case int temp when temp < 50:
                            maxSize = 32;
                            break;
                        case int temp when temp < 100:
                            maxSize = 64;
                            break;
                        case int temp when temp < 150:
                            maxSize = 128;
                            break;
                        case int temp when temp < 300:
                            maxSize = 256;
                            break;
                        case int temp when temp < 600:
                            maxSize = 512;
                            break;
                        case int temp when temp < 1100:
                            maxSize = 1024;
                            break;
                        case int temp when temp < 2100:
                            maxSize = 2048;
                            break;
                        default:
                            Debug.LogError("自动设置图片大小失败,UI图片大小不应该大于2048");
                            return;
                    }

                    textureImporter.maxTextureSize = maxSize;
                    Debug.Log("设置UI图片尺寸为：" + maxSize);
                }
            }
        }
        //精灵导入之后调用
        public void OnPostprocessSprites(Sprite spr)
        {
        }
        //声音导入前调用
        public void OnPreprocessAudio()
        {
            //每次更改都会调用
            return;
            AudioImporter audioImporter = (AudioImporter)assetImporter;
            string path = Application.dataPath + assetPath.Substring(6);

            FileInfo fileInfo = new FileInfo(path);

            //UI音效设置为单通道
            if (assetPath.ToUpper().Contains("UI"))
            {
                audioImporter.forceToMono = true;

                //短音效-后台加载-使声音的加载不阻塞主线程(太长的音效后台加载可能导致声音和画面不同步)---长音效同步加载(游戏可能会卡),但声音和画面是同步
                if (fileInfo.Length < 100 * 1024)
                {
                    audioImporter.loadInBackground = true;
                }

                //环境音，一般手机项目都不勾选的
                audioImporter.ambisonic = false;
            }

            #region 加载方式和质量设置
            /*
             * AudioClipLoadType加载音频的方式：
             * DecompressOnLoad 表示加载完音频文件之后，无压缩的释放到内存内，这样做的好处是播放的时候无需解压，速度快，减少CPU的开销，坏处是占用较多的内存。小于2秒的选用此选项。
             * CompressInMemory 表示加载完音频文件之后，以压缩的方式放到内存中，这样做的好处是节省了内存，坏处是播放的时候会消耗CPU进行解压处理。一般大于2秒，小于10秒选择这个选项。
             * Streaming 播放音频的时候流式加载，好处是文件不占用内存，坏处是加载的时候对IO、CPU都会有开销。我们项目一般对大于10秒的文件才会勾选此选项。
             * 
             * Preload Audio Data 预加载音效数据，这个是在进入场景的时候进行预加载的，会占用内存，大于10秒的文件都不进行预加载，除非有特殊情况。
             * 
             * CompressionFormat压缩格式：
             * PCM 最高质量和最大文件的方式。小于2秒的文件。
             * ADPCM 是介于PCM和Vorbis之间的压缩格式，官方推荐一些包含噪声且被多次播放的音效文件例如脚步声、打击声、武器碰撞声等可以选择，2-5秒的文件选用此选项。
             * Vorbis/Mp3 低质量的,压缩更小的文件,压缩率可以在选择了Vorbis格式之后，在Quality中进行选择，值越小，压缩越厉害，文件也越小。我们项目选择的值是：70。大于5秒的文件选择Vorbis
             * 
             * 暂时选择：加载方式-> 长:Streaming 中：CompressedInMemory 短：DecompressOnLoad  原因：长音效用流模式,短音效加载放在内存中
             * 暂时选择：压缩格-> 长：PCM 中：ADPCM 短:Vorbis 原因：按质量分=长音效为背景音乐需要高质量,短音效不需要高质量(备用按大小分：长音效用低质量，短音效用高质量,大的需要压缩)
             */

            //2秒钟(00.02)音效长度23337  23337/1024=22.8  1秒=10左右
            //长音效(背景)--20秒
            if (fileInfo.Length > 200 * 1024)
            {

                AudioImporterSampleSettings defaultSetting = audioImporter.defaultSampleSettings;
                if (defaultSetting.loadType != AudioClipLoadType.Streaming || defaultSetting.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    defaultSetting.loadType = AudioClipLoadType.Streaming;
                    defaultSetting.compressionFormat = AudioCompressionFormat.Vorbis;
                    defaultSetting.quality = 70;

                    audioImporter.defaultSampleSettings = defaultSetting;
                }
            }
            //较长音效--10秒
            else if (fileInfo.Length > 100 * 1024)
            {
                AudioImporterSampleSettings defaultSetting = audioImporter.defaultSampleSettings;
                if (defaultSetting.loadType != AudioClipLoadType.CompressedInMemory || defaultSetting.compressionFormat != AudioCompressionFormat.ADPCM)
                {
                    defaultSetting.loadType = AudioClipLoadType.CompressedInMemory;
                    defaultSetting.compressionFormat = AudioCompressionFormat.ADPCM;
                    audioImporter.defaultSampleSettings = defaultSetting;
                }
            }
            else//短音效
            {
                AudioImporterSampleSettings defaultSetting = audioImporter.defaultSampleSettings;
                if (defaultSetting.loadType != AudioClipLoadType.DecompressOnLoad || defaultSetting.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    defaultSetting.loadType = AudioClipLoadType.DecompressOnLoad;
                    defaultSetting.compressionFormat = AudioCompressionFormat.Vorbis;
                    defaultSetting.quality = 70;
                    audioImporter.defaultSampleSettings = defaultSetting;
                }
            }
            #endregion
        }
        //声音导入后调用
        public void OnPostprocessAudio(AudioClip clip)
        {
        }
        #endregion
    }
    #endregion

    #region 编辑器创建物体(添加自动设置)
    public static partial class EditorAutoTools
    {
        [MenuItem("GameObject/UI/YangImage")]
        static void CreatImage()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject("Image", typeof(Image));
                    go.transform.SetParent(Selection.activeTransform);
                    go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    go.GetComponent<Image>().raycastTarget = false;
                }
            }
        }

        [MenuItem("GameObject/UI/YangText")]
        static void CreatText()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject("Text", typeof(Text));
                    go.transform.SetParent(Selection.activeTransform);
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 30);
                    go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                    Text script = go.GetComponent<Text>();
                    script.text = "Text";
                    //script.font = AssetDatabase.LoadAssetAtPath<Font>("Assets//Fonts/KANKIN.TTF");   // 默认字体
                    script.fontSize = 20;
                    script.lineSpacing = 1.05f;
                    script.alignment = TextAnchor.MiddleCenter;
                    script.color = Color.black;
                    script.raycastTarget = false;
                }
            }
        }
    }
    #endregion

    #region 备注
    //Process.Start(fullTablePath);C# 打开文件方法
    #endregion

    /*  移动文件夹 
         /// <summary>
    /// 角色管理窗口
    /// </summary>
    public class RoleManagerWindow : EditorWindow
    {
        private Vector2 scroll;
        private const string runtimePath = "Assets/0.Game/_Bundle/Prefab_EditCreateRole";
        private const string editorPath = "Assets/GameEditor/角色编辑器/角色保存";
        private static bool moveType;//移动文件类型 true:编辑器移动到打包 false:打包移动到编辑器
        private static bool lastMoveType;
        private static bool needRefresh;
        public static string currentSelect;//当前选择角色
        public static List<string> allRoleName = new List<string>();
        /// <summary>
        /// 打开页面
        /// </summary>
        public static void OpenWindow()
        {
            GetWindow<RoleManagerWindow>("角色管理窗口");
            needRefresh = true;
            moveType = true;
            lastMoveType = moveType;
            currentSelect = null;
            Refresh();
        }
        private void OnGUI()
        {
            GUILayout.Label("选择要操作的文件");
            Texture2D editorImage = EditorGUIUtility.FindTexture("d_BuildSettings.Standalone");
            Texture2D runtimeImage = EditorGUIUtility.FindTexture("d_BuildSettings.Android");

            GUILayout.BeginHorizontal();
            moveType = GUILayout.Toggle(moveType, moveType ? editorImage : runtimeImage);
            GUILayout.BeginVertical();
            GUILayout.Label("===>");
            GUILayout.Label("===>");
            GUILayout.EndVertical();

            GUILayout.Label(moveType ? runtimeImage : editorImage);
            GUILayout.EndHorizontal();

            if (moveType)
            {
                GUILayout.Label("现在是:编辑器路径=>打包路径");
            }
            else
            {
                GUILayout.Label("现在是:打包路径=>编辑器路径");
            }

            if (lastMoveType != moveType)
            {
                needRefresh = true;
            }

            if (needRefresh)
            {
                Refresh();
            }

            if (currentSelect != null)
            {
                GUILayout.Label("当前选择角色:" + currentSelect);
            }

            //列表
            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < allRoleName.Count; i++)
            {
                var item = allRoleName[i];
                if (GUILayout.Button(allRoleName[i]))
                {
                    currentSelect = item;
                }
            }

            GUILayout.Space(10);
            GUILayout.EndScrollView();

            lastMoveType = moveType;

            if (GUILayout.Button("开始移动"))
            {
                StartMove();
            }
            GUILayout.Space(10);

            if (GUILayout.Button("关闭"))
            {
                Close();
            }
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public static void Refresh()
        {
            allRoleName.Clear();

            string currentPath = moveType ? editorPath : runtimePath;//当前路径
            DirectoryInfo root = new DirectoryInfo(currentPath);
            DirectoryInfo[] dics = root.GetDirectories();

            for (int i = 0; i < dics.Length; i++)
            {
                allRoleName.Add(dics[i].Name);
            }

            currentSelect = null;
            needRefresh = false;
        }
        public static void StartMove()
        {
            if (currentSelect == null)
            {
                EditorUtility.DisplayDialog("提示", "需要选择单个角色", "确定");
                return;
            }

            string oldPath = moveType ? editorPath : runtimePath;//当前路径
            string targtPath = moveType ? runtimePath : editorPath;//目标路径
            oldPath += "/" + currentSelect;
            targtPath += "/" + currentSelect;
            AssetDatabase.MoveAsset(oldPath, targtPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("结果", "移动完成", "确定");
            currentSelect = null;
            needRefresh = true;
        }
    }
     
     
     */
}