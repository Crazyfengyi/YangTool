/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-05-21 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// 物理子弹
/// </summary>
public class PhysicsBullet : BulletBase
{
    public Rigidbody body;
    public Vector3 targetPos;

    public float h = 100;
    public float gravity = -9.8f;
    public PhysicsBullet(BulletData data, GameObject Obj) : base(data, Obj)
    {
        body = bulletObj.GetComponent<Rigidbody>();
        targetPos = data.TargetPostion;

        Init();
    }

    //TODO:用刚体做,有物理表现--碰撞到建筑后反弹
    public void Init()
    {
        body.useGravity = true;
        h = targetPos.y + 2;
        body.velocity = CalculateLaunchVelocity().initVelocity;
        DrawPath();
    }
    public override void OnUpdate()
    {
        //base.OnUpdate();
        if (Data == null || bulletObj == null) return;
        if (isDie) return;

        bulletObj.transform.LookAt(bulletObj.transform.position + body.velocity.normalized);

        CheckAllCollision();
    }
    /// <summary>
    /// 计算发射速度和时间
    /// </summary>
    /// <returns></returns>
    private (Vector3 initVelocity, float timeToTarget) CalculateLaunchVelocity()
    {
        //Y的位移
        float displacementY = targetPos.y - body.position.y;
        //XZ的位移
        Vector3 displacementXZ = new Vector3(targetPos.x - body.position.x,
            0, targetPos.z - body.position.z);

        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;

        return (velocityY * -Mathf.Sign(gravity) + velocityXZ, time);
    }
    /// <summary>
    /// Debug画线
    /// </summary>
    void DrawPath()
    {
        var temp = CalculateLaunchVelocity();
        Vector3 previousDrawPoint = body.position;

        //画点个数
        int resolution = 30;
        for (int i = 1; i <= resolution; i++)
        {
            float simulationTime = i / (float)resolution * temp.timeToTarget;
            Vector3 displacement = temp.initVelocity * simulationTime + Vector3.up * gravity * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = body.position + displacement;
            Debug.DrawLine(previousDrawPoint, drawPoint, Color.green,2f);
            previousDrawPoint = drawPoint;
        }
    }
}