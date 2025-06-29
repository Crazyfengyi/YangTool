/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-07-24
*/

using Cinemachine;
using UnityEngine;
using YangTools;
using Sirenix.OdinInspector;
using YangTools.Scripts.Core;

/// <summary>
/// 相机管理器
/// </summary>
public class CameraManager : MonoSingleton<CameraManager>
{
    #region 相机引用
    /// <summary>
    /// 预制体
    /// </summary>
    public GameObject cameraPrefab;
    [SerializeField]
    [ShowInInspector]
    //相机节点控制引用
    private GameObject cameraRoot;
    //左右
    protected Transform cameraLeftRightTransform;
    public Transform CameraLeftRightTransform => cameraLeftRightTransform;
    //上下
    protected Transform cameraUpDownTransform;
    private Camera mainCamera;
    /// <summary>
    /// 玩家相机
    /// </summary>
    public Camera PlayerCamera
    {
        get
        {
            if (mainCamera == null)
            {
                GameObject obj = Instantiate(cameraPrefab);
                cameraRoot = obj;
                Init();
            }
            return mainCamera;
        }
    }
    /// <summary>
    /// 虚拟相机
    /// </summary>
    private CinemachineVirtualCameraBase mainCM;
    public CinemachineVirtualCameraBase MainCM
    {
        get
        {
            if (mainCM == null)
            {
                _ = PlayerCamera;
            }
            return mainCM;
        }
    }
    #endregion

    #region 相机参数
    [LabelText("旋转速度")]
    public float cameraSpeed = 250f;
    [LabelText("旋转平滑")]
    public bool smoothCameraRotation = false;
    [LabelText("插值平滑速度")]
    [Range(1f, 50f)]
    public float cameraSmoothingFactor = 25f;
    [LabelText("垂直旋转的上限角度")]
    [Range(0f, 90f)]
    public float upperVerticalLimit = 60f;
    [LabelText("垂直旋转的下限角度")]
    [Range(0f, 90f)]
    public float lowerVerticalLimit = 60f;

    //当前旋转值(以度为单位);
    private float currentXAngle = 0f;
    private float currentYAngle = 0f;
    //插值存储的旧旋转值
    private float oldHorizontalInput = 0f;
    private float oldVerticalInput = 0f;
    //前方
    private Vector3 facingDirection;
    //右方
    private Vector3 rightDirection;
    //上方
    private Vector3 upwardsDirection;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if (cameraRoot) Init();

        _ = PlayerCamera;
        //设置角度变量为当前变换的旋转角度
        currentXAngle = cameraLeftRightTransform.localRotation.eulerAngles.x;
        currentYAngle = cameraLeftRightTransform.localRotation.eulerAngles.y;
        //执行一次相机旋转代码来计算面向和向上的方向
        RotateCamera(0f, 0f);
    }
    private void LateUpdate()
    {
        HandleCameraRotation();
    }

    #region 内部方法
    private void Init()
    {
        cameraLeftRightTransform = cameraRoot.transform.GetChild(0).transform;
        cameraUpDownTransform = cameraLeftRightTransform.GetChild(0).transform;
        cameraRoot.transform.SetParent(transform);
        mainCamera = cameraRoot.GetComponentInChildren<Camera>();
        mainCM = cameraRoot.GetComponentInChildren<CinemachineVirtualCameraBase>();
    }
    /// <summary>
    /// 相机旋转
    /// </summary>
    protected virtual void HandleCameraRotation()
    {
        if (GameInputManager.Instance.GameInput.Player.enabled == false) return;

        float inputHorizontal = Input.GetAxisRaw("Mouse X");
        float inputVertical = Input.GetAxisRaw("Mouse Y");
        RotateCamera(inputHorizontal, inputVertical);
    }
    /// <summary>
    /// 相机旋转
    /// </summary>
    protected void RotateCamera(float _newHorizontalInput, float _newVerticalInput)
    {
        if (smoothCameraRotation)
        {
            oldHorizontalInput = Mathf.Lerp(oldHorizontalInput, _newHorizontalInput, Time.deltaTime * cameraSmoothingFactor);
            oldVerticalInput = Mathf.Lerp(oldVerticalInput, _newVerticalInput, Time.deltaTime * cameraSmoothingFactor);
        }
        else
        {
            oldHorizontalInput = _newHorizontalInput;
            oldVerticalInput = _newVerticalInput;
        }
        //为摄像机角度添加输入
        currentYAngle += oldHorizontalInput * cameraSpeed * Time.deltaTime;
        currentXAngle += oldVerticalInput * cameraSpeed * Time.deltaTime;
        currentXAngle = Mathf.Clamp(currentXAngle, lowerVerticalLimit, upperVerticalLimit);

        UpdateRotation();
    }
    /// <summary>
    /// 更新相机旋转
    /// </summary>
    protected void UpdateRotation()
    {
        //左右
        cameraLeftRightTransform.localRotation = Quaternion.Euler(new Vector3(0, currentYAngle, 0));
        //上下
        cameraUpDownTransform.localRotation = Quaternion.Euler(new Vector3(currentXAngle, 0, 0));

        facingDirection = cameraLeftRightTransform.forward;
        rightDirection = cameraLeftRightTransform.right;
        upwardsDirection = cameraLeftRightTransform.up;
    }
    #endregion

    #region 对外方法
    /// <summary>
    /// 设置主角
    /// </summary>
    public void SetMainPlayer(PlayerController mainPlayer)
    {
        mainCM.Follow = mainPlayer.transform;
        mainCM.LookAt = mainPlayer.transform;
        cameraRoot.transform.SetParent(mainPlayer.transform);
        cameraRoot.transform.localPosition = Vector3.zero;
        mainPlayer.OnDieAction += DieAction;

        void DieAction()
        {
            mainCM.Follow = null;
            mainCM.LookAt = null;
            cameraRoot.transform.parent = null;
            mainPlayer.OnDieAction -= DieAction;
        };
    }

    /// <summary>
    /// 设置FOV
    /// </summary>
    public void SetFOV(float _fov)
    {
        PlayerCamera.fieldOfView = _fov;
    }
    /// <summary>
    /// 设置旋转
    /// </summary>
    public void SetRotationAngles(float _xAngle, float _yAngle)
    {
        currentXAngle = _xAngle;
        currentYAngle = _yAngle;
        UpdateRotation();
    }
    /// <summary>
    /// 旋转摄像机指向场景中的世界位置
    /// </summary>
    public void RotateTowardPosition(Vector3 _position, float _lookSpeed)
    {
        Vector3 _direction = (_position - cameraLeftRightTransform.position);
        RotateTowardDirection(_direction, _lookSpeed);
    }
    /// <summary>
    /// 旋转镜头向场景中的一个look向量
    /// </summary>
    public void RotateTowardDirection(Vector3 _direction, float _lookSpeed)
    {
        _direction.Normalize();
        //变换目标视觉向量到这个变换的局部空间;
        _direction = cameraLeftRightTransform.parent.InverseTransformDirection(_direction);
        //计算(局部)当前look向量
        Vector3 _currentLookVector = GetAimingDirection();
        _currentLookVector = cameraLeftRightTransform.parent.InverseTransformDirection(_currentLookVector);
        //计算x角度差值
        float _xAngleDifference = GetAngle(new Vector3(0f, _currentLookVector.y, 1f), new Vector3(0f, _direction.y, 1f), Vector3.right);
        //计算y角度差值
        _currentLookVector.y = 0f;
        _direction.y = 0f;
        float _yAngleDifference = GetAngle(_currentLookVector, _direction, Vector3.up);
        //转向角度值到Vector2变量更好的限制;
        Vector2 _currentAngles = new Vector2(currentXAngle, currentYAngle);
        Vector2 _angleDifference = new Vector2(_xAngleDifference, _yAngleDifference);
        //计算归一化方向
        float _angleDifferenceMagnitude = _angleDifference.magnitude;
        if (_angleDifferenceMagnitude == 0f)
            return;
        Vector2 _angleDifferenceDirection = _angleDifference / _angleDifferenceMagnitude;
        //检查过度
        if (_lookSpeed * Time.deltaTime > _angleDifferenceMagnitude)
        {
            _currentAngles += _angleDifferenceDirection * _angleDifferenceMagnitude;
        }
        else
        {
            _currentAngles += _angleDifferenceDirection * _lookSpeed * Time.deltaTime;
        }
        //设置新的角度
        currentYAngle = _currentAngles.y;
        //限制纵向旋转
        currentXAngle = Mathf.Clamp(_currentAngles.x, -upperVerticalLimit, lowerVerticalLimit);
        UpdateRotation();
    }
    /// <summary>
    /// 有符号夹角(范围从-180到+180)
    /// </summary>
    public float GetAngle(Vector3 _vector1, Vector3 _vector2, Vector3 _planeNormal)
    {
        //计算角度和符号
        float _angle = Vector3.Angle(_vector1, _vector2);
        float _sign = Mathf.Sign(Vector3.Dot(_planeNormal, Vector3.Cross(_vector1, _vector2)));
        //结合角度和符号;
        float _signedAngle = _angle * _sign;
        return _signedAngle;
    }

    #region 取值

    public float GetCurrentXAngle()
    {
        return currentXAngle;
    }

    public float GetCurrentYAngle()
    {
        return currentYAngle;
    }

    #endregion 取值

    #region 相机相关向量

    /// <summary>
    /// 前方
    /// </summary>
    public Vector3 GetFacingDirection()
    {
        return facingDirection;
    }
    /// <summary>
    /// 右方
    /// </summary>
    public Vector3 GetRightDirection()
    {
        return rightDirection;
    }
    /// <summary>
    /// 上方
    /// </summary>
    public Vector3 GetUpDirection()
    {
        return upwardsDirection;
    }
    /// <summary>
    /// 瞄准方向
    /// </summary>
    public Vector3 GetAimingDirection()
    {
        //return cameraLeftRightTransform.forward;
        return new(currentYAngle, currentYAngle, 0);
    }
    #endregion 相机相关向量

    #endregion 对外
}