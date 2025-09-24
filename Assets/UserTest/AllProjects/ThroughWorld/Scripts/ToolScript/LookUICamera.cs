/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-16 
*/
using UnityEngine;

public class LookUICamera : MonoBehaviour
{
    public void LateUpdate()
    {
        if (CameraManager.Instance.PlayerCamera != null)
        {
            //竖
            var v = GameUIManager.Instance.UICamera.transform.parent.eulerAngles.x;
            //横
            var h = GameUIManager.Instance.UICamera.transform.parent.parent.eulerAngles.y;
            transform.localRotation = Quaternion.Euler(new Vector3(v, h, 0));
        }
    }
}