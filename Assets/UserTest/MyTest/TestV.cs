/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-01-22 
*/
using UnityEngine;
using System.Collections;

public class TestV : MonoBehaviour
{
    public Rigidbody rigidbody;
    public bool isJumping = false;

    public Transform point1;
    public Transform point2;
    public float timeLog;

    public float distance;//距离
    public float time;//时间
    public float hight;//高度

    public Vector3 direction;//方向
    public Vector3 v;
    public Vector3 a;

    public void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    public void FixedUpdate()
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            transform.position = point1.position;
            distance = Vector3.Distance(point1.position, point2.position);
            direction = (point2.position - point1.position).normalized;

            var g = (2 * hight) / ((time / 2) * (time / 2));
            //加速度
            var vaValue = (2 * distance) / (time * time);
            Vector3 va = 0 * -direction;

            Vector3 tempVax = Vector3.Project(va, Vector3.right);
            Vector3 tempVaz = Vector3.Project(va, Vector3.forward);
            a = new Vector3(0, -g, 0) + tempVax + tempVaz;
            //初速度
            float v0Value = distance / time;
            Vector3 v0 = v0Value * direction;
            //float v0Value = 2 * distance / time;
            //Vector3 v0 = 0 * direction;

            Vector3 tempx = Vector3.Project(v0, Vector3.right);
            Vector3 tempz = Vector3.Project(v0, Vector3.forward);
            Vector3 tempy = new Vector3(0, g * (time / 2), 0);
            v = tempy + tempx + tempz;
            rigidbody.velocity = v;
            timeLog = 0;
        }
        if (isJumping)
        {
            Jumping();
        }
    }
    public void Jumping()
    {
        timeLog += Time.fixedDeltaTime;
        v = v + a * Time.fixedDeltaTime;
        rigidbody.velocity = v;

        if (timeLog >= time)
        {
            rigidbody.velocity = Vector3.zero;
            isJumping = false;
            Debug.LogError($"用时:{timeLog}");
        }
    }
}