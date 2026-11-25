using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
/// <summary>
/// 区分顶点 三角面和共面区域的拾取方式
/// </summary>
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

/// <summary>
/// 演示鼠标拾取网格区域 高亮绘制与两次选择后的物体吸附
/// </summary>
public class SceneEditor : MonoBehaviour
{
    //平面缓存
    Dictionary<MeshCollider, Dictionary<int, HashSet<int>>> planeCached = new Dictionary<MeshCollider, Dictionary<int, HashSet<int>>>();

    // 当前网格拾取模式
    public SelectMode selectMode;

    // 绘制选中区域使用的材质
    [FormerlySerializedAs("SelectedFace")] 
    public Material selectedFace;

    // 顶点拾取使用的可视标记对象
    private GameObject verticeObj;

    // 当前射线拾取的物体
    private GameObject currentSelect;
    // 当前拾取点或区域中心
    private Vector3 currentSelectVertice;

    private GameObject from;//起点面
    // 首次选择的吸附参考点
    private Vector3 fromVertice;
    private GameObject to;//目标面
    // 第二次选择的吸附参考点
    private Vector3 toVertice;

    // 切换顶点模式的按钮
    public Button verticeSelect;
    // 切换共面区域模式的按钮
    public Button PlaneSelect;
    // 切换三角面模式的按钮
    public Button TriangleSelect;

    /// <summary>
    /// 将三个选择按钮绑定到相应的拾取模式
    /// </summary>
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

    /// <summary>
    /// 响应鼠标点击并将两次选中的物体按顶点或面进行吸附
    /// </summary>
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

    /// <summary>
    /// 在相机渲染结束后绘制当前拾取区域
    /// </summary>
    private void OnPostRender()
    {
        DrawSelectedArea();
    }

    /// <summary>
    /// 按拾取模式更新选中对象并绘制顶点标记或面片高亮
    /// </summary>
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
                int[] triangles = mesh.triangles;  // 三角形顶点索引


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
                    // 首次访问该碰撞体时建立面片缓存
                    meshCached = new Dictionary<int, HashSet<int>>();
                    planeCached.Add(collider, meshCached);
                }

                // 未命中缓存时查询共面三角形
                if (hsIndex == null)
                {
                    hsIndex = EditorHelper.GetPlaneTrianglesByFaceIndex(collider, index);
                    // 写入当前三角面的共面区域缓存
                    meshCached.Add(index, hsIndex);
                }

                // 读取用于高亮绘制的网格数据
                Mesh mesh = collider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles; // 三角形顶点索引

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
    /// <summary>
    /// 向当前 GL 绘制批次提交一个三维顶点
    /// </summary>
    /// <param name="v">提交到当前绘制批次的顶点</param>
    void GLVertex3(Vector3 v)
    {
        GL.Vertex3(v.x, v.y, v.z);
    }
    /// <summary>
    /// 结束 GL 批次并恢复矩阵栈
    /// </summary>
    public void EndDraw()
    {
        GL.End();
        GL.PopMatrix();
    }
    /// <summary>
    /// 将物体局部矩阵与颜色应用到新的 GL 绘制批次
    /// </summary>
    /// <param name="mode">GL 绘制图元模式</param>
    /// <param name="trans">提供局部到世界矩阵的物体变换</param>
    /// <param name="color">本次绘制或动画使用的颜色</param>
    public void BeginLocalDraw(int mode, Transform trans, Color color)
    {
        GL.PushMatrix();
        GL.MultMatrix(trans.localToWorldMatrix);
        GL.Begin(mode);
        GL.Color(color);
    }
    /// <summary>
    /// 切换后续射线拾取使用的选择模式
    /// </summary>
    /// <param name="selectMode">后续拾取使用的模式</param>
    public void SetSelectMode(SelectMode selectMode)
    {
        this.selectMode = selectMode;
    }
}
