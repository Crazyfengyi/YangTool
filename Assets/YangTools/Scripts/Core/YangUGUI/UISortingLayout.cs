using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// 为每个 Sorting Layer 分配互不重叠的组和页面排序区间
    /// </summary>
    internal sealed class UISortingLayout
    {
        private readonly Dictionary<Canvas, int> originalOrders = new(); //首次捕获的排序值
        private readonly Dictionary<int, int> nextOrders = new(); //各图层的下一个排序值
        private readonly List<UIGroup> groups = new(); //组排序快照
        private readonly List<IUIPanel> panels = new(); //页面快照
        private readonly List<Canvas> canvases = new(); //Canvas 扫描缓存
        private readonly List<Canvas> destroyedCanvases = new(); //已销毁的缓存键
        private readonly List<CanvasEntry> entries = new(); //页面局部排序
        private readonly List<CanvasAssignment> assignments = new(); //待提交排序
        private readonly int maxOrder; //排序值上限

        /// <summary>
        /// 设置不超过 Unity 有效范围的排序容量
        /// </summary>
        internal UISortingLayout(int maxOrder = short.MaxValue)
        {
            if (maxOrder < 0 || maxOrder > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(maxOrder));
            this.maxOrder = maxOrder;
        }

        /// <summary>
        /// 构建完整布局并校验容量 此步骤不写入 Canvas
        /// </summary>
        internal void Prepare(List<UIGroup> source)
        {
            assignments.Clear();
            nextOrders.Clear();
            destroyedCanvases.Clear();
            foreach (Canvas canvas in originalOrders.Keys)
            {
                if (!canvas) destroyedCanvases.Add(canvas);
            }
            foreach (Canvas canvas in destroyedCanvases) originalOrders.Remove(canvas);

            groups.Clear();
            groups.AddRange(source);
            groups.Sort(CompareGroups);
            foreach (UIGroup group in groups)
            {
                if (group.Helper is Component helper && helper)
                {
                    Canvas canvas = helper.GetComponent<Canvas>(); //组根 Canvas
                    if (canvas) AddAssignment(canvas, NextOrder(canvas.sortingLayerID));
                }
                panels.Clear();
                group.GetAllPanels(panels);
                for (int i = panels.Count - 1; i >= 0; i--)
                {
                    if (panels[i] is Component panel && panel) PreparePanel(panel);
                }
            }
        }

        /// <summary>
        /// 保留同一页面内 Canvas 的原始顺序和相同排序值
        /// </summary>
        private void PreparePanel(Component panel)
        {
            Canvas root = panel.GetComponent<Canvas>(); //页面根 Canvas
            canvases.Clear();
            panel.GetComponentsInChildren(true, canvases);
            entries.Clear();
            foreach (Canvas canvas in canvases)
            {
                if (!canvas || (canvas != root && !canvas.overrideSorting)) continue;
                if (!originalOrders.TryGetValue(canvas, out int originalOrder))
                {
                    originalOrder = canvas.sortingOrder;
                    originalOrders.Add(canvas, originalOrder);
                }
                entries.Add(new CanvasEntry(canvas, originalOrder));
            }
            entries.Sort(CompareEntries);
            int previousLayer = 0; //上一个图层
            int previousOriginal = 0; //上一个原始排序
            int order = 0; //当前分配值
            for (int i = 0; i < entries.Count; i++)
            {
                CanvasEntry entry = entries[i]; //当前 Canvas
                int layer = entry.Canvas.sortingLayerID; //当前图层
                if (i == 0 || layer != previousLayer || entry.OriginalOrder != previousOriginal)
                {
                    order = NextOrder(layer);
                }
                AddAssignment(entry.Canvas, order);
                previousLayer = layer;
                previousOriginal = entry.OriginalOrder;
            }
        }

        /// <summary>
        /// 预留一个合法排序值
        /// </summary>
        private int NextOrder(int layer)
        {
            nextOrders.TryGetValue(layer, out int order);
            if (order > maxOrder) throw new InvalidOperationException($"UI排序容量超过 {maxOrder}");
            nextOrders[layer] = order + 1;
            return order;
        }

        /// <summary>
        /// 保存排序写入计划
        /// </summary>
        private void AddAssignment(Canvas canvas, int order)
        {
            assignments.Add(new CanvasAssignment(canvas, order));
        }

        /// <summary>
        /// 提交已通过容量校验的完整布局
        /// </summary>
        internal void Apply()
        {
            foreach (CanvasAssignment assignment in assignments)
            {
                if (assignment.Canvas) assignment.Canvas.sortingOrder = assignment.Order;
            }
        }

        /// <summary>
        /// 清理缓存引用
        /// </summary>
        internal void Clear()
        {
            originalOrders.Clear();
            nextOrders.Clear();
            groups.Clear();
            panels.Clear();
            canvases.Clear();
            entries.Clear();
            assignments.Clear();
            destroyedCanvases.Clear();
        }

        /// <summary>
        /// 相同深度时按组注册顺序排列
        /// </summary>
        private static int CompareGroups(UIGroup left, UIGroup right)
        {
            int result = left.Depth.CompareTo(right.Depth); //深度比较
            return result != 0 ? result : left.RegistrationOrder.CompareTo(right.RegistrationOrder);
        }

        /// <summary>
        /// 各图层内按原始 Canvas 排序排列
        /// </summary>
        private static int CompareEntries(CanvasEntry left, CanvasEntry right)
        {
            int result = left.Canvas.sortingLayerID.CompareTo(right.Canvas.sortingLayerID); //图层比较
            return result != 0 ? result : left.OriginalOrder.CompareTo(right.OriginalOrder);
        }

        /// <summary>
        /// 保存 Canvas 的原始局部排序
        /// </summary>
        private readonly struct CanvasEntry
        {
            public readonly Canvas Canvas; //目标 Canvas
            public readonly int OriginalOrder; //原始排序

            /// <summary>
            /// 创建局部排序记录
            /// </summary>
            public CanvasEntry(Canvas canvas, int originalOrder)
            {
                Canvas = canvas;
                OriginalOrder = originalOrder;
            }
        }

        /// <summary>
        /// 保存预校验后的 Canvas 排序
        /// </summary>
        private readonly struct CanvasAssignment
        {
            public readonly Canvas Canvas; //目标 Canvas
            public readonly int Order; //目标排序

            /// <summary>
            /// 创建排序写入记录
            /// </summary>
            public CanvasAssignment(Canvas canvas, int order)
            {
                Canvas = canvas;
                Order = order;
            }
        }
    }
}
