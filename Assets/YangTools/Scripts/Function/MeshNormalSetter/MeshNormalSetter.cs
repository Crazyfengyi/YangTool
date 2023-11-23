using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
public class MeshNormalSetter : MonoBehaviour
{
    //  如果不指定原始网格，则直接使用MeshFilter保留的网格
    public Mesh sourceMesh;

    // 如果不指定中心点，则使用对象的原点
    public Transform center;

    // 新创建的法线与原始法线混合的百分比 0 表示与原始法线没有变化 1 表示全新的法线
    [Range(0, 1)]
    public float combineRate = 1f;

    [ContextMenu("Set Normals")]
    public void SetNormals()
    {
        MeshFilter targetFilter = GetComponent<MeshFilter>();

        // 获取底层网格
        Mesh sourceMesh = this.sourceMesh ? this.sourceMesh : targetFilter.sharedMesh;
        if (sourceMesh == null) return;

        // 创建新的普通数据
        Vector3 centerPos = center ? center.position : transform.position;
        List<Vector3> newNormals = new List<Vector3>();
        for (int i = 0; i < sourceMesh.vertexCount; i++)
        {
            Vector3 centerToVertex = sourceMesh.vertices[i] - transform.InverseTransformPoint(centerPos);
            newNormals.Add(Vector3.Lerp(sourceMesh.normals[i], centerToVertex.normalized, combineRate));
        }

        //  创建一个新的网格
        Mesh newMesh = Instantiate(sourceMesh) as Mesh;
        newMesh.SetNormals(newNormals);

        newMesh.name = sourceMesh.name;
        if (!newMesh.name.Contains("(Normals Changed!)")) newMesh.name += " (Normals Changed!)";

        // 应用网格
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(newMesh, "Set Normals");
        Undo.RecordObject(targetFilter, "Applied To MeshFilter");
#endif

        targetFilter.mesh = newMesh;
    }
}