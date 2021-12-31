using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using YangToolDebuger;

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
        /// <summary>
        /// 自定义设置显隐
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="isShow">显隐</param>
        /// <param name="callName">调用者</param>
        public static void CustomSetActive(GameObject node, bool isShow, [CallerMemberNameAttribute] string callName = "")
        {
            if (node == null)
            {
                Debuger.ToError($"尝试对null对象设置显隐:调用来源{callName}");
                return;
            }

            node.AutoSetActive(isShow);
        }
        /// <summary>
        /// 获得某物体的bounds
        /// </summary>
        public static Bounds GetBounds(GameObject obj)
        {
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Renderer[] renders = obj.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renders.Length; i++)
            {
                if (renders[i].bounds.min.x < Min.x)
                    Min.x = renders[i].bounds.min.x;
                if (renders[i].bounds.min.y < Min.y)
                    Min.y = renders[i].bounds.min.y;
                if (renders[i].bounds.min.z < Min.z)
                    Min.z = renders[i].bounds.min.z;

                if (renders[i].bounds.max.x > Max.x)
                    Max.x = renders[i].bounds.max.x;
                if (renders[i].bounds.max.y > Max.y)
                    Max.y = renders[i].bounds.max.y;
                if (renders[i].bounds.max.z > Max.z)
                    Max.z = renders[i].bounds.max.z;
            }
            Vector3 center = (Min + Max) / 2;
            Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }
        /// <summary>
        /// UGUI通用动画
        /// </summary>
        /// <param name="rectTrans">动画对象</param>
        /// <param name="callback">回调</param>
        public static void CommonAni(RectTransform rectTrans, Action<RectTransform> callback = null)
        {
            DOTween.Kill(rectTrans, true);

            Vector2 oldPivot = rectTrans.pivot;
            Vector2 oldScale = rectTrans.localScale;
            rectTrans.pivot = new Vector2(0.5f, 0.5f);
            rectTrans.localScale = oldScale * 0.36f;

            rectTrans.DOScale(oldScale, 0.16f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    rectTrans.pivot = oldPivot;
                    rectTrans.localScale = oldScale;
                    callback?.Invoke(rectTrans);
                })
                .SetTarget(rectTrans);
        }

    }
}