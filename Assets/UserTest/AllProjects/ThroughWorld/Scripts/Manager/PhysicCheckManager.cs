/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-03 
*/
#if UNITY_EDITOR
#define ShowCheckDebugRang
#endif

using UnityEngine;
using System.Collections;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using YangTools;
using DataStruct;
using System.Linq;
using UnityEditorInternal;
using cfg.player;
using YangTools.Scripts.Core.YangExtend;
using YangTools.Scripts.Core;

/// <summary>
/// 物理检测管理类
/// </summary>
public class PhysicsCheckManager : Singleton<PhysicsCheckManager>
{
    private List<Collider> allCollider = new List<Collider>();
    public LayerMask GetAtkLayer(RoleBase role)
    {
        List<string> strings = new List<string>();
        switch (role.canAtkCamp)
        {
            case ActorCampType.Player:
                strings.Add("Player");
                break;
            case ActorCampType.Monster:
                strings.Add("Enemy");
                break;
            case ActorCampType.Npc:
                strings.Add("NPC");
                break;
            case ActorCampType.Building:
                strings.Add("Build");
                break;
            case ActorCampType.MonsterAndBuilding:
                strings.Add("Enemy");
                strings.Add("Build");
                break;
            case ActorCampType.PlayerAndBuilding:
                strings.Add("Player");
                strings.Add("Build");
                break;
            default:
                break;
        }
        LayerMask result = LayerMask.GetMask(strings.ToArray());
        return result;
    }

    /// <summary>
    /// 根据信息物理检测
    /// </summary>
    public void PhysicCheck(CheckConfig checkConfig, RoleBase role, out Collider[] colliders, bool isIncludeSelf = false)
    {
        allCollider.Clear();
        if (checkConfig == null)
        {
            colliders = allCollider.ToArray();
            return;
        }

        Vector3 startPos = GetStartPos(checkConfig, role.transform, role.ModelInfo.Root);

        switch (checkConfig.checkType)
        {
            case HitCheckType.射线:
                colliders = Raycast(startPos, role.ModelInfo.Root.forward, checkConfig.distanceOfLine,
                    GetAtkLayer(role));
                break;
            case HitCheckType.矩形:
                {
                    colliders = SquareCheck(startPos, role.ModelInfo.Root.forward, 0.5f * new Vector3(checkConfig.Width, checkConfig.Hight, checkConfig.Long),
                        Quaternion.Euler(role.ModelInfo.Root.eulerAngles), GetAtkLayer(role));
                    break;
                }
            case HitCheckType.扇形:
                colliders = SectorCheck(startPos, role.ModelInfo.Root.forward, checkConfig.diameter, checkConfig.Angle, GetAtkLayer(role));
                break;
            case HitCheckType.圆形:
                colliders = CircleCheck(startPos, checkConfig.radius, GetAtkLayer(role));
                break;
            case HitCheckType.圆环:
                colliders = RingCheck(startPos, checkConfig.insideRadius, checkConfig.outsideRadius,
                    GetAtkLayer(role));
                break;
            case HitCheckType.球形射线:
                colliders = SphereRaycast(startPos, role.ModelInfo.Root.forward, checkConfig.distance,
                    checkConfig.radiusOfBallLine, GetAtkLayer(role));
                break;
            default:
                colliders = allCollider.ToArray();
                break;
        }

        if (isIncludeSelf == false)
        {
            Collider[] tempCollider = role.GetComponentsInChildren<Collider>(true);
            List<Collider> tempList = colliders.ToList();
            for (int i = 0; i < tempCollider.Length; i++)
            {
                if (tempList.Contains(tempCollider[i]))
                {
                    tempList.Remove(tempCollider[i]);
                }
            }
            colliders = tempList.ToArray();
        }
    }
    /// <summary>
    /// 获得起始点
    /// </summary>
    public Vector3 GetStartPos(CheckConfig config, Transform transform, Transform model)
    {
        Vector3 result = transform.position;
        Vector3 offsetz = model.forward * config.offsetVector.z;
        Vector3 offsetx = model.right * config.offsetVector.x;
        Vector3 offsety = model.up * config.offsetVector.y;

        Vector3 offset = offsetz + offsetx + offsety;
        result += offset;

        return result;
    }

