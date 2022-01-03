/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-01-03 
*/  
using UnityEngine;  
using System.Collections;
using UnityEditor;
using System.Linq;

public static class AnimatorFactoryTool 
{
	public static AnimationClip LoadAnimClip(string path)
	{
		return (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
	}

	public static AnimationClip LoadAnimClip(string fbxPath, string animPath)
	{
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
		return objs.Where(item => item is AnimationClip && item.name.Equals(animPath)).Select(o => o as AnimationClip).FirstOrDefault();
	}
} 