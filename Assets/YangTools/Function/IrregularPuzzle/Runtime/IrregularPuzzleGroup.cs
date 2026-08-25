using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 运行时已经拼接在一起的一组碎片。
    /// </summary>
    internal sealed class IrregularPuzzleGroup
    {
        private readonly List<IrregularPuzzlePiece> pieces = new List<IrregularPuzzlePiece>();

        /// <summary>此组的 UI 根节点。</summary>
        internal RectTransform Root { get; }
        /// <summary>此组包含的碎片。</summary>
        internal IReadOnlyList<IrregularPuzzlePiece> Pieces => pieces;

        /// <summary>
        /// 创建一个新的拼图组。
        /// </summary>
        internal IrregularPuzzleGroup(RectTransform root)
        {
            Root = root;
        }

        /// <summary>
        /// 将碎片加入此组并同步运行时归属。
        /// </summary>
        internal void AddPiece(IrregularPuzzlePiece piece)
        {
            pieces.Add(piece);
            piece.SetGroup(this);
        }

        /// <summary>
        /// 将另一组的全部碎片转移到此组。
        /// </summary>
        internal void Absorb(IrregularPuzzleGroup other)
        {
            for (int i = 0; i < other.pieces.Count; i++)
            {
                IrregularPuzzlePiece piece = other.pieces[i];
                piece.RectTransform.SetParent(Root, false);
                AddPiece(piece);
            }

            other.pieces.Clear();
        }
    }
}
