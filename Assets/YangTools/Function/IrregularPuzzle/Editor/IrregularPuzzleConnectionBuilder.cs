#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle.Editor
{
    /// <summary>
    /// 根据碎片像素归属图计算相邻碎片之间的连接数据。
    /// </summary>
    internal static class IrregularPuzzleConnectionBuilder
    {
        /// <summary>
        /// 扫描横向和纵向相邻像素，构建每对碎片唯一的连接记录。
        /// </summary>
        internal static IrregularPuzzleConnection[] Build(int width, int height, int[] owners)
        {
            if (width <= 0 || height <= 0 || owners == null || owners.Length != width * height)
            {
                throw new ArgumentException("碎片归属图尺寸无效。", nameof(owners));
            }

            Dictionary<PiecePair, BoundaryAccumulator> boundaries = new Dictionary<PiecePair, BoundaryAccumulator>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentOwner = owners[y * width + x];
                    if (currentOwner < 0)
                    {
                        continue;
                    }

                    if (x + 1 < width)
                    {
                        AddBoundary(boundaries, currentOwner, owners[y * width + x + 1],
                            new Vector2(x + 1f, y + 0.5f));
                    }

                    if (y + 1 < height)
                    {
                        AddBoundary(boundaries, currentOwner, owners[(y + 1) * width + x],
                            new Vector2(x + 0.5f, y + 1f));
                    }
                }
            }

            List<PiecePair> pairs = new List<PiecePair>(boundaries.Keys);
            pairs.Sort();
            Vector2 sourceCenter = new Vector2(width * 0.5f, height * 0.5f);
            IrregularPuzzleConnection[] connections = new IrregularPuzzleConnection[pairs.Count];
            for (int i = 0; i < pairs.Count; i++)
            {
                PiecePair pair = pairs[i];
                BoundaryAccumulator boundary = boundaries[pair];
                IrregularPuzzleConnection connection = new IrregularPuzzleConnection();
                connection.Configure(i, pair.FirstIndex, pair.SecondIndex,
                    boundary.SumPosition / boundary.PixelCount - sourceCenter, boundary.PixelCount);
                connections[i] = connection;
            }

            return connections;
        }

        /// <summary>
        /// 将一条内部像素边界计入对应碎片对。
        /// </summary>
        private static void AddBoundary(Dictionary<PiecePair, BoundaryAccumulator> boundaries, int firstOwner,
            int secondOwner, Vector2 boundaryPosition)
        {
            if (secondOwner < 0 || firstOwner == secondOwner)
            {
                return;
            }

            PiecePair pair = new PiecePair(firstOwner, secondOwner);
            if (!boundaries.TryGetValue(pair, out BoundaryAccumulator accumulator))
            {
                accumulator = new BoundaryAccumulator();
                boundaries.Add(pair, accumulator);
            }

            accumulator.SumPosition += boundaryPosition;
            accumulator.PixelCount++;
        }

        /// <summary>
        /// 忽略方向的碎片对键。
        /// </summary>
        private readonly struct PiecePair : IComparable<PiecePair>
        {
            internal readonly int FirstIndex;
            internal readonly int SecondIndex;

            internal PiecePair(int firstIndex, int secondIndex)
            {
                FirstIndex = Mathf.Min(firstIndex, secondIndex);
                SecondIndex = Mathf.Max(firstIndex, secondIndex);
            }

            /// <summary>
            /// 按碎片编号稳定排序。
            /// </summary>
            public int CompareTo(PiecePair other)
            {
                int firstComparison = FirstIndex.CompareTo(other.FirstIndex);
                return firstComparison != 0 ? firstComparison : SecondIndex.CompareTo(other.SecondIndex);
            }

            /// <summary>
            /// 判断两个键是否代表同一碎片对。
            /// </summary>
            public override bool Equals(object obj)
            {
                return obj is PiecePair other && FirstIndex == other.FirstIndex && SecondIndex == other.SecondIndex;
            }

            /// <summary>
            /// 获取稳定哈希值。
            /// </summary>
            public override int GetHashCode()
            {
                return FirstIndex * 397 ^ SecondIndex;
            }
        }

        /// <summary>
        /// 一对碎片的共享边界统计信息。
        /// </summary>
        private sealed class BoundaryAccumulator
        {
            internal Vector2 SumPosition;
            internal int PixelCount;
        }
    }
}
#endif