    #region 具体各种检测
    /// <summary>
    /// 扇形检测
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="length">距离</param>
    /// <param name="layerMask">层级</param>
    /// <param name="angle">角度</param>
    /// <param name="forward">前方</param>
    private Collider[] SectorCheck(Vector3 pos, Vector3 forward, float length, float angle, int layerMask)
    {
        Collider[] collider = Physics.OverlapSphere(pos, length, layerMask);
        GizmosManager.Instance.GizmosDrawSector(pos, forward, length, angle);

        //角度的一半
        float sectorAngle = angle * 0.5f;
        allCollider.Clear();
        for (int i = 0; i < collider.Length; i++)
        {
            //角度
            float tempAngle = Vector3.Angle(collider[i].transform.position - pos, forward);
            if (tempAngle < sectorAngle)
            {
                allCollider.Add(collider[i]);
            }
        }

        return allCollider.ToArray();
    }
    /// <summary>
    /// 矩形检测
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="halfExtents">长度的一半</param>
    /// <param name="quaternion">旋转</param>
    /// <param name="layerMask">层级</param>
    private Collider[] SquareCheck(Vector3 pos, Vector3 forward, Vector3 halfExtents, Quaternion quaternion, int layerMask)
    {
        Collider[] collider = Physics.OverlapBox(pos + forward * halfExtents.z, halfExtents, quaternion, layerMask);
        GizmosManager.Instance.GizmosDrawCube(pos + forward * halfExtents.z, quaternion, halfExtents * 2);

        allCollider.Clear();
        for (int i = 0; i < collider.Length; i++)
        {
            allCollider.Add(collider[i]);
        }

        return allCollider.ToArray();
    }
    /// <summary>
    /// 圆形查找
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="radius">长度</param>
    /// <param name="layerMask">层级</param>
    public Collider[] CircleCheck(Vector3 pos, float radius, int layerMask)
    {
        Collider[] collider = Physics.OverlapSphere(pos, radius, layerMask);
        GizmosManager.Instance.GizmosDrawCircle(pos, radius);

        allCollider.Clear();
        for (int i = 0; i < collider.Length; i++)
        {
            allCollider.Add(collider[i]);
        }

        return allCollider.ToArray();
    }
    /// <summary>
    /// 圆环查找
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="minDis">内环距离</param>
    /// <param name="maxDis">外环距离</param>
    /// <param name="layerMask">层级</param>
    private Collider[] RingCheck(Vector3 pos, float insideRadius, float outideRadius, int layerMask)
    {
        Collider[] collider = Physics.OverlapSphere(pos, outideRadius, layerMask);
        GizmosManager.Instance.GizmosDrawRing(pos, insideRadius, outideRadius);

        //内圆半径平方
        float smallDisSqr = outideRadius * outideRadius;
        allCollider.Clear();
        for (int i = 0; i < collider.Length; i++)
        {
            float tempDis = (collider[i].transform.position.SetYValue() - pos.SetYValue()).sqrMagnitude;
            if (tempDis > smallDisSqr)
            {
                allCollider.Add(collider[i]);
            }
        }
        return allCollider.ToArray();
    }
    /// <summary>
    ///  直线球形射线查找
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="radius">半径</param>
    /// <param name="direction">方向</param>
    /// <param name="distance">距离</param>
    private Collider[] SphereRaycast(Vector3 pos, Vector3 direction, float distance, float radius, int layerMask)
    {
        GizmosManager.Instance.GizmosDrawSphereRay(pos, direction, distance, radius);

        allCollider.Clear();
        if (Physics.SphereCast(pos, radius, direction, out RaycastHit hitInfo, distance, layerMask))
        {
            allCollider.Add(hitInfo.collider);
        }
        return allCollider.ToArray();
    }
    /// <summary>
    /// 射线查找
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="direction">方向</param>
    /// <param name="distance">距离</param>
    private Collider[] Raycast(Vector3 pos, Vector3 direction, float distance, int layerMask)
    {
        GizmosManager.Instance.GizmosDrawRay(pos, direction, distance);

        allCollider.Clear();
        if (Physics.Raycast(pos, direction, out RaycastHit hitInfo, distance, layerMask))
        {
            allCollider.Add(hitInfo.collider);
        }
        return allCollider.ToArray();
    }
    #endregion
}