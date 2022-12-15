/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-13 
*/
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    #region 变量
    public Transform target;
    private Vector3 offset;
    #endregion

    #region 生命周期
    private void Awake()
    {
        offset = target.position - transform.position;
    }
    private void LateUpdate()
    {
        transform.position = target.position - offset;
    }
    #endregion
}