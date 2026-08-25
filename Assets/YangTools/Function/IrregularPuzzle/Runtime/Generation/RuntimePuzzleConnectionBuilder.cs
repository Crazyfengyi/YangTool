using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 根据运行时像素归属图构建碎片连接关系
    /// </summary>
    internal static class RuntimePuzzleConnectionBuilder
    {
        /// <summary>
        /// 扫描像素共享边界，生成每对碎片唯一的连接
        /// </summary>
        internal static IrregularPuzzleConnection[] Build(int width, int height, int[] owners)
        {
            if (owners == null || owners.Length != width * height) throw new ArgumentException("碎片归属图无效。", nameof(owners));
            Dictionary<PiecePair, Boundary> boundaries = new Dictionary<PiecePair, Boundary>();
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int owner = owners[y * width + x];
                if (owner < 0) continue;
                if (x + 1 < width) Add(boundaries, owner, owners[y * width + x + 1], new Vector2(x + 1f, y + 0.5f));
                if (y + 1 < height) Add(boundaries, owner, owners[(y + 1) * width + x], new Vector2(x + 0.5f, y + 1f));
            }

            List<PiecePair> pairs = new List<PiecePair>(boundaries.Keys);
            pairs.Sort();
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            IrregularPuzzleConnection[] connections = new IrregularPuzzleConnection[pairs.Count];
            for (int i = 0; i < pairs.Count; i++)
            {
                Boundary boundary = boundaries[pairs[i]];
                IrregularPuzzleConnection connection = new IrregularPuzzleConnection();
                connection.Configure(i, pairs[i].first, pairs[i].second, boundary.sum / boundary.count - center, boundary.count);
                connections[i] = connection;
            }

            return connections;
        }

        /// <summary>记录一条内部共享边界。</summary>
        private static void Add(Dictionary<PiecePair, Boundary> boundaries, int first, int second, Vector2 position)
        {
            if (second < 0 || first == second) return;
            PiecePair pair = new PiecePair(first, second);
            if (!boundaries.TryGetValue(pair, out Boundary boundary))
            {
                boundary = new Boundary();
                boundaries.Add(pair, boundary);
            }

            boundary.sum += position;
            boundary.count++;
        }

        /// <summary>
        /// 无方向的碎片对键
        /// </summary>
        private readonly struct PiecePair : IComparable<PiecePair>
        {
            internal readonly int first;
            internal readonly int second;
            internal PiecePair(int firstIndex, int secondIndex) { first = Mathf.Min(firstIndex, secondIndex); second = Mathf.Max(firstIndex, secondIndex); }
            public int CompareTo(PiecePair other) { int result = first.CompareTo(other.first); return result != 0 ? result : second.CompareTo(other.second); }
            public override bool Equals(object obj) { return obj is PiecePair other && first == other.first && second == other.second; }
            public override int GetHashCode() { return first * 397 ^ second; }
        }

        /// <summary>
        /// 一对碎片共享边界的累计信息
        /// </summary>
        private sealed class Boundary { internal Vector2 sum; internal int count; }
    }
}
