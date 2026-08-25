using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 不规则 UI 拼图的运行时控制器。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class IrregularPuzzleLevel : MonoBehaviour
    {
        [SerializeField] private IrregularPuzzlePiece[] pieces = Array.Empty<IrregularPuzzlePiece>();
        [SerializeField] private IrregularPuzzleConnection[] connections = Array.Empty<IrregularPuzzleConnection>();
        [SerializeField] private Vector2 layoutSize;
        [SerializeField] private Rect scatterArea = new Rect(-600f, -400f, 1200f, 800f);
        [SerializeField] private int randomSeed = 12345;
        [SerializeField] private float snapDistance = 32f;
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();

        private readonly Dictionary<int, IrregularPuzzlePiece> pieceByIndex = new Dictionary<int, IrregularPuzzlePiece>();
        private readonly List<IrregularPuzzleGroup> groups = new List<IrregularPuzzleGroup>();
        private RectTransform rectTransform;
        private Rect canvasArea;
        private bool hasCanvasArea;
        private IrregularPuzzleGroup draggingGroup;
        private Vector2 dragOffset;
        private bool hasCompleted;

        /// <summary>
        /// 拼图完整布局的像素尺寸
        /// </summary>
        public Vector2 LayoutSize => layoutSize;
        /// <summary>
        /// 全部拼图完成时触发的事件
        /// </summary>
        public UnityEvent OnCompleted => onCompleted;

        /// <summary>
        /// 初始化并将所有碎片随机散开
        /// </summary>
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            hasCanvasArea = TryGetCanvasArea(out canvasArea);
            CreateGroups();
            TryComplete();
        }

        /// <summary>
        /// 写入编辑器生成的关卡数据
        /// </summary>
        public void Configure(IrregularPuzzlePiece[] generatedPieces, IrregularPuzzleConnection[] generatedConnections,
            Vector2 generatedLayoutSize)
        {
            pieces = generatedPieces ?? Array.Empty<IrregularPuzzlePiece>();
            connections = generatedConnections ?? Array.Empty<IrregularPuzzleConnection>();
            layoutSize = generatedLayoutSize;
            scatterArea = new Rect(-layoutSize.x * 0.75f, -layoutSize.y * 0.75f, layoutSize.x * 1.5f, layoutSize.y * 1.5f);
            snapDistance = Mathf.Max(16f, Mathf.Min(layoutSize.x, layoutSize.y) * 0.04f);
        }

        /// <summary>
        /// 开始拖动指定碎片所在的拼图组
        /// </summary>
        internal void BeginDrag(IrregularPuzzlePiece piece, PointerEventData eventData)
        {
            if (piece == null || piece.Group == null || rectTransform == null)
            {
                return;
            }

            draggingGroup = piece.Group;
            draggingGroup.Root.SetAsLastSibling();
            if (TryGetPointerPosition(eventData, out Vector2 pointerPosition))
            {
                dragOffset = draggingGroup.Root.anchoredPosition - pointerPosition;
            }
        }

        /// <summary>
        /// 持续更新拖拽组的位置
        /// </summary>
        internal void Drag(PointerEventData eventData)
        {
            if (draggingGroup == null || !TryGetPointerPosition(eventData, out Vector2 pointerPosition))
            {
                return;
            }

            draggingGroup.Root.anchoredPosition = pointerPosition + dragOffset;
        }

        /// <summary>
        /// 尝试吸附当前拖拽组到可连接的其他组。
        /// </summary>
        internal void EndDrag()
        {
            if (draggingGroup == null)
            {
                return;
            }

            TrySnap(draggingGroup);
            draggingGroup = null;
        }

        /// <summary>
        /// 为每个碎片建立独立的初始逻辑组。
        /// </summary>
        private void CreateGroups()
        {
            pieceByIndex.Clear();
            groups.Clear();
            System.Random random = new System.Random(randomSeed);
            for (int i = 0; i < pieces.Length; i++)
            {
                IrregularPuzzlePiece piece = pieces[i];
                if (piece == null || pieceByIndex.ContainsKey(piece.PieceIndex))
                {
                    continue;
                }

                pieceByIndex.Add(piece.PieceIndex, piece);
                RectTransform groupRoot = CreateGroupRoot(piece.PieceIndex);
                IrregularPuzzleGroup group = new IrregularPuzzleGroup(groupRoot);
                piece.RectTransform.SetParent(groupRoot, false);
                piece.RectTransform.anchoredPosition = piece.SolvedPosition;
                groupRoot.anchoredPosition = GetRandomPosition(random, piece.RectTransform.rect.size) - piece.SolvedPosition;
                group.AddPiece(piece);
                groups.Add(group);
            }
        }

        /// <summary>
        /// 创建仅用于整体移动的空 UI 根节点。
        /// </summary>
        private RectTransform CreateGroupRoot(int pieceIndex)
        {
            GameObject groupObject = new GameObject($"PuzzleGroup_{pieceIndex}", typeof(RectTransform));
            RectTransform groupRoot = groupObject.GetComponent<RectTransform>();
            groupRoot.SetParent(rectTransform, false);
            groupRoot.anchorMin = new Vector2(0.5f, 0.5f);
            groupRoot.anchorMax = new Vector2(0.5f, 0.5f);
            groupRoot.pivot = new Vector2(0.5f, 0.5f);
            groupRoot.sizeDelta = Vector2.zero;
            return groupRoot;
        }

        /// <summary>
        /// 生成稳定的初始散开坐标。
        /// </summary>
        private Vector2 GetRandomPosition(System.Random random, Vector2 pieceSize)
        {
            Rect availableArea = GetAvailableScatterArea();
            float x = GetRandomCoordinate(random, availableArea.xMin, availableArea.xMax, pieceSize.x * 0.5f);
            float y = GetRandomCoordinate(random, availableArea.yMin, availableArea.yMax, pieceSize.y * 0.5f);
            return new Vector2(x, y);
        }

        /// <summary>
        /// 获取配置散开区与 Canvas 可用区域的交集。
        /// </summary>
        private Rect GetAvailableScatterArea()
        {
            if (!hasCanvasArea)
            {
                return scatterArea;
            }

            float xMin = Mathf.Max(scatterArea.xMin, canvasArea.xMin);
            float yMin = Mathf.Max(scatterArea.yMin, canvasArea.yMin);
            float xMax = Mathf.Min(scatterArea.xMax, canvasArea.xMax);
            float yMax = Mathf.Min(scatterArea.yMax, canvasArea.yMax);
            return xMax > xMin && yMax > yMin
                ? Rect.MinMaxRect(xMin, yMin, xMax, yMax)
                : canvasArea;
        }

        /// <summary>
        /// 在给定范围中生成一个可容纳碎片尺寸的随机中心坐标。
        /// </summary>
        private static float GetRandomCoordinate(System.Random random, float min, float max, float halfSize)
        {
            float minimumCenter = min + halfSize;
            float maximumCenter = max - halfSize;
            if (maximumCenter <= minimumCenter)
            {
                return (min + max) * 0.5f;
            }

            return minimumCenter + (float)random.NextDouble() * (maximumCenter - minimumCenter);
        }

        /// <summary>
        /// 将父 Canvas 的四个世界角转换为当前关卡的本地可用区域。
        /// </summary>
        private bool TryGetCanvasArea(out Rect result)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null || !(canvas.transform is RectTransform canvasTransform))
            {
                result = default;
                return false;
            }

            Vector3[] corners = new Vector3[4];
            canvasTransform.GetWorldCorners(corners);
            Vector3 firstCorner = rectTransform.InverseTransformPoint(corners[0]);
            float xMin = firstCorner.x;
            float xMax = firstCorner.x;
            float yMin = firstCorner.y;
            float yMax = firstCorner.y;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 localCorner = rectTransform.InverseTransformPoint(corners[i]);
                xMin = Mathf.Min(xMin, localCorner.x);
                xMax = Mathf.Max(xMax, localCorner.x);
                yMin = Mathf.Min(yMin, localCorner.y);
                yMax = Mathf.Max(yMax, localCorner.y);
            }

            result = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            return result.width > 0f && result.height > 0f;
        }

        /// <summary>
        /// 在合法连接中寻找距离最近的可吸附目标。
        /// </summary>
        private void TrySnap(IrregularPuzzleGroup sourceGroup)
        {
            IrregularPuzzleConnection bestConnection = null;
            IrregularPuzzleGroup targetGroup = null;
            float bestDistanceSquared = snapDistance * snapDistance;
            for (int i = 0; i < connections.Length; i++)
            {
                IrregularPuzzleConnection connection = connections[i];
                if (connection == null || connection.IsConnected ||
                    !pieceByIndex.TryGetValue(connection.FirstPieceIndex, out IrregularPuzzlePiece firstPiece) ||
                    !pieceByIndex.TryGetValue(connection.SecondPieceIndex, out IrregularPuzzlePiece secondPiece))
                {
                    continue;
                }

                IrregularPuzzleGroup firstGroup = firstPiece.Group;
                IrregularPuzzleGroup secondGroup = secondPiece.Group;
                if (firstGroup == secondGroup || (firstGroup != sourceGroup && secondGroup != sourceGroup))
                {
                    continue;
                }

                IrregularPuzzleGroup otherGroup = firstGroup == sourceGroup ? secondGroup : firstGroup;
                Vector2 sourceAnchor = sourceGroup.Root.anchoredPosition + connection.AnchorPosition;
                Vector2 targetAnchor = otherGroup.Root.anchoredPosition + connection.AnchorPosition;
                float distanceSquared = (sourceAnchor - targetAnchor).sqrMagnitude;
                if (distanceSquared > bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                bestConnection = connection;
                targetGroup = otherGroup;
            }

            if (bestConnection == null || targetGroup == null)
            {
                return;
            }

            Vector2 offset = targetGroup.Root.anchoredPosition - sourceGroup.Root.anchoredPosition;
            sourceGroup.Root.anchoredPosition += offset;
            targetGroup.Absorb(sourceGroup);
            groups.Remove(sourceGroup);
            bestConnection.Connect();
            Destroy(sourceGroup.Root.gameObject);
            TryComplete();
        }

        /// <summary>
        /// 将屏幕指针坐标转换为关卡本地 UI 坐标。
        /// </summary>
        private bool TryGetPointerPosition(PointerEventData eventData, out Vector2 pointerPosition)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                eventData.pressEventCamera, out pointerPosition);
        }

        /// <summary>
        /// 在全部碎片合并后触发完成事件。
        /// </summary>
        private void TryComplete()
        {
            if (hasCompleted || groups.Count != 1 || pieceByIndex.Count != pieces.Length)
            {
                return;
            }
                
            Debug.LogError("关卡完成");
            hasCompleted = true;
            onCompleted?.Invoke();
        }
    }
}
