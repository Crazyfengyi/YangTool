/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-16 
*/
using UnityEngine;
using System.Collections;

public class LookCamera : MonoBehaviour
{
    public void LateUpdate()
    {
        if (CameraManager.Instance.mainCamera != null)
        {
            //竖
            var v = CameraManager.Instance.mainCamera.transform.parent.eulerAngles.x;
            //横
            var h = CameraManager.Instance.mainCamera.transform.parent.parent.eulerAngles.y;

            transform.localRotation = Quaternion.Euler(new Vector3(v, h, 0));
        }
    }
}