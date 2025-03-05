using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public enum SelectMode
{
    /// <summary>
    /// 顶点
    /// </summary>
    Vertex,
    /// <summary>
    /// 三角面
    /// </summary>
    Triangle,
    /// <summary>
    /// 平面
    /// </summary>
    Plane,
}

public class SceneEditor : MonoBehaviour
{
    //平面缓存
    Dictionary<MeshCollider, Dictionary<int, HashSet<int>>> planeCached = new Dictionary<MeshCollider, Dictionary<int, HashSet<int>>>();

    public SelectMode selectMode;

    [FormerlySerializedAs("SelectedFace")] 
    public Material selectedFace;

    private GameObject verticeObj;

    private GameObject currentSelect;
    private Vector3 currentSelectVertice;

    private GameObject from;//起点面
    private Vector3 fromVertice;
    private GameObject to;//目标面
    private Vector3 toVertice;

    public Button verticeSelect;
    public Button PlaneSelect;
    public Button TriangleSelect;

    private void Awake()
    {
        verticeSelect.onClick.AddListener(() =>
        {
            selectMode = SelectMode.Vertex;

        });
        PlaneSelect.onClick.AddListener(() =>
        {
            selectMode = SelectMode.Plane;

        }); TriangleSelect.onClick.AddListener(() =>
        {
            selectMode = SelectMode.Triangle;

        });
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (from == null)
                {
                    from = currentSelect;
                    fromVertice = currentSelectVertice;
                }
                else
                {
                    to = currentSelect;
                    toVertice = currentSelectVertice;

                    if (from == to)
                    {
                        from = null;
                        return;
                    }

                    if (selectMode == SelectMode.Vertex)
                    {
                        Debug.LogError("模型顶点吸附物体");
                        EditorHelper.AttachTwoGameObjectByVertice(from, to, fromVertice, toVertice);
                    }
                    else
                    {
                        Debug.LogError("模型面吸附物体");
                        EditorHelper.AttachTwoGameObject(from, to);
                    }

                    from = null;
                }
            }
        }
    }

    private void OnPostRender()
    {
        DrawSelectedArea();
    }

    void DrawSelectedArea()
    {
        selectedFace.SetPass(0);
        Ray TouchRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        MeshCollider collider;
        if (selectMode == SelectMode.Triangle)
        {
            int triangleIndex;
            if (EditorHelper.TryPickTriangle(TouchRay, out collider, out triangleIndex))
            {
                Mesh mesh = collider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;  //����


                BeginLocalDraw(GL.TRIANGLES, collider.transform, Color.yellow);

                GLVertex3(vertices[triangles[triangleIndex * 3]]);
                GLVertex3(vertices[triangles[triangleIndex * 3 + 1]]);
                GLVertex3(vertices[triangles[triangleIndex * 3 + 2]]);
                EndDraw();

                currentSelect = collider.gameObject;
                //currentSelectVertice = EditorHelper.GetVerticalRay(mesh, hsIndex);
            }
            else
            {
                //Debug.LogError(123);
            }
        }
        else if (selectMode == SelectMode.Plane)
        {
            int index;
            if (EditorHelper.TryPickTriangle(TouchRay, out collider, out index))
            {
                HashSet<int> hsIndex = null;
                Dictionary<int, HashSet<int>> meshCached;
                if (planeCached.TryGetValue(collider, out meshCached))
                {
                    meshCached.TryGetValue(index, out hsIndex);
                }
                else
                {
                    //��ȻҪ����
                    meshCached = new Dictionary<int, HashSet<int>>();
                    planeCached.Add(collider, meshCached);
                }

                //û���ҵ���ȥ��ѯ
                if (hsIndex == null)
                {
                    hsIndex = EditorHelper.GetPlaneTrianglesByFaceIndex(collider, index);
                    //д�뻺��
                    meshCached.Add(index, hsIndex);
                }

                //����
                Mesh mesh = collider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles; //����

                BeginLocalDraw(GL.TRIANGLES, collider.transform, Color.yellow);
                foreach (var triangleIndex in hsIndex)
                {
                    GLVertex3(vertices[triangles[triangleIndex * 3]]);
                    GLVertex3(vertices[triangles[triangleIndex * 3 + 1]]);
                    GLVertex3(vertices[triangles[triangleIndex * 3 + 2]]);
                }
                EndDraw();


                currentSelect = collider.gameObject;
                currentSelectVertice = EditorHelper.GetMeshLocalCenter(mesh, hsIndex);
            }
        }
        else if (selectMode == SelectMode.Vertex)
        {

            if (EditorHelper.TryPickTriangleVertice(TouchRay, out collider, out Vector3 vertice))
            {
                if (verticeObj == null)
                {
                    verticeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    verticeObj.GetComponent<MeshRenderer>().material = selectedFace;
                    verticeObj.transform.localScale = Vector3.one * 0.05f;
                }

                verticeObj.transform.position = vertice;
                currentSelect = collider.gameObject;
                currentSelectVertice = vertice;
            }
        }
    }
    void GLVertex3(Vector3 v)
    {
        GL.Vertex3(v.x, v.y, v.z);
    }
    public void EndDraw()
    {
        GL.End();
        GL.PopMatrix();
    }
    public void BeginLocalDraw(int mode, Transform trans, Color color)
    {
        GL.PushMatrix();
        GL.MultMatrix(trans.localToWorldMatrix);
        GL.Begin(mode);
        GL.Color(color);
    }
    public void SetSelectMode(SelectMode selectMode)
    {
        this.selectMode = selectMode;
    }
}
