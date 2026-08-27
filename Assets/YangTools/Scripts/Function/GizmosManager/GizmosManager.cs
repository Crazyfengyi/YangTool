using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理运行时调试图形的绘制
/// </summary>
public class GizmosManager : MonoBehaviour
{
    public static GizmosManager Instance; //当前实例

    private const float SectorStepAngle = 5f; //扇形绘制步长

    private readonly List<DrawInfo> drawInfos = new List<DrawInfo>(); //待绘制信息

    /// <summary>
    /// 初始化单例实例
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{nameof(GizmosManager)} 场景中存在多个实例", this);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 清理失效单例和绘制信息
    /// </summary>
    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        Instance = null;
        drawInfos.Clear();
    }

    #region 绘制接口

    /// <summary>
    /// 绘制矩形
    /// </summary>
    /// <param name="center">中心位置</param>
    /// <param name="quaternion">旋转角度</param>
    /// <param name="size">矩形尺寸</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawCube(Vector3 center, Quaternion quaternion, Vector3 size, float time = 5f)
    {
        AddDrawInfo(new DrawCubeInfo
        {
            center = center,
            rotate = quaternion,
            size = size,
            overTime = Time.unscaledTime + time
        });
    }

    /// <summary>
    /// 绘制扇形
    /// </summary>
    /// <param name="pos">中心位置</param>
    /// <param name="direction">朝向</param>
    /// <param name="length">半径长度</param>
    /// <param name="angle">扇形角度</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawSector(Vector3 pos, Vector3 direction, float length, float angle, float time = 5f)
    {
        if (length <= 0f || angle <= 0f)
        {
            return;
        }

        AddDrawInfo(new DrawSectorInfo
        {
            pos = pos,
            direction = direction,
            length = length,
            angle = angle,
            overTime = Time.unscaledTime + time
        });
    }

    /// <summary>
    /// 绘制圆形
    /// </summary>
    /// <param name="pos">圆心位置</param>
    /// <param name="radius">圆形半径</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawCircle(Vector3 pos, float radius, float time = 5f)
    {
        if (radius <= 0f)
        {
            return;
        }

        AddDrawInfo(new DrawCircleInfo
        {
            pos = pos,
            radius = radius,
            overTime = Time.unscaledTime + time
        });
    }

    /// <summary>
    /// 绘制圆环
    /// </summary>
    /// <param name="pos">圆心位置</param>
    /// <param name="insideRadius">内圆半径</param>
    /// <param name="outsideRadius">外圆半径</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawRing(Vector3 pos, float insideRadius, float outsideRadius, float time = 5f)
    {
        if (insideRadius < 0f || outsideRadius < insideRadius)
        {
            return;
        }

        AddDrawInfo(new DrawRingInfo
        {
            pos = pos,
            insideRadius = insideRadius,
            outsideRadius = outsideRadius,
            overTime = Time.unscaledTime + time
        });
    }

    /// <summary>
    /// 绘制球形射线
    /// </summary>
    /// <param name="pos">起点位置</param>
    /// <param name="direction">射线方向</param>
    /// <param name="distance">射线距离</param>
    /// <param name="radius">球体半径</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawSphereRay(Vector3 pos, Vector3 direction, float distance, float radius, float time = 5f)
    {
        if (distance <= 0f || radius <= 0f)
        {
            return;
        }

        AddDrawInfo(new DrawSphereRayInfo
        {
            pos = pos,
            direction = direction,
            distance = distance,
            radius = radius,
            overTime = Time.unscaledTime + time
        });
    }

    /// <summary>
    /// 绘制射线
    /// </summary>
    /// <param name="pos">起点位置</param>
    /// <param name="direction">射线方向</param>
    /// <param name="distance">射线距离</param>
    /// <param name="time">显示时长</param>
    public void GizmosDrawRay(Vector3 pos, Vector3 direction, float distance, float time = 5f)
    {
        if (distance <= 0f)
        {
            return;
        }

        AddDrawInfo(new DrawRayInfo
        {
            pos = pos,
            direction = direction,
            distance = distance,
            overTime = Time.unscaledTime + time
        });
    }

    #endregion

    #region 绘制实现

    /// <summary>
    /// 添加待绘制信息并清理过期数据
    /// </summary>
    /// <param name="drawInfo">绘制信息</param>
    private void AddDrawInfo(DrawInfo drawInfo)
    {
        RemoveExpiredDrawInfos();
        drawInfos.Add(drawInfo);
    }

    /// <summary>
    /// 绘制所有有效调试图形
    /// </summary>
    private void OnDrawGizmos()
    {
        RemoveExpiredDrawInfos();
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.red;

        foreach (DrawInfo item in drawInfos)
        {
            switch (item)
            {
                case DrawCubeInfo cubeInfo:
                    DrawCube(cubeInfo);
                    break;
                case DrawSectorInfo sectorInfo:
                    DrawSector(sectorInfo);
                    break;
                case DrawCircleInfo circleInfo:
                    Gizmos.DrawWireSphere(circleInfo.pos, circleInfo.radius);
                    break;
                case DrawRingInfo ringInfo:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(ringInfo.pos, ringInfo.insideRadius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(ringInfo.pos, ringInfo.outsideRadius);
                    break;
                case DrawSphereRayInfo sphereRayInfo:
                    DrawSphereRay(sphereRayInfo);
                    break;
                case DrawRayInfo rayInfo:
                    Gizmos.DrawLine(rayInfo.pos, rayInfo.pos + rayInfo.direction * rayInfo.distance);
                    break;
            }
        }

        Gizmos.color = previousColor;
    }

    /// <summary>
    /// 清理过期绘制信息
    /// </summary>
    private void RemoveExpiredDrawInfos()
    {
        for (int i = drawInfos.Count - 1; i >= 0; i--)
        {
            if (Time.unscaledTime >= drawInfos[i].overTime)
            {
                drawInfos.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 绘制带旋转的矩形
    /// </summary>
    /// <param name="drawInfo">矩形绘制信息</param>
    private void DrawCube(DrawCubeInfo drawInfo)
    {
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(drawInfo.center, drawInfo.rotate, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, drawInfo.size);
        Gizmos.matrix = previousMatrix;
    }

    /// <summary>
    /// 绘制扇形边界和弧线
    /// </summary>
    /// <param name="drawInfo">扇形绘制信息</param>
    private void DrawSector(DrawSectorInfo drawInfo)
    {
        float halfAngle = drawInfo.angle * 0.5f;
        int segmentCount = Mathf.Max(1, Mathf.CeilToInt(drawInfo.angle / SectorStepAngle));
        Vector3 previousPoint = drawInfo.pos + Quaternion.Euler(0f, -halfAngle, 0f) * drawInfo.direction * drawInfo.length;
        Gizmos.DrawLine(drawInfo.pos, previousPoint);

        for (int i = 1; i <= segmentCount; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segmentCount);
            Vector3 currentPoint = drawInfo.pos + Quaternion.Euler(0f, angle, 0f) * drawInfo.direction * drawInfo.length;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        Gizmos.DrawLine(drawInfo.pos, previousPoint);
    }

    /// <summary>
    /// 绘制球形射线的起点路径和终点
    /// </summary>
    /// <param name="drawInfo">球形射线绘制信息</param>
    private void DrawSphereRay(DrawSphereRayInfo drawInfo)
    {
        Gizmos.DrawWireSphere(drawInfo.pos, drawInfo.radius);
        for (float currentDistance = drawInfo.radius; currentDistance < drawInfo.distance; currentDistance += drawInfo.radius)
        {
            Gizmos.DrawWireSphere(drawInfo.pos + drawInfo.direction * currentDistance, drawInfo.radius);
        }

        Gizmos.DrawWireSphere(drawInfo.pos + drawInfo.direction * drawInfo.distance, drawInfo.radius);
    }

    #endregion
}

/// <summary>
/// 调试图形的基础信息
/// </summary>
public class DrawInfo
{
    public float overTime; //过期时间
}

/// <summary>
/// 矩形绘制信息
/// </summary>
public class DrawCubeInfo : DrawInfo
{
    public Vector3 center; //中心位置
    public Quaternion rotate; //旋转角度
    public Vector3 size; //矩形尺寸
}

/// <summary>
/// 扇形绘制信息
/// </summary>
public class DrawSectorInfo : DrawInfo
{
    public Vector3 pos; //中心位置
    public Vector3 direction; //朝向
    public float length; //半径长度
    public float angle; //扇形角度
}

/// <summary>
/// 圆形绘制信息
/// </summary>
public class DrawCircleInfo : DrawInfo
{
    public Vector3 pos; //圆心位置
    public float radius; //圆形半径
}

/// <summary>
/// 圆环绘制信息
/// </summary>
public class DrawRingInfo : DrawInfo
{
    public Vector3 pos; //圆心位置
    public float insideRadius; //内圆半径
    public float outsideRadius; //外圆半径
}

/// <summary>
/// 球形射线绘制信息
/// </summary>
public class DrawSphereRayInfo : DrawInfo
{
    public Vector3 pos; //起点位置
    public Vector3 direction; //射线方向
    public float distance; //射线距离
    public float radius; //球体半径
}

/// <summary>
/// 射线绘制信息
/// </summary>
public class DrawRayInfo : DrawInfo
{
    public Vector3 pos; //起点位置
    public Vector3 direction; //射线方向
    public float distance; //射线距离
}
