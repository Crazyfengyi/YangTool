#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.EditorStudio.ImageFragmentTool
{
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
        public int fragmentCount;
        public int gapPixels;
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
            int seed, int fragmentCount, int gapPixels, IReadOnlyList<RectInt> fragmentRects)
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
                seed = seed,
                fragmentCount = fragmentCount,
                gapPixels = gapPixels,
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
