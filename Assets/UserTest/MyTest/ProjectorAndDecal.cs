using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProjectorAndDecal : MonoBehaviour
{
    /// <summary> ��ƽ�� </summary>
    public float nearClipPlane = 0.1f;

    /// <summary> Զƽ�� </summary>
    public float farClipPlane = 100;

    /// <summary> ���ݱ� </summary>
    public float aspectRatio = 1;

    /// <summary> ����ͶӰ���С </summary>
    public float size = 10;

    /// <summary> ���� </summary>
    public Material material;

    /// <summary> ��ͼ�� </summary>
    private Transform viewBox;

    /// <summary> ������������� </summary>
    private MeshFilter meshFilter;

    /// <summary> �������Ⱦ�� </summary>
    private MeshRenderer meshRenderer;

    private void OnValidate()
    {
        GenerateViewBox();
    }

    /// <summary> ����ͶӰ��Χ </summary>
    private void GenerateViewBox()
    {
        viewBox = transform.Find("ViewBox");
        if (viewBox == null)
        {
            viewBox = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            DestroyImmediate(viewBox.GetComponent<BoxCollider>());
            viewBox.SetParent(transform);
            viewBox.name = "ViewBox";
        }

        if (viewBox != null)
        {
            meshFilter = viewBox.GetComponent<MeshFilter>();
            meshRenderer = viewBox.GetComponent<MeshRenderer>();
            if (material != null)
            {
                meshRenderer.sharedMaterial = material;
            }
        }

        viewBox.localScale = new Vector3(aspectRatio * size, size, farClipPlane - nearClipPlane);
        viewBox.localPosition = new Vector3(0, 0, farClipPlane * 0.5f + nearClipPlane * 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (meshFilter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, size * 0.05f);
            Gizmos.color = Color.white;
            Gizmos.DrawWireMesh(meshFilter.sharedMesh, viewBox.position, viewBox.rotation, viewBox.localScale);
        }
    }
}

