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
    private WaitForSeconds interactorTimerInterval = new WaitForSeconds(0.3f);//交互物检测间隔
    /// <summary>
    /// 最近的可交互物
    /// </summary>
    private IInteractive nearestInteractive;
    private void Start()
    {
        StartCoroutine("UpdateNearestInteractive");
    }
    /// <summary>
    /// 执行交互
    /// </summary>
    public void OnInter()
    {
        if (nearestInteractive == null) return;

        nearestInteractive.InterAction(GameActorManager.Instance.MainPlayer);
        nearestInteractive = null;
    }

    private Vector3 recordPos;//记录位置
    private Collider[] recordCollider = new Collider[20];//记录碰撞体
    /// <summary>
    /// 更新最近的可交互物
    /// </summary>
    private IEnumerator UpdateNearestInteractive()
    {
        while (true)
        {
            //nearestInteractive = null;
            recordPos = GameActorManager.Instance.MainPlayer ? GameActorManager.Instance.MainPlayer.transform.position : Vector3.zero;
            Physics.OverlapSphereNonAlloc(recordPos, InteractorRange, recordCollider);
            foreach (var item in recordCollider)
            {
                if (item == null || !item.TryGetComponent<IInteractive>(out var interactive)) continue;
                if (nearestInteractive == null || interactive.Distance(recordPos) < nearestInteractive.Distance(recordPos))
                {
                    nearestInteractive = interactive;
                }
            }

            yield return interactorTimerInterval;
        }
    }
}