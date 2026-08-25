using UnityEngine;
using UnityEngine.EventSystems;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 单个 UI 拼图碎片，负责把拖拽事件交给所属关卡。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class IrregularPuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private int pieceIndex;
        [SerializeField] private Vector2 solvedPosition;

        private RectTransform rectTransform;
        private IrregularPuzzleLevel level;
        private IrregularPuzzleGroup group;

        /// <summary>碎片在关卡中的唯一编号。</summary>
        public int PieceIndex => pieceIndex;
        /// <summary>碎片在完整拼图中的局部坐标。</summary>
        public Vector2 SolvedPosition => solvedPosition;
        internal RectTransform RectTransform => rectTransform;
        internal IrregularPuzzleGroup Group => group;

        /// <summary>
        /// 初始化组件引用。
        /// </summary>
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (level == null)
            {
                level = GetComponentInParent<IrregularPuzzleLevel>();
            }
        }

        /// <summary>
        /// 写入编辑器生成的碎片数据。
        /// </summary>
        public void Configure(IrregularPuzzleLevel puzzleLevel, int index, Vector2 correctPosition)
        {
            level = puzzleLevel;
            pieceIndex = index;
            solvedPosition = correctPosition;
        }

        /// <summary>
        /// 处理开始拖拽。
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            level?.BeginDrag(this, eventData);
        }

        /// <summary>
        /// 处理拖拽过程。
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            level?.Drag(eventData);
        }

        /// <summary>
        /// 处理结束拖拽。
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            level?.EndDrag();
        }

        /// <summary>
        /// 设置此碎片当前所属的逻辑组。
        /// </summary>
        internal void SetGroup(IrregularPuzzleGroup puzzleGroup)
        {
            group = puzzleGroup;
        }
    }
}
