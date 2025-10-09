/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-01-22 
*/
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using UnityEngine;

public class TestV : MonoBehaviour
{

    public Transform point1;
    public Transform point2;

    private Rigidbody rigidbody;
    private Transform body;
    public void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        body = GetComponent<Transform>();
    }
    public void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 10;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        if (body&& rigidbody && rigidbody.linearVelocity != Vector3.zero)
        {
            //看向目标方向
            transform.rotation = Quaternion.LookRotation(rigidbody.linearVelocity);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = point1.position;
            //看向目标方向
            Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Vector3 targetXZPos = new Vector3(point2.position.x, 0.0f, point2.position.z);
            transform.LookAt(targetXZPos);

            //发射角度
            var LaunchAngle = 60;
            //重力加速度
            var G = Physics.gravity.y;
            //水平距离
            var R = Vector3.Distance(projectileXZPos, targetXZPos);
            //垂直距离
            var H = point2.position.y - transform.position.y;
            //tan(α)
            float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);

            //计算弹丸落在目标物体上所需的初始速度
            float Vz = Mathf.Sqrt(Mathf.Abs(G * R * R / (2.0f * (H - R * tanAlpha))));
            float Vy = tanAlpha * Vz;

            //在局部空间中创建速度矢量，并转换成全局空间
            Vector3 localVelocity = new Vector3(0f, Vy, Vz);
            Vector3 globalVelocity = transform.TransformDirection(localVelocity);

            //通过设置物体的初始速度和翻转物体的状态来发射物体
            rigidbody.linearVelocity = globalVelocity;
            transform.rotation = Quaternion.LookRotation(rigidbody.linearVelocity);
        }
    }
}