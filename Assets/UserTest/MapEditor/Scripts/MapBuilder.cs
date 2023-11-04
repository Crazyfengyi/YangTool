using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Minimalist.MapBuilder
{
    [ExecuteInEditMode]
    public class MapBuilder : MonoBehaviour
    {
        // Public properties
        public Transform[] GridTransforms
        {
            get
            {
                return _gridTransform.GetComponents<Transform>();
            }
        }
        /// <summary>
        /// 进行实例化
        /// </summary>
        public bool DoneInstantiating { get; set; }
        /// <summary>
        /// 编辑状态
        /// </summary>
        public bool EditState { get; set; }
        /// <summary>
        /// 要放置的位置
        /// </summary>
        public Vector3 HoveredPosition { get; set; }
        /// <summary>
        /// 选择的位置列表
        /// </summary>
        public List<Vector3> SelectedPositions { get; set; }
        /// <summary>
        /// 拖拽中
        /// </summary>
        public bool Dragging { get; set; }

        // Public fields
        [Header("基础:")]
        public EditModeInstanceBhv basePrefab;
        public Color baseColor = Color.black;
        public Vector3Int baseScale = new Vector3Int(25, 1, 25);

        [Header("放置块:")]
        public EditModeInstanceBhv tilePrefab;
        public Color tileColor = Color.white;
        public Vector3Int tileScale = new Vector3Int(1, 1, 1);

        // Private fields
        private Transform _transform;

        private Transform _baseTransform;
        private EditModeInstanceBhv _baseEditModeInstance;
        private Renderer _baseRenderer;
        private Collider _baseCollider;
        private Material _baseMaterial;

        private Transform _gridTransform;
        private MeshFilter _gridMeshFilter;
        private List<Vector3> _gridVertices;

        private Transform _tileParentTransform;
        private Material _tileMaterial;
        private Vector3 _tileHalfHeight;

        private Vector3 _offset;
        private bool _busy;

        private void Awake()
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfAnyPrefab(this.gameObject))
            {
                PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
#endif
        }

        private void OnValidate()
        {
            if (_gridTransform != null || _baseTransform != null && _baseTransform.localScale.y > baseScale.y)
            {
                Update();
            }
        }

        private void Update()
        {
            if (_busy || !this.EditState)
            {
                return;
            }

            _busy = true;

            PseudoAwake();

            ValidateBaseScale();

            UpdateBase();

            UpdateGrid();

            UpdateTileParent();

            ValidateTileScale();

            UpdateGridMesh();

            DoneInstantiating = true;

            _busy = false;
        }

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        private void PseudoAwake()
        {
            _transform = this.GetComponent<Transform>();

            _gridTransform = _transform.Find("Grid");

            if (_gridTransform == null)
            {
                this.InstantiateGrid();
            }

            _gridTransform.hideFlags = HideFlags.NotEditable;

            _gridMeshFilter = _gridTransform.GetComponent<MeshFilter>();

            if (_gridMeshFilter.sharedMesh == null)
            {
                _gridMeshFilter.sharedMesh = new Mesh() { name = "Grid" };
            }

            _baseTransform = _transform.Find("Base");

            if (_baseTransform == null)
            {
                this.InstantiateBase();
            }

            _baseEditModeInstance = _baseTransform.GetComponent<EditModeInstanceBhv>();

            _baseRenderer = _baseTransform.GetComponent<Renderer>();

            _baseCollider = _baseTransform.GetComponent<Collider>();

            _tileParentTransform = _transform.Find("Tiles");

            if (_tileParentTransform == null)
            {
                this.InstantiateTileParent();
            }

            _baseMaterial = basePrefab.GetComponent<Renderer>().sharedMaterial;

            _tileMaterial = tilePrefab.GetComponent<Renderer>().sharedMaterial;

            if (this.SelectedPositions == null)
            {
                this.SelectedPositions = new List<Vector3>();
            }

            _offset = _baseTransform.position;
        }
        /// <summary>
        /// 创建网格辅助
        /// </summary>
        private void InstantiateGrid()
        {
            GameObject gridGameObject = new GameObject("Grid");

            gridGameObject.AddComponent<MeshFilter>();

            gridGameObject.AddComponent<MeshRenderer>();

            _gridTransform = gridGameObject.transform;

            _gridTransform.parent = _transform;
        }
        /// <summary>
        /// 创建基础
        /// </summary>
        private void InstantiateBase()
        {
            basePrefab.scale = baseScale;

            basePrefab.material = _baseMaterial;

            basePrefab.color = baseColor;

            _baseEditModeInstance = Instantiate(basePrefab, Vector3.zero, Quaternion.identity, _transform);

            _baseTransform = _baseEditModeInstance.GetComponent<Transform>();

            _baseTransform.name = "Base";
        }
        /// <summary>
        /// 创建放置块父节点
        /// </summary>
        private void InstantiateTileParent()
        {
            GameObject tileParentGameObject = new GameObject("Tiles");

            _tileParentTransform = tileParentGameObject.transform;

            _tileParentTransform.parent = _transform;
        }
        #endregion

        /// <summary>
        /// 验证基准比例
        /// </summary>
        private void ValidateBaseScale()
        {
            int x = (int)Mathf.Clamp(baseScale.x, 1, 50);
            int y = (int)Mathf.Clamp(baseScale.y, 1, 50);
            int z = (int)Mathf.Clamp(baseScale.z, 1, 50);

            baseScale = new Vector3Int(x, y, z);
        }
        /// <summary>
        /// 更新基础
        /// </summary>
        private void UpdateBase()
        {
            _baseEditModeInstance.scale = baseScale;

            _baseEditModeInstance.color = baseColor;

            _baseTransform.localScale = _baseEditModeInstance.scale;

            _baseRenderer.material = new Material(_baseMaterial) { color = baseColor };
        }
        /// <summary>
        /// 更新网格
        /// </summary>
        private void UpdateGrid()
        {
            _gridTransform.position = _baseTransform.position;
        }
        /// <summary>
        /// 更新放置块的父节点
        /// </summary>
        private void UpdateTileParent()
        {
            _tileParentTransform.position = _offset + Vector3.up * baseScale.y / 2f;
        }
        /// <summary>
        /// 验证放置块比例
        /// </summary>
        private void ValidateTileScale()
        {
            int x = (int)Mathf.Clamp(tileScale.x, 1, baseScale.x);
            int y = (int)Mathf.Clamp(tileScale.y, 1, Mathf.Infinity);
            int z = (int)Mathf.Clamp(tileScale.z, 1, baseScale.z);

            tileScale = new Vector3Int(x, y, z);

            _tileHalfHeight = Vector3.up * tileScale.y / 2f;
        }
        /// <summary>
        /// 更新网格Mesh
        /// </summary>
        public void UpdateGridMesh()
        {
            int xSize = baseScale.x - (int)(tileScale.x - 1);
            int zSize = baseScale.z - (int)(tileScale.z - 1);

            List<Vector3> allVertices = new List<Vector3>();

            for (int i = 0; i < zSize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    float x = j - baseScale.x / 2f + tileScale.x / 2f;
                    float z = i - baseScale.z / 2f + tileScale.z / 2f;

                    Vector3 rayOrigin = new Vector3(x, int.MaxValue, z) + _offset;

                    RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, Mathf.Infinity);

                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.point.y < _offset.y)
                        {
                            continue;
                        }

                        float y = hit.point.y - _offset.y;

                        Vector3 vertex = new Vector3(x, y, z);

                        allVertices.Add(vertex);
                    }
                }
            }

            _gridVertices = new List<Vector3>(allVertices);

            foreach (Vector3 vertex in allVertices)
            {
                bool isValid = true;

                Vector3 globalVertex = vertex + _offset + _tileHalfHeight;

                Bounds vertexBounds = new Bounds(globalVertex, (Vector3)tileScale * .95f);

                Collider[] placedCollidersInReach = Physics.OverlapBox(globalVertex, (Vector3)tileScale / 2f);

                foreach (Collider placedTileCollider in placedCollidersInReach)
                {
                    if (placedTileCollider == _baseCollider)
                    {
                        continue;
                    }

                    if (placedTileCollider.bounds.Intersects(vertexBounds))
                    {
                        isValid = false;

                        break;
                    }
                }

                if (!isValid)
                {
                    _gridVertices.Remove(vertex);

                    continue;
                }

                foreach (Vector3 selectedPosition in this.SelectedPositions)
                {
                    Bounds selectedBounds = new Bounds(selectedPosition + _tileHalfHeight, (Vector3)tileScale * 0.95f);

                    if (selectedBounds.Intersects(vertexBounds))
                    {
                        isValid = false;

                        break;
                    }
                }

                if (!isValid)
                {
                    _gridVertices.Remove(vertex);
                }
            }

            _gridMeshFilter.sharedMesh.Clear();

            _gridMeshFilter.sharedMesh.vertices = _gridVertices.ToArray();
        }
        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!this.EditState || _baseTransform == null || _tileParentTransform == null)
            {
                return;
            }

            Gizmos.color = new Color(tileColor.r, tileColor.g, tileColor.b, .01f);

            for (int i = 0; i < _gridVertices.Count; i++)
            {
                Gizmos.DrawCube(_gridVertices[i] + _offset + _tileHalfHeight, tileScale);

                Gizmos.DrawWireCube(_gridVertices[i] + _offset + _tileHalfHeight, tileScale);
            }

            if (!this.Dragging)
            {
                Gizmos.color = new Color(tileColor.r, tileColor.g, tileColor.b, .25f);

                Gizmos.DrawCube(this.HoveredPosition + _tileHalfHeight, tileScale);

                Gizmos.color = new Color(tileColor.r, tileColor.g, tileColor.b, .5f);

                Gizmos.DrawWireCube(this.HoveredPosition + _tileHalfHeight, tileScale);
            }
            else
            {
                foreach (Vector3 selectedPosition in this.SelectedPositions)
                {
                    Gizmos.color = new Color(tileColor.r, tileColor.g, tileColor.b, .25f);

                    Gizmos.DrawCube(selectedPosition + _tileHalfHeight, tileScale);

                    Gizmos.color = new Color(tileColor.r, tileColor.g, tileColor.b, .5f);

                    Gizmos.DrawWireCube(selectedPosition + _tileHalfHeight, tileScale);
                }
            }

            //Gizmos.color = new Color(1 - baseColor.r, 1 - baseColor.g, 1 - baseColor.b, .5f);

            //for (int i = 0; i < _gridVertices.Count; i++)
            //{
            //    Gizmos.DrawSphere(_gridVertices[i] + _offset + Vector3.up * .05f, .1f);
            //}
        }
        /// <summary>
        /// 生成放置块
        /// </summary>
        public void InstantiateTile(Vector3 position)
        {
            tilePrefab.scale = tileScale;

            tilePrefab.material = _tileMaterial;

            tilePrefab.color = tileColor;

            Instantiate(tilePrefab, position + _tileHalfHeight, Quaternion.identity, _tileParentTransform);
        }
        /// <summary>
        /// 生成一堆放置块
        /// </summary>
        public void InstantiateTiles()
        {
            foreach (Vector3 selectedPosition in this.SelectedPositions)
            {
                tilePrefab.scale = tileScale;

                tilePrefab.material = _tileMaterial;

                tilePrefab.color = tileColor;

                Instantiate(tilePrefab, selectedPosition + _tileHalfHeight, Quaternion.identity, _tileParentTransform);
            }

            this.SelectedPositions.Clear();
        }
        /// <summary>
        /// 删除放置块
        /// </summary>
        public void DestroyTile(Collider collider)
        {
            if (collider != _baseCollider)
            {
                DestroyImmediate(collider.gameObject);
            }
        }
        /// <summary>
        /// 删除所有放置块
        /// </summary>
        public void DestroyAllTiles()
        {
            DestroyImmediate(_tileParentTransform.gameObject);
        }
    }
}