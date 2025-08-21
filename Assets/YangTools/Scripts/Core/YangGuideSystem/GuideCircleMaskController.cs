using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 圆形遮罩镂空引导
/// </summary>
public class GuideCircleMaskController : MonoBehaviour
{
    /// <summary>
    /// 要高亮显示的目标
    /// </summary>
    public RectTransform target;
    
    private Canvas canvas;
    //区域范围缓存
    private Vector3[] corners = new Vector3[4];
    //镂空区域圆心
    private Vector4 _center;
    //镂空区域半径
    private float _radius;
    //遮罩材质
    private Material _material;
    //当前高亮区域的半径
    private float _currentRadius;
    //高亮区域缩放的动画时间
    private float _shrinkTime = 0.06f;
    //收缩速度
    private float _shrinkVelocity = 0f;
    
    /// <summary>
    /// 世界坐标向画布坐标转换
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="world">世界坐标</param>
    /// <returns>返回画布上的二维坐标</returns>
    private Vector2 WorldToCanvasPos(Canvas canvas, Vector3 world)
    {
        Vector2 position;
        Vector2 scenePos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            scenePos, canvas.GetComponent<Camera>(), out position);
        return position;
    }

    public void SetTarget(RectTransform target)
    {
        this.target = target;
        RefreshMask();
    }

    public void RefreshMask()
    {
        //canvas = UIWindowMgr.Instance.canvas;
        //获取高亮区域的四个顶点的世界坐标
        target.GetWorldCorners(corners);
        //计算最终高亮显示区域的半径
        _radius = Vector2.Distance(WorldToCanvasPos(canvas, corners[0]), WorldToCanvasPos(canvas, corners[2])) / 2f;
        //计算高亮显示区域的圆心
        float x = corners[0].x + ((corners[3].x - corners[0].x) / 2f);
        float y = corners[0].y + ((corners[1].y - corners[0].y) / 2f);
        Vector3 centerWorld = new Vector3(x, y, 0);
        Vector2 center = WorldToCanvasPos(canvas, centerWorld);
        //设置遮罩材料中的圆心变量
        Vector4 centerMat = new Vector4(center.x, center.y, 0, 0);
        _material = GetComponent<Image>().material;
        _material.SetVector("_Center", centerMat);
        //计算当前高亮显示区域的半径
        RectTransform canRectTransform = canvas.transform as RectTransform;
        if (canRectTransform != null)
        {
            //获取画布区域的四个顶点
            canRectTransform.GetWorldCorners(corners);
            //将画布顶点距离高亮区域中心最远的距离作为当前高亮区域半径的初始值
            foreach (Vector3 corner in corners)
            {
                _currentRadius = Mathf.Max(Vector3.Distance(WorldToCanvasPos(canvas, corner), center), _currentRadius);
            }
        }
        _material.SetFloat("_Slider", _currentRadius);
    }

    private void Update()
    {
        //从当前半径到目标半径差值显示收缩动画
        float value = Mathf.SmoothDamp(_currentRadius, _radius, ref _shrinkVelocity, _shrinkTime);
        if (!Mathf.Approximately(value, _currentRadius))
        {
            _currentRadius = value;
            _material.SetFloat("_Slider", _currentRadius);
        }
    }
}