#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace YangTools.EditorStudio.ImageFragmentTool
{
    /// <summary>
    /// Voronoi 图片碎片算法的编辑器单元测试。
    /// </summary>
    public sealed class VoronoiImageFragmentGeneratorTests
    {
        /// <summary>
        /// 相同参数应始终得到相同的像素归属结果。
        /// </summary>
        [Test]
        public void Generate_WithSameParameters_ReturnsDeterministicResult()
        {
            VoronoiFragmentResult firstResult = VoronoiImageFragmentGenerator.Generate(48, 32, 8, 12345, 0);
            VoronoiFragmentResult secondResult = VoronoiImageFragmentGenerator.Generate(48, 32, 8, 12345, 0);

            CollectionAssert.AreEqual(firstResult.seeds, secondResult.seeds);
            CollectionAssert.AreEqual(firstResult.owners, secondResult.owners);
            CollectionAssert.AreEqual(firstResult.fragmentRects, secondResult.fragmentRects);
        }

        /// <summary>
        /// 零间隙时每个像素应恰好归属于一个保留碎片。
        /// </summary>
        [Test]
        public void Generate_WithZeroGap_KeepsEverySourcePixel()
        {
            const int width = 40;
            const int height = 24;
            const int fragmentCount = 6;
            VoronoiFragmentResult result = VoronoiImageFragmentGenerator.Generate(width, height, fragmentCount, 6789, 0);
            int[] retainedCountByFragment = new int[fragmentCount];

            for (int index = 0; index < result.owners.Length; index++)
            {
                Assert.IsTrue(result.retainedPixels[index]);
                retainedCountByFragment[result.owners[index]]++;
            }

            for (int fragmentIndex = 0; fragmentIndex < fragmentCount; fragmentIndex++)
            {
                Assert.Greater(retainedCountByFragment[fragmentIndex], 0);
            }
        }

        /// <summary>
        /// 设置间隙后应移除分界像素，同时每个碎片仍保留有效区域。
        /// </summary>
        [Test]
        public void Generate_WithGap_RemovesBoundaryPixelsAndKeepsFragments()
        {
            const int fragmentCount = 9;
            VoronoiFragmentResult result = VoronoiImageFragmentGenerator.Generate(96, 64, fragmentCount, 2468, 1);
            int[] retainedCountByFragment = new int[fragmentCount];
            bool hasRemovedPixel = false;

            for (int index = 0; index < result.owners.Length; index++)
            {
                if (!result.retainedPixels[index])
                {
                    hasRemovedPixel = true;
                    continue;
                }

                retainedCountByFragment[result.owners[index]]++;
            }

            Assert.IsTrue(hasRemovedPixel);
            for (int fragmentIndex = 0; fragmentIndex < fragmentCount; fragmentIndex++)
            {
                Assert.Greater(retainedCountByFragment[fragmentIndex], 0);
            }
        }

        /// <summary>
        /// 清单应包含稳定的文件名和相对原图中心的 UI 坐标。
        /// </summary>
        [Test]
        public void CreateManifest_ContainsFragmentFileAndUiPosition()
        {
            RectInt[] rects =
            {
                new RectInt(0, 0, 10, 8),
                new RectInt(10, 8, 6, 4),
            };

            ImageFragmentAlgorithmSettings settings = new ImageFragmentAlgorithmSettings
            {
                algorithm = ImageFragmentAlgorithm.NoiseBoundary,
                maxFragmentCount = 4,
                seed = 123,
                gapPixels = 0,
            };
            ImageFragmentManifest manifest = ImageFragmentManifestFactory.Create("Assets/UI/Test.png", 20, 16,
                settings, rects);

            Assert.AreEqual(2, manifest.fragments.Length);
            Assert.AreEqual(ImageFragmentAlgorithm.NoiseBoundary, manifest.algorithm);
            Assert.AreEqual(4, manifest.requestedMaxFragmentCount);
            Assert.AreEqual(2, manifest.actualFragmentCount);
            Assert.AreEqual("Test_0.png", manifest.fragments[0].fileName);
            Assert.AreEqual("Test_1.png", manifest.fragments[1].fileName);
            Assert.AreEqual(new Vector2(-5f, -4f), manifest.fragments[0].uiLocalCenter);
            Assert.AreEqual(new Vector2(3f, 2f), manifest.fragments[1].uiLocalCenter);
        }

        /// <summary>
        /// 所有算法都应生成连续编号、没有重叠保留像素的结果。
        /// </summary>
        [Test]
        public void Generate_AllAlgorithms_ReturnsValidFragmentResult()
        {
            const int width = 48;
            const int height = 32;
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte alpha = x < 4 || y < 4 ? (byte)0 : (byte)255;
                    pixels[y * width + x] = x < width / 2
                        ? new Color32(240, 80, 80, alpha)
                        : new Color32(80, 120, 240, alpha);
                }
            }

            ImageFragmentAlgorithm[] algorithms = (ImageFragmentAlgorithm[])System.Enum.GetValues(typeof(ImageFragmentAlgorithm));
            for (int algorithmIndex = 0; algorithmIndex < algorithms.Length; algorithmIndex++)
            {
                ImageFragmentAlgorithmSettings settings = new ImageFragmentAlgorithmSettings
                {
                    algorithm = algorithms[algorithmIndex],
                    maxFragmentCount = 6,
                    seed = 2468,
                };
                VoronoiFragmentResult result = ImageFragmentAlgorithmGenerator.Generate(width, height, pixels, settings);

                Assert.Greater(result.fragmentRects.Length, 0, algorithms[algorithmIndex].ToString());
                for (int pixelIndex = 0; pixelIndex < result.owners.Length; pixelIndex++)
                {
                    if (result.retainedPixels[pixelIndex])
                    {
                        Assert.That(result.owners[pixelIndex], Is.GreaterThanOrEqualTo(0));
                        Assert.That(result.owners[pixelIndex], Is.LessThan(result.fragmentRects.Length));
                    }
                }
            }
        }

        /// <summary>
        /// Alpha 轮廓模式不能保留低于阈值的像素。
        /// </summary>
        [Test]
        public void Generate_AlphaContour_DoesNotRetainPixelsBelowThreshold()
        {
            const int width = 16;
            const int height = 16;
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = new Color32(255, 255, 255, x >= 4 && y >= 4 ? (byte)255 : (byte)0);
                }
            }

            VoronoiFragmentResult result = ImageFragmentAlgorithmGenerator.Generate(width, height, pixels,
                new ImageFragmentAlgorithmSettings
                {
                    algorithm = ImageFragmentAlgorithm.AlphaContour,
                    maxFragmentCount = 4,
                    alphaThreshold = 1,
                });

            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                if (pixels[pixelIndex].a == 0)
                {
                    Assert.IsFalse(result.retainedPixels[pixelIndex]);
                }
            }
        }
    }
}
#endif
