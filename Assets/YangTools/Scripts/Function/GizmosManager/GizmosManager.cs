using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GizmosManager : MonoBehaviour
{
    public static GizmosManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private List<DrawInfo> drawInfos = new List<DrawInfo>();

    /*
     * if (Input.GetKeyDown(KeyCode.Space))
        {
            GizmosManager.Instance.GizmosDrawCube(Vector3.zero, new Vector3(2,2,2));
        }
     */
    /// <summary>
    /// 矩形
    /// </summary>
    /// <param name="center"></param>
    /// <param name="size"></param>
    /// <param name="time"></param>
    public void GizmosDrawCube(Vector3 center, Vector3 size, float time = 2f)
    {
        drawInfos.Add(new DrawCubeInfo
        {
            center = center,
            size = size,
            time = time
        });
    }
    /// <summary>
    /// 扇形
    /// </summary>
    public void GizmosDrawSector(Vector3 pos, Vector3 direction, float length, float angle, float time = 2f)
    {
        drawInfos.Add(new DrawSectorInfo
        {
            pos = pos,
            direction = direction,
            length = length,
            angle = angle,
            time = time
        });
    }
    /// <summary>
    /// 园形
    /// </summary>
    public void GizmosDrawCircle(Vector3 pos, float radius, float time = 2f)
    {
        drawInfos.Add(new DrawCircleInfo
        {
            pos = pos,
            radius = radius,
            time = time
        });
    }
    /// <summary>
    /// 园环
    /// </summary>
    public void GizmosDrawRing(Vector3 pos, float insideRadius, float outsideRadius, float time = 2f)
    {
        drawInfos.Add(new DrawRingInfo
        {
            pos = pos,
            insideRadius = insideRadius,
            outsideRadius = outsideRadius,
            time = time
        });
    }

    /// <summary>
    /// 球形射线
    /// </summary>
    public void GizmosDrawSphereRay(Vector3 pos, Vector3 direction, float distance, float radius, float time = 2f)
    {
        drawInfos.Add(new DrawSphereRayInfo
        {
            pos = pos,
            direction = direction,
            distance = distance,
            radius = radius,
            time = time
        });
    }

    /// <summary>
    /// 射线
    /// </summary>
    public void GizmosDrawRay(Vector3 pos, Vector3 direction, float distance, float time = 2f)
    {
        drawInfos.Add(new DrawRayInfo
        {
            pos = pos,
            direction = direction,
            distance = distance,
            time = time
        });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (DrawInfo item in drawInfos)
        {
            switch (item)
            {
                case DrawCubeInfo temp:
                    Gizmos.DrawWireCube(temp.center, temp.size);
                    break;
                case DrawSectorInfo temp:
                    for (float i = -(temp.angle / 2); i < temp.angle; i += 5)
                    {
                        Ray r = new Ray();
                        r.origin = temp.pos;
                        r.direction = Quaternion.Euler(0, i, 0) * temp.direction;
                        Gizmos.DrawLine(temp.pos, r.GetPoint(temp.length));
                    }
                    break;
                case DrawCircleInfo temp:
                    Gizmos.DrawWireSphere(temp.pos, temp.radius);
                    break;
                case DrawRingInfo temp:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(temp.pos, temp.insideRadius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(temp.pos, temp.outsideRadius);
                    Gizmos.color = Color.red;
                    break;
                case DrawSphereRayInfo temp:
                    for (float i = temp.radius; i < temp.distance; i += temp.radius)
                    {
                        Ray r = new Ray();
                        r.origin = temp.pos;
                        r.direction = temp.direction;
                        Gizmos.DrawWireSphere(r.GetPoint(i), temp.radius);
                    }
                    break;
                case DrawRayInfo temp:
                    {
                        Ray r = new Ray();
                        r.origin = temp.pos;
                        r.direction = temp.direction;
                        Gizmos.DrawLine(temp.pos, r.GetPoint(temp.distance));
                    }
                    break;
            }

            item.time -= Time.deltaTime;
        }

        for (int i = drawInfos.Count - 1; i >= 0; i--)
        {
            if (drawInfos[i].time <= 0f)
            {
                drawInfos.RemoveAt(i);
            }
        }
        Gizmos.color = Color.white;
    }
}

public class DrawInfo
{
    public float time;
}
/// <summary>
/// 矩形
/// </summary>
public class DrawCubeInfo : DrawInfo
{
    public Vector3 center;
    public Vector3 size;
}
/// <summary>
/// 扇形
/// </summary>
public class DrawSectorInfo : DrawInfo
{
    public Vector3 pos;
    public Vector3 direction;
    public float length;
    public float angle;
}
/// <summary>
/// 园形
/// </summary>
public class DrawCircleInfo : DrawInfo
{
    public Vector3 pos;
    public float radius;
}

/// <summary>
/// 园环
/// </summary>
public class DrawRingInfo : DrawInfo
{
    public Vector3 pos;
    public float insideRadius;
    public float outsideRadius;
}

/// <summary>
/// 球形射线
/// </summary>
public class DrawSphereRayInfo : DrawInfo
{
    public Vector3 pos;
    public Vector3 direction;
    public float distance;
    public float radius;
}
/// <summary>
/// 射线
/// </summary>
public class DrawRayInfo : DrawInfo
{
    public Vector3 pos;
    public Vector3 direction;
    public float distance;
}