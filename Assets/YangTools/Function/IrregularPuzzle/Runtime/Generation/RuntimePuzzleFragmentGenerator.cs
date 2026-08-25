using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 运行时不规则碎片的切割算法
    /// </summary>
    public enum RuntimePuzzleFragmentAlgorithm
    {
        Voronoi,
        JitteredGrid,
        NoiseBoundary,
        AlphaContour,
        DelaunayVoronoi,
    }

    /// <summary>
    /// 运行时碎片切割参数
    /// </summary>
    [Serializable]
    public sealed class RuntimePuzzleFragmentSettings
    {
        [SerializeField] private RuntimePuzzleFragmentAlgorithm algorithm = RuntimePuzzleFragmentAlgorithm.Voronoi;
        [SerializeField] private int maxFragmentCount = 16;
        [SerializeField] private int seed = 12345;
        [SerializeField] private int gapPixels = 1;
        [SerializeField] private float gridJitter = 0.35f;
        [SerializeField] private float noiseScale = 64f;
        [SerializeField] private float noiseStrength = 12f;
        [SerializeField] private byte alphaThreshold = 1;

        /// <summary>切割算法。</summary>
        public RuntimePuzzleFragmentAlgorithm Algorithm { get => algorithm; set => algorithm = value; }
        /// <summary>目标最大碎片数。</summary>
        public int MaxFragmentCount { get => maxFragmentCount; set => maxFragmentCount = value; }
        /// <summary>稳定随机种子。</summary>
        public int Seed { get => seed; set => seed = value; }
        /// <summary>碎片边缘收缩像素数。</summary>
        public int GapPixels { get => gapPixels; set => gapPixels = value; }
        /// <summary>抖动网格的边界抖动幅度。</summary>
        public float GridJitter { get => gridJitter; set => gridJitter = value; }
        /// <summary>噪声边界采样尺度。</summary>
        public float NoiseScale { get => noiseScale; set => noiseScale = value; }
        /// <summary>噪声边界扭曲强度。</summary>
        public float NoiseStrength { get => noiseStrength; set => noiseStrength = value; }
        /// <summary>透明轮廓模式的有效 Alpha 阈值。</summary>
        public byte AlphaThreshold { get => alphaThreshold; set => alphaThreshold = value; }
    }

    /// <summary>运行时切图得到的像素归属与碎片范围。</summary>
    internal sealed class RuntimePuzzleFragmentResult
    {
        internal int[] owners;
        internal int[] connectionOwners;
        internal bool[] retainedPixels;
        internal RectInt[] fragmentRects;
    }

    /// <summary>不依赖 UnityEditor 的运行时像素切图实现。</summary>
    internal static class RuntimePuzzleFragmentGenerator
    {
        /// <summary>按配置生成碎片像素归属。</summary>
        internal static RuntimePuzzleFragmentResult Generate(int width, int height, Color32[] pixels,
            RuntimePuzzleFragmentSettings settings)
        {
            Validate(width, height, pixels, settings);
            bool[] validPixels = CreateValidMask(width, height, pixels, settings);
            int[] rawOwners;
            switch (settings.Algorithm)
            {
                case RuntimePuzzleFragmentAlgorithm.JitteredGrid:
                    rawOwners = GenerateJitteredGrid(width, height, settings);
                    break;
                case RuntimePuzzleFragmentAlgorithm.NoiseBoundary:
                    rawOwners = GenerateNearestOwners(width, height, validPixels, settings, true, false);
                    break;
                case RuntimePuzzleFragmentAlgorithm.DelaunayVoronoi:
                    rawOwners = GenerateNearestOwners(width, height, validPixels, settings, false, true);
                    break;
                default:
                    rawOwners = GenerateNearestOwners(width, height, validPixels, settings, false, false);
                    break;
            }

            return BuildResult(width, height, rawOwners, validPixels, settings.GapPixels);
        }

        /// <summary>验证切图参数。</summary>
        private static void Validate(int width, int height, Color32[] pixels, RuntimePuzzleFragmentSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (pixels == null || pixels.Length != width * height) throw new ArgumentException("图片像素数据无效。", nameof(pixels));
            if (settings.MaxFragmentCount < 2) throw new ArgumentOutOfRangeException(nameof(settings.MaxFragmentCount));
            if (settings.GapPixels < 0) throw new ArgumentOutOfRangeException(nameof(settings.GapPixels));
        }

        /// <summary>创建算法所需的有效像素掩码。</summary>
        private static bool[] CreateValidMask(int width, int height, Color32[] pixels, RuntimePuzzleFragmentSettings settings)
        {
            bool[] valid = new bool[width * height];
            bool useAlpha = settings.Algorithm == RuntimePuzzleFragmentAlgorithm.AlphaContour;
            int validCount = 0;
            for (int i = 0; i < valid.Length; i++)
            {
                valid[i] = !useAlpha || pixels[i].a >= settings.AlphaThreshold;
                if (valid[i]) validCount++;
            }

            if (validCount == 0) throw new InvalidOperationException("图片没有可用于切割的像素。");
            return valid;
        }

        /// <summary>通过最近种子点生成 Voronoi 类碎片。</summary>
        private static int[] GenerateNearestOwners(int width, int height, bool[] valid, RuntimePuzzleFragmentSettings settings,
            bool useNoise, bool useDelaunayRelaxation)
        {
            Vector2Int[] seeds = CreateSeeds(width, height, valid, Mathf.Min(settings.MaxFragmentCount, CountTrue(valid)), settings.Seed);
            if (useDelaunayRelaxation) RelaxSeedsByNeighbors(width, height, seeds);
            int[] owners = new int[width * height];
            Fill(owners, -1);
            float scale = Mathf.Max(1f, settings.NoiseScale);
            float offsetX = settings.Seed % 997;
            float offsetY = (settings.Seed / 997) % 991;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = ToIndex(x, y, width);
                    if (!valid[index]) continue;
                    float sampleX = x;
                    float sampleY = y;
                    if (useNoise)
                    {
                        sampleX += (Mathf.PerlinNoise((x + offsetX) / scale, (y + offsetY) / scale) - 0.5f) * settings.NoiseStrength;
                        sampleY += (Mathf.PerlinNoise((x + offsetX + 313f) / scale, (y + offsetY + 719f) / scale) - 0.5f) * settings.NoiseStrength;
                    }

                    int owner = 0;
                    float distance = float.MaxValue;
                    for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
                    {
                        float deltaX = sampleX - seeds[seedIndex].x;
                        float deltaY = sampleY - seeds[seedIndex].y;
                        float candidate = deltaX * deltaX + deltaY * deltaY;
                        if (candidate < distance)
                        {
                            distance = candidate;
                            owner = seedIndex;
                        }
                    }

                    owners[index] = owner;
                }
            }

            return owners;
        }

        /// <summary>生成共享边界的抖动网格碎片。</summary>
        private static int[] GenerateJitteredGrid(int width, int height, RuntimePuzzleFragmentSettings settings)
        {
            int columns = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(settings.MaxFragmentCount * width / (float)height)));
            int rows = Mathf.Max(1, settings.MaxFragmentCount / columns);
            System.Random random = new System.Random(settings.Seed);
            float cellWidth = width / (float)columns;
            float cellHeight = height / (float)rows;
            float[,] verticalLines = new float[rows + 1, columns + 1];
            for (int row = 0; row <= rows; row++)
            for (int column = 0; column <= columns; column++)
            {
                float position = column * cellWidth;
                if (column > 0 && column < columns)
                {
                    position += ((float)random.NextDouble() * 2f - 1f) * Mathf.Clamp01(settings.GridJitter) * cellWidth * 0.45f;
                }

                verticalLines[row, column] = position;
            }

            int[] owners = new int[width * height];
            for (int y = 0; y < height; y++)
            {
                int row = Mathf.Min(rows - 1, y * rows / height);
                float rowT = (y - row * cellHeight) / cellHeight;
                for (int x = 0; x < width; x++)
                {
                    int column = 0;
                    while (column + 1 < columns && x >= Mathf.Lerp(verticalLines[row, column + 1], verticalLines[row + 1, column + 1], rowT)) column++;
                    owners[ToIndex(x, y, width)] = row * columns + column;
                }
            }

            return owners;
        }

        /// <summary>生成稳定且分散的种子点。</summary>
        private static Vector2Int[] CreateSeeds(int width, int height, bool[] valid, int count, int seed)
        {
            System.Random random = new System.Random(seed);
            Vector2Int[] seeds = new Vector2Int[count];
            HashSet<int> used = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                int bestIndex = -1;
                int bestDistance = -1;
                for (int candidate = 0; candidate < 128; candidate++)
                {
                    int index = random.Next(valid.Length);
                    if (!valid[index] || used.Contains(index)) continue;
                    int minDistance = int.MaxValue;
                    for (int seedIndex = 0; seedIndex < i; seedIndex++)
                    {
                        int deltaX = index % width - seeds[seedIndex].x;
                        int deltaY = index / width - seeds[seedIndex].y;
                        minDistance = Mathf.Min(minDistance, deltaX * deltaX + deltaY * deltaY);
                    }

                    if (minDistance > bestDistance)
                    {
                        bestDistance = minDistance;
                        bestIndex = index;
                    }
                }

                if (bestIndex < 0) bestIndex = FindValidIndex(valid, used);
                if (bestIndex < 0) throw new InvalidOperationException("无法创建足够的碎片种子。");
                used.Add(bestIndex);
                seeds[i] = new Vector2Int(bestIndex % width, bestIndex / width);
            }

            return seeds;
        }

        /// <summary>使用最近邻拓扑执行一次轻量的 Delaunay 风格种子松弛。</summary>
        private static void RelaxSeedsByNeighbors(int width, int height, Vector2Int[] seeds)
        {
            for (int i = 0; i < seeds.Length; i++)
            {
                Vector2 average = Vector2.zero;
                int neighborCount = 0;
                for (int j = 0; j < seeds.Length; j++)
                {
                    if (i == j) continue;
                    average += seeds[j];
                    neighborCount++;
                }

                if (neighborCount == 0) continue;
                Vector2 moved = seeds[i] + ((Vector2)seeds[i] - average / neighborCount) * 0.12f;
                seeds[i] = new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(moved.x), 0, width - 1),
                    Mathf.Clamp(Mathf.RoundToInt(moved.y), 0, height - 1));
            }
        }

        /// <summary>将归属图转换为连续碎片编号、可见掩码与裁剪矩形。</summary>
        private static RuntimePuzzleFragmentResult BuildResult(int width, int height, int[] rawOwners, bool[] valid, int gapPixels)
        {
            bool[] retained = CreateRetainedMask(width, height, rawOwners, valid, gapPixels);
            Dictionary<int, int> remap = new Dictionary<int, int>();
            for (int i = 0; i < rawOwners.Length; i++)
            {
                if (retained[i] && rawOwners[i] >= 0 && !remap.ContainsKey(rawOwners[i])) remap.Add(rawOwners[i], remap.Count);
            }

            if (remap.Count == 0) throw new InvalidOperationException("当前参数没有生成有效碎片。");
            int[] owners = new int[rawOwners.Length];
            int[] connectionOwners = new int[rawOwners.Length];
            Fill(owners, -1);
            Fill(connectionOwners, -1);
            for (int i = 0; i < rawOwners.Length; i++)
            {
                if (rawOwners[i] >= 0 && remap.TryGetValue(rawOwners[i], out int mappedOwner))
                {
                    connectionOwners[i] = mappedOwner;
                    if (retained[i]) owners[i] = mappedOwner;
                }
            }

            return new RuntimePuzzleFragmentResult
            {
                owners = owners,
                connectionOwners = connectionOwners,
                retainedPixels = retained,
                fragmentRects = CalculateRects(width, height, remap.Count, owners),
            };
        }

        /// <summary>从内部边界向碎片内侧收缩可见像素。</summary>
        private static bool[] CreateRetainedMask(int width, int height, int[] owners, bool[] valid, int gap)
        {
            bool[] retained = new bool[valid.Length];
            if (gap == 0)
            {
                Array.Copy(valid, retained, valid.Length);
                return retained;
            }

            int[] distance = new int[valid.Length];
            Fill(distance, -1);
            Queue<int> queue = new Queue<int>();
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = ToIndex(x, y, width);
                if (valid[index] && IsBoundary(x, y, width, height, owners, valid))
                {
                    distance[index] = 0;
                    queue.Enqueue(index);
                }
            }

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                if (distance[index] >= gap - 1) continue;
                int x = index % width;
                int y = index / width;
                ExpandDistance(x - 1, y, width, height, owners, valid, distance, queue, index);
                ExpandDistance(x + 1, y, width, height, owners, valid, distance, queue, index);
                ExpandDistance(x, y - 1, width, height, owners, valid, distance, queue, index);
                ExpandDistance(x, y + 1, width, height, owners, valid, distance, queue, index);
            }

            for (int i = 0; i < retained.Length; i++) retained[i] = valid[i] && (distance[i] < 0 || distance[i] >= gap);
            return retained;
        }

        /// <summary>判断像素是否接触另一块碎片。</summary>
        private static bool IsBoundary(int x, int y, int width, int height, int[] owners, bool[] valid)
        {
            int owner = owners[ToIndex(x, y, width)];
            return HasDifferentNeighbor(x - 1, y, width, height, owners, valid, owner) ||
                   HasDifferentNeighbor(x + 1, y, width, height, owners, valid, owner) ||
                   HasDifferentNeighbor(x, y - 1, width, height, owners, valid, owner) ||
                   HasDifferentNeighbor(x, y + 1, width, height, owners, valid, owner);
        }

        /// <summary>判断相邻像素是否属于另一有效碎片。</summary>
        private static bool HasDifferentNeighbor(int x, int y, int width, int height, int[] owners, bool[] valid, int owner)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return false;
            int index = ToIndex(x, y, width);
            return valid[index] && owners[index] != owner;
        }

        /// <summary>在同一归属区域内扩展边界距离。</summary>
        private static void ExpandDistance(int x, int y, int width, int height, int[] owners, bool[] valid, int[] distance,
            Queue<int> queue, int sourceIndex)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;
            int index = ToIndex(x, y, width);
            if (!valid[index] || distance[index] >= 0 || owners[index] != owners[sourceIndex]) return;
            distance[index] = distance[sourceIndex] + 1;
            queue.Enqueue(index);
        }

        /// <summary>计算每个碎片的像素包围矩形。</summary>
        private static RectInt[] CalculateRects(int width, int height, int count, int[] owners)
        {
            int[] minX = new int[count];
            int[] minY = new int[count];
            int[] maxX = new int[count];
            int[] maxY = new int[count];
            Fill(minX, width); Fill(minY, height); Fill(maxX, -1); Fill(maxY, -1);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int owner = owners[ToIndex(x, y, width)];
                if (owner < 0) continue;
                minX[owner] = Mathf.Min(minX[owner], x); minY[owner] = Mathf.Min(minY[owner], y);
                maxX[owner] = Mathf.Max(maxX[owner], x); maxY[owner] = Mathf.Max(maxY[owner], y);
            }

            RectInt[] rects = new RectInt[count];
            for (int i = 0; i < count; i++) rects[i] = new RectInt(minX[i], minY[i], maxX[i] - minX[i] + 1, maxY[i] - minY[i] + 1);
            return rects;
        }

        private static int CountTrue(bool[] values) { int count = 0; for (int i = 0; i < values.Length; i++) if (values[i]) count++; return count; }
        private static int FindValidIndex(bool[] valid, HashSet<int> used) { for (int i = 0; i < valid.Length; i++) if (valid[i] && !used.Contains(i)) return i; return -1; }
        private static int ToIndex(int x, int y, int width) { return y * width + x; }
        private static void Fill<T>(T[] values, T value) { for (int i = 0; i < values.Length; i++) values[i] = value; }
    }
}
