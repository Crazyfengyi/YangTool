#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.EditorStudio.ImageFragmentTool
{
    /// <summary>
    /// 图片碎片的切割算法类型。
    /// </summary>
    public enum ImageFragmentAlgorithm
    {
        Voronoi = 0,
        JitteredGrid = 1,
        NoiseBoundary = 3,
        AlphaContour = 4,
        DelaunayVoronoi = 6,
    }

    /// <summary>
    /// 图片碎片生成参数。
    /// </summary>
    [Serializable]
    public sealed class ImageFragmentAlgorithmSettings
    {
        public ImageFragmentAlgorithm algorithm = ImageFragmentAlgorithm.Voronoi;
        public int maxFragmentCount = 16;
        public int seed = 12345;
        public int gapPixels;
        public float gridJitter = 0.35f;
        public float noiseScale = 64f;
        public float noiseStrength = 12f;
        public byte alphaThreshold = 1;
    }

    /// <summary>
    /// 图片碎片的导出清单。
    /// </summary>
    [Serializable]
    public sealed class ImageFragmentManifest
    {
        public string sourceAssetPath;
        public int sourceWidth;
        public int sourceHeight;
        public int seed;
        public ImageFragmentAlgorithm algorithm;
        public int requestedMaxFragmentCount;
        public int actualFragmentCount;
        public int fragmentCount;
        public int gapPixels;
        public ImageFragmentAlgorithmSettings algorithmSettings;
        public ImageFragmentEntry[] fragments;
    }

    /// <summary>
    /// 单个图片碎片的文件与复原位置信息。
    /// </summary>
    [Serializable]
    public sealed class ImageFragmentEntry
    {
        public int index;
        public string fileName;
        public RectInt sourceRect;
        public Vector2 uiLocalCenter;
    }

    /// <summary>
    /// 根据切割结果创建导出清单。
    /// </summary>
    internal static class ImageFragmentManifestFactory
    {
        /// <summary>
        /// 创建供 UI 复原碎片位置使用的清单数据。
        /// </summary>
        internal static ImageFragmentManifest Create(string sourceAssetPath, int sourceWidth, int sourceHeight,
            ImageFragmentAlgorithmSettings settings, IReadOnlyList<RectInt> fragmentRects)
        {
            ImageFragmentEntry[] entries = new ImageFragmentEntry[fragmentRects.Count];
            Vector2 sourceCenter = new Vector2(sourceWidth * 0.5f, sourceHeight * 0.5f);
            string sourceName = System.IO.Path.GetFileNameWithoutExtension(sourceAssetPath);
            for (int i = 0; i < fragmentRects.Count; i++)
            {
                RectInt rect = fragmentRects[i];
                entries[i] = new ImageFragmentEntry
                {
                    index = i,
                    fileName = GetFragmentFileName(sourceName, i),
                    sourceRect = rect,
                    uiLocalCenter = rect.center - sourceCenter,
                };
            }

            return new ImageFragmentManifest
            {
                sourceAssetPath = sourceAssetPath,
                sourceWidth = sourceWidth,
                sourceHeight = sourceHeight,
                seed = settings.seed,
                algorithm = settings.algorithm,
                requestedMaxFragmentCount = settings.maxFragmentCount,
                actualFragmentCount = fragmentRects.Count,
                fragmentCount = fragmentRects.Count,
                gapPixels = settings.gapPixels,
                algorithmSettings = settings,
                fragments = entries,
            };
        }

        /// <summary>
        /// 获取按序号排列的碎片文件名。
        /// </summary>
        internal static string GetFragmentFileName(string sourceName, int index)
        {
            return $"{sourceName}_{index}.png";
        }
    }
}
#endif
