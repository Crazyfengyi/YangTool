/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-01-03 
*/
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    public static class AnimatorFactoryTool
    {
        public static AnimationClip LoadAnimClip(string path)
        {
            return (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
        }
        public static AnimationClip[] LoadAllAnimClip(string path)
        {
            List<AnimationClip> result = new List<AnimationClip>();
            //拿文件的数据
            UnityEngine.Object[] datas = AssetDatabase.LoadAllAssetsAtPath(path);
            //动画文件
            for (int i = 0; i < datas.Length; i++)
            {
                if (!(datas[i] is AnimationClip)) continue;//只关注AnimationClip
                AnimationClip temp = datas[i] as AnimationClip;
                result.Add(temp);
            }
            return result.ToArray();
        }
        public static AnimationClip LoadAnimClip(string fbxPath, string animPath)
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            return objs.Where(item => item is AnimationClip && item.name.Equals(animPath)).Select(o => o as AnimationClip).FirstOrDefault();
        }
    }
}
