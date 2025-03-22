/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-07-31
*/

using System.Collections;
using DataStruct;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangExtend;
using YangTools.Scripts.Core;

/// <summary>
/// 交互系统
/// </summary>
public class InteractorSystem : MonoSingleton<InteractorSystem>
{
    public float InteractorRange = 3;//可交互范围
    private WaitForSeconds interactorTimerInterval = new WaitForSeconds(0.2f);//交互物检测间隔

    //交互者
    private RoleBase target;

    public GameObject ShowCircle;//显示的圆

    //最近的可交互物
    private IInteractive currentInteractive;

    //上一个交互物
    private IInteractive lastInteractive;

    //强制刷新可交互物
    private bool forceUpdate;

    /// <summary>
    /// 强制刷新
    /// </summary>
    public bool ForceUpdate
    {
        get { return forceUpdate; }
        set { forceUpdate = value; }
    }

    private InterActiveType interActiveType;

    /// <summary>
    /// 当前物体交互类型
    /// </summary>
    private InterActiveType InterActiveType
    {
        get { return interActiveType; }
    }

    private void Start()
    {
        target = GameActorManager.Instance.MainPlayer;
        StartCoroutine("UpdateNearestInteractive");
    }

    private void LateUpdate()
    {
        if (target == null) target = GameActorManager.Instance.MainPlayer;

        if (currentInteractive != lastInteractive || ForceUpdate)
        {
            //进入范围
            if (currentInteractive != null && !currentInteractive.Equals(null) && currentInteractive.IsValid())
            {
                currentInteractive.OnEnterRang(target);
                ShowCircle.transform.position = currentInteractive.GetPos().SetYValue();
            }
            //退出范围
            if (lastInteractive != null && !lastInteractive.Equals(null) && lastInteractive.IsValid())
            {
                lastInteractive.OnExitRang(target);
            }

            //更新记录
            lastInteractive = currentInteractive;
            ForceUpdate = false;
        }

        //更新物体交互类型
        if (currentInteractive != null && !currentInteractive.Equals(null) && currentInteractive.IsValid())
        {
            interActiveType = currentInteractive.GetInterActiveType();
            ShowCircle.transform.position = currentInteractive.GetPos().SetYValue();
            ShowCircle.transform.localScale = Vector3.one * currentInteractive.GetSize(RoleSizeType.ColliderSize);
        }
        ShowCircle.SetActive(currentInteractive != null);
    }

    /// <summary>
    /// 执行交互
    /// </summary>
    public void OnInter()
    {
        if (currentInteractive == null) return;
        currentInteractive.Interact(target);
        currentInteractive = null;
        ForceUpdate = true;
    }

    private Vector3 recordPos;//记录位置
    private Collider[] recordCollider; //记录碰撞体

    /// <summary>
    /// 更新最近的可交互物
    /// </summary>
    private IEnumerator UpdateNearestInteractive()
    {
        while (true)
        {
            recordCollider = new Collider[20];
            currentInteractive = null;
            recordPos = target ? target.transform.position : Vector3.zero;
            Physics.OverlapSphereNonAlloc(recordPos, InteractorRange, recordCollider, ~LayerMask.GetMask("Player"));
            float currentMinDistance = float.MaxValue;
            foreach (var item in recordCollider)
            {
                if (item == null || !item.TryGetComponent<IInteractive>(out var interactive) || !interactive.CanInter() || !interactive.IsValid()) continue;
                if (interactive == null || interactive.Equals(null)) continue;

                float overideDistance = interactive.GetOverideMaxDistance();//覆盖范围
                float distance = interactive.DistanceToPoint(recordPos);//距离
                if (distance < overideDistance && distance < currentMinDistance)
                {
                    currentMinDistance = distance;
                    currentInteractive = interactive;
                }
            }

            yield return interactorTimerInterval;
        }
    }
}