//Author：AboutVFX
//Email:54315031@qq.com
//QQ Group：156875373

using System.Collections.Generic;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 定时烘焙蒙皮网格并绘制逐渐消退的运动残影
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class GhostTrail : MonoBehaviour
    {
        // 是否打开残影拖尾
        public bool openGhostTrail = true;
        // 间隔时间
        [Range(0.05f, 0.5f)]
        public float intervalTime = 0.1f;
        // 生命时间
        [Range(0.1f, 1.0f)]
        public float lifeTime = 0.5f;
        // 隐藏时间
        [Range(0.1f, 1.0f)]
        public float fadeTime = 0.3f;
        // 更新时间
        private float updateTime = 0.2f;
        // 残影列表
        private List<GhostInfo> ghostList = new List<GhostInfo>();
        // 渲染器
        private SkinnedMeshRenderer skinnedMesh;
        // 是否使用残影材质
        public bool useGhostMaterial = false;
        // 残影材质
        public Material ghostMaterial;
        // 上一帧的位置
        private Vector3 lastPos;
        /// <summary>
        /// 缓存需要生成残影的蒙皮网格渲染器
        /// </summary>
        void Start()
        {
            skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        }
        /// <summary>
        /// 按位移与采样间隔创建残影并更新现存残影的淡出绘制
        /// </summary>
        void LateUpdate()
        {
            if ((lastPos - transform.position).sqrMagnitude > 0.01f * 0.01f)
            {
                if (openGhostTrail == true)
                {
                    updateTime += Time.deltaTime;
                    if (updateTime >= intervalTime)
                    {
                        updateTime = 0f;
                        CreateGhostItem();
                    }
                }
            }

            lastPos = transform.position;
            if (ghostList.Count > 0)
            {
                SetGhostItemFade();
                DrawGhostItem();
            }
        }
        /// <summary>
        /// 创建残影
        /// </summary>
        void CreateGhostItem()
        {
            Mesh GhostMesh = new Mesh();
            skinnedMesh.BakeMesh(GhostMesh);
            Material material = null;
            if (useGhostMaterial)
            {
                material = new Material(ghostMaterial);
            }
            else
            {
                material = new Material(skinnedMesh.material);
            }
            ghostList.Add(new GhostInfo(GhostMesh, transform.localToWorldMatrix, material));
        }
        /// <summary>
        /// 设置残影透明度
        /// </summary>
        void SetGhostItemFade()
        {
            for (int i = (ghostList.Count - 1); i >= 0; i--)
            {
                if (ghostList[i].lifeTime > lifeTime)
                {
                    ghostList[i].fadeTime += (1 / fadeTime) * Time.deltaTime;
                    ghostList[i].alpha = Mathf.Lerp(1f, 0f, ghostList[i].fadeTime);
                    if (ghostList[i].fadeTime > 1)
                    {
                        Destroy(ghostList[i].mesh);
                        Destroy(ghostList[i].material);
                        ghostList.RemoveAt(i);
                    }
                    else
                    {
                        //TODO 根据shader更改
                        ghostList[i].material.SetFloat("_Opacity", ghostList[i].alpha);
                    }
                }
            }
        }
        /// <summary>
        /// 渲染残影
        /// </summary>
        void DrawGhostItem()
        {
            foreach (GhostInfo item in ghostList)
            {
                item.lifeTime += Time.deltaTime;
                Graphics.DrawMesh(item.mesh, item.matrix, item.material, gameObject.layer);
            }
        }
    }
    /// <summary>
    /// 残影物体信息
    /// </summary>
    public class GhostInfo
    {
        //残影网格
        public Mesh mesh;
        //残影位置
        public Matrix4x4 matrix;
        //残影纹理
        public Material material;
        //残影生存时间
        public float lifeTime = 0f;
        //残影隐藏时间
        public float fadeTime = 0f;
        //透明度
        public float alpha = 1.0f;

        /// <summary>
        /// 记录残影网格 世界变换矩阵与材质
        /// </summary>
        /// <param name="_mesh">残影烘焙后的网格</param>
        /// <param name="_matrix">残影生成时的世界变换矩阵</param>
        /// <param name="_material">残影独立使用的材质</param>
        public GhostInfo(Mesh _mesh, Matrix4x4 _matrix, Material _material)
        {
            mesh = _mesh;
            matrix = _matrix;
            material = _material;
        }
    }
}