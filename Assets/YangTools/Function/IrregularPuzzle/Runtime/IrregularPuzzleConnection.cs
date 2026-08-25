using System;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 两个碎片之间的一条有效拼接边。
    /// </summary>
    [Serializable]
    public sealed class IrregularPuzzleConnection
    {
        [SerializeField] private int id;
        [SerializeField] private int firstPieceIndex;
        [SerializeField] private int secondPieceIndex;
        [SerializeField] private Vector2 anchorPosition;
        [SerializeField] private int sharedBoundaryPixelCount;
        [NonSerialized] private bool isConnected;

        /// <summary>连接的稳定编号。</summary>
        public int Id => id;
        /// <summary>连接的第一个碎片编号。</summary>
        public int FirstPieceIndex => firstPieceIndex;
        /// <summary>连接的第二个碎片编号。</summary>
        public int SecondPieceIndex => secondPieceIndex;
        /// <summary>相对完整拼图中心的连接锚点。</summary>
        public Vector2 AnchorPosition => anchorPosition;
        /// <summary>原图中共享边界的像素数量。</summary>
        public int SharedBoundaryPixelCount => sharedBoundaryPixelCount;
        /// <summary>当前运行时是否已经完成连接。</summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// 写入由编辑器工具计算出的连接数据。
        /// </summary>
        public void Configure(int connectionId, int firstIndex, int secondIndex, Vector2 anchor, int boundaryPixelCount)
        {
            id = connectionId;
            firstPieceIndex = firstIndex;
            secondPieceIndex = secondIndex;
            anchorPosition = anchor;
            sharedBoundaryPixelCount = boundaryPixelCount;
            isConnected = false;
        }

        /// <summary>
        /// 标记此连接已经生效。
        /// </summary>
        internal void Connect()
        {
            isConnected = true;
        }
    }
}
