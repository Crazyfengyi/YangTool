// Version 1.6.2
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;
using UnityMesh = UnityEngine.Mesh;

namespace Exploder
{
    public class ExploderMesh
    {
        public int[] triangles;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uv;
        public Vector4[] tangents;
        public Color32[] colors32;

        public Vector3 centroid;

        public ExploderMesh()
        {
        }

        public ExploderMesh(UnityMesh unityMesh)
        {
            triangles = unityMesh.triangles;
            vertices = unityMesh.vertices;
            normals = unityMesh.normals;
            uv = unityMesh.uv;
            tangents = unityMesh.tangents;
            colors32 = unityMesh.colors32;

            CalculateCentroid();
        }

        public void CalculateCentroid()
        {
            centroid = Vector3.zero;

            foreach (var v in vertices)
            {
                centroid += v;
            }

            centroid /= vertices.Length;
        }

        public UnityMesh ToUnityMesh()
        {
            return new UnityMesh
            {
                vertices = vertices,
                normals = normals,
                uv = uv,
                tangents = tangents,
                colors32 = colors32,
                triangles = triangles
            };
        }
    }
}
