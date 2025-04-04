/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-23 
*/

using DataStruct;
using UnityEngine;
using YangTools.Scripts.Core.YangExtend;

public abstract class GameActor : MonoBehaviour, ICustomLife, IInteractive, IAtker, IBeHit
{
    #region 组件
    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            return animator;
        }
    }

    private Collider firstCollider;
    public Collider FirstCollider
    {
        get
        {
            if (firstCollider == null) firstCollider = GetComponentInChildren<Collider>(true);
            return firstCollider;
        }
    }
    #endregion

    #region 目标和阵营
    /// <summary>
    /// 目标物体
    /// </summary>
    public virtual GameObject Target
    {
        get { return null; }
    }
    /// <summary>
    /// 目标点
    /// </summary>
    public virtual Vector3? TargetPos
    {
        get { return null; }
    }
    /// <summary>
    /// 阵营
    /// </summary>
    public ActorCampType campType;
    /// <summary>
    /// 可攻击阵营
    /// </summary>
    public ActorCampType canAtkCamp;
    #endregion

    #region 生命周期
    public abstract void IInit();
    public abstract void IUpdate();
    public abstract void ILateUpdate();
    public abstract void IFixedUpdate();
    public abstract void IDie();
    public abstract void IDestroy();
    #endregion

    #region 攻击和被击接口实现
    public virtual void Atk(AtkInfo atkInfo)
    {

    }
    public virtual void BeHit(ref DamageInfo damageInfo)
    {

    }
    public virtual DamageInfo GetDamageInfo()
    {
        return null;
    }
    public virtual DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        return damageInfo;
    }
    public virtual bool IsCanAtk()
    {
        return false;
    }
    public virtual bool IsCanBeHit()
    {
        return false;
    }
    public virtual void ShowAtkEffect(EffectInfo atkEffectInfo)
    {
    }
    public virtual void ShowBeHitEffect(EffectInfo hitEffectInfo)
    {
    }
    #endregion

    #region 方法
    public Vector3 GetMeshSize()
    {
        MeshFilter temp = gameObject.GetComponentInChildren<MeshFilter>();
        Vector3 result = default;
        result.x = temp.mesh.bounds.size.x * temp.transform.localScale.x;
        result.y = temp.mesh.bounds.size.y * temp.transform.localScale.y;
        result.z = temp.mesh.bounds.size.z * temp.transform.localScale.z;
        return result;
    }

    public Vector3 GetColliderSize()
    {
        Collider temp = GetComponent<Collider>();
        Vector3 result = default;
        result.x = temp.bounds.size.x;
        result.y = temp.bounds.size.y;
        result.z = temp.bounds.size.z;
        return result;
    }
    #endregion

    #region 交互
    /// <summary>
    /// 获得大小(半径)
    /// </summary>
    public virtual float GetSize(RoleSizeType roleSizeType)
    {
        switch (roleSizeType)
        {
            case RoleSizeType.ColliderSize:
                return GetColliderSize().SetYValue().magnitude / 2;
            case RoleSizeType.MeshSize:
                return GetMeshSize().SetYValue().magnitude / 2;
            default:
                break;
        }

        return 0;
    }
    /// <summary>
    /// 是有效的
    /// </summary>
    public virtual bool IsValid()
    {
        return gameObject != null;
    }
    /// <summary>
    /// 获得位置
    /// </summary>
    public virtual Vector3 GetPos()
    {
        return transform.position;
    }
    /// <summary>
    /// 距离
    /// </summary>
    public virtual float DistanceToPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }
    /// <summary>
    /// 覆盖范围
    /// </summary>
    public virtual float GetOverideMaxDistance()
    {
        return float.MaxValue;
    }
    /// <summary>
    /// 可以交互
    /// </summary>
    public virtual bool CanInter()
    {
        return true;
    }
    /// <summary>
    /// 进入范围
    /// </summary>
    public virtual void OnEnterRang(RoleBase role)
    {

    }
    /// <summary>
    /// 退出范围
    /// </summary>
    public virtual void OnExitRang(RoleBase role)
    {

    }
    /// <summary>
    /// 交互
    /// </summary>
    public virtual void Interact(RoleBase role)
    {

    }
    #endregion

    #region 获得点
    /// <summary>
    /// 返回碰撞器上最接近给定位置的点
    /// </summary>
    public Vector3 ClosestColliderPos(Vector3 source)
    {
        return FirstCollider.ClosestPoint(source);
    }
    #endregion
}