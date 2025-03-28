/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-27
*/

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace YangTools.Scripts.Core.YangMiniMap
{
    /// <summary>
    /// 小地图Item(挂载在角色身上)
    /// </summary>
    public class MiniMapItem : MonoBehaviour
    {
        #region 目标
        [LabelText("UIIcon预制体")]
        public GameObject uiPrefab;
        [LabelText("跟随目标")]
        public Transform Target;
        [LabelText("相对目标的偏移")]
        public Vector3 OffSet = Vector3.zero;
        #endregion 目标

        #region 图标

        [LabelText("图标")]
        public Sprite icon;
        [LabelText("死亡图标")]
        public Sprite DeathIcon;
        [LabelText("图标颜色")]
        public Color iconColor = new Color(1, 1, 1, 0.9f);
        [LabelText("图标大小")]
        [Range(1, 100)]
        public float Size = 20;

        [LabelText("显示圆形范围")]
        public bool ShowCircleArea = false;
        [LabelText("圆形范围半径")]
        [Range(1, 100)]
        public float CircleAreaRadius = 10;
        [LabelText("圆形范围颜色")]
        public Color CircleAreaColor = new Color(1, 1, 1, 0.9f);
        #endregion 图标

        [LabelText("是否可以交互")]
        public bool isInteractable = true;
        [FormerlySerializedAs("InfoItem")]
        [LabelText("显示信息")]
        [TextArea(2, 2)]
        public string infoItem = "Info Icon here";
        [FormerlySerializedAs("OffScreen")] [LabelText("图标限制在屏幕内")]
        public bool offScreen = true;
        [FormerlySerializedAs("DestroyWithObject")] [LabelText("对象销毁删除图标")]
        public bool destroyWithObject = false;
        [FormerlySerializedAs("BorderOffScreen")]
        [LabelText("屏幕边框")]
        [Range(0, 5)]
        public float borderOffScreen = 0.01f;

        [FormerlySerializedAs("OffScreenSize")]
        [LabelText("屏幕外大小")]
        [Range(1, 50)]
        public float offScreenSize = 10;
        [LabelText("是N图标")]
        public bool isNMark = false;
        [FormerlySerializedAs("RenderDelay")]
        [LabelText("图标延时显示时间")]
        [Range(0, 3)]
        public float renderDelay = 0.3f;

        /// <summary>
        /// uiIcon出现效果
        /// </summary>
        [FormerlySerializedAs("m_Effect")] 
        public UIIconShowEffect mEffect = UIIconShowEffect.None;

        //UI脚本
        private GameObject uiIconObject;
        private MiniMapIcon miniMapIcon;

        private RectTransform mapUIRoot;
        private RectTransform circleAreaRect;
        private Vector3 position;
        private MiniMapManager miniMap;

        public Vector3 TargetPosition
        {
            get
            {
                if (Target == null) return Vector3.zero;
                return new Vector3(Target.position.x, 0, Target.position.z);
            }
        }

        private void Start()
        {
            miniMap = MiniMapManager.Instance;

            if (MiniMapManager.MapUIRoot != null)
            {
                CreateIcon();
            }
            else
            {
                Debug.LogError("没有找到miniMap管理器");
            }
        }

        /// <summary>
        /// 创建UGUIIcon
        /// </summary>
        private void CreateIcon()
        {
            mapUIRoot = MiniMapManager.MapUIRoot;
            //实例化UI在Canvas上
            uiIconObject = Instantiate(uiPrefab, mapUIRoot.transform, false);
            miniMapIcon = uiIconObject.GetComponent<MiniMapIcon>();

            if (icon != null)
            {
                miniMapIcon.SetIcon(icon, iconColor);
            }

            uiIconObject.GetComponent<CanvasGroup>().interactable = isInteractable;
            miniMapIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (Target == null) { Target = this.GetComponent<Transform>(); }
            UIIconStartShowEffect();

            miniMapIcon.DelayStartShow(renderDelay);
            miniMapIcon.SetTextShow(infoItem);
            if (ShowCircleArea)
            {
                circleAreaRect = miniMapIcon.SetCircleArea(CircleAreaRadius, CircleAreaColor);
            }
        }

        /// <summary>
        /// 初始显示效果
        /// </summary>
        private void UIIconStartShowEffect()
        {
            switch (mEffect)
            {
                case UIIconShowEffect.Fade:
                    //TODO:动画
                    break;
                case UIIconShowEffect.None:
                default:
                    break;
            }
        }

        private void Update()
        {
            if (Target == null) return;
            if (miniMapIcon == null) return;

            if (isNMark)
            {
                if (miniMap.Target != null)
                {
                    transform.position = miniMap.Target.TransformPoint((miniMap.Target.forward) * 100);
                }
            }
            //获得UIIocn的Rect
            RectTransform iconRectTran = miniMapIcon.GetComponent<RectTransform>();
            //添加偏移的世界坐标
            Vector3 correctWorldPosition = TargetPosition + OffSet;
            //目标位置->视口坐标
            Vector2 vp2 = MiniMapManager.MiniMapCamera.WorldToViewportPoint(correctWorldPosition);
            //视口坐标->屏幕坐标/局部坐标(相对于根节点)
            position = new Vector2((vp2.x * mapUIRoot.sizeDelta.x) - (mapUIRoot.sizeDelta.x * 0.5f),
                (vp2.y * mapUIRoot.sizeDelta.y) - (mapUIRoot.sizeDelta.y * 0.5f));
            //未限制的坐标
            Vector2 unClampPosition = position;
            //如果显示在屏幕外
            if (offScreen)
            {
                //计算移动UI的最大和最小距离
                //这个限制在MapUIRoot大小边界内
                position.x = Mathf.Clamp(position.x, -((mapUIRoot.sizeDelta.x * 0.5f) - borderOffScreen), ((mapUIRoot.sizeDelta.x * 0.5f) - borderOffScreen));
                position.y = Mathf.Clamp(position.y, -((mapUIRoot.sizeDelta.y * 0.5f) - borderOffScreen), ((mapUIRoot.sizeDelta.y * 0.5f) - borderOffScreen));
            }

            //重新计算UI的位置，确定是否脱离屏幕如果脱离屏幕缩小大小
            float size = Size;
            //圆形地图
            if (MiniMapManager.Instance.useCompassRotation)
            {
                //罗盘式旋转
                Vector3 screenPos = Vector3.zero;
                //计算朝向
                Vector3 forward = Target.position - MiniMapManager.Instance.TargetPosition;
                //目标在摄像机中的位置
                Vector3 cameraRelativeDir = MiniMapManager.MiniMapCamera.transform.InverseTransformDirection(forward);
                //修复在屏蔽上的法向量
                cameraRelativeDir.z = 0;
                cameraRelativeDir = cameraRelativeDir.normalized / 2;
                //将屏幕上和屏幕外的计算区域的值转换为正。
                float posPositiveX = Mathf.Abs(position.x);
                float relativePositiveX = Mathf.Abs((0.5f + (cameraRelativeDir.x * MiniMapManager.Instance.CompassSize)));
                //当目标离屏时，限制位置在圆区域。
                if (posPositiveX >= relativePositiveX)
                {
                    screenPos.x = 0.5f + (cameraRelativeDir.x * MiniMapManager.Instance.CompassSize)/*/ Camera.main.aspect*/;
                    screenPos.y = 0.5f + (cameraRelativeDir.y * MiniMapManager.Instance.CompassSize);
                    position = screenPos;
                    size = offScreenSize;
                }
                else
                {
                    size = Size;
                }
            }
            else
            {
                if (Mathf.Approximately(position.x, (mapUIRoot.sizeDelta.x * 0.5f) - borderOffScreen) || Mathf.Approximately(position.y, (mapUIRoot.sizeDelta.y * 0.5f) - borderOffScreen) ||
                    Mathf.Approximately(position.x, -(mapUIRoot.sizeDelta.x * 0.5f) - borderOffScreen) || Mathf.Approximately(-position.y, (mapUIRoot.sizeDelta.y * 0.5f) - borderOffScreen))
                {
                    size = offScreenSize;
                }
                else
                {
                    size = Size;
                }
            }
            //应用位置到UIIcon
            iconRectTran.anchoredPosition = position;
            if (circleAreaRect != null) { circleAreaRect.anchoredPosition = unClampPosition; }
            //平滑过渡改变大小
            float correctSize = size * miniMap.iconMultiplier;
            iconRectTran.sizeDelta = Vector2.Lerp(iconRectTran.sizeDelta, new Vector2(correctSize, correctSize), Time.deltaTime * 8);

            if (miniMap.RotationAlwaysFront)
            {
                //这样图标的旋转就会保持不变(对于正面)
                Quaternion r = Quaternion.identity;
                r.x = Target.rotation.x;
                iconRectTran.localRotation = r;
            }
            else
            {
                //这样旋转图标将取决于目标
                Vector3 vre = miniMap.transform.eulerAngles;
                Vector3 re = Vector3.zero;
                //Fix player rotation for apply to el icon.
                //修复玩家旋转为了保存到icon
                re.z = ((-this.Target.rotation.eulerAngles.y) + vre.y);
                Quaternion q = Quaternion.Euler(re);
                iconRectTran.rotation = q;
            }
        }

        /// <summary>
        /// 删除图标
        /// </summary>
        /// <param name="immediate">理解删除</param>
        public void DestroyIcon(bool immediate)
        {
            if (miniMapIcon == null)
            {
                Debug.LogError($"图标删除失败{name}");
                return;
            }

            if (DeathIcon == null || immediate)
            {
                miniMapIcon.DestroyIcon(immediate);
            }
            else
            {
                miniMapIcon.DestroyIcon(false, DeathIcon);
            }
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(Sprite argIcon)
        {
            if (miniMapIcon == null)
            {
                Debug.LogWarning("设置图标失败");
                return;
            }
            miniMapIcon.SetIcon(argIcon);
        }

        /// <summary>
        /// 设置攻击范围
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="areaColor">颜色</param>
        public void SetCircleArea(float radius, Color areaColor)
        {
            circleAreaRect = miniMapIcon.SetCircleArea(radius, areaColor);
        }

        /// <summary>
        /// 隐藏攻击范围
        /// </summary>
        public void HideCircleArea()
        {
            miniMapIcon.HideCircleArea();
            //CircleAreaRect = null;
        }

        /// <summary>
        /// 显示图标
        /// </summary>
        public void ShowItem()
        {
            if (uiIconObject != null)
            {
                uiIconObject.SetActive(true);
                miniMapIcon.SetVisibleAlpha();
            }
            else
            {
                Debug.LogError("显示图标失败");
            }
        }

        /// <summary>
        /// 隐藏图标
        /// </summary>
        public void HideIcon()
        {
            if (uiIconObject != null)
            {
                uiIconObject.SetActive(false);
            }
            else
            {
                Debug.LogError("隐藏失败");
            }
        }

        private void OnDestroy()
        {
            if (destroyWithObject)
            {
                DestroyIcon(true);
            }
        }
    }
}