/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0b14 
 *创建时间:         2022-05-14 
*/
using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour
{
    [SerializeField] private float width = 0.1f;
    [SerializeField] private Color color = Color.grey;

    private LineRenderer currentLR;
    private Vector2 previousPoint;

    private Camera camera;
    public void Awake()
    {
        camera = Camera.main;
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //线条渲染
            currentLR = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            currentLR.material = new Material(Shader.Find("Sprites/Default")) { color = color };
            currentLR.widthMultiplier = width;
            currentLR.useWorldSpace = false;
            currentLR.positionCount = 1;
            currentLR.SetPosition(0, (Vector2)camera.ScreenToWorldPoint(Input.mousePosition));

            //更新数据
            previousPoint = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            if (previousPoint != (Vector2)camera.ScreenToWorldPoint(Input.mousePosition))
            {
                //线条渲染
                currentLR.positionCount++;
                currentLR.SetPosition(currentLR.positionCount - 1, (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition));

                //碰撞器
                BoxCollider2D collider = new GameObject("BoxCollider2D").AddComponent<BoxCollider2D>();
                collider.transform.parent = currentLR.transform;
                Vector2 latestPoint = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
                collider.transform.position = (previousPoint + latestPoint) * 0.5f;
                float angle = Mathf.Atan2((latestPoint - previousPoint).y, (latestPoint - previousPoint).x) * Mathf.Rad2Deg;
                collider.transform.eulerAngles = new Vector3(0, 0, angle);
                collider.size = new Vector2(Vector2.Distance(latestPoint, previousPoint), width);

                //更新数据
                previousPoint = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentLR.transform.childCount > 0)
            {
                currentLR.gameObject.AddComponent<Rigidbody2D>().useAutoMass = true;
            }
        }
    }
}