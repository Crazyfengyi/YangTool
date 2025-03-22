/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-26
*/

namespace YangTools.Scripts.Core.YangMiniMap
{
    /// <summary>
    /// 小地图渲染类型
    /// </summary>
    [System.Serializable]
    public enum RenderType
    {
        RealTime,
        Picture,
    }

    [System.Serializable]
    public enum RenderMode
    {
        Mode2D,
        Mode3D,
    }

    [System.Serializable]
    public enum MapType
    {
        Target,
        World,
    }

    /// <summary>
    /// 小地图uiIcon出现效果
    /// </summary>
    public enum UIIconShowEffect
    {
        None,

        /// <summary>
        /// 淡出
        /// </summary>
        Fade,
    }
}