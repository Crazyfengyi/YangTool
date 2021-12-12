using System.Collections.Generic;
using UnityEngine;

namespace YangTools
{
    public partial class Extends
    {
        /// <summary>
        /// 开关鼠标指针
        /// </summary>
        public static void SetCursorLock(bool lockCursor)
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}