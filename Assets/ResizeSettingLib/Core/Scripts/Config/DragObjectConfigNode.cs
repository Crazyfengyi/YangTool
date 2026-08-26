using System;
using UnityEngine;

namespace ResizeSettingLib.Core.Config
{
    /// <summary>
    /// 可拖动对象配置节点
    /// </summary>
    public class DragObjectConfigNode : ConfigNodeBase
    {
        public AnchorTypeEnum anchorType; //锚点类型
        public Vector2 anchorPosition; //锚点相对位置
        public float size; //当前尺寸
        public float sizeMin; //最小尺寸
        public float sizeMax; //最大尺寸
        public float sizeStep; //尺寸增加步长
        public Sprite sprite; //所用图标

        public DragObjectConfigNode CopySelf()
        {
            DragObjectConfigNode node = new DragObjectConfigNode();
            node.anchorType = this.anchorType;
            node.id = this.id;
            node.name = this.name;
            node.anchorPosition = this.anchorPosition;
            node.size = this.size;
            node.sizeMin = this.sizeMin;
            node.sizeMax = this.sizeMax;
            node.sizeStep = this.sizeStep;
            node.sprite = this.sprite;
            return node;
        }
    }
}