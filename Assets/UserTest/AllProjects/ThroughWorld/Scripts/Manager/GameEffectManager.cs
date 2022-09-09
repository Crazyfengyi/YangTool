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

public class GameEffectManager : MonoSingleton<GameEffectManager>
{
    /*
     * effectTypeId: 特效类型id，关联配置表相关项。
       controlPoints：特效相关控制点数据，后文详细讲述。
       controlEntities：特效相关单位控制数据，后文详细讲述。
       bRetain：EffectManager是否持有特效，后文详细讲述。
       effectHandle: 当bRetain=true时，创建特效后返回有效句柄，数值大于0，供游戏逻辑使用控制特效行为
       AttachType的定义如下：
       Attach_AbsOrigin: 特效基于目标坐标创建。
       Attach_AbsOrigin_Follow: 特效基于目标坐标创建并跟随目标位置移动。
       Attach_Point: 特效基于目标挂点(attachName)位置创建，但不跟随目标。
       Attach_Point_Follow: 特效基于目标挂点(attachName)位置创建，跟随目标。
     * */

}