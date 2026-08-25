#if UNITY_EDITOR
using NUnit.Framework;

namespace YangTools.Function.IrregularPuzzle.Editor.Tests
{
    /// <summary>
    /// 不规则拼图连接数据生成的编辑器单元测试。
    /// </summary>
    public sealed class IrregularPuzzleConnectionBuilderTests
    {
        /// <summary>
        /// 相同归属图必须生成相同顺序和内容的连接数据。
        /// </summary>
        [Test]
        public void Build_WithSameOwners_ReturnsDeterministicConnections()
        {
            int[] owners =
            {
                0, 0, 1,
                0, 2, 1,
            };

            IrregularPuzzleConnection[] first = IrregularPuzzleConnectionBuilder.Build(3, 2, owners);
            IrregularPuzzleConnection[] second = IrregularPuzzleConnectionBuilder.Build(3, 2, owners);

            Assert.AreEqual(first.Length, second.Length);
            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].FirstPieceIndex, second[i].FirstPieceIndex);
                Assert.AreEqual(first[i].SecondPieceIndex, second[i].SecondPieceIndex);
                Assert.AreEqual(first[i].AnchorPosition, second[i].AnchorPosition);
                Assert.AreEqual(first[i].SharedBoundaryPixelCount, second[i].SharedBoundaryPixelCount);
            }
        }

        /// <summary>
        /// 每对相邻碎片只能生成一条边界长度有效的连接。
        /// </summary>
        [Test]
        public void Build_WithThreeAdjacentPieces_ReturnsUniqueValidPairs()
        {
            int[] owners =
            {
                0, 0, 1,
                0, 2, 1,
            };

            IrregularPuzzleConnection[] connections = IrregularPuzzleConnectionBuilder.Build(3, 2, owners);

            Assert.AreEqual(3, connections.Length);
            for (int i = 0; i < connections.Length; i++)
            {
                Assert.Less(connections[i].FirstPieceIndex, connections[i].SecondPieceIndex);
                Assert.Greater(connections[i].SharedBoundaryPixelCount, 0);
                if (i > 0)
                {
                    Assert.Less(connections[i - 1].FirstPieceIndex * 1000 + connections[i - 1].SecondPieceIndex,
                        connections[i].FirstPieceIndex * 1000 + connections[i].SecondPieceIndex);
                }
            }
        }

        /// <summary>
        /// 透明或间隙像素不会错误地创建碎片连接。
        /// </summary>
        [Test]
        public void Build_WithTransparentGapPixels_IgnoresInvalidOwners()
        {
            int[] owners =
            {
                0, -1, 1,
                0, -1, 1,
            };

            IrregularPuzzleConnection[] connections = IrregularPuzzleConnectionBuilder.Build(3, 2, owners);

            Assert.AreEqual(0, connections.Length);
        }
    }
}
#endif
