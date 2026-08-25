#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle.Editor.Tests
{
    /// <summary>运行时碎片算法和连接生成的单元测试。</summary>
    public sealed class RuntimePuzzleFragmentGeneratorTests
    {
        /// <summary>五种运行时算法都应生成可连接的有效碎片。</summary>
        [Test]
        public void Generate_AllAlgorithms_ReturnsValidFragmentsAndConnections()
        {
            const int width = 32;
            const int height = 24;
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = new Color32(255, 255, 255, x > 2 && y > 2 ? (byte)255 : (byte)0);
            }

            foreach (RuntimePuzzleFragmentAlgorithm algorithm in System.Enum.GetValues(typeof(RuntimePuzzleFragmentAlgorithm)))
            {
                RuntimePuzzleFragmentSettings settings = new RuntimePuzzleFragmentSettings
                {
                    Algorithm = algorithm,
                    MaxFragmentCount = 6,
                    Seed = 2026,
                    GapPixels = 1,
                    AlphaThreshold = 1,
                };
                RuntimePuzzleFragmentResult result = RuntimePuzzleFragmentGenerator.Generate(width, height, pixels, settings);
                IrregularPuzzleConnection[] connections = RuntimePuzzleConnectionBuilder.Build(width, height, result.connectionOwners);

                Assert.Greater(result.fragmentRects.Length, 1, algorithm.ToString());
                Assert.Greater(connections.Length, 0, algorithm.ToString());
                for (int i = 0; i < connections.Length; i++)
                {
                    Assert.Less(connections[i].FirstPieceIndex, connections[i].SecondPieceIndex);
                    Assert.Greater(connections[i].SharedBoundaryPixelCount, 0);
                }
            }
        }
    }
}
#endif
