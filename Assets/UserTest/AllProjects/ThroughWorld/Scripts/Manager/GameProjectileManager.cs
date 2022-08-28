/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-28 
*/  
using UnityEngine;  
using System.Collections;
using YangTools;
  
public class GameProjectileManager : MonoSingleton<GameProjectileManager> 
{
	/*
	 * 创建追踪子弹需要的参数主要有：
		Owner：表示子弹的创建者
		Ability：表示子弹关联的技能
		FromPosition：子弹的出发地点
		Target：子弹追踪的目标
		Speed：子弹的飞行速率
		创建线性子弹需要的参数主要有：

		Owner：表示子弹的创建者
		Ability：表示子弹关联的技能
		FromPosition：子弹的出发地点
		Velocity：子弹的飞行速度和方向
		StartWidth，EndWith，Distance：（等腰梯形检测盒）起点宽度，终点宽度，飞行距离
		FilterTargetInfo：子弹筛选目标信息

		由于子弹在创建时传入了Ability参数，那么当子弹检测到命中目标后便可以通过调用GetAbility().OnProjectileHit(projHandle, hitTarget, hitPosition)在Ability Class中执行命中逻辑，
		这样它就可以在技能模块中通过执行策划配置来实现各种效果了。子弹本身是没有任何特殊逻辑的，它只有位置更新，
		检查命中目标和是否结束并销毁这几个简单的功能。
	*/
}