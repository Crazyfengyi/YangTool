#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-01-03 
*/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 切割动画弹窗
    /// </summary>
    public class SplitAni : EditorWindow
    {
        private static GameObject model;
        //动画文件
        public static UnityEngine.Object aniObj;
        //切割表
        public static UnityEngine.Object frameObj;
        //打开界面
        [MenuItem(SettingInfo.YongToolsFunctionPath + "自动切割动画")]
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
    
    public class monsterInfo
    {
        public string id;
    }
}
#endif