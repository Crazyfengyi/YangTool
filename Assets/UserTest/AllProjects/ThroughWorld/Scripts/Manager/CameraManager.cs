/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-24 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using Cinemachine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Camera mainCamera;
    public CinemachineVirtualCameraBase mainCM;
    //相机旋转速度
    public float cameraSpeed = 250f;
    //是否对相机旋转值进行平滑处理;
    public bool smoothCameraRotation = false;
    //这个值控制如何平稳地将旧的相机旋转角度插值到新的相机旋转角度;
    //设置此值为'50f'(或以上)将导致根本没有平滑;
    //设置此值为'1f'(或以下)将会产生非常明显的平滑效果;
    //在大多数情况下，建议值为'25f';
    [Range(1f, 50f)]
    public float cameraSmoothingFactor = 25f;

    //对transform和camera组件的引用
    protected Transform tr;
    protected Camera cam;

    //当前旋转值(以度为单位);
    float currentXAngle = 0f;
    float currentYAngle = 0f;
    //垂直旋转的上限和下限(游戏物体的局部x轴);
    [Range(0f, 90f)]
    public float upperVerticalLimit = 60f;
    [Range(0f, 90f)]
    public float lowerVerticalLimit = 60f;
    //插值存储的旧旋转值
    float oldHorizontalInput = 0f;
    float oldVerticalInput = 0f;
    ////面向和向上方向
    Vector3 facingDirection;
    Vector3 upwardsDirection;
    protected override void Awake()
    {
        base.Awake();
        tr = transform;
        cam = mainCamera;
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }
        //设置角度变量为当前变换的旋转角度
        currentXAngle = tr.localRotation.eulerAngles.x;
        currentYAngle = tr.localRotation.eulerAngles.y;
        //执行一次相机旋转代码来计算面向和向上的方向
        RotateCamera(0f, 0f);
        Setup();
    }
    /// <summary>
    /// 这个函数在Awake()之后被调用;它可以通过继承脚本来重写
    /// </summary>
    protected virtual void Setup()
    {

    }
    void Update()
    {
        HandleCameraRotation();
    }
    /// <summary>
    /// 获取用户输入并处理相机旋转,这个方法可以重写从这个基类派生的类来修改相机的行为
    /// </summary>
    protected virtual void HandleCameraRotation()
    {
        float _inputHorizontal = Input.GetAxisRaw("Mouse X"); ;
        float _inputVertical = Input.GetAxisRaw("Mouse Y"); ;
        RotateCamera(_inputHorizontal, _inputVertical);
    }
    /// <summary>
    /// 旋转相机
    /// </summary>
    /// <param name="_newHorizontalInput">横向输入</param>
    /// <param name="_newVerticalInput">纵向输入</param>
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
        currentXAngle = Mathf.Clamp(currentXAngle, -upperVerticalLimit, lowerVerticalLimit);

        UpdateRotation();
    }
    /// <summary>
    /// 更新相机旋转
    /// </summary>
    protected void UpdateRotation()
    {
        tr.localRotation = Quaternion.Euler(new Vector3(0, currentYAngle, 0));
        //保存“facingDirection”和“upwardsDirection”稍后使用
        facingDirection = tr.forward;
        upwardsDirection = tr.up;
        tr.localRotation = Quaternion.Euler(new Vector3(currentXAngle, currentYAngle, 0));
    }

    #region 对外
    /// <summary>
    /// 设置FOV
    /// </summary>
    public void SetFOV(float _fov)
    {
        if (cam) cam.fieldOfView = _fov;
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
    /// 旋转摄像机指向场景中的世界位置;
    /// </summary>
    public void RotateTowardPosition(Vector3 _position, float _lookSpeed)
    {
        Vector3 _direction = (_position - tr.position);
        RotateTowardDirection(_direction, _lookSpeed);
    }
    /// <summary>
    /// 旋转镜头向场景中的一个look向量
    /// </summary>
    public void RotateTowardDirection(Vector3 _direction, float _lookSpeed)
    {
        _direction.Normalize();
        //变换目标视觉向量到这个变换的局部空间;
        _direction = tr.parent.InverseTransformDirection(_direction);
        //计算(局部)当前look向量
        Vector3 _currentLookVector = GetAimingDirection();
        _currentLookVector = tr.parent.InverseTransformDirection(_currentLookVector);
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
    /// 计算'_vector_1'和'_vector_2'之间的signed角(范围从-180到+180)
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
    #endregion

    #region 相机相关向量
    //返回相机面对的方向，没有任何垂直旋转,该矢量应用于与移动相关的目的(例如，向前移动);
    public Vector3 GetFacingDirection()
    {
        return facingDirection;
    }
    /// <summary>
    /// 返回游戏物体的“前方”向量,这个矢量指向相机“瞄准”的方向，可以用于实例化投射物或光线投射
    /// </summary>
    public Vector3 GetAimingDirection()
    {
        return tr.forward;
    }
    /// <summary>
    /// 返回游戏对象的“右方”向量
    /// </summary>
    public Vector3 GetStrafeDirection()
    {
        return tr.right;
    }
    /// <summary>
    /// 返回游戏物体的"向上"向量
    /// </summary>
    public Vector3 GetUpDirection()
    {
        return upwardsDirection;
    }
    #endregion

    #endregion
}