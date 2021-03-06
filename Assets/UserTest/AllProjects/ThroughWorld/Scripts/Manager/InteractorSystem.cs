/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-31 
*/
using UnityEngine;
using System.Collections;
using YangTools;
/// <summary>
/// 交互系统
/// </summary>
public class InteractorSystem : MonoSingleton<InteractorSystem>
{
    public float InteractorRange = 3;//可交互范围
    private WaitForSeconds interactorTimerInterval = new WaitForSeconds(0.2f);//交互物检测间隔
    //交互者
    private RoleBase target;
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
    private void Update()
    {
        if (currentInteractive != lastInteractive || ForceUpdate)
        {
            //进入范围
            if (currentInteractive != null) currentInteractive.EnterRang(target);
            //退出范围
            if (lastInteractive != null) lastInteractive.ExitRang(target);
            //更新物体交互类型
            if (currentInteractive != null) interActiveType = currentInteractive.GetInterActiveType();

            //更新记录
            lastInteractive = currentInteractive;
            ForceUpdate = false;
        }
    }

    /// <summary>
    /// 执行交互
    /// </summary>
    public void OnInter()
    {
        if (currentInteractive == null) return;
        currentInteractive.InterAction(target);
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
                if (item == null || !item.TryGetComponent<IInteractive>(out var interactive) || !interactive.CanInter()) continue;
                float overideDistance = interactive.GetOverideMaxDistance();//覆盖范围
                float distance = interactive.Distance(recordPos);//距离
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