#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.EditorStudio.ImageFragmentTool
{
    /// <summary>图片切割算法的统一像素结果。</summary>
    internal sealed class VoronoiFragmentResult
    {
        internal int[] owners;
        internal bool[] retainedPixels;
        internal Vector2Int[] seeds;
        internal RectInt[] fragmentRects;
    }

    /// <summary>兼容原有 Voronoi 调用的生成入口。</summary>
    internal static class VoronoiImageFragmentGenerator
    {
        /// <summary>生成基础 Voronoi 碎片。</summary>
        internal static VoronoiFragmentResult Generate(int width, int height, int fragmentCount, int seed, int gapPixels)
        {
            return ImageFragmentAlgorithmGenerator.Generate(width, height, null, new ImageFragmentAlgorithmSettings
            {
                algorithm = ImageFragmentAlgorithm.Voronoi,
                maxFragmentCount = fragmentCount,
                seed = seed,
                gapPixels = gapPixels,
            });
        }
    }

    /// <summary>根据不同算法生成图片碎片的像素归属。</summary>
    internal static class ImageFragmentAlgorithmGenerator
    {
        /// <summary>调度所选算法并构建统一导出结果。</summary>
        internal static VoronoiFragmentResult Generate(int width, int height, Color32[] pixels, ImageFragmentAlgorithmSettings settings)
        {
            ValidateArguments(width, height, pixels, settings);
            bool[] validPixels = CreateFullMask(width * height);
            int[] owners;
            Vector2Int[] seeds = Array.Empty<Vector2Int>();
            switch (settings.algorithm)
            {
                case ImageFragmentAlgorithm.JitteredGrid:
                    owners = GenerateJitteredGrid(width, height, settings, out seeds);
                    break;
                case ImageFragmentAlgorithm.NoiseBoundary:
                    owners = GenerateVoronoiOwners(width, height, validPixels, settings, true, out seeds);
                    break;
                case ImageFragmentAlgorithm.AlphaContour:
                    validPixels = CreateAlphaMask(pixels, settings.alphaThreshold);
                    owners = GenerateVoronoiOwners(width, height, validPixels, settings, false, out seeds);
                    break;
                case ImageFragmentAlgorithm.DelaunayVoronoi:
                    owners = GenerateDelaunayVoronoi(width, height, validPixels, settings, out seeds);
                    break;
                default:
                    owners = GenerateVoronoiOwners(width, height, validPixels, settings, false, out seeds);
                    break;
            }

            return BuildResult(width, height, owners, validPixels, settings.gapPixels, seeds);
        }

        /// <summary>验证生成输入。</summary>
        private static void ValidateArguments(int width, int height, Color32[] pixels, ImageFragmentAlgorithmSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width), "图片尺寸必须大于零。");
            if (settings.maxFragmentCount < 2) throw new ArgumentOutOfRangeException(nameof(settings.maxFragmentCount), "最大碎片数量至少为 2。");
            if (settings.gapPixels < 0) throw new ArgumentOutOfRangeException(nameof(settings.gapPixels), "碎片间隙不能小于零。");
            bool requiresPixels = settings.algorithm == ImageFragmentAlgorithm.AlphaContour;
            if (requiresPixels && (pixels == null || pixels.Length != width * height))
                throw new ArgumentException("当前算法需要与图片尺寸一致的像素数据。", nameof(pixels));
        }

        /// <summary>生成完整矩形范围内的有效像素掩码。</summary>
        private static bool[] CreateFullMask(int length)
        {
            bool[] mask = new bool[length];
            Fill(mask, true);
            return mask;
        }

        /// <summary>根据 Alpha 阈值生成轮廓有效掩码。</summary>
        private static bool[] CreateAlphaMask(Color32[] pixels, byte alphaThreshold)
        {
            bool[] mask = new bool[pixels.Length];
            int validCount = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                mask[i] = pixels[i].a >= alphaThreshold;
                if (mask[i]) validCount++;
            }
            if (validCount == 0) throw new InvalidOperationException("图片没有满足 Alpha 阈值的可切割像素。");
            return mask;
        }

        /// <summary>生成基础或噪声扭曲的 Voronoi 区域。</summary>
        private static int[] GenerateVoronoiOwners(int width, int height, bool[] validPixels, ImageFragmentAlgorithmSettings settings, bool useNoise, out Vector2Int[] seeds)
        {
            int seedCount = Mathf.Min(settings.maxFragmentCount, CountValidPixels(validPixels));
            seeds = CreateSeeds(width, height, validPixels, seedCount, settings.seed);
            return AssignNearestSeed(width, height, validPixels, seeds, settings, useNoise);
        }

        /// <summary>使用共享竖向边界生成抖动网格碎片。</summary>
        private static int[] GenerateJitteredGrid(int width, int height, ImageFragmentAlgorithmSettings settings, out Vector2Int[] seeds)
        {
            GetGridSize(width, height, settings.maxFragmentCount, out int columns, out int rows);
            System.Random random = new System.Random(settings.seed);
            float[,] verticalLines = new float[rows + 1, columns + 1];
            float cellWidth = width / (float)columns;
            float cellHeight = height / (float)rows;
            float jitter = Mathf.Clamp01(settings.gridJitter) * cellWidth * 0.45f;
            for (int row = 0; row <= rows; row++)
            for (int column = 0; column <= columns; column++)
            {
                float position = column * cellWidth;
                if (column > 0 && column < columns) position += ((float)random.NextDouble() * 2f - 1f) * jitter;
                verticalLines[row, column] = position;
            }

            int[] owners = new int[width * height];
            seeds = new Vector2Int[rows * columns];
            for (int y = 0; y < height; y++)
            {
                int row = Mathf.Min(rows - 1, y * rows / height);
                float rowT = (y - row * cellHeight) / cellHeight;
                for (int x = 0; x < width; x++)
                    owners[ToIndex(x, y, width)] = row * columns + FindGridColumn(x, row, rowT, columns, verticalLines);
            }
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                seeds[row * columns + column] = new Vector2Int(Mathf.Clamp(Mathf.RoundToInt((column + 0.5f) * cellWidth), 0, width - 1), Mathf.Clamp(Mathf.RoundToInt((row + 0.5f) * cellHeight), 0, height - 1));
            return owners;
        }

        /// <summary>根据图片比例和最大数量计算网格行列。</summary>
        private static void GetGridSize(int width, int height, int maxCount, out int columns, out int rows)
        {
            columns = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(maxCount * width / (float)height)));
            rows = Mathf.Max(1, maxCount / columns);
            while (columns * (rows + 1) <= maxCount) rows++;
        }

        /// <summary>按当前行的抖动边界寻找所属列。</summary>
        private static int FindGridColumn(int x, int row, float rowT, int columns, float[,] verticalLines)
        {
            for (int column = 1; column < columns; column++)
                if (x < Mathf.Lerp(verticalLines[row, column], verticalLines[row + 1, column], rowT)) return column - 1;
            return columns - 1;
        }

        /// <summary>生成经单次质心松弛的 Delaunay-Voronoi 块状碎片。</summary>
        private static int[] GenerateDelaunayVoronoi(int width, int height, bool[] validPixels, ImageFragmentAlgorithmSettings settings, out Vector2Int[] seeds)
        {
            int seedCount = Mathf.Min(settings.maxFragmentCount, CountValidPixels(validPixels));
            seeds = CreateSeeds(width, height, validPixels, seedCount, settings.seed);
            RelaxSeedsByDelaunayNeighbors(width, height, seeds);
            return AssignNearestSeed(width, height, validPixels, seeds, settings, false);
        }

        /// <summary>根据 Delaunay 三角网邻接关系将种子轻微向外推开。</summary>
        private static void RelaxSeedsByDelaunayNeighbors(int width, int height, Vector2Int[] seeds)
        {
            List<Vector2> points = new List<Vector2>(seeds.Length + 3);
            for (int i = 0; i < seeds.Length; i++) points.Add(seeds[i]);
            List<DelaunayTriangle> triangles = Triangulate(points);
            List<HashSet<int>> neighbors = new List<HashSet<int>>(seeds.Length);
            for (int i = 0; i < seeds.Length; i++) neighbors.Add(new HashSet<int>());
            for (int i = 0; i < triangles.Count; i++)
            {
                DelaunayTriangle triangle = triangles[i];
                AddDelaunayNeighborPair(triangle.a, triangle.b, seeds.Length, neighbors);
                AddDelaunayNeighborPair(triangle.b, triangle.c, seeds.Length, neighbors);
                AddDelaunayNeighborPair(triangle.c, triangle.a, seeds.Length, neighbors);
            }

            HashSet<int> usedPositions = new HashSet<int>();
            for (int i = 0; i < seeds.Length; i++)
            {
                Vector2Int original = seeds[i];
                if (neighbors[i].Count == 0)
                {
                    usedPositions.Add(ToIndex(original.x, original.y, width));
                    continue;
                }

                Vector2 average = Vector2.zero;
                foreach (int neighborIndex in neighbors[i]) average += seeds[neighborIndex];
                average /= neighbors[i].Count;
                Vector2 candidate = original + ((Vector2)original - average) * 0.12f;
                Vector2Int moved = new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(candidate.x), 0, width - 1),
                    Mathf.Clamp(Mathf.RoundToInt(candidate.y), 0, height - 1));
                int movedIndex = ToIndex(moved.x, moved.y, width);
                if (!usedPositions.Contains(movedIndex))
                {
                    seeds[i] = moved;
                    usedPositions.Add(movedIndex);
                }
                else
                {
                    usedPositions.Add(ToIndex(original.x, original.y, width));
                }
            }
        }

        /// <summary>构建二维种子点的 Bowyer-Watson Delaunay 三角网。</summary>
        private static List<DelaunayTriangle> Triangulate(List<Vector2> points)
        {
            int originalCount = points.Count;
            float extent = 1f;
            for (int i = 0; i < points.Count; i++) extent = Mathf.Max(extent, Mathf.Max(Mathf.Abs(points[i].x), Mathf.Abs(points[i].y)));
            points.Add(new Vector2(-extent * 8f, -extent * 4f));
            points.Add(new Vector2(0f, extent * 8f));
            points.Add(new Vector2(extent * 8f, -extent * 4f));

            List<DelaunayTriangle> triangles = new List<DelaunayTriangle>
            {
                new DelaunayTriangle(originalCount, originalCount + 1, originalCount + 2),
            };
            for (int pointIndex = 0; pointIndex < originalCount; pointIndex++)
            {
                Dictionary<DelaunayEdge, int> edgeCounts = new Dictionary<DelaunayEdge, int>();
                for (int triangleIndex = triangles.Count - 1; triangleIndex >= 0; triangleIndex--)
                {
                    DelaunayTriangle triangle = triangles[triangleIndex];
                    if (!IsPointInCircumcircle(points[pointIndex], triangle, points)) continue;
                    AddTriangleEdges(triangle, edgeCounts);
                    triangles.RemoveAt(triangleIndex);
                }

                foreach (KeyValuePair<DelaunayEdge, int> pair in edgeCounts)
                    if (pair.Value == 1) triangles.Add(new DelaunayTriangle(pair.Key.first, pair.Key.second, pointIndex));
            }

            for (int i = triangles.Count - 1; i >= 0; i--)
                if (triangles[i].ContainsVertex(originalCount) || triangles[i].ContainsVertex(originalCount + 1) || triangles[i].ContainsVertex(originalCount + 2)) triangles.RemoveAt(i);
            points.RemoveRange(originalCount, 3);
            return triangles;
        }

        /// <summary>将三角形的三条边计入边界计数。</summary>
        private static void AddTriangleEdges(DelaunayTriangle triangle, Dictionary<DelaunayEdge, int> edgeCounts)
        {
            AddEdgeCount(new DelaunayEdge(triangle.a, triangle.b), edgeCounts);
            AddEdgeCount(new DelaunayEdge(triangle.b, triangle.c), edgeCounts);
            AddEdgeCount(new DelaunayEdge(triangle.c, triangle.a), edgeCounts);
        }

        /// <summary>累计 Delaunay 边出现次数。</summary>
        private static void AddEdgeCount(DelaunayEdge edge, Dictionary<DelaunayEdge, int> edgeCounts)
        {
            edgeCounts.TryGetValue(edge, out int count);
            edgeCounts[edge] = count + 1;
        }

        /// <summary>判断点是否位于三角形外接圆内。</summary>
        private static bool IsPointInCircumcircle(Vector2 point, DelaunayTriangle triangle, List<Vector2> points)
        {
            Vector2 first = points[triangle.a] - point;
            Vector2 second = points[triangle.b] - point;
            Vector2 third = points[triangle.c] - point;
            float determinant = (first.sqrMagnitude * (second.x * third.y - third.x * second.y)) -
                                (second.sqrMagnitude * (first.x * third.y - third.x * first.y)) +
                                (third.sqrMagnitude * (first.x * second.y - second.x * first.y));
            Vector2 firstEdge = points[triangle.b] - points[triangle.a];
            Vector2 secondEdge = points[triangle.c] - points[triangle.a];
            float orientation = firstEdge.x * secondEdge.y - firstEdge.y * secondEdge.x;
            return orientation >= 0f ? determinant > 0f : determinant < 0f;
        }

        /// <summary>向相互有效的种子记录 Delaunay 邻接关系。</summary>
        private static void AddDelaunayNeighborPair(int first, int second, int seedCount, List<HashSet<int>> neighbors)
        {
            if (first >= seedCount || second >= seedCount || first == second) return;
            neighbors[first].Add(second);
            neighbors[second].Add(first);
        }

        /// <summary>创建位于有效区域且相互分散的随机种子。</summary>
        private static Vector2Int[] CreateSeeds(int width, int height, bool[] validPixels, int seedCount, int seed)
        {
            System.Random random = new System.Random(seed); Vector2Int[] seeds = new Vector2Int[seedCount]; HashSet<int> usedPixels = new HashSet<int>();
            for (int seedIndex = 0; seedIndex < seedCount; seedIndex++)
            {
                Vector2Int bestCandidate = default; int bestDistance = -1; bool hasCandidate = false;
                int candidateCount = Mathf.Min(128, Mathf.Max(24, seedCount * 4));
                for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
                {
                    int index = FindRandomValidIndex(width, height, validPixels, random);
                    if (index < 0 || usedPixels.Contains(index)) continue;
                    Vector2Int candidate = new Vector2Int(index % width, index / width);
                    int distance = GetMinimumSquaredDistance(candidate, seeds, seedIndex);
                    if (!hasCandidate || distance > bestDistance) { bestCandidate = candidate; bestDistance = distance; hasCandidate = true; }
                }
                if (!hasCandidate)
                {
                    int index = FindFirstUnusedValidIndex(validPixels, usedPixels);
                    if (index < 0) break;
                    bestCandidate = new Vector2Int(index % width, index / width);
                }
                seeds[seedIndex] = bestCandidate; usedPixels.Add(ToIndex(bestCandidate.x, bestCandidate.y, width));
            }
            return seeds;
        }

        /// <summary>在有效掩码内随机寻找像素索引。</summary>
        private static int FindRandomValidIndex(int width, int height, bool[] validPixels, System.Random random)
        {
            for (int i = 0; i < 128; i++) { int index = ToIndex(random.Next(width), random.Next(height), width); if (validPixels[index]) return index; }
            return FindFirstUnusedValidIndex(validPixels, null);
        }

        /// <summary>按顺序寻找未使用的有效像素。</summary>
        private static int FindFirstUnusedValidIndex(bool[] validPixels, HashSet<int> usedPixels)
        {
            for (int i = 0; i < validPixels.Length; i++) if (validPixels[i] && (usedPixels == null || !usedPixels.Contains(i))) return i;
            return -1;
        }

        /// <summary>将有效像素归属给最近种子点。</summary>
        private static int[] AssignNearestSeed(int width, int height, bool[] validPixels, Vector2Int[] seeds, ImageFragmentAlgorithmSettings settings, bool useNoise)
        {
            int[] owners = new int[width * height]; Fill(owners, -1);
            float offsetX = settings.seed % 997, offsetY = (settings.seed / 997) % 991;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = ToIndex(x, y, width); if (!validPixels[index]) continue;
                float sampleX = x, sampleY = y;
                if (useNoise)
                {
                    float scale = Mathf.Max(1f, settings.noiseScale);
                    sampleX += (Mathf.PerlinNoise((x + offsetX) / scale, (y + offsetY) / scale) - 0.5f) * settings.noiseStrength;
                    sampleY += (Mathf.PerlinNoise((x + offsetX + 313f) / scale, (y + offsetY + 719f) / scale) - 0.5f) * settings.noiseStrength;
                }
                int closestSeed = 0; float closestDistance = float.MaxValue;
                for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
                {
                    float dx = sampleX - seeds[seedIndex].x, dy = sampleY - seeds[seedIndex].y, distance = dx * dx + dy * dy;
                    if (distance < closestDistance) { closestDistance = distance; closestSeed = seedIndex; }
                }
                owners[index] = closestSeed;
            }
            return owners;
        }

        /// <summary>将原始标签转换为连续导出标签与裁剪矩形。</summary>
        private static VoronoiFragmentResult BuildResult(int width, int height, int[] owners, bool[] validPixels, int gapPixels, Vector2Int[] seeds)
        {
            bool[] retainedPixels = CreateRetainedMask(width, height, owners, validPixels, gapPixels);
            Dictionary<int, int> remap = new Dictionary<int, int>(); int[] normalizedOwners = new int[owners.Length]; Fill(normalizedOwners, -1);
            for (int i = 0; i < owners.Length; i++)
            {
                if (!retainedPixels[i] || owners[i] < 0) continue;
                if (!remap.TryGetValue(owners[i], out int normalizedOwner)) { normalizedOwner = remap.Count; remap.Add(owners[i], normalizedOwner); }
                normalizedOwners[i] = normalizedOwner;
            }
            if (remap.Count == 0) throw new InvalidOperationException("当前参数使所有碎片都变为空白。");
            return new VoronoiFragmentResult { owners = normalizedOwners, retainedPixels = retainedPixels, seeds = seeds, fragmentRects = CalculateFragmentRects(width, height, remap.Count, normalizedOwners, retainedPixels) };
        }

        /// <summary>从内部交界向每个碎片收缩指定像素数。</summary>
        private static bool[] CreateRetainedMask(int width, int height, int[] owners, bool[] validPixels, int gapPixels)
        {
            bool[] retainedPixels = new bool[owners.Length];
            if (gapPixels == 0) { Array.Copy(validPixels, retainedPixels, validPixels.Length); return retainedPixels; }
            int[] distances = new int[owners.Length]; Fill(distances, -1); Queue<int> queue = new Queue<int>();
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = ToIndex(x, y, width);
                if (validPixels[index] && IsInternalBoundary(x, y, width, height, owners, validPixels)) { distances[index] = 0; queue.Enqueue(index); }
            }
            while (queue.Count > 0)
            {
                int index = queue.Dequeue(); if (distances[index] >= gapPixels - 1) continue;
                int x = index % width, y = index / width;
                TryExpandDistance(x - 1, y, width, height, owners, validPixels, distances, queue, index);
                TryExpandDistance(x + 1, y, width, height, owners, validPixels, distances, queue, index);
                TryExpandDistance(x, y - 1, width, height, owners, validPixels, distances, queue, index);
                TryExpandDistance(x, y + 1, width, height, owners, validPixels, distances, queue, index);
            }
            for (int i = 0; i < retainedPixels.Length; i++) retainedPixels[i] = validPixels[i] && (distances[i] < 0 || distances[i] >= gapPixels);
            return retainedPixels;
        }

        /// <summary>判断像素是否与另一有效碎片共享边界。</summary>
        private static bool IsInternalBoundary(int x, int y, int width, int height, int[] owners, bool[] validPixels)
        {
            int owner = owners[ToIndex(x, y, width)];
            return IsDifferentValidNeighbor(x - 1, y, width, height, owners, validPixels, owner) || IsDifferentValidNeighbor(x + 1, y, width, height, owners, validPixels, owner) || IsDifferentValidNeighbor(x, y - 1, width, height, owners, validPixels, owner) || IsDifferentValidNeighbor(x, y + 1, width, height, owners, validPixels, owner);
        }

        /// <summary>判断邻居是否属于不同的有效碎片。</summary>
        private static bool IsDifferentValidNeighbor(int x, int y, int width, int height, int[] owners, bool[] validPixels, int owner)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            int index = ToIndex(x, y, width); return validPixels[index] && owners[index] != owner;
        }

        /// <summary>在同一碎片内扩展到边界的距离。</summary>
        private static void TryExpandDistance(int x, int y, int width, int height, int[] owners, bool[] validPixels, int[] distances, Queue<int> queue, int sourceIndex)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            int targetIndex = ToIndex(x, y, width);
            if (!validPixels[targetIndex] || distances[targetIndex] >= 0 || owners[targetIndex] != owners[sourceIndex]) return;
            distances[targetIndex] = distances[sourceIndex] + 1; queue.Enqueue(targetIndex);
        }

        /// <summary>计算每个有效碎片的像素包围矩形。</summary>
        private static RectInt[] CalculateFragmentRects(int width, int height, int fragmentCount, int[] owners, bool[] retainedPixels)
        {
            int[] minXs = new int[fragmentCount], minYs = new int[fragmentCount], maxXs = new int[fragmentCount], maxYs = new int[fragmentCount];
            Fill(minXs, width); Fill(minYs, height); Fill(maxXs, -1); Fill(maxYs, -1);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = ToIndex(x, y, width), owner = owners[index]; if (!retainedPixels[index] || owner < 0) continue;
                minXs[owner] = Mathf.Min(minXs[owner], x); minYs[owner] = Mathf.Min(minYs[owner], y); maxXs[owner] = Mathf.Max(maxXs[owner], x); maxYs[owner] = Mathf.Max(maxYs[owner], y);
            }
            RectInt[] rects = new RectInt[fragmentCount];
            for (int i = 0; i < fragmentCount; i++) rects[i] = new RectInt(minXs[i], minYs[i], maxXs[i] - minXs[i] + 1, maxYs[i] - minYs[i] + 1);
            return rects;
        }

        /// <summary>计算候选种子到既有种子的最小平方距离。</summary>
        private static int GetMinimumSquaredDistance(Vector2Int candidate, Vector2Int[] seeds, int seedCount)
        {
            if (seedCount == 0) return int.MaxValue;
            int minimumDistance = int.MaxValue;
            for (int i = 0; i < seedCount; i++) { int dx = candidate.x - seeds[i].x, dy = candidate.y - seeds[i].y; minimumDistance = Mathf.Min(minimumDistance, dx * dx + dy * dy); }
            return minimumDistance;
        }

        /// <summary>统计有效像素数量。</summary>
        private static int CountValidPixels(bool[] validPixels)
        {
            int count = 0; for (int i = 0; i < validPixels.Length; i++) if (validPixels[i]) count++; return count;
        }

        /// <summary>将二维坐标转换为一维像素索引。</summary>
        private static int ToIndex(int x, int y, int width) { return y * width + x; }

        /// <summary>使用循环填充数组以兼容 .NET Framework 目标。</summary>
        private static void Fill<T>(T[] values, T value) { for (int i = 0; i < values.Length; i++) values[i] = value; }

        /// <summary>Delaunay 三角网中的三角形索引。</summary>
        private readonly struct DelaunayTriangle
        {
            internal readonly int a;
            internal readonly int b;
            internal readonly int c;

            internal DelaunayTriangle(int a, int b, int c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            internal bool ContainsVertex(int vertex)
            {
                return a == vertex || b == vertex || c == vertex;
            }
        }

        /// <summary>忽略方向的 Delaunay 边。</summary>
        private readonly struct DelaunayEdge : IEquatable<DelaunayEdge>
        {
            internal readonly int first;
            internal readonly int second;

            internal DelaunayEdge(int first, int second)
            {
                if (first < second)
                {
                    this.first = first;
                    this.second = second;
                }
                else
                {
                    this.first = second;
                    this.second = first;
                }
            }

            public bool Equals(DelaunayEdge other)
            {
                return first == other.first && second == other.second;
            }

            public override bool Equals(object obj)
            {
                return obj is DelaunayEdge other && Equals(other);
            }

            public override int GetHashCode()
            {
                return first * 397 ^ second;
            }
        }

    }
}
#endif
