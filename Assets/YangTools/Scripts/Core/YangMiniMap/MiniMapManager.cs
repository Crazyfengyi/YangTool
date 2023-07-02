/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-26
*/

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YangTools.MiniMap
{
    public class MiniMapManager : MonoSingleton<MiniMapManager>
    {
        #region 基础设置

        [BoxGroup("基础设置")]
        [LabelText("主角色")]
        public GameObject mainTarget;

        [BoxGroup("基础设置")]
        [LabelText("名称")]
        public string levelName;

        [BoxGroup("基础设置")]
        [LabelText("小地图层")]
        public int miniMapLayer;

        [BoxGroup("基础设置")]
        [LabelText("相机")]
        public Camera managerCamera;

        [BoxGroup("基础设置")]
        [LabelText("按键代码切换地图大小模式(世界地图和迷你地图)")]
        public KeyCode toogleKey = KeyCode.E;

        [BoxGroup("基础设置")]
        [LabelText("渲染类型")]
        public RenderType renderType = RenderType.Picture;

        [BoxGroup("基础设置")]
        [LabelText("渲染模式")]
        public RenderMode renderMode = RenderMode.Mode2D;

        [BoxGroup("基础设置")]
        [LabelText("地图类型")]
        public MapType mapType = MapType.Target;

        [BoxGroup("基础设置")]
        [LabelText("是否正交视图")]
        public bool Ortographic2D = false;

        #endregion 基础设置

        #region 地图高度

        [BoxGroup("地图高度")]
        [LabelText("图标缩放")]
        [Range(0.05f, 2)]
        public float iconMultiplier = 1;

        [BoxGroup("地图高度")]
        [LabelText("鼠标灵敏度")]
        [Range(1, 10)]
        public int scrollSensitivity = 3;

        [BoxGroup("地图高度")]
        [LabelText("默认查看高度")]
        [Range(5, 500)]
        public float DefaultHeight = 20;

        [BoxGroup("地图高度")]
        [LabelText("摄像机最小高度")]
        public float minHeight = 5;

        [BoxGroup("地图高度")]
        [LabelText("摄像机最大高度")]
        public float maxHeight = 80;

        [BoxGroup("地图高度")]
        [LabelText("控制减小高度")]
        public KeyCode decreaseHeightKey = KeyCode.KeypadMinus;

        [BoxGroup("地图高度")]
        [LabelText("控制增大高度")]
        public KeyCode increaseHeightKey = KeyCode.KeypadPlus;

        [BoxGroup("地图高度")]
        [LabelText("高度变化的平滑速度")]
        [Range(1, 15)]
        public float lerpHeight = 8;

        #endregion 地图高度

        #region 地图旋转

        [BoxGroup("地图旋转")]
        [LabelText("使用指南针旋转")]
        public bool useCompassRotation = false;

        [BoxGroup("地图旋转")]
        [LabelText("圆旋转直径大小")]
        [Range(25, 500)]
        public float CompassSize = 175f;

        [BoxGroup("地图旋转")]
        [LabelText("旋转始终朝前")]
        public bool RotationAlwaysFront = true;

        [BoxGroup("地图旋转")]
        [LabelText("随着玩家旋转")]
        public bool DynamicRotation = true;

        [BoxGroup("地图旋转")]
        [LabelText("是否平滑")]
        public bool SmoothRotation = true;

        [BoxGroup("地图旋转")]
        [LabelText("平滑速度")]
        [Range(1, 15)]
        public float LerpRotation = 8;

        #endregion 地图旋转

        #region 附加网格

        [BoxGroup("附加网格")]
        [LabelText("显示附加网格")]
        [SerializeField]
        private bool showAreaGrid = true;

        [BoxGroup("附加网格")]
        [LabelText("附加网格大小")]
        [Range(1, 20)]
        public float areasSize = 4;

        [BoxGroup("附加网格")]
        [LabelText("附加网格材质")]
        [SerializeField]
        private Material areaMaterial;

        //[Separator("Animations")]
        //[CustomToggle("Show Level Name")] public bool ShowLevelName = true;
        //[CustomToggle("Show Panel Info")] public bool ShowPanelInfo = true;
        //[CustomToggle("Fade OnFull Screen")] public bool FadeOnFullScreen = false;
        //[Range(0.1f, 5)] public float HitEffectSpeed = 1.5f;
        //[SerializeField] private Animator BottonAnimator;
        //[SerializeField] private Animator PanelInfoAnimator;
        //[SerializeField] private Animator HitEffectAnimator;

        #endregion 区域网格

        #region 地图切换设置

        [BoxGroup("切换设置")]
        [LabelText("大地图移动到的位置")]
        public Vector3 FullMapPosition = Vector2.zero;
        [BoxGroup("切换设置")]
        [LabelText("大地图的旋转")]
        public Vector3 FullMapRotation = Vector3.zero;
        [BoxGroup("切换设置")]
        [LabelText("大地图大小")]
        public Vector2 FullMapSize = Vector2.zero;
        //小地图初始值
        private Vector3 MiniMapPosition = Vector2.zero;
        private Vector3 MiniMapRotation = Vector3.zero;
        private Vector2 MiniMapSize = Vector2.zero;
        [BoxGroup("切换设置")]
        [LabelText("平滑速度")]
        [Range(1, 15)]
        public float LerpTransition = 7;

        #endregion 地图区域

        #region 拖拽设置

        [BoxGroup("拖拽设置")]
        [LabelText("是否可以拖拽小地图")]
        public bool CanDragMiniMap = true;

        [BoxGroup("拖拽设置")]
        [LabelText("是否仅在大地图时可以拖拽")]
        public bool DragOnlyOnFullScreen = true;

        [BoxGroup("拖拽设置")]
        [LabelText("改变时重置位置")]
        public bool ResetOffSetOnChange = true;

        [BoxGroup("拖拽设置")]
        [LabelText("拖拽移动速度")]
        public Vector2 DragMovementSpeed = new Vector2(0.5f, 0.35f);

        [BoxGroup("拖拽设置")]
        [LabelText("最大偏移位置")]
        public Vector2 MaxOffSetPosition = new Vector2(1000, 1000);

        [BoxGroup("拖拽设置")]
        [LabelText("拖拽图标")]
        public Texture2D DragCursorIcon;

        [BoxGroup("拖拽设置")]
        [LabelText("聚光区")]
        public Vector2 HotSpot = Vector2.zero;

        #endregion 拖拽设置

        #region 图片模式设置

        [BoxGroup("图片模式设置")]
        [LabelText("纹理的小地图渲染器(截图)")]
        public Texture MapTexture = null;

        [BoxGroup("图片模式设置")]
        [LabelText("色调")]
        public Color TintColor = new Color(1, 1, 1, 0.9f);

        [BoxGroup("图片模式设置")]
        [LabelText("高光")]
        public Color SpecularColor = new Color(1, 1, 1, 0.9f);

        [BoxGroup("图片模式设置")]
        [LabelText("散射光")]
        public Color EmessiveColor = new Color(0, 0, 0, 0.9f);

        [BoxGroup("图片模式设置")]
        [LabelText("发射速率")]
        [Range(0.1f, 4)]
        public float EmissionAmount = 1;

        [BoxGroup("图片模式设置")]
        [LabelText("引用材质")]
        [SerializeField]
        private Material ReferenceMat;

        [BoxGroup("图片模式设置")]
        [LabelText("非实时地图渲染平面")]
        public GameObject mapPlanePrefab;

        [BoxGroup("图片模式设置")]
        [LabelText("附加网格预制体")]
        [SerializeField]
        private GameObject areaPrefab;

        [BoxGroup("图片模式设置")]
        [LabelText("世界空间参考")]
        public RectTransform WorldSpace;

        #endregion 图片模式设置

        #region UI设置

        [BoxGroup("UI设置")]
        [LabelText("UICanvas")]
        public Canvas m_Canvas;

        [BoxGroup("UI设置")]
        [LabelText("UI小地图父节点")]
        public RectTransform MMUIRoot;

        [BoxGroup("UI设置")]
        [LabelText("玩家Icon")]
        public Image PlayerIcon;

        [BoxGroup("UI设置")]
        [LabelText("CanvasGroup")]
        [SerializeField]
        private CanvasGroup rootCanvasGroup;

        [BoxGroup("UI设置")]
        [LabelText("Item简单预制体")]
        [SerializeField]
        private GameObject ItemPrefabSimple;

        [BoxGroup("UI设置")]
        [LabelText("N标记")]
        [SerializeField]
        private GameObject NMarkPrefab;

        [BoxGroup("UI设置")]
        [LabelText("Item预制体")]
        [SerializeField]
        private GameObject ItemPrefab;

        [BoxGroup("UI设置")]
        [LabelText("Item预制体列表")]
        public Dictionary<string, Transform> ItemsList = new Dictionary<string, Transform>();

        #endregion UI

        #region 属性

        private Transform t;
        private Transform m_Transform
        {
            get
            {
                if (t == null)
                {
                    t = this.GetComponent<Transform>();
                }
                return t;
            }
        }
        public Transform Target
        {
            get
            {
                if (mainTarget != null)
                {
                    return mainTarget.GetComponent<Transform>();
                }
                return this.GetComponent<Transform>();
            }
        }
        public Vector3 TargetPosition
        {
            get
            {
                Vector3 temp = Vector3.zero;
                if (mainTarget != null)
                {
                    temp = mainTarget.transform.position;
                }
                return temp;
            }
        }

        #endregion 属性

        #region 私有变量

        //全局变量
        public static bool isFullScreen;
        //小地图相机
        private static Camera miniMapCamera;
        public static Camera MiniMapCamera => miniMapCamera;
        //UIRoot
        private static RectTransform mapUIRoot;
        public static RectTransform MapUIRoot => mapUIRoot;

        //拖拽偏移
        private Vector3 DragOffset = Vector3.zero;
        //默认旋转模式
        private bool defaultRotationMode;
        private Vector3 deafultMapRot = Vector3.zero;
        private bool defaultRotationCircle;
        //非实时地图渲染平面
        private GameObject plane;
        private GameObject areaInstance;
        private float defaultYCameraPosition;
        private const string MMHeightKey = "MinimapCameraHeight";
        private bool getDelayPositionCamera;
        private bool isAlphaComplete;

        #endregion 私有变量

        protected override void Awake()
        {
            base.Awake();
            //获得小地图大小
            GetMiniMapSize();
            //默认值
            miniMapCamera = managerCamera;
            mapUIRoot = MMUIRoot;
            defaultRotationMode = DynamicRotation;
            deafultMapRot = m_Transform.eulerAngles;
            defaultRotationCircle = useCompassRotation;
            //设置N图标
            SetNMark();
            //用贴图纹理创建一个平面小地图相机将只渲染这个平面。这是更优化的RealTime类型。
            CreateMapPlane(renderType == RenderType.RealTime);
            //避免UI世界空间与场景中的其他物体碰撞
            if (renderMode == RenderMode.Mode3D) ConfigureCamera3D();
        }
        private void Start()
        {
            if (mapType == MapType.Target)
            {
                //获取存档高度
                DefaultHeight = PlayerPrefs.GetFloat(MMHeightKey, DefaultHeight);
            }
            else
            {
                CreateWorldTarget();
                PlayerIcon.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!isAlphaComplete)
            {
                if (rootCanvasGroup != null)
                {
                    StartCoroutine(StartFade(0));
                }
            }
        }

        private void Update()
        {
            if (mainTarget == null) return;
            if (miniMapCamera == null) return;

            // 小地图的输入
            Inputs();
            // 控制小地图跟随目标
            PositionControll();
            // 旋转控制
            RotationControll();
            // 小地图和世界地图控制
            MapSize();
        }

        #region 内部方法
        /// <summary>
        /// 设置主角
        /// </summary>
        public void SetMainPlayer(GameObject target)
        {
            mainTarget = target;
            managerCamera.transform.position = target.transform.position + new Vector3(0, 20, 0);
        }

        /// <summary>
        /// 渐显
        /// </summary>
        private IEnumerator StartFade(float delay)
        {
            rootCanvasGroup.alpha = 0;
            yield return new WaitForSeconds(delay);
            while (rootCanvasGroup.alpha < 1)
            {
                rootCanvasGroup.alpha += Time.deltaTime;
                yield return null;
            }
            isAlphaComplete = true;
        }

        /// <summary>
        /// 获得小地图大小
        /// </summary>
        private void GetMiniMapSize()
        {
            MiniMapSize = MMUIRoot.sizeDelta;
            MiniMapPosition = MMUIRoot.anchoredPosition;
            MiniMapRotation = MMUIRoot.eulerAngles;
        }

        /// <summary>
        /// 设置N图标
        /// </summary>
        public void SetNMark()
        {
            if (NMarkPrefab == null || mapType == MapType.World) return;
            GameObject newItem = Instantiate(NMarkPrefab, new Vector3(0, 0, 100), Quaternion.identity) as GameObject;
            MiniMapItem mmItem = newItem.GetComponent<MiniMapItem>();
            mmItem.Target = newItem.transform;
        }

        /// <summary>
        /// 创建地图平面
        /// </summary>
        private void CreateMapPlane(bool area)
        {
            //检查
            if (miniMapLayer == 0)
            {
                Debug.LogError("小地图层是空的,请在检查器中指定它.");
                MMUIRoot.gameObject.SetActive(false);
                this.enabled = false;
                return;
            }
            if (MapTexture == null)
            {
                Debug.LogError("贴图纹理没有被分配");
                return;
            }

            //获取位置
            Vector3 pos = WorldSpace.localPosition;
            //获取大小
            Vector3 size = WorldSpace.sizeDelta;
            //设置为相机显示小地图层
            if (!area)
            {
                miniMapCamera.cullingMask = 1 << miniMapLayer;
                //创建平面--非实时地图渲染平面
                plane = Instantiate(mapPlanePrefab) as GameObject;
                //设置位置
                plane.transform.localPosition = pos;
                //设置修正大小
                plane.transform.localScale = (new Vector3(size.x, 10, size.y) / 10);
                //应用贴图纹理材质
                plane.GetComponent<Renderer>().material = CreateMaterial();
                //设置地图层
                plane.layer = miniMapLayer;
                plane.SetActive(false);
                plane.SetActive(true);
                //显示网格区域
                if (!showAreaGrid) { plane.transform.GetChild(0).gameObject.SetActive(false); }

                Invoke("DelayPositionInvoke", 2);
            }
            else if (areaPrefab != null && showAreaGrid)
            {
                //显示附加网格(覆盖在地图上)
                areaInstance = Instantiate(areaPrefab) as GameObject;
                //设置位置
                areaInstance.transform.localPosition = pos;
                //设置修正大小
                areaInstance.transform.localScale = (new Vector3(size.x, 10, size.y) / 10);
                //设置地图层
                areaInstance.layer = miniMapLayer;
            }
        }

        /// <summary>
        /// 创建材质
        /// </summary>
        public Material CreateMaterial()
        {
            Material mat = new Material(ReferenceMat);

            mat.mainTexture = MapTexture;
            mat.SetTexture("_EmissionMap", MapTexture);
            mat.SetFloat("_EmissionScaleUI", EmissionAmount);
            mat.SetColor("_EmissionColor", EmessiveColor);
            mat.SetColor("_SpecColor", SpecularColor);
            mat.EnableKeyword("_EMISSION");

            return mat;
        }

        /// <summary>
        /// 延迟位置调用
        /// </summary>
        private void DelayPositionInvoke()
        {
            defaultYCameraPosition = MiniMapCamera.transform.position.y; getDelayPositionCamera = true;
        }

        /// <summary>
        /// 避免UI世界空间与场景中的其他物体碰撞
        /// </summary>
        public void ConfigureCamera3D()
        {
            Camera cam = (Camera.main != null) ? Camera.main : Camera.current;
            if (cam == null)
            {
                Debug.LogWarning("没有找到一个摄像头配置,请分配这个.");
                return;
            }
            m_Canvas.worldCamera = cam;
            //避免向3D UI转移场景中的其他物体。
            cam.nearClipPlane = 0.015f;
            m_Canvas.planeDistance = 0.1f;
        }

        /// <summary>
        /// 世界模式设置主角的MiniMapItem
        /// </summary>
        public void CreateWorldTarget()
        {
            if (mainTarget == null) return;

            MiniMapItem miniMapItem = mainTarget.AddComponent<MiniMapItem>();
            miniMapItem.uiPrefab = ItemPrefab;
            miniMapItem.icon = PlayerIcon.sprite;
            miniMapItem.iconColor = PlayerIcon.color;
            miniMapItem.Target = mainTarget.transform;
        }

        private void Inputs()
        {
            //切换小地图和大地图模式
            if (Input.GetKeyDown(toogleKey))
            {
                ToggleSize();
            }
            if (Input.GetKeyDown(decreaseHeightKey) && DefaultHeight < maxHeight)
            {
                ChangeHeight(true);
            }
            if (Input.GetKeyDown(increaseHeightKey) && DefaultHeight > minHeight)
            {
                ChangeHeight(false);
            }
        }

        /// <summary>
        /// 位置控制
        /// </summary>
        private void PositionControll()
        {
            if (mapType == MapType.Target)
            {
                Vector3 tempPos = m_Transform.position;
                //根据主角位置更新相机变换。
                tempPos.x = Target.position.x;
                if (!Ortographic2D)
                {
                    tempPos.z = Target.position.z;
                }
                else
                {
                    tempPos.y = Target.position.y;
                }
                tempPos += DragOffset;

                //计算玩家位置
                if (Target != null)
                {
                    Vector3 pp = MiniMapCamera.WorldToViewportPoint(TargetPosition);
                    PlayerIcon.rectTransform.anchoredPosition = MiniMapUtils.CalculateMiniMapPosition(pp, MapUIRoot);
                }

                //为此，我们添加预定义的(但变量，见下文)height var。
                if (!Ortographic2D)
                {
                    tempPos.y = (maxHeight + minHeight / 2) + (Target.position.y * 2);
                }
                else
                {
                    tempPos.z = ((Target.position.z) * 2) - (maxHeight + minHeight / 2);
                }
                //相机跟随目标
                m_Transform.position = Vector3.Lerp(m_Transform.position, tempPos, Time.deltaTime * 10);
            }

            if (plane != null && getDelayPositionCamera)
            {
                Vector3 v = plane.transform.position;
                //从世界空间矩形中获取位置
                Vector3 pos = WorldSpace.position;
                float ydif = defaultYCameraPosition - MiniMapCamera.transform.position.y;
                v.y = pos.y - ydif;
                plane.transform.position = v;
            }
        }

        /// <summary>
        /// 旋转控制
        /// </summary>
        private void RotationControll()
        {
            RectTransform playerRectTrans = PlayerIcon.GetComponent<RectTransform>();
            //不是世界模式,并且动态旋转
            if (DynamicRotation && mapType != MapType.World)
            {
                //获得局部引用
                Vector3 eulerAngles = m_Transform.eulerAngles;
                eulerAngles.y = Target.eulerAngles.y;
                if (SmoothRotation)
                {
                    if (renderMode == RenderMode.Mode2D)
                    {
                        //2D模式
                        playerRectTrans.eulerAngles = Vector3.zero;
                    }
                    else
                    {
                        //3D模式
                        playerRectTrans.localEulerAngles = Vector3.zero;
                    }

                    if (m_Transform.eulerAngles.y != eulerAngles.y)
                    {
                        //计算不同
                        float d = eulerAngles.y - m_Transform.eulerAngles.y;
                        //避免lerp从360到0或反向
                        if (d > 180 || d < -180)
                        {
                            m_Transform.eulerAngles = eulerAngles;
                        }
                    }
                    //差值旋转
                    m_Transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles, eulerAngles, Time.deltaTime * LerpRotation);
                }
                else
                {
                    m_Transform.eulerAngles = eulerAngles;
                }
            }
            else
            {
                m_Transform.eulerAngles = deafultMapRot;
                if (renderMode == RenderMode.Mode2D)
                {
                    //当地图旋转是静态的，只旋转玩家图标
                    Vector3 e = Vector3.zero;
                    //获得并固定目标的正确角度旋转
                    e.z = -Target.eulerAngles.y;
                    playerRectTrans.eulerAngles = e;
                }
                else
                {
                    //在3D模式中使用局部旋转
                    Vector3 tr = Target.localEulerAngles;
                    Vector3 r = Vector3.zero;
                    r.z = -tr.y;
                    playerRectTrans.localEulerAngles = r;
                }
            }
        }

        /// <summary>
        /// 地图大小控制(大小地图切换缓动)
        /// </summary>
        private void MapSize()
        {
            RectTransform rt = MMUIRoot;
            if (isFullScreen)
            {
                if (DynamicRotation)
                {
                    DynamicRotation = false;
                    ResetMapRotation();
                }
                rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, FullMapSize, Time.deltaTime * LerpTransition);
                rt.anchoredPosition = Vector3.Lerp(rt.anchoredPosition, FullMapPosition, Time.deltaTime * LerpTransition);
                rt.localEulerAngles = Vector3.Lerp(rt.localEulerAngles, FullMapRotation, Time.deltaTime * LerpTransition);
            }
            else
            {
                if (DynamicRotation != defaultRotationMode) { DynamicRotation = defaultRotationMode; }
                rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, MiniMapSize, Time.deltaTime * LerpTransition);
                rt.anchoredPosition = Vector3.Lerp(rt.anchoredPosition, MiniMapPosition, Time.deltaTime * LerpTransition);
                rt.localEulerAngles = Vector3.Lerp(rt.localEulerAngles, MiniMapRotation, Time.deltaTime * LerpTransition);
            }
            MiniMapCamera.orthographicSize = Mathf.Lerp(MiniMapCamera.orthographicSize, DefaultHeight, Time.deltaTime * lerpHeight);
        }

        /// <summary>
        /// 重设地图旋转
        /// </summary>
        private void ResetMapRotation()
        {
            m_Transform.eulerAngles = new Vector3(90, 0, 0);
        }

        /// <summary>
        /// 控制大小--切换全地图和小地图
        /// </summary>
        private void ToggleSize()
        {
            isFullScreen = !isFullScreen;
            //全屏
            if (isFullScreen)
            {
                if (mapType != MapType.World)
                {
                    //当切换到全屏时，高度为Max
                    DefaultHeight = maxHeight;
                }
                useCompassRotation = false;
                MiniMapMaskHelper.Instance.OnChange(true);
            }
            else
            {
                if (mapType != MapType.World)
                {
                    //全屏返回时，返回当前高度
                    DefaultHeight = PlayerPrefs.GetFloat(MMHeightKey, DefaultHeight);
                }
                if (useCompassRotation != defaultRotationCircle)
                {
                    useCompassRotation = defaultRotationCircle;
                }
                MiniMapMaskHelper.Instance.OnChange(false);
            }
            //重设偏移位置
            if (ResetOffSetOnChange)
            {
                GoToTarget();
            }
        }

        public void GoToTarget()
        {
            StopCoroutine("ResetOffset");
            StartCoroutine("ResetOffset");
        }

        private IEnumerator ResetOffset()
        {
            while (Vector3.Distance(DragOffset, Vector3.zero) > 0.2f)
            {
                DragOffset = Vector3.Lerp(DragOffset, Vector3.zero, Time.deltaTime * LerpTransition);
                yield return null;
            }
            DragOffset = Vector3.zero;
        }

        /// <summary>
        /// 切换高度
        /// </summary>
        public void ChangeHeight(bool b)
        {
            if (mapType == MapType.World) return;

            if (b)
            {
                if (DefaultHeight + scrollSensitivity <= maxHeight)
                {
                    DefaultHeight += scrollSensitivity;
                }
                else
                {
                    DefaultHeight = maxHeight;
                }
            }
            else
            {
                if (DefaultHeight - scrollSensitivity >= minHeight)
                {
                    DefaultHeight -= scrollSensitivity;
                }
                else
                {
                    DefaultHeight = minHeight;
                }
            }
            PlayerPrefs.SetFloat(MMHeightKey, DefaultHeight);
        }

        #endregion 内部方法

        #region 对外方法
        /// <summary>
        /// 设置小地图显隐
        /// </summary>
        public void SetMiniMapShow(bool isShow)
        {
            m_Canvas.gameObject.SetActive(isShow);
        }

        /// <summary>
        /// 设置显隐网格
        /// </summary>
        public void SetActiveGrid(bool active)
        {
            if (renderType == RenderType.Picture && plane != null)
            {
                plane.transform.GetChild(0).gameObject.SetActive(active);
            }
            else if (areaInstance != null)
            {
                areaInstance.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 设置网格大小
        /// </summary>
        public void SetGridSize(float value)
        {
            if (areaMaterial != null)
            {
                Vector2 r = areaMaterial.GetTextureScale("_MainTex");
                r.x = value;
                r.y = value;
                areaMaterial.SetTextureScale("_MainTex", r);
            }
        }

        /// <summary>
        /// 设置Icon大小缩放
        /// </summary>
        public void SetIconMulti(float v)
        {
            iconMultiplier = v;
        }

        /// <summary>
        /// 设置是否动态旋转
        /// </summary>
        public void SetMapRotation(bool dynamic)
        {
            DynamicRotation = dynamic;
            defaultRotationMode = dynamic;
            m_Transform.eulerAngles = new Vector3(0, 0, 0);
        }

        #endregion 对外方法
    }
}