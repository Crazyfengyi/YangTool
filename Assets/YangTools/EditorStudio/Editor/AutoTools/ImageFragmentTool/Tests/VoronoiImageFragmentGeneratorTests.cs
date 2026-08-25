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

            ImageFragmentManifest manifest = ImageFragmentManifestFactory.Create("Assets/UI/Test.png", 20, 16,
                123, 2, 0, rects);

            Assert.AreEqual(2, manifest.fragments.Length);
            Assert.AreEqual("Test_0.png", manifest.fragments[0].fileName);
            Assert.AreEqual("Test_1.png", manifest.fragments[1].fileName);
            Assert.AreEqual(new Vector2(-5f, -4f), manifest.fragments[0].uiLocalCenter);
            Assert.AreEqual(new Vector2(3f, 2f), manifest.fragments[1].uiLocalCenter);
        }
    }
}
#endif
