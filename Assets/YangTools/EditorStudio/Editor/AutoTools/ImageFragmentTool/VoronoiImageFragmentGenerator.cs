#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.EditorStudio.ImageFragmentTool
{
    /// <summary>
    /// Voronoi 图片切割的像素分区结果。
    /// </summary>
    internal sealed class VoronoiFragmentResult
    {
        internal int[] owners;
        internal bool[] retainedPixels;
        internal Vector2Int[] seeds;
        internal RectInt[] fragmentRects;
    }

    /// <summary>
    /// 生成可复现的 Voronoi 不规则图片碎片。
    /// </summary>
    internal static class VoronoiImageFragmentGenerator
    {
        /// <summary>
        /// 生成图片每个像素所属的碎片区域及其保留掩码。
        /// </summary>
        internal static VoronoiFragmentResult Generate(int width, int height, int fragmentCount, int seed,
            int gapPixels)
        {
            ValidateArguments(width, height, fragmentCount, gapPixels);

            Vector2Int[] seeds = CreateSeeds(width, height, fragmentCount, seed);
            int[] owners = AssignOwners(width, height, seeds);
            bool[] retainedPixels = CreateRetainedMask(width, height, owners, gapPixels);
            RectInt[] fragmentRects = CalculateFragmentRects(width, height, fragmentCount, owners, retainedPixels);

            return new VoronoiFragmentResult
            {
                owners = owners,
                retainedPixels = retainedPixels,
                seeds = seeds,
                fragmentRects = fragmentRects,
            };
        }

        /// <summary>
        /// 验证生成参数是否能产生有效的碎片。
        /// </summary>
        private static void ValidateArguments(int width, int height, int fragmentCount, int gapPixels)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "图片尺寸必须大于零。");
            }

            if (fragmentCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(fragmentCount), "碎片数量至少为 2。");
            }

            if (fragmentCount > width * height)
            {
                throw new ArgumentOutOfRangeException(nameof(fragmentCount), "碎片数量不能超过图片像素数量。");
            }

            if (gapPixels < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gapPixels), "碎片间隙不能小于零。");
            }
        }

        /// <summary>
        /// 使用最佳候选采样创建彼此分散的随机种子点。
        /// </summary>
        private static Vector2Int[] CreateSeeds(int width, int height, int fragmentCount, int seed)
        {
            System.Random random = new System.Random(seed);
            Vector2Int[] seeds = new Vector2Int[fragmentCount];
            HashSet<int> usedPixels = new HashSet<int>();
            for (int seedIndex = 0; seedIndex < fragmentCount; seedIndex++)
            {
                int candidateCount = Mathf.Min(128, Mathf.Max(24, fragmentCount * 4));
                Vector2Int bestCandidate = default;
                int bestDistance = -1;
                bool hasCandidate = false;
                for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
                {
                    Vector2Int candidate = new Vector2Int(random.Next(width), random.Next(height));
                    if (usedPixels.Contains(ToIndex(candidate.x, candidate.y, width)))
                    {
                        continue;
                    }

                    int minimumDistance = GetMinimumSquaredDistance(candidate, seeds, seedIndex);
                    if (!hasCandidate || minimumDistance > bestDistance)
                    {
                        bestCandidate = candidate;
                        bestDistance = minimumDistance;
                        hasCandidate = true;
                    }
                }

                if (!hasCandidate)
                {
                    bestCandidate = FindUnusedPixel(width, height, usedPixels);
                }

                seeds[seedIndex] = bestCandidate;
                usedPixels.Add(ToIndex(bestCandidate.x, bestCandidate.y, width));
            }

            return seeds;
        }

        /// <summary>
        /// 获取候选点到已生成种子点的最小平方距离。
        /// </summary>
        private static int GetMinimumSquaredDistance(Vector2Int candidate, Vector2Int[] seeds, int seedCount)
        {
            if (seedCount == 0)
            {
                return int.MaxValue;
            }

            int minimumDistance = int.MaxValue;
            for (int i = 0; i < seedCount; i++)
            {
                int xDelta = candidate.x - seeds[i].x;
                int yDelta = candidate.y - seeds[i].y;
                int distance = xDelta * xDelta + yDelta * yDelta;
                minimumDistance = Mathf.Min(minimumDistance, distance);
            }

            return minimumDistance;
        }

        /// <summary>
        /// 在极小图片的候选点碰撞时查找未占用的像素。
        /// </summary>
        private static Vector2Int FindUnusedPixel(int width, int height, HashSet<int> usedPixels)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!usedPixels.Contains(ToIndex(x, y, width)))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            throw new InvalidOperationException("没有可用的 Voronoi 种子点。");
        }

        /// <summary>
        /// 为每个像素分配最近的 Voronoi 种子点。
        /// </summary>
        private static int[] AssignOwners(int width, int height, Vector2Int[] seeds)
        {
            int[] owners = new int[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int closestSeed = 0;
                    int closestDistance = int.MaxValue;
                    for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
                    {
                        int xDelta = x - seeds[seedIndex].x;
                        int yDelta = y - seeds[seedIndex].y;
                        int distance = xDelta * xDelta + yDelta * yDelta;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestSeed = seedIndex;
                        }
                    }

                    owners[ToIndex(x, y, width)] = closestSeed;
                }
            }

            return owners;
        }

        /// <summary>
        /// 根据间隙像素从每个碎片的内部边缘向内收缩。
        /// </summary>
        private static bool[] CreateRetainedMask(int width, int height, int[] owners, int gapPixels)
        {
            bool[] retainedPixels = new bool[owners.Length];
            if (gapPixels == 0)
            {
                Fill(retainedPixels, true);
                return retainedPixels;
            }

            int[] distances = new int[owners.Length];
            Fill(distances, -1);
            Queue<int> boundaryQueue = new Queue<int>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = ToIndex(x, y, width);
                    if (!IsInternalBoundary(x, y, width, height, owners))
                    {
                        continue;
                    }

                    distances[index] = 0;
                    boundaryQueue.Enqueue(index);
                }
            }

            while (boundaryQueue.Count > 0)
            {
                int index = boundaryQueue.Dequeue();
                if (distances[index] >= gapPixels - 1)
                {
                    continue;
                }

                int x = index % width;
                int y = index / width;
                TryExpandDistance(x - 1, y, width, height, owners, distances, boundaryQueue, index);
                TryExpandDistance(x + 1, y, width, height, owners, distances, boundaryQueue, index);
                TryExpandDistance(x, y - 1, width, height, owners, distances, boundaryQueue, index);
                TryExpandDistance(x, y + 1, width, height, owners, distances, boundaryQueue, index);
            }

            for (int i = 0; i < retainedPixels.Length; i++)
            {
                retainedPixels[i] = distances[i] < 0 || distances[i] >= gapPixels;
            }

            return retainedPixels;
        }

        /// <summary>
        /// 判断像素是否与其他碎片共享四方向边界。
        /// </summary>
        private static bool IsInternalBoundary(int x, int y, int width, int height, int[] owners)
        {
            int owner = owners[ToIndex(x, y, width)];
            return (x > 0 && owners[ToIndex(x - 1, y, width)] != owner) ||
                   (x < width - 1 && owners[ToIndex(x + 1, y, width)] != owner) ||
                   (y > 0 && owners[ToIndex(x, y - 1, width)] != owner) ||
                   (y < height - 1 && owners[ToIndex(x, y + 1, width)] != owner);
        }

        /// <summary>
        /// 在同一碎片内扩展边缘到当前像素的距离。
        /// </summary>
        private static void TryExpandDistance(int x, int y, int width, int height, int[] owners, int[] distances,
            Queue<int> boundaryQueue, int sourceIndex)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            int targetIndex = ToIndex(x, y, width);
            if (distances[targetIndex] >= 0 || owners[targetIndex] != owners[sourceIndex])
            {
                return;
            }

            distances[targetIndex] = distances[sourceIndex] + 1;
            boundaryQueue.Enqueue(targetIndex);
        }

        /// <summary>
        /// 计算每个碎片保留像素的最小包围矩形。
        /// </summary>
        private static RectInt[] CalculateFragmentRects(int width, int height, int fragmentCount, int[] owners,
            bool[] retainedPixels)
        {
            int[] minXs = new int[fragmentCount];
            int[] minYs = new int[fragmentCount];
            int[] maxXs = new int[fragmentCount];
            int[] maxYs = new int[fragmentCount];
            Fill(minXs, width);
            Fill(minYs, height);
            Fill(maxXs, -1);
            Fill(maxYs, -1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = ToIndex(x, y, width);
                    if (!retainedPixels[index])
                    {
                        continue;
                    }

                    int owner = owners[index];
                    minXs[owner] = Mathf.Min(minXs[owner], x);
                    minYs[owner] = Mathf.Min(minYs[owner], y);
                    maxXs[owner] = Mathf.Max(maxXs[owner], x);
                    maxYs[owner] = Mathf.Max(maxYs[owner], y);
                }
            }

            RectInt[] fragmentRects = new RectInt[fragmentCount];
            for (int i = 0; i < fragmentCount; i++)
            {
                if (maxXs[i] < minXs[i] || maxYs[i] < minYs[i])
                {
                    throw new InvalidOperationException($"碎片 {i} 在当前间隙设置下没有可保留的像素。");
                }

                fragmentRects[i] = new RectInt(minXs[i], minYs[i], maxXs[i] - minXs[i] + 1,
                    maxYs[i] - minYs[i] + 1);
            }

            return fragmentRects;
        }

        /// <summary>
        /// 将二维坐标转换为像素数组索引。
        /// </summary>
        private static int ToIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        /// <summary>
        /// 使用循环填充数组，兼容项目当前的 .NET Framework 目标框架。
        /// </summary>
        private static void Fill<T>(T[] values, T value)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = value;
            }
        }
    }
}
#endif
